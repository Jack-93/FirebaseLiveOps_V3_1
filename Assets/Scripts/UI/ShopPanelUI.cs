using System;
using TMPro;
using UnityEngine;

public sealed class ShopPanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text shopText;
    private readonly ShopProductButtonsUI productButtons;

    public GameObject GameObject => panel.gameObject;

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

        shopText = RuntimeUiFactory.CreateText(
            "ShopText",
            card,
            ShopPanelSummaryFormatter.DataUnavailable,
            24,
            new Vector2(0.07f, 0.78f),
            new Vector2(0.93f, 0.89f),
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
            shopText.text = ShopPanelSummaryFormatter.DataUnavailable;
            productButtons.SetRealMoneyButtonsInteractable(false);
            return;
        }

        shopText.text = ShopPanelSummaryFormatter.FormatWallet(
            data,
            monetization);
        productButtons.Refresh(data, monetization);
    }
}
