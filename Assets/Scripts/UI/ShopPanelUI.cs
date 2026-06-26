using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopPanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text shopText;
    private readonly Button starterPackButton;
    private readonly Button smallGemPackButton;
    private readonly Button largeGemPackButton;
    private readonly Button rewardedAdButton;

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
            "Shop data unavailable.",
            24,
            new Vector2(0.07f, 0.78f),
            new Vector2(0.93f, 0.89f),
            TextAlignmentOptions.TopLeft,
            Color.white);

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
            $"{FormatCompact(GameBalanceConfig.ShopGoldPouchGold)} GOLD\n" +
            $"{GameBalanceConfig.ShopGoldPouchGemCost} GEM",
            new Vector2(0.03f, 0.02f),
            new Vector2(0.32f, 0.14f),
            Gold,
            () => buyGoldPouch?.Invoke());

        RuntimeUiFactory.CreateButton(
            "BuyTicketBundleButton",
            card,
            $"{GameBalanceConfig.ShopTicketBundleTickets} TICKETS\n" +
            $"{GameBalanceConfig.ShopTicketBundleGemCost} GEM",
            new Vector2(0.35f, 0.02f),
            new Vector2(0.65f, 0.14f),
            Accent,
            () => buyTicketBundle?.Invoke());

        RuntimeUiFactory.CreateButton(
            "BuyGrowthChestButton",
            card,
            $"{FormatCompact(GameBalanceConfig.ShopGrowthChestGold)} GOLD\n" +
            $"{GameBalanceConfig.ShopGrowthChestGemCost} GEM",
            new Vector2(0.68f, 0.02f),
            new Vector2(0.97f, 0.14f),
            Success,
            () => buyGrowthChest?.Invoke());
    }

    public void Refresh(PlayerData data, MonetizationManager monetization)
    {
        if (data == null)
        {
            shopText.text = LocalizationManager.Text(
                "Shop data unavailable.",
                "상점 정보를 불러올 수 없습니다.");
            SetButtonsInteractable(false);
            return;
        }

        int gems = GachaEconomy.GetItemCount(data, "Gem");
        int tickets = GachaEconomy.GetItemCount(data, "GachaTicket");
        shopText.text =
            $"{LocalizationManager.Text("Gems", "젬")} {gems:N0}  |  " +
            $"{LocalizationManager.Text("Gold", "골드")} {data.gold:N0}  |  " +
            $"{LocalizationManager.Text("Tickets", "티켓")} {tickets:N0}\n" +
            (monetization?.GetStoreStatus() ??
             LocalizationManager.Text(
                 "Monetization service unavailable",
                 "결제 서비스를 사용할 수 없습니다."));

        if (monetization == null)
        {
            SetButtonsInteractable(false);
            return;
        }

        bool busy = monetization.IsBusy;
        bool starterOwned = data.ownedPurchaseProducts != null &&
            data.ownedPurchaseProducts.Contains(
                MonetizationManager.GetProductId(
                    RealMoneyProduct.StarterPack));

        SetButtonLabel(
            starterPackButton,
            starterOwned
                ? LocalizationManager.Text(
                    "STARTER PACK  OWNED",
                    "스타터팩 보유 중")
                : LocalizationManager.Text(
                    "STARTER PACK",
                    "스타터팩") +
                  "  " +
                  monetization.GetPriceLabel(
                      RealMoneyProduct.StarterPack) +
                  "\n" +
                  FormatCompact(GameBalanceConfig.StarterPackGems) +
                  " " +
                  LocalizationManager.Text("GEMS", "젬") +
                  " + " +
                  GameBalanceConfig.StarterPackTickets +
                  " " +
                  LocalizationManager.Text("TICKETS", "티켓") +
                  " + " +
                  FormatCompact(GameBalanceConfig.StarterPackGold) +
                  " " +
                  LocalizationManager.Text("GOLD", "골드"));

        SetButtonLabel(
            smallGemPackButton,
            $"{GameBalanceConfig.SmallGemPackGems:N0} " +
            $"{LocalizationManager.Text("GEMS", "젬")}  " +
            monetization.GetPriceLabel(
                RealMoneyProduct.GemPackSmall));
        SetButtonLabel(
            largeGemPackButton,
            $"{GameBalanceConfig.LargeGemPackGems:N0} " +
            $"{LocalizationManager.Text("GEMS", "젬")}  " +
            monetization.GetPriceLabel(
                RealMoneyProduct.GemPackLarge));

        starterPackButton.interactable = !busy && !starterOwned;
        smallGemPackButton.interactable = !busy;
        largeGemPackButton.interactable = !busy;

        bool canWatch = monetization.CanWatchRewardedAd(out string reason);
        rewardedAdButton.interactable =
            !busy && monetization.RewardedAdReady && canWatch;
        SetButtonLabel(
            rewardedAdButton,
            !monetization.AdProviderReady
                ? LocalizationManager.Text(
                    "AD SDK PENDING",
                    "광고 SDK 대기 중")
                : canWatch
                    ? $"{LocalizationManager.Text("WATCH AD", "광고 보기")}  " +
                      $"+{MonetizationManager.RewardedAdGemAmount} " +
                      $"{LocalizationManager.Text("GEMS", "젬")}"
                    : LocalizationManager.Translate(
                        reason.ToUpperInvariant()));
    }

    private void SetButtonsInteractable(bool interactable)
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
            label.text = LocalizationManager.Translate(value);
    }

    private static string FormatCompact(int value)
    {
        return value >= 1000 && value % 1000 == 0
            ? $"{value / 1000}K"
            : value.ToString("N0");
    }
}
