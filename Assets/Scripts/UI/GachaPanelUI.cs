using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class GachaPanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text currencyText;
    private readonly TMP_Text pityText;
    private readonly TMP_Text statusText;
    private readonly Button singleButton;
    private readonly Button tenButton;
    private readonly GachaResultUI resultUI;
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

    public GachaPanelUI(
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

        currencyText = RuntimeUiFactory.CreateText(
            "GachaCurrency",
            panel,
            "Gem 0  Ticket 0",
            28,
            new Vector2(0.06f, 0.85f),
            new Vector2(0.66f, 0.91f),
            TextAlignmentOptions.Left,
            Gold);

        pityText = RuntimeUiFactory.CreateText(
            "GachaPity",
            panel,
            "SSR in 100",
            27,
            new Vector2(0.67f, 0.85f),
            new Vector2(0.94f, 0.91f),
            TextAlignmentOptions.Right,
            Color.white);

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

        singleButton = RuntimeUiFactory.CreateButton(
            "RecruitSingleButton",
            panel,
            $"RECRUIT 1\nTicket 1 / Gem {GachaEconomy.SingleGemCost}",
            new Vector2(0.06f, 0.045f),
            new Vector2(0.46f, 0.15f),
            PanelLight,
            () => rollGacha?.Invoke(1));

        tenButton = RuntimeUiFactory.CreateButton(
            "RecruitTenButton",
            panel,
            $"RECRUIT 10\nTicket 10 / Gem {GachaEconomy.TenGemCost}",
            new Vector2(0.54f, 0.045f),
            new Vector2(0.94f, 0.15f),
            Accent,
            () => rollGacha?.Invoke(10));
    }

    public void Refresh(PlayerData data)
    {
        if (data == null)
            return;

        currencyText.text =
            $"{LocalizationManager.Translate("Gem")} " +
            $"{GachaEconomy.GetItemCount(data, "Gem"):N0}   " +
            $"{LocalizationManager.Translate("Ticket")} " +
            $"{GachaEconomy.GetItemCount(data, "GachaTicket"):N0}";
        int remaining = Mathf.Max(1, GachaManager.PityLimit - data.pityCount);
        pityText.text =
            $"{LocalizationManager.Translate("SSR in")} {remaining}";
        resultUI?.SetPoint(data.pityCount);
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
}
