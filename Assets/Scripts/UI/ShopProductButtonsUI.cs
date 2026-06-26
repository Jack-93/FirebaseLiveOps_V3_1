using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopProductButtonsUI
{
    private readonly Button starterPackButton;
    private readonly Button smallGemPackButton;
    private readonly Button largeGemPackButton;
    private readonly Button rewardedAdButton;

    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);

    public ShopProductButtonsUI(
        RectTransform card,
        Action buyStarterPack,
        Action buySmallGemPack,
        Action buyLargeGemPack,
        Action watchRewardedAd,
        Action buyGoldPouch,
        Action buyTicketBundle,
        Action buyGrowthChest)
    {
        starterPackButton = RuntimeUiFactory.CreateButton(
            "StarterPackButton",
            card,
            "STARTER PACK",
            new Vector2(0.07f, 0.62f),
            new Vector2(0.93f, 0.75f),
            Gold,
            () => buyStarterPack?.Invoke());

        smallGemPackButton = RuntimeUiFactory.CreateButton(
            "SmallGemPackButton",
            card,
            $"{GameBalanceConfig.SmallGemPackGems:N0} GEMS",
            new Vector2(0.07f, 0.47f),
            new Vector2(0.93f, 0.59f),
            Accent,
            () => buySmallGemPack?.Invoke());

        largeGemPackButton = RuntimeUiFactory.CreateButton(
            "LargeGemPackButton",
            card,
            $"{GameBalanceConfig.LargeGemPackGems:N0} GEMS",
            new Vector2(0.07f, 0.32f),
            new Vector2(0.93f, 0.44f),
            Success,
            () => buyLargeGemPack?.Invoke());

        rewardedAdButton = RuntimeUiFactory.CreateButton(
            "RewardedAdButton",
            card,
            $"WATCH AD  +{GameBalanceConfig.RewardedAdGemAmount} GEMS",
            new Vector2(0.07f, 0.18f),
            new Vector2(0.93f, 0.29f),
            PanelLight,
            () => watchRewardedAd?.Invoke());

        RuntimeUiFactory.CreateButton(
            "BuyGoldPouchButton",
            card,
            ShopPanelSummaryFormatter.FormatGoldPouchButton(),
            new Vector2(0.03f, 0.02f),
            new Vector2(0.32f, 0.14f),
            Gold,
            () => buyGoldPouch?.Invoke());

        RuntimeUiFactory.CreateButton(
            "BuyTicketBundleButton",
            card,
            ShopPanelSummaryFormatter.FormatTicketBundleButton(),
            new Vector2(0.35f, 0.02f),
            new Vector2(0.65f, 0.14f),
            Accent,
            () => buyTicketBundle?.Invoke());

        RuntimeUiFactory.CreateButton(
            "BuyGrowthChestButton",
            card,
            ShopPanelSummaryFormatter.FormatGrowthChestButton(),
            new Vector2(0.68f, 0.02f),
            new Vector2(0.97f, 0.14f),
            Success,
            () => buyGrowthChest?.Invoke());
    }

    public void Refresh(PlayerData data, MonetizationManager monetization)
    {
        if (monetization == null)
        {
            SetRealMoneyButtonsInteractable(false);
            return;
        }

        bool busy = monetization.IsBusy;
        bool starterOwned = data.ownedPurchaseProducts != null &&
            data.ownedPurchaseProducts.Contains(
                MonetizationManager.GetProductId(
                    RealMoneyProduct.StarterPack));

        SetButtonLabel(
            starterPackButton,
            ShopPanelSummaryFormatter.FormatStarterPackButton(
                monetization,
                starterOwned));
        SetButtonLabel(
            smallGemPackButton,
            ShopPanelSummaryFormatter.FormatGemPackButton(
                RealMoneyProduct.GemPackSmall,
                GameBalanceConfig.SmallGemPackGems,
                monetization));
        SetButtonLabel(
            largeGemPackButton,
            ShopPanelSummaryFormatter.FormatGemPackButton(
                RealMoneyProduct.GemPackLarge,
                GameBalanceConfig.LargeGemPackGems,
                monetization));

        starterPackButton.interactable = !busy && !starterOwned;
        smallGemPackButton.interactable = !busy;
        largeGemPackButton.interactable = !busy;

        bool canWatch = monetization.CanWatchRewardedAd(out string reason);
        rewardedAdButton.interactable =
            !busy && monetization.AdProviderReady && canWatch;
        SetButtonLabel(
            rewardedAdButton,
            ShopPanelSummaryFormatter.FormatRewardedAdButton(
                monetization,
                canWatch,
                reason));
    }

    public void SetRealMoneyButtonsInteractable(bool interactable)
    {
        starterPackButton.interactable = interactable;
        smallGemPackButton.interactable = interactable;
        largeGemPackButton.interactable = interactable;
        rewardedAdButton.interactable = interactable;
    }

    private static void SetButtonLabel(Button button, string value)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = value;
    }
}
