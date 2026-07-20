using System.Collections.Generic;
using UnityEngine;

public readonly struct NotificationBadgeState
{
    public NotificationBadgeState(
        bool questReady,
        bool eventReady,
        bool shopReady,
        bool equipmentReady,
        bool dailyReady,
        bool growthReady,
        bool gachaReady,
        bool collectionReady,
        bool moreReady)
    {
        QuestReady = questReady;
        EventReady = eventReady;
        ShopReady = shopReady;
        EquipmentReady = equipmentReady;
        DailyReady = dailyReady;
        GrowthReady = growthReady;
        GachaReady = gachaReady;
        CollectionReady = collectionReady;
        MoreReady = moreReady;
    }

    public bool QuestReady { get; }
    public bool EventReady { get; }
    public bool ShopReady { get; }
    public bool EquipmentReady { get; }
    public bool DailyReady { get; }
    public bool GrowthReady { get; }
    public bool GachaReady { get; }
    public bool CollectionReady { get; }
    public bool MoreReady { get; }
}

public static class NotificationBadgePolicy
{
    public static NotificationBadgeState Evaluate(
        PlayerData data,
        GrowthManager growthManager,
        CompanionManager companionManager)
    {
        bool hasData = data != null;
        bool questReady = hasData && IsCurrentQuestReady(data);
        bool eventReady = hasData && IsEventRewardReady(data);
        bool dailyReady =
            DailyRewardManager.Instance != null &&
            DailyRewardManager.Instance.CanClaimReward();
        bool mailReady =
            hasData &&
            data.mailbox != null &&
            data.mailbox.Count > 0;
        bool shopReady = IsRewardedAdReady();
        bool gachaReady = hasData && IsGachaReady(data);
        bool growthReady = hasData && IsGrowthReady(data, growthManager);
        bool equipmentReady = hasData && IsEquipmentReady(data);
        bool collectionReady = hasData &&
            IsCollectionReady(companionManager);

        return new NotificationBadgeState(
            questReady,
            eventReady,
            shopReady,
            equipmentReady,
            dailyReady,
            growthReady,
            gachaReady,
            collectionReady,
            dailyReady || mailReady || shopReady);
    }

    private static bool IsCurrentQuestReady(PlayerData data)
    {
        if (data == null || QuestManager.QuestCount <= 0)
            return false;

        int questIndex = Mathf.Clamp(
            data.sequentialQuestIndex,
            0,
            QuestManager.QuestCount - 1);
        int target = QuestManager.GetTargetForIndex(questIndex);
        return data.sequentialQuestProgress >= target;
    }

    private static bool IsEventRewardReady(PlayerData data)
    {
        return data != null &&
            !data.eventRewardClaimed &&
            data.eventMissionPoints >=
            EventMissionManager.RewardPointTarget;
    }

    private static bool IsRewardedAdReady()
    {
        MonetizationManager monetization = MonetizationManager.Instance;
        if (monetization == null || monetization.IsBusy)
            return false;

        return monetization.RewardedAdReady &&
            monetization.CanWatchRewardedAd(out _);
    }

    private static bool IsGachaReady(PlayerData data)
    {
        return GachaEconomy.GetItemCount(data, "GachaTicket") > 0 ||
            GachaEconomy.GetItemCount(data, "Gem") >=
            GachaEconomy.SingleGemCost;
    }

    private static bool IsGrowthReady(
        PlayerData data,
        GrowthManager growthManager)
    {
        if (growthManager == null || data == null)
            return false;

        return data.gold >= growthManager.GetCost(UpgradeType.Attack) ||
            data.gold >= growthManager.GetCost(UpgradeType.Health) ||
            data.gold >= growthManager.GetCost(UpgradeType.AttackSpeed);
    }

    private static bool IsEquipmentReady(PlayerData data)
    {
        if (data == null)
            return false;

        bool weaponReady =
            !string.IsNullOrEmpty(data.equippedWeapon) &&
            EquipmentManager.GetEnhancementLevel(
                data,
                EquipmentSlot.Weapon) <
            GameBalanceConfig.EquipmentStarForceMaxLevel &&
            data.gold >=
            EquipmentManager.GetUpgradeCost(
                EquipmentManager.GetEnhancementLevel(
                    data,
                    EquipmentSlot.Weapon));
        bool armorReady =
            !string.IsNullOrEmpty(data.equippedArmor) &&
            EquipmentManager.GetEnhancementLevel(
                data,
                EquipmentSlot.Armor) <
            GameBalanceConfig.EquipmentStarForceMaxLevel &&
            data.gold >=
            EquipmentManager.GetUpgradeCost(
                EquipmentManager.GetEnhancementLevel(
                    data,
                    EquipmentSlot.Armor));
        return weaponReady || armorReady;
    }

    private static bool IsCollectionReady(
        CompanionManager companionManager)
    {
        if (companionManager == null)
            return false;

        List<CharacterData> characters =
            companionManager.GetAllCharacters();
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];
            int owned = companionManager.GetOwnedCount(
                character.characterName);
            int stars = companionManager.GetStars(
                character.characterName);
            int cost = companionManager.GetPromotionCost(
                character.characterName);

            if (owned > 0 && stars < 5 && cost > 0 && owned - 1 >= cost)
                return true;
        }

        return false;
    }
}
