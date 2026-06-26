using System.Text;

public static class MorePanelSummaryFormatter
{
    public static string InventoryUnavailable =>
        LocalizationManager.Translate("Inventory data unavailable.");

    public static string FormatInventory(
        PlayerData data,
        CompanionManager companionManager)
    {
        StringBuilder builder = new StringBuilder();
        if (data.inventory?.items != null)
        {
            foreach (var item in data.inventory.items)
            {
                if (companionManager == null ||
                    !companionManager.IsCharacterItem(item.Key))
                {
                    builder.AppendLine($"{item.Key}   x{item.Value}");
                }
            }
        }

        builder.AppendLine(
            $"{LocalizationManager.Translate("Mailbox")}   " +
            $"{data.mailbox.Count} " +
            $"{LocalizationManager.Translate("waiting")}");
        builder.AppendLine(
            $"{LocalizationManager.Translate("Monsters defeated")}   " +
            $"{data.totalMonstersDefeated:N0}");
        return builder.ToString();
    }

    public static string FormatCompanions(CompanionManager companionManager)
    {
        StringBuilder builder = new StringBuilder();
        var party = companionManager?.GetEquippedParty();
        if (party == null || party.Count == 0)
        {
            builder.AppendLine(
                $"{LocalizationManager.Translate("Party")} 0/3");
            builder.Append(
                LocalizationManager.Translate("Recruit one in Gacha."));
            return builder.ToString();
        }

        int bonus = 0;
        builder.AppendLine(
            $"{LocalizationManager.Translate("PARTY")} " +
            $"{party.Count}/{CompanionManager.PartySize}");
        for (int i = 0; i < party.Count; i++)
        {
            CharacterData character = party[i];
            if (i > 0)
                builder.Append(", ");

            builder.Append(
                $"[{character.rarity}] {character.characterName}");
            bonus += CompanionManager.GetAttackBonusPercent(
                character.rarity);
        }

        builder.AppendLine();
        builder.Append(
            $"{LocalizationManager.Translate("Team Attack")} " +
            $"+{bonus}%");
        CompanionSynergyResult synergy =
            companionManager.GetSynergyResult();
        builder.AppendLine();
        builder.Append(synergy.GetSummary());
        return builder.ToString();
    }

    public static string FormatAccount(
        PlayerData data,
        AccountLinkManager accounts)
    {
        string accountType = accounts != null &&
            accounts.IsLinked(AccountLinkProvider.Google)
                ? LocalizationManager.Translate("Linked account")
                : LocalizationManager.Translate("Guest account");
        return
            $"{accountType}  |  " +
            $"{LocalizationManager.Translate("Highest")} " +
            $"{data.highestStage}";
    }

    public static string FormatDailyReward(DailyRewardManager dailyRewards)
    {
        if (dailyRewards == null)
        {
            return LocalizationManager.Translate(
                "Daily reward unavailable.");
        }

        int day = dailyRewards.GetNextRewardDay();
        return dailyRewards.CanClaimReward()
            ? $"{LocalizationManager.Translate("Daily Reward Day")} " +
              $"{day} " +
              $"{LocalizationManager.Translate("is ready")}"
            : $"{LocalizationManager.Translate("Daily Reward Day")} " +
              $"{day} " +
              $"{LocalizationManager.Translate("already claimed")}";
    }
}
