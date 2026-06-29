using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class GachaPanelUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private RectTransform panel;
    private TMP_Text gemLabelText;
    private TMP_Text ticketLabelText;
    private TMP_Text pityLabelText;
    private TMP_Text statusText;
    private SpriteNumberText gemNumberText;
    private SpriteNumberText ticketNumberText;
    private SpriteNumberText pityNumberText;
    private Button singleButton;
    private Button tenButton;
    private GachaResultUI resultUI;
    private bool resultModeVisible;

    public GameObject GameObject => panel.gameObject;
    public bool IsResultVisible =>
        resultModeVisible || resultUI?.IsVisible == true;

    private static readonly Color OverlayBackground =
        new Color32(12, 18, 30, 218);
    private static readonly Color Panel =
        new Color32(37, 49, 73, 245);
    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);
    private const string GachaPanelPrefabResourcePath =
        "Prefabs/UI/GachaPanel";

    public GachaPanelUI(
        RectTransform root,
        Action<int> rollGacha,
        UnityAction clearResult,
        bool usePrefab = true)
    {
        if (usePrefab && TryBuildFromPrefab(root, rollGacha, clearResult))
            return;

        BuildGenerated(root, rollGacha, clearResult);
    }

    public void BuildGenerated(
        RectTransform root,
        Action<int> rollGacha,
        UnityAction clearResult)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "GachaPanel",
            root,
            OverlayBackground,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateText(
            "GachaTitle",
            panel,
            "RECRUIT",
            48,
            new Vector2(0.05f, 0.91f),
            new Vector2(0.95f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        gemLabelText = RuntimeUiFactory.CreateText(
            "GachaGemLabel",
            panel,
            "Gem",
            28,
            new Vector2(0.06f, 0.85f),
            new Vector2(0.18f, 0.91f),
            TextAlignmentOptions.Left,
            Gold);
        gemNumberText = new SpriteNumberText(
            panel,
            "GachaGemNumberText",
            NumberResourceRoot,
            28f,
            new Vector2(0.18f, 0.85f),
            new Vector2(0.34f, 0.91f));
        ticketLabelText = RuntimeUiFactory.CreateText(
            "GachaTicketLabel",
            panel,
            "Ticket",
            28,
            new Vector2(0.37f, 0.85f),
            new Vector2(0.5f, 0.91f),
            TextAlignmentOptions.Left,
            Gold);
        ticketNumberText = new SpriteNumberText(
            panel,
            "GachaTicketNumberText",
            NumberResourceRoot,
            28f,
            new Vector2(0.5f, 0.85f),
            new Vector2(0.66f, 0.91f));

        pityLabelText = RuntimeUiFactory.CreateText(
            "GachaPityLabel",
            panel,
            "SSR in",
            27,
            new Vector2(0.67f, 0.85f),
            new Vector2(0.82f, 0.91f),
            TextAlignmentOptions.Right,
            Color.white);
        pityNumberText = new SpriteNumberText(
            panel,
            "GachaPityNumberText",
            NumberResourceRoot,
            27f,
            new Vector2(0.82f, 0.85f),
            new Vector2(0.94f, 0.91f));

        BuildBanner();

        resultUI = new GachaResultUI(
            panel,
            clearResult,
            Accent);

        statusText = RuntimeUiFactory.CreateText(
            "GachaStatus",
            panel,
            "Tickets are used before Gems.",
            24,
            new Vector2(0.08f, 0.16f),
            new Vector2(0.92f, 0.21f),
            TextAlignmentOptions.Center,
            MutedText);

        singleButton = CreateRecruitButton(
            "RecruitSingleButton",
            panel,
            1,
            1,
            GachaEconomy.SingleGemCost,
            new Vector2(0.06f, 0.045f),
            new Vector2(0.46f, 0.15f),
            PanelLight,
            () => rollGacha?.Invoke(1));

        tenButton = CreateRecruitButton(
            "RecruitTenButton",
            panel,
            10,
            10,
            GachaEconomy.TenGemCost,
            new Vector2(0.54f, 0.045f),
            new Vector2(0.94f, 0.15f),
            Accent,
            () => rollGacha?.Invoke(10));
    }

    private bool TryBuildFromPrefab(
        RectTransform root,
        Action<int> rollGacha,
        UnityAction clearResult)
    {
        GameObject prefab =
            Resources.Load<GameObject>(GachaPanelPrefabResourcePath);
        if (prefab == null)
            return false;

        GameObject instance = UnityEngine.Object.Instantiate(
            prefab,
            root,
            false);
        instance.name = "GachaPanel";
        panel = instance.GetComponent<RectTransform>();
        if (panel == null)
            return false;

        BindPrefab(rollGacha, clearResult);
        return true;
    }

    private void BindPrefab(
        Action<int> rollGacha,
        UnityAction clearResult)
    {
        gemLabelText =
            RuntimeUiBinder.FindText(panel, "GachaGemLabel");
        gemNumberText = new SpriteNumberText(
            RuntimeUiBinder.FindRect(panel, "GachaGemNumberText"),
            NumberResourceRoot,
            28f);
        ticketLabelText =
            RuntimeUiBinder.FindText(panel, "GachaTicketLabel");
        ticketNumberText = new SpriteNumberText(
            RuntimeUiBinder.FindRect(panel, "GachaTicketNumberText"),
            NumberResourceRoot,
            28f);
        pityLabelText =
            RuntimeUiBinder.FindText(panel, "GachaPityLabel");
        pityNumberText = new SpriteNumberText(
            RuntimeUiBinder.FindRect(panel, "GachaPityNumberText"),
            NumberResourceRoot,
            27f);

        resultUI = new GachaResultUI(
            panel,
            clearResult,
            Accent,
            true);

        statusText =
            RuntimeUiBinder.FindText(panel, "GachaStatus");
        singleButton =
            RuntimeUiBinder.FindButton(panel, "RecruitSingleButton");
        RuntimeUiBinder.ReplaceButtonAction(
            singleButton,
            () => rollGacha?.Invoke(1));
        tenButton =
            RuntimeUiBinder.FindButton(panel, "RecruitTenButton");
        RuntimeUiBinder.ReplaceButtonAction(
            tenButton,
            () => rollGacha?.Invoke(10));
    }

    public void Refresh(PlayerData data)
    {
        if (data == null)
            return;

        if (gemLabelText != null)
            gemLabelText.text = LocalizationManager.Translate("Gem");
        gemNumberText.SetText(
            CompactNumberFormatter.Format(
                GachaEconomy.GetItemCount(data, "Gem")));
        if (ticketLabelText != null)
            ticketLabelText.text = LocalizationManager.Translate("Ticket");
        ticketNumberText.SetText(
            CompactNumberFormatter.Format(
                GachaEconomy.GetItemCount(data, "GachaTicket")));

        int remaining = Mathf.Max(1, GachaManager.PityLimit - data.pityCount);
        if (pityLabelText != null)
            pityLabelText.text = LocalizationManager.Translate("SSR in");
        pityNumberText.SetText(CompactNumberFormatter.Format(remaining));
        resultUI?.SetPoint(data.pityCount);

        if (TutorialManager.Instance?.IsWaitingForTutorialGacha == true)
        {
            SetStatus(
                LocalizationManager.Text(
                    "Use 10 tickets for 10x recruitment.",
                    "\uD2F0\uCF13 10\uC7A5\uC73C\uB85C 10\uD68C \uBAA8\uC9D1\uC744 \uB20C\uB7EC\uC8FC\uC138\uC694."));
        }
        else if (TutorialManager.Instance?.ShouldShowTutorialTicketGift == true)
        {
            SetStatus(
                LocalizationManager.Text(
                    "Receive the tutorial ticket gift first.",
                    "\uBA3C\uC800 \uD29C\uD1A0\uB9AC\uC5BC \uD2F0\uCF13 \uC120\uBB3C\uC744 \uBC1B\uC73C\uC138\uC694."));
        }
    }

    public void ShowResults(
        List<CharacterData> results,
        Dictionary<string, int> ownedBefore)
    {
        resultUI?.ShowResults(results, ownedBefore);
        resultModeVisible = resultUI?.IsVisible == true;
    }

    public void ClearResult(string status)
    {
        resultUI?.Clear();
        resultModeVisible = false;
        SetResultMode(false, false);
        SetStatus(status);
    }

    public void SetResultMode(bool visible, bool isRolling)
    {
        resultModeVisible = visible;

        if (singleButton != null)
            singleButton.gameObject.SetActive(!visible);
        if (tenButton != null)
            tenButton.gameObject.SetActive(!visible);

        SetButtonsInteractable(!visible && !isRolling);
    }

    public void SetButtonsInteractable(bool interactable)
    {
        if (singleButton != null)
            singleButton.interactable =
                interactable && !resultModeVisible;
        if (tenButton != null)
            tenButton.interactable =
                interactable && !resultModeVisible;
    }

    public void SetStatus(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }

    private void BuildBanner()
    {
        RectTransform banner = RuntimeUiFactory.CreatePanel(
            "GachaBannerCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.68f),
            new Vector2(0.94f, 0.84f));

        Image bannerArt = RuntimeUiFactory.CreateSpriteImage(
            "GachaBannerArt",
            banner,
            PrototypeUiArt.StandardGachaBanner,
            new Vector2(0.03f, 0.18f),
            new Vector2(0.97f, 0.97f));
        bannerArt.preserveAspect = false;

        RuntimeUiFactory.CreatePanel(
            "GachaBannerShade",
            banner,
            new Color32(5, 8, 16, 95),
            new Vector2(0.03f, 0.18f),
            new Vector2(0.97f, 0.97f));

        RuntimeUiFactory.CreateSpriteImage(
            "GachaEmblem",
            banner,
            PrototypeUiArt.GetButtonIcon("GachaNav"),
            new Vector2(0.36f, 0.39f),
            new Vector2(0.64f, 0.9f));

        RuntimeUiFactory.CreateText(
            "GachaBannerName",
            banner,
            "STANDARD RECRUITMENT",
            31,
            new Vector2(0.08f, 0.24f),
            new Vector2(0.92f, 0.4f),
            TextAlignmentOptions.Center,
            Gold);

        RuntimeUiFactory.CreateText(
            "GachaRates",
            banner,
            $"SSR {GachaConfig.SSRRate}%   " +
            $"SR {GachaConfig.SRRate}%   " +
            $"R {100 - GachaConfig.SSRRate - GachaConfig.SRRate}%\n" +
            "10 recruits: SR+ guaranteed / SSR within 100",
            20,
            new Vector2(0.07f, 0.04f),
            new Vector2(0.93f, 0.24f),
            TextAlignmentOptions.Center,
            MutedText);
    }

    private static Button CreateRecruitButton(
        string name,
        RectTransform parent,
        int rollCount,
        int ticketCost,
        int gemCost,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        UnityAction action)
    {
        Button button = RuntimeUiFactory.CreateButton(
            name,
            parent,
            "RECRUIT",
            anchorMin,
            anchorMax,
            color,
            action);
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.06f, 0.54f);
            labelRect.anchorMax = new Vector2(0.55f, 0.94f);
            label.alignment = TextAlignmentOptions.Right;
            label.fontSizeMax = 22f;
            label.fontSizeMin = 12f;
        }

        SpriteNumberText rollNumberText = new SpriteNumberText(
            button.transform,
            "RecruitRollNumberText",
            NumberResourceRoot,
            21f,
            new Vector2(0.55f, 0.54f),
            new Vector2(0.74f, 0.94f));
        rollNumberText.SetText(CompactNumberFormatter.Format(rollCount));

        RuntimeUiFactory.CreateText(
            "TicketCostLabel",
            button.transform,
            "Ticket",
            15,
            new Vector2(0.06f, 0.08f),
            new Vector2(0.24f, 0.48f),
            TextAlignmentOptions.Left,
            Color.white);
        SpriteNumberText ticketNumberText = new SpriteNumberText(
            button.transform,
            "TicketCostNumberText",
            NumberResourceRoot,
            17f,
            new Vector2(0.24f, 0.08f),
            new Vector2(0.37f, 0.48f));
        ticketNumberText.SetText(CompactNumberFormatter.Format(ticketCost));
        RuntimeUiFactory.CreateText(
            "CostSeparator",
            button.transform,
            "/",
            15,
            new Vector2(0.39f, 0.08f),
            new Vector2(0.44f, 0.48f),
            TextAlignmentOptions.Center,
            Color.white);
        RuntimeUiFactory.CreateText(
            "GemCostLabel",
            button.transform,
            "Gem",
            15,
            new Vector2(0.46f, 0.08f),
            new Vector2(0.6f, 0.48f),
            TextAlignmentOptions.Left,
            Color.white);
        SpriteNumberText gemNumberText = new SpriteNumberText(
            button.transform,
            "GemCostNumberText",
            NumberResourceRoot,
            17f,
            new Vector2(0.6f, 0.08f),
            new Vector2(0.94f, 0.48f));
        gemNumberText.SetText(CompactNumberFormatter.Format(gemCost));
        return button;
    }
}
