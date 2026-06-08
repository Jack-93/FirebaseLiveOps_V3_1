using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VerticalGachaUI : MonoBehaviour
{
    private static readonly Color Background =
        new Color32(20, 28, 45, 255);
    private static readonly Color Panel =
        new Color32(37, 49, 73, 255);
    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color Danger =
        new Color32(238, 91, 103, 255);

    private TMP_Text currencyText;
    private TMP_Text pityText;
    private TMP_Text resultText;
    private TMP_Text statusText;
    private Button singleButton;
    private Button tenButton;
    private Button backButton;
    private bool isRolling;

    private void Start()
    {
        GameSettingsManager.ApplySavedSettings();
        MobileScreenLayout.ApplyPortraitSettings();
        EnsureEventSystem();
        BuildInterface();
        RefreshHeader();
        LocalizationManager.ApplyTo(transform);
    }

    private void BuildInterface()
    {
        RectTransform portrait =
            MobileScreenLayout.CreateSafeAreaCanvas(
                "VerticalGachaCanvas",
                Background);

        CreateText(
            "Title",
            portrait,
            "COMPANION GACHA",
            58,
            new Vector2(0.06f, 0.9f),
            new Vector2(0.94f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        currencyText = CreateText(
            "Currency",
            portrait,
            "Gem 0  Ticket 0",
            34,
            new Vector2(0.05f, 0.84f),
            new Vector2(0.66f, 0.9f),
            TextAlignmentOptions.Left,
            Gold);

        pityText = CreateText(
            "Pity",
            portrait,
            "Pity 0/100",
            31,
            new Vector2(0.67f, 0.84f),
            new Vector2(0.95f, 0.9f),
            TextAlignmentOptions.Right);

        RectTransform banner = CreatePanel(
            "Banner",
            portrait,
            Panel,
            new Vector2(0.06f, 0.55f),
            new Vector2(0.94f, 0.82f));

        CreateText(
            "BannerName",
            banner,
            "STANDARD RECRUITMENT",
            44,
            new Vector2(0.05f, 0.66f),
            new Vector2(0.95f, 0.92f),
            TextAlignmentOptions.Center);

        CreateText(
            "BannerRates",
            banner,
            $"SSR {GachaConfig.SSRRate}%   " +
            $"SR {GachaConfig.SRRate}%   " +
            $"R {100 - GachaConfig.SSRRate - GachaConfig.SRRate}%\n" +
            "10 recruits: SR+ guaranteed\n" +
            "SSR guaranteed within 100 recruits",
            27,
            new Vector2(0.08f, 0.1f),
            new Vector2(0.92f, 0.6f),
            TextAlignmentOptions.Center,
            new Color32(189, 204, 228, 255));

        RectTransform resultCard = CreatePanel(
            "ResultCard",
            portrait,
            Panel,
            new Vector2(0.06f, 0.23f),
            new Vector2(0.94f, 0.52f));

        resultText = CreateText(
            "ResultText",
            resultCard,
            "Recruit companions to see results.",
            32,
            new Vector2(0.06f, 0.08f),
            new Vector2(0.94f, 0.92f),
            TextAlignmentOptions.Center);

        statusText = CreateText(
            "Status",
            portrait,
            "Tickets are used before Gems.",
            26,
            new Vector2(0.08f, 0.17f),
            new Vector2(0.92f, 0.22f),
            TextAlignmentOptions.Center,
            new Color32(178, 193, 218, 255));

        singleButton = CreateButton(
            "SingleButton",
            portrait,
            "RECRUIT 1\nTicket 1 / Gem 100",
            new Vector2(0.06f, 0.07f),
            new Vector2(0.46f, 0.16f),
            PanelLight,
            RollSingle);

        tenButton = CreateButton(
            "TenButton",
            portrait,
            "RECRUIT 10\nTicket 10 / Gem 900",
            new Vector2(0.54f, 0.07f),
            new Vector2(0.94f, 0.16f),
            Accent,
            RollTen);

        backButton = CreateButton(
            "BackButton",
            portrait,
            "BACK",
            new Vector2(0.06f, 0.015f),
            new Vector2(0.94f, 0.058f),
            Danger,
            ReturnToMain);
    }

    private async void RollSingle()
    {
        await RollAsync(1);
    }

    private async void RollTen()
    {
        await RollAsync(10);
    }

    private async Task RollAsync(int count)
    {
        if (isRolling)
            return;

        if (GachaManager.Instance == null ||
            PlayerDataManager.Instance?.playerData == null ||
            InventoryManager.Instance == null)
        {
            statusText.text = "Game data is not ready.";
            return;
        }

        PlayerData data = PlayerDataManager.Instance.playerData;
        if (!GachaEconomy.TrySpend(
                data,
                count,
                out GachaPayment payment))
        {
            statusText.text = count == 1
                ? "Need 1 Ticket or 100 Gems."
                : "Need 10 Tickets or 900 Gems.";
            return;
        }

        isRolling = true;
        SetButtonsInteractable(false);

        try
        {
            List<CharacterData> results = count == 1
                ? new List<CharacterData>
                {
                    GachaManager.Instance.RollCharacterWithPity()
                }
                : GachaManager.Instance.RollTen();

            StringBuilder builder = new StringBuilder();
            int ssrCount = 0;
            int srCount = 0;
            foreach (CharacterData character in results)
            {
                InventoryManager.Instance.AddItem(
                    character.characterName,
                    1,
                    false);

                builder.AppendLine(FormatResult(character));
                AnalyticsManager.Instance?.LogGachaRoll(character);

                if (character.rarity == "SSR")
                {
                    ssrCount++;
                    AnalyticsManager.Instance?.LogSSR(character);
                }
                else if (character.rarity == "SR")
                {
                    srCount++;
                }
            }

            string summary = count == 10
                ? $"\nSSR {ssrCount}   SR {srCount}   " +
                  $"R {count - ssrCount - srCount}"
                : "";
            resultText.text =
                builder.ToString().TrimEnd() + summary;
            statusText.text =
                payment.UsedTickets
                    ? $"Used {payment.Amount} ticket(s)."
                    : $"Used {payment.Amount:N0} Gems.";

            QuestManager.Instance?.ReportGacha(results.Count);
            EventMissionManager.Instance?.ReportGacha(results.Count);

            if (FirestoreManager.Instance != null)
            {
                await FirestoreManager.Instance.SavePlayerDataAsync(
                    PlayerDataManager.Instance.playerData);
            }

            PlayerDataManager.Instance.NotifyPlayerDataChanged();
            RefreshHeader();
        }
        catch (Exception exception)
        {
            GachaEconomy.Refund(data, payment);
            statusText.text = "Recruitment failed. Cost refunded.";
            Debug.LogException(exception);
        }
        finally
        {
            isRolling = false;
            SetButtonsInteractable(true);
        }
    }

    private async void ReturnToMain()
    {
        if (isRolling)
            return;

        SetButtonsInteractable(false);

        try
        {
            PlayerData data = PlayerDataManager.Instance?.playerData;
            if (data != null && FirestoreManager.Instance != null)
                await FirestoreManager.Instance.SavePlayerDataAsync(data);

            SceneManager.LoadScene("MainGameScene");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            statusText.text = "Could not return to battle.";
            SetButtonsInteractable(true);
        }
    }

    private void RefreshHeader()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        currencyText.text =
            $"Gem {GachaEconomy.GetItemCount(data, "Gem"):N0}   " +
            $"Ticket {GachaEconomy.GetItemCount(data, "GachaTicket"):N0}";
        int remaining = Mathf.Max(
            1,
            GachaManager.PityLimit - data.pityCount);
        pityText.text = $"SSR in {remaining}";
    }

    private static string FormatResult(CharacterData character)
    {
        string color;
        switch (character.rarity)
        {
            case "SSR":
                color = "#FFD34D";
                break;
            case "SR":
                color = "#B68CFF";
                break;
            default:
                color = "#B9CCE8";
                break;
        }

        return
            $"<color={color}>[{character.rarity}] " +
            $"{character.characterName}</color>";
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (singleButton != null)
            singleButton.interactable = interactable;
        if (tenButton != null)
            tenButton.interactable = interactable;
        if (backButton != null)
            backButton.interactable = interactable;
    }

    private static RectTransform CreatePanel(
        string name,
        Transform parent,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject panel = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = color;
        return rect;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string value,
        float fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax,
        TextAlignmentOptions alignment,
        Color? color = null)
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect =
            textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text =
            textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color ?? Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = CreatePanel(
            name,
            parent,
            color,
            anchorMin,
            anchorMax);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(action);

        CreateText(
            "Label",
            rect,
            label,
            27,
            new Vector2(0.03f, 0.04f),
            new Vector2(0.97f, 0.96f),
            TextAlignmentOptions.Center);
        return button;
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
            return;

        new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(InputSystemUIInputModule));
    }

}
