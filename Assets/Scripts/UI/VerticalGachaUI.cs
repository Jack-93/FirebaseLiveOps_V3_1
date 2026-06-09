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
    private static readonly Color Success =
        new Color32(80, 214, 145, 255);
    private static readonly Color MutedText =
        new Color32(178, 193, 218, 255);

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
        EnsureAudioManager();
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
            "RECRUIT",
            60,
            new Vector2(0.06f, 0.9f),
            new Vector2(0.94f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        CreateText(
            "Subtitle",
            portrait,
            "Companion banner placeholder. Art can be replaced later.",
            25,
            new Vector2(0.08f, 0.865f),
            new Vector2(0.92f, 0.905f),
            TextAlignmentOptions.Center,
            MutedText);

        currencyText = CreateText(
            "Currency",
            portrait,
            "Gem 0  Ticket 0",
            32,
            new Vector2(0.05f, 0.825f),
            new Vector2(0.66f, 0.865f),
            TextAlignmentOptions.Left,
            Gold);

        pityText = CreateText(
            "Pity",
            portrait,
            "Pity 0/100",
            29,
            new Vector2(0.67f, 0.825f),
            new Vector2(0.95f, 0.865f),
            TextAlignmentOptions.Right);

        RectTransform banner = CreatePanel(
            "Banner",
            portrait,
            Panel,
            new Vector2(0.06f, 0.54f),
            new Vector2(0.94f, 0.81f));

        RectTransform artPlaceholder = CreatePanel(
            "BannerArtPlaceholder",
            banner,
            PanelLight,
            new Vector2(0.05f, 0.42f),
            new Vector2(0.95f, 0.92f));

        CreateText(
            "BannerArtText",
            artPlaceholder,
            "ART NEEDED\nBanner Character / Featured Companions",
            27,
            new Vector2(0.05f, 0.08f),
            new Vector2(0.95f, 0.92f),
            TextAlignmentOptions.Center,
            MutedText);

        CreateText(
            "BannerName",
            banner,
            "STANDARD RECRUITMENT",
            32,
            new Vector2(0.05f, 0.28f),
            new Vector2(0.95f, 0.42f),
            TextAlignmentOptions.Center,
            Gold);

        CreateText(
            "BannerRates",
            banner,
            $"SSR {GachaConfig.SSRRate}%   " +
            $"SR {GachaConfig.SRRate}%   " +
            $"R {100 - GachaConfig.SSRRate - GachaConfig.SRRate}%\n" +
            "10 recruits: SR+ guaranteed / SSR within 100",
            24,
            new Vector2(0.08f, 0.06f),
            new Vector2(0.92f, 0.27f),
            TextAlignmentOptions.Center,
            MutedText);

        RectTransform resultCard = CreatePanel(
            "ResultCard",
            portrait,
            Panel,
            new Vector2(0.06f, 0.225f),
            new Vector2(0.94f, 0.515f));

        CreateText(
            "ResultTitle",
            resultCard,
            "RESULT",
            29,
            new Vector2(0.06f, 0.84f),
            new Vector2(0.94f, 0.96f),
            TextAlignmentOptions.Left,
            Success);

        resultText = CreateText(
            "ResultText",
            resultCard,
            "Recruit companions to see results.",
            30,
            new Vector2(0.06f, 0.08f),
            new Vector2(0.94f, 0.82f),
            TextAlignmentOptions.Center);

        statusText = CreateText(
            "Status",
            portrait,
            "Tickets are used before Gems.",
            26,
            new Vector2(0.08f, 0.17f),
            new Vector2(0.92f, 0.22f),
            TextAlignmentOptions.Center,
            MutedText);

        singleButton = CreateButton(
            "SingleButton",
            portrait,
            $"RECRUIT 1\nTicket 1 / Gem {GachaEconomy.SingleGemCost}",
            new Vector2(0.06f, 0.07f),
            new Vector2(0.46f, 0.16f),
            PanelLight,
            RollSingle);

        tenButton = CreateButton(
            "TenButton",
            portrait,
            $"RECRUIT 10\nTicket 10 / Gem {GachaEconomy.TenGemCost}",
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
            statusText.text = LocalizationManager.Text(
                "Game data is not ready.",
                "게임 정보를 아직 불러오지 못했습니다.");
            return;
        }

        PlayerData data = PlayerDataManager.Instance.playerData;
        if (!GachaEconomy.TrySpend(
                data,
                count,
                out GachaPayment payment))
        {
            statusText.text = count == 1
                ? LocalizationManager.Text(
                    "Need 1 Ticket or 100 Gems.",
                    $"티켓 1개 또는 젬 {GachaEconomy.SingleGemCost}개가 필요합니다.")
                : LocalizationManager.Text(
                    "Need 10 Tickets or 900 Gems.",
                    $"티켓 10개 또는 젬 {GachaEconomy.TenGemCost}개가 필요합니다.");
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
                    ? $"{LocalizationManager.Text("Used", "사용")} " +
                      $"{payment.Amount} " +
                      $"{LocalizationManager.Text("ticket(s).", "티켓")}"
                    : $"{LocalizationManager.Text("Used", "사용")} " +
                      $"{payment.Amount:N0} " +
                      $"{LocalizationManager.Text("Gems.", "젬")}";

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
            statusText.text = LocalizationManager.Text(
                "Recruitment failed. Cost refunded.",
                "모집에 실패했습니다. 비용을 돌려받았습니다.");
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
            statusText.text = LocalizationManager.Text(
                "Could not return to battle.",
                "전투 화면으로 돌아갈 수 없습니다.");
            SetButtonsInteractable(true);
        }
    }

    private void RefreshHeader()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        currencyText.text =
            $"{LocalizationManager.Text("Gem", "젬")} " +
            $"{GachaEconomy.GetItemCount(data, "Gem"):N0}   " +
            $"{LocalizationManager.Text("Ticket", "티켓")} " +
            $"{GachaEconomy.GetItemCount(data, "GachaTicket"):N0}";
        int remaining = Mathf.Max(
            1,
            GachaManager.PityLimit - data.pityCount);
        pityText.text =
            $"{LocalizationManager.Text("SSR in", "SSR까지")} {remaining}";
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
        text.text = LocalizationManager.Translate(value);
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = Mathf.Max(14f, fontSize * 0.58f);
        text.lineSpacing = -8f;
        text.alignment = alignment;
        text.color = color ?? Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        GameFont.Apply(text);
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
        button.onClick.AddListener(
            () => AudioManager.Instance?.PlayButtonClick());
        button.onClick.AddListener(action);

        ColorBlock colors = button.colors;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.15f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        colors.disabledColor = new Color(
            color.r,
            color.g,
            color.b,
            0.4f);
        button.colors = colors;

        CreateText(
            "Label",
            rect,
            LocalizationManager.Translate(label),
            27,
            new Vector2(0.05f, 0.07f),
            new Vector2(0.95f, 0.93f),
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

    private static void EnsureAudioManager()
    {
        if (FindAnyObjectByType<AudioManager>() != null)
            return;

        new GameObject("AudioManager", typeof(AudioManager));
    }
}
