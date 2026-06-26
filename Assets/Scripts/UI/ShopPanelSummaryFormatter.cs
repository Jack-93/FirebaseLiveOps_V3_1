public static class ShopPanelSummaryFormatter
{
    public static string DataUnavailable =>
        LocalizationManager.Translate("Shop data unavailable.");

    public static string MonetizationUnavailable =>
        LocalizationManager.Translate("Monetization service unavailable");

    public static string FormatWallet(
        PlayerData data,
        MonetizationManager monetization)
    {
        int gems = GachaEconomy.GetItemCount(data, "Gem");
        int tickets = GachaEconomy.GetItemCount(data, "GachaTicket");
        return
            $"{LocalizationManager.Translate("Gems")} {gems:N0}  |  " +
            $"{LocalizationManager.Translate("Gold")} {data.gold:N0}  |  " +
            $"{LocalizationManager.Translate("Tickets")} {tickets:N0}\n" +
            (monetization?.GetStoreStatus() ?? MonetizationUnavailable);
    }

    public static string FormatStarterPackButton(
        MonetizationManager monetization,
        bool owned)
    {
        if (owned)
            return LocalizationManager.Translate("STARTER PACK  OWNED");

        return
            $"{LocalizationManager.Translate("STARTER PACK")}  " +
            $"{monetization.GetPriceLabel(RealMoneyProduct.StarterPack)}\n" +
            $"{FormatCompact(GameBalanceConfig.StarterPackGems)} " +
            $"{LocalizationManager.Translate("GEMS")} + " +
            $"{GameBalanceConfig.StarterPackTickets} " +
            $"{LocalizationManager.Translate("TICKETS")} + " +
            $"{FormatCompact(GameBalanceConfig.StarterPackGold)} " +
            $"{LocalizationManager.Translate("GOLD")}";
    }

    public static string FormatGemPackButton(
        RealMoneyProduct product,
        int gems,
        MonetizationManager monetization)
    {
        return
            $"{gems:N0} {LocalizationManager.Translate("GEMS")}  " +
            monetization.GetPriceLabel(product);
    }

    public static string FormatRewardedAdButton(
        MonetizationManager monetization,
        bool canWatch,
        string reason)
    {
        if (!monetization.AdProviderReady)
            return LocalizationManager.Translate("AD SDK PENDING");

        if (canWatch)
        {
            return
                $"{LocalizationManager.Translate("WATCH AD")}  " +
                $"+{MonetizationManager.RewardedAdGemAmount} " +
                $"{LocalizationManager.Translate("GEMS")}";
        }

        return LocalizationManager.Translate(reason.ToUpperInvariant());
    }

    public static string FormatGoldPouchButton()
    {
        return
            $"{FormatCompact(GameBalanceConfig.ShopGoldPouchGold)} " +
            $"{LocalizationManager.Translate("GOLD")}\n" +
            $"{GameBalanceConfig.ShopGoldPouchGemCost} " +
            $"{LocalizationManager.Translate("Gem")}";
    }

    public static string FormatTicketBundleButton()
    {
        return
            $"{GameBalanceConfig.ShopTicketBundleTickets} " +
            $"{LocalizationManager.Translate("TICKETS")}\n" +
            $"{GameBalanceConfig.ShopTicketBundleGemCost} " +
            $"{LocalizationManager.Translate("Gem")}";
    }

    public static string FormatGrowthChestButton()
    {
        return
            $"{FormatCompact(GameBalanceConfig.ShopGrowthChestGold)} " +
            $"{LocalizationManager.Translate("GOLD")}\n" +
            $"{GameBalanceConfig.ShopGrowthChestGemCost} " +
            $"{LocalizationManager.Translate("Gem")}";
    }

    public static string FormatCompact(int value)
    {
        return value >= 1000 && value % 1000 == 0
            ? $"{value / 1000}K"
            : value.ToString("N0");
    }
}
