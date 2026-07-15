using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopProductButtonsUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private readonly ShopProductButtonView starterPackButton;
    private readonly ShopProductButtonView smallGemPackButton;
    private readonly ShopProductButtonView largeGemPackButton;
    private readonly ShopProductButtonView rewardedAdButton;
    private readonly ShopProductButtonView goldPouchButton;
    private readonly ShopProductButtonView ticketBundleButton;
    private readonly ShopProductButtonView growthChestButton;

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
        Action buyGrowthChest,
        bool bindExisting = false)
    {
        starterPackButton = CreateProductButton(
            "StarterPackButton",
            card,
            "STARTER PACK",
            new Vector2(0.07f, 0.62f),
            new Vector2(0.93f, 0.75f),
            Gold,
            () => buyStarterPack?.Invoke(),
            bindExisting);
        starterPackButton.SetResourceLine(
            GameBalanceConfig.StarterPackGems,
            "GEMS",
            GameBalanceConfig.StarterPackTickets,
            "TICKETS",
            GameBalanceConfig.StarterPackGold,
            "GOLD");

        smallGemPackButton = CreateProductButton(
            "SmallGemPackButton",
            card,
            "GEM PACK",
            new Vector2(0.07f, 0.47f),
            new Vector2(0.93f, 0.59f),
            Accent,
            () => buySmallGemPack?.Invoke(),
            bindExisting);
        smallGemPackButton.SetResourceLine(
            GameBalanceConfig.SmallGemPackGems,
            "GEMS");

        largeGemPackButton = CreateProductButton(
            "LargeGemPackButton",
            card,
            "BIG GEM PACK",
            new Vector2(0.07f, 0.32f),
            new Vector2(0.93f, 0.44f),
            Success,
            () => buyLargeGemPack?.Invoke(),
            bindExisting);
        largeGemPackButton.SetResourceLine(
            GameBalanceConfig.LargeGemPackGems,
            "GEMS");

        rewardedAdButton = CreateProductButton(
            "RewardedAdButton",
            card,
            "WATCH AD",
            new Vector2(0.07f, 0.18f),
            new Vector2(0.93f, 0.29f),
            PanelLight,
            () => watchRewardedAd?.Invoke(),
            bindExisting);
        rewardedAdButton.SetResourceLine(
            GameBalanceConfig.RewardedAdGemAmount,
            "GEMS",
            "+");

        goldPouchButton = CreateProductButton(
            "BuyGoldPouchButton",
            card,
            "GOLD POUCH",
            new Vector2(0.03f, 0.02f),
            new Vector2(0.32f, 0.14f),
            Gold,
            () => buyGoldPouch?.Invoke(),
            bindExisting);
        goldPouchButton.SetResourceLine(
            GameBalanceConfig.ShopGoldPouchGold,
            "GOLD");
        goldPouchButton.SetCostLine(
            GameBalanceConfig.ShopGoldPouchGemCost,
            "Gem");

        ticketBundleButton = CreateProductButton(
            "BuyTicketBundleButton",
            card,
            "TICKET BUNDLE",
            new Vector2(0.35f, 0.02f),
            new Vector2(0.65f, 0.14f),
            Accent,
            () => buyTicketBundle?.Invoke(),
            bindExisting);
        ticketBundleButton.SetResourceLine(
            GameBalanceConfig.ShopTicketBundleTickets,
            "TICKETS");
        ticketBundleButton.SetCostLine(
            GameBalanceConfig.ShopTicketBundleGemCost,
            "Gem");

        growthChestButton = CreateProductButton(
            "BuyGrowthChestButton",
            card,
            "GROWTH CHEST",
            new Vector2(0.68f, 0.02f),
            new Vector2(0.97f, 0.14f),
            Success,
            () => buyGrowthChest?.Invoke(),
            bindExisting);
        growthChestButton.SetResourceLine(
            GameBalanceConfig.ShopGrowthChestGold,
            "GOLD");
        growthChestButton.SetCostLine(
            GameBalanceConfig.ShopGrowthChestGemCost,
            "Gem");
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

        starterPackButton.SetTitle(starterOwned
            ? LocalizationManager.Translate("STARTER PACK  OWNED")
            : LocalizationManager.Translate("STARTER PACK") + "  " +
              monetization.GetPriceLabel(RealMoneyProduct.StarterPack));
        starterPackButton.SetResourcesVisible(!starterOwned);
        smallGemPackButton.SetTitle(
            monetization.GetPriceLabel(RealMoneyProduct.GemPackSmall));
        largeGemPackButton.SetTitle(
            monetization.GetPriceLabel(RealMoneyProduct.GemPackLarge));

        starterPackButton.Interactable = !busy && !starterOwned;
        smallGemPackButton.Interactable = !busy;
        largeGemPackButton.Interactable = !busy;

        bool canWatch = monetization.CanWatchRewardedAd(out string reason);
        rewardedAdButton.Interactable =
            !busy && monetization.AdProviderReady && canWatch;
        rewardedAdButton.SetTitle(GetRewardedAdTitle(
            monetization,
            canWatch,
            reason));
        rewardedAdButton.SetResourcesVisible(
            monetization.AdProviderReady && canWatch);
    }

    public void SetRealMoneyButtonsInteractable(bool interactable)
    {
        starterPackButton.Interactable = interactable;
        smallGemPackButton.Interactable = interactable;
        largeGemPackButton.Interactable = interactable;
        rewardedAdButton.Interactable = interactable;
    }

    private static ShopProductButtonView CreateProductButton(
        string name,
        RectTransform parent,
        string title,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        UnityEngine.Events.UnityAction action,
        bool bindExisting)
    {
        Button button;
        if (bindExisting)
        {
            button = RuntimeUiBinder.FindButton(parent, name);
            if (button == null)
            {
                button = RuntimeUiFactory.CreateButton(
                    name,
                    parent,
                    title,
                    anchorMin,
                    anchorMax,
                    color,
                    action);
            }
            else
            {
                RuntimeUiBinder.ReplaceButtonAction(button, action);
            }
        }
        else
        {
            button = RuntimeUiFactory.CreateButton(
                name,
                parent,
                title,
                anchorMin,
                anchorMax,
                color,
                action);
        }

        return new ShopProductButtonView(button, title, bindExisting);
    }

    private static string GetRewardedAdTitle(
        MonetizationManager monetization,
        bool canWatch,
        string reason)
    {
        if (!monetization.AdProviderReady)
            return LocalizationManager.Translate("AD SDK PENDING");

        return canWatch
            ? LocalizationManager.Translate("WATCH AD")
            : LocalizationManager.Translate(reason.ToUpperInvariant());
    }

    private sealed class ShopProductButtonView
    {
        private readonly Button button;
        private readonly TMP_Text titleText;
        private readonly TMP_Text resourceLabelText;
        private readonly TMP_Text secondResourceLabelText;
        private readonly TMP_Text thirdResourceLabelText;
        private readonly TMP_Text costLabelText;
        private readonly SpriteNumberText resourceNumberText;
        private readonly SpriteNumberText secondResourceNumberText;
        private readonly SpriteNumberText thirdResourceNumberText;
        private readonly SpriteNumberText costNumberText;
        private bool hasSecondResource;
        private bool hasThirdResource;

        public ShopProductButtonView(
            Button button,
            string title,
            bool bindExisting)
        {
            this.button = button;
            if (button == null)
                return;

            titleText = RuntimeUiBinder.FindText(
                button.transform,
                "Label") ?? button.GetComponentInChildren<TMP_Text>();
            if (titleText != null)
            {
                titleText.text = LocalizationManager.Translate(title);
                if (!bindExisting)
                {
                    RectTransform rect =
                        titleText.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.04f, 0.58f);
                    rect.anchorMax = new Vector2(0.96f, 0.96f);
                    titleText.fontSizeMax = 19f;
                    titleText.fontSizeMin = 11f;
                }
            }

            resourceNumberText = CreateNumber(
                "ResourceNumber",
                0.04f,
                bindExisting);
            resourceLabelText = CreateLabel(
                "ResourceLabel",
                0.2f,
                bindExisting);
            secondResourceNumberText =
                CreateNumber("SecondResourceNumber", 0.35f, bindExisting);
            secondResourceLabelText =
                CreateLabel("SecondResourceLabel", 0.51f, bindExisting);
            thirdResourceNumberText =
                CreateNumber("ThirdResourceNumber", 0.66f, bindExisting);
            thirdResourceLabelText =
                CreateLabel("ThirdResourceLabel", 0.82f, bindExisting);
            costNumberText = CreateNumber(
                "CostNumber",
                0.04f,
                bindExisting,
                0.02f,
                0.36f);
            costLabelText = CreateLabel(
                "CostLabel",
                0.2f,
                bindExisting,
                0.02f,
                0.36f);
            SetResourcesVisible(false);
            SetCostVisible(false);
        }

        public bool Interactable
        {
            set
            {
                if (button != null)
                    button.interactable = value;
            }
        }

        public void SetTitle(string title)
        {
            if (titleText != null)
                titleText.text = title;
        }

        public void SetResourceLine(
            int amount,
            string label,
            string prefix = "")
        {
            SetResourceLine(
                amount,
                label,
                0,
                string.Empty,
                0,
                string.Empty,
                prefix);
        }

        public void SetResourceLine(
            int amount,
            string label,
            int secondAmount,
            string secondLabel,
            int thirdAmount,
            string thirdLabel)
        {
            SetResourceLine(
                amount,
                label,
                secondAmount,
                secondLabel,
                thirdAmount,
                thirdLabel,
                string.Empty);
        }

        public void SetCostLine(int amount, string label)
        {
            costNumberText?.SetText(CompactNumberFormatter.Format(amount));
            if (costLabelText != null)
                costLabelText.text = LocalizationManager.Translate(label);
            SetCostVisible(true);
        }

        public void SetResourcesVisible(bool visible)
        {
            resourceNumberText?.SetActive(visible);
            if (resourceLabelText != null)
                resourceLabelText.gameObject.SetActive(visible);
            secondResourceNumberText?.SetActive(
                visible && hasSecondResource);
            if (secondResourceLabelText != null)
            {
                secondResourceLabelText.gameObject.SetActive(
                    visible && hasSecondResource);
            }
            thirdResourceNumberText?.SetActive(visible && hasThirdResource);
            if (thirdResourceLabelText != null)
            {
                thirdResourceLabelText.gameObject.SetActive(
                    visible && hasThirdResource);
            }
        }

        private void SetResourceLine(
            int amount,
            string label,
            int secondAmount,
            string secondLabel,
            int thirdAmount,
            string thirdLabel,
            string prefix)
        {
            resourceNumberText?.SetText(
                CompactNumberFormatter.Format(amount, prefix));
            if (resourceLabelText != null)
                resourceLabelText.text = LocalizationManager.Translate(label);
            hasSecondResource = !string.IsNullOrEmpty(secondLabel);
            hasThirdResource = !string.IsNullOrEmpty(thirdLabel);
            if (hasSecondResource)
            {
                secondResourceNumberText?.SetText(
                    CompactNumberFormatter.Format(secondAmount));
                if (secondResourceLabelText != null)
                {
                    secondResourceLabelText.text =
                        LocalizationManager.Translate(secondLabel);
                }
            }

            if (hasThirdResource)
            {
                thirdResourceNumberText?.SetText(
                    CompactNumberFormatter.Format(thirdAmount));
                if (thirdResourceLabelText != null)
                {
                    thirdResourceLabelText.text =
                        LocalizationManager.Translate(thirdLabel);
                }
            }

            SetResourcesVisible(true);
        }

        private void SetCostVisible(bool visible)
        {
            costNumberText?.SetActive(visible);
            if (costLabelText != null)
                costLabelText.gameObject.SetActive(visible);
        }

        private SpriteNumberText CreateNumber(
            string name,
            float left,
            bool bindExisting,
            float yMin = 0.12f,
            float yMax = 0.48f)
        {
            if (bindExisting)
            {
                return RuntimeUiBinder.BindNumber(
                    button.transform,
                    name,
                    NumberResourceRoot,
                    18f);
            }

            return new SpriteNumberText(
                button.transform,
                name,
                NumberResourceRoot,
                18f,
                new Vector2(left, yMin),
                new Vector2(left + 0.16f, yMax));
        }

        private TMP_Text CreateLabel(
            string name,
            float left,
            bool bindExisting,
            float yMin = 0.12f,
            float yMax = 0.48f)
        {
            if (bindExisting)
                return RuntimeUiBinder.FindText(button.transform, name);

            return RuntimeUiFactory.CreateText(
                name,
                button.transform,
                "",
                15,
                new Vector2(left, yMin),
                new Vector2(left + 0.14f, yMax),
                TextAlignmentOptions.Left,
                Color.white);
        }
    }
}
