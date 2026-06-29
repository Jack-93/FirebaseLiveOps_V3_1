using System;
using TMPro;
using UnityEngine;

public sealed class ShopPanelUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private RectTransform panel;
    private TMP_Text shopText;
    private TMP_Text gemsLabelText;
    private TMP_Text goldLabelText;
    private TMP_Text ticketsLabelText;
    private SpriteNumberText gemsNumberText;
    private SpriteNumberText goldNumberText;
    private SpriteNumberText ticketsNumberText;
    private ShopProductButtonsUI productButtons;

    public GameObject GameObject => panel == null ? null : panel.gameObject;

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
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public ShopPanelUI(
        RectTransform root,
        Action showMore,
        Action buyStarterPack,
        Action buySmallGemPack,
        Action buyLargeGemPack,
        Action watchRewardedAd,
        Action buyGoldPouch,
        Action buyTicketBundle,
        Action buyGrowthChest,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "ShopPanel",
                root,
                out panel))
        {
            Bind(
                showMore,
                buyStarterPack,
                buySmallGemPack,
                buyLargeGemPack,
                watchRewardedAd,
                buyGoldPouch,
                buyTicketBundle,
                buyGrowthChest);
            return;
        }

        BuildGenerated(
            root,
            showMore,
            buyStarterPack,
            buySmallGemPack,
            buyLargeGemPack,
            watchRewardedAd,
            buyGoldPouch,
            buyTicketBundle,
            buyGrowthChest);
    }

    public void BuildGenerated(
        RectTransform root,
        Action showMore,
        Action buyStarterPack,
        Action buySmallGemPack,
        Action buyLargeGemPack,
        Action watchRewardedAd,
        Action buyGoldPouch,
        Action buyTicketBundle,
        Action buyGrowthChest)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "ShopPanel",
            root,
            OverlayBackground,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateButton(
            "ShopBackButton",
            panel,
            "BACK",
            new Vector2(0.05f, 0.9f),
            new Vector2(0.25f, 0.98f),
            PanelLight,
            () => showMore?.Invoke());

        RuntimeUiFactory.CreateText(
            "ShopTitle",
            panel,
            "SHOP",
            48,
            new Vector2(0.3f, 0.9f),
            new Vector2(0.7f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RuntimeUiFactory.CreateText(
            "ShopSubtitle",
            panel,
            "Store and rewarded ad placeholders.",
            24,
            new Vector2(0.12f, 0.86f),
            new Vector2(0.88f, 0.9f),
            TextAlignmentOptions.Center,
            MutedText);

        RectTransform card = RuntimeUiFactory.CreatePanel(
            "ShopCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.18f),
            new Vector2(0.94f, 0.84f));

        RuntimeUiFactory.CreateText(
            "ShopCardTitle",
            card,
            "PRODUCTS",
            27,
            new Vector2(0.07f, 0.9f),
            new Vector2(0.93f, 0.98f),
            TextAlignmentOptions.Left,
            Gold);

        gemsLabelText = RuntimeUiFactory.CreateText(
            "ShopGemsLabel",
            card,
            "Gems",
            21,
            new Vector2(0.07f, 0.82f),
            new Vector2(0.18f, 0.89f),
            TextAlignmentOptions.Left,
            Color.white);
        gemsNumberText = new SpriteNumberText(
            card,
            "ShopGemsNumberText",
            NumberResourceRoot,
            22f,
            new Vector2(0.18f, 0.82f),
            new Vector2(0.33f, 0.89f));
        goldLabelText = RuntimeUiFactory.CreateText(
            "ShopGoldLabel",
            card,
            "Gold",
            21,
            new Vector2(0.36f, 0.82f),
            new Vector2(0.47f, 0.89f),
            TextAlignmentOptions.Left,
            Color.white);
        goldNumberText = new SpriteNumberText(
            card,
            "ShopGoldNumberText",
            NumberResourceRoot,
            22f,
            new Vector2(0.47f, 0.82f),
            new Vector2(0.64f, 0.89f));
        ticketsLabelText = RuntimeUiFactory.CreateText(
            "ShopTicketsLabel",
            card,
            "Tickets",
            21,
            new Vector2(0.67f, 0.82f),
            new Vector2(0.8f, 0.89f),
            TextAlignmentOptions.Left,
            Color.white);
        ticketsNumberText = new SpriteNumberText(
            card,
            "ShopTicketsNumberText",
            NumberResourceRoot,
            22f,
            new Vector2(0.8f, 0.82f),
            new Vector2(0.93f, 0.89f));

        shopText = RuntimeUiFactory.CreateText(
            "ShopText",
            card,
            ShopPanelSummaryFormatter.DataUnavailable,
            21,
            new Vector2(0.07f, 0.76f),
            new Vector2(0.93f, 0.82f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        productButtons = new ShopProductButtonsUI(
            card,
            buyStarterPack,
            buySmallGemPack,
            buyLargeGemPack,
            watchRewardedAd,
            buyGoldPouch,
            buyTicketBundle,
            buyGrowthChest);
    }

    public void Refresh(PlayerData data, MonetizationManager monetization)
    {
        if (data == null)
        {
            SetText(shopText, ShopPanelSummaryFormatter.DataUnavailable);
            SetWalletVisible(false);
            productButtons?.SetRealMoneyButtonsInteractable(false);
            return;
        }

        SetWalletVisible(true);
        RefreshWallet(data);
        SetText(
            shopText,
            monetization?.GetStoreStatus() ??
            ShopPanelSummaryFormatter.MonetizationUnavailable);
        productButtons?.Refresh(data, monetization);
    }

    private void RefreshWallet(PlayerData data)
    {
        SetText(gemsLabelText, LocalizationManager.Translate("Gems"));
        gemsNumberText?.SetText(
            CompactNumberFormatter.Format(
                GachaEconomy.GetItemCount(data, "Gem")));
        SetText(goldLabelText, LocalizationManager.Translate("Gold"));
        goldNumberText?.SetText(
            CompactNumberFormatter.Format(data.gold));
        SetText(ticketsLabelText, LocalizationManager.Translate("Tickets"));
        ticketsNumberText?.SetText(
            CompactNumberFormatter.Format(
                GachaEconomy.GetItemCount(data, "GachaTicket")));
    }

    private void SetWalletVisible(bool visible)
    {
        SetTextActive(gemsLabelText, visible);
        SetTextActive(goldLabelText, visible);
        SetTextActive(ticketsLabelText, visible);
        gemsNumberText?.SetActive(visible);
        goldNumberText?.SetActive(visible);
        ticketsNumberText?.SetActive(visible);
    }

    private void Bind(
        Action showMore,
        Action buyStarterPack,
        Action buySmallGemPack,
        Action buyLargeGemPack,
        Action watchRewardedAd,
        Action buyGoldPouch,
        Action buyTicketBundle,
        Action buyGrowthChest)
    {
        RectTransform card = RuntimeUiBinder.FindRect(panel, "ShopCard");
        gemsLabelText = RuntimeUiBinder.FindText(panel, "ShopGemsLabel");
        goldLabelText = RuntimeUiBinder.FindText(panel, "ShopGoldLabel");
        ticketsLabelText =
            RuntimeUiBinder.FindText(panel, "ShopTicketsLabel");
        gemsNumberText = BindNumber("ShopGemsNumberText", 22f);
        goldNumberText = BindNumber("ShopGoldNumberText", 22f);
        ticketsNumberText = BindNumber("ShopTicketsNumberText", 22f);
        shopText = RuntimeUiBinder.FindText(panel, "ShopText");
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(panel, "ShopBackButton"),
            () => showMore?.Invoke());

        productButtons = new ShopProductButtonsUI(
            card,
            buyStarterPack,
            buySmallGemPack,
            buyLargeGemPack,
            watchRewardedAd,
            buyGoldPouch,
            buyTicketBundle,
            buyGrowthChest,
            true);
    }

    private SpriteNumberText BindNumber(string name, float height)
    {
        return RuntimeUiBinder.BindNumber(
            panel,
            name,
            NumberResourceRoot,
            height);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private static void SetTextActive(TMP_Text text, bool active)
    {
        if (text != null)
            text.gameObject.SetActive(active);
    }
}
