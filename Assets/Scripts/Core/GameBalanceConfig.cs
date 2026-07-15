public static class GameBalanceConfig
{
    public static readonly string[] DailyRewardItemNames =
    {
        "Gem",
        "Gem",
        "Gem",
        "GachaTicket",
        "Gem",
        "GachaTicket",
        "SSRTicket"
    };

    public static readonly int[] DailyRewardAmounts =
    {
        100,
        200,
        300,
        1,
        500,
        3,
        1
    };

    public const int EnemiesPerStage = 5;
    public const int MaxOfflineHours = 8;
    public const float BossTimeLimit = 30f;
    public const float BossPatternWarningSeconds = 1.25f;

    public const int PlayerBaseAttack = 8;
    public const int PlayerAttackPerLevel = 2;
    public const int PlayerAttackPerUpgrade = 6;
    public const int PlayerBaseHealth = 105;
    public const int PlayerHealthPerLevel = 10;
    public const int PlayerHealthPerUpgrade = 34;
    public const float PlayerBaseAttackInterval = 1.2f;
    public const float PlayerAttackIntervalReduction = 0.045f;
    public const float PlayerMinAttackInterval = 0.25f;
    public const float PlayerAbsoluteMinAttackInterval = 0.2f;

    public const double EnemyBaseHealth = 42d;
    // Stage 1-30: free progression should fund Attack, Pole DUR, and SPD together.
    public const double EnemyHealthGrowth = 1.135d;
    public const double EnemyStageHealthCycleBonus = 0.04d;
    public const double BossHealthMultiplier = 3.5d;
    public const double EnemyBaseAttack = 4d;
    public const double EnemyAttackGrowth = 1.065d;
    public const double EnemyStageAttackCycleBonus = 0.03d;
    public const double BossAttackMultiplier = 1.45d;
    public const double EnemyBaseGold = 14d;
    public const double EnemyGoldGrowth = 1.15d;
    public const double EnemyStageGoldCycleBonus = 0.05d;
    public const double BossGoldMultiplier = 9d;

    public const int HeroUpgradeBaseCost = 100;
    public const int AttackSpeedUpgradeBaseCost = 180;
    public const double HeroUpgradeCostGrowth = 1.18d;

    public const int EquipmentWeaponBaseAttack = 4;
    public const int EquipmentWeaponAttackPerTier = 8;
    public const int EquipmentWeaponAttackPerLevel = 3;
    public const int EquipmentArmorBaseHealth = 20;
    public const int EquipmentArmorHealthPerTier = 35;
    public const int EquipmentArmorHealthPerLevel = 12;
    public const float EquipmentArmorBaseDamageReductionPercent = 2f;
    public const float EquipmentArmorDamageReductionPerTier = 3f;
    public const float EquipmentArmorDamageReductionPerLevel = 0.35f;
    public const float EquipmentArmorMaxDamageReductionPercent = 45f;
    public const int EquipmentStarForceMaxLevel = 20;
    public const int EquipmentStarForceBaseCost = 150;
    public const int EquipmentStarForceQuadraticCost = 75;
    public const int EquipmentUpgradeBaseCost = EquipmentStarForceBaseCost;
    public const int EquipmentUpgradeQuadraticCost =
        EquipmentStarForceQuadraticCost;
    public const float NormalEquipmentDropChance = 0.12f;
    public const int EquipmentOptionMinPercent = 1;
    public const int EquipmentOptionMaxPercent = 15;
    public const int EquipmentCoinTier0 = 5;
    public const int EquipmentCoinTier1 = 15;
    public const int EquipmentCoinTier2 = 40;
    public const int EquipmentCoinTier3 = 100;
    public const int EquipmentRerollCoinTier1 = 10;
    public const int EquipmentRerollCoinTier2 = 30;
    public const int EquipmentRerollCoinTier3 = 75;

    public const int GachaSingleGemCost = 100;
    public const int GachaTenGemCost = 900;
    public const int GachaPityLimit = 100;

    public const int QuestRewardGold = 300;
    public const int AchievementStageFiveGemReward = 500;
    public const int AchievementKillFiftyTicketReward = 3;

    public const int EventKillTarget = 4;
    public const int EventGachaTarget = 3;
    public const int EventRewardPointTarget = 30;
    public const int EventKillPoints = 10;
    public const int EventGachaPoints = 20;
    public const int EventRewardGems = 300;
    public const int EventRewardTickets = 2;

    public const int ShopGoldPouchGemCost = 100;
    public const int ShopGoldPouchGold = 5000;
    public const int ShopTicketBundleGemCost = 250;
    public const int ShopTicketBundleTickets = 3;
    public const int ShopGrowthChestGemCost = 500;
    public const int ShopGrowthChestGold = 30000;

    public const int RewardedAdDailyLimit = 5;
    public const int RewardedAdGemAmount = 100;
    public const int RewardedAdCooldownSeconds = 60;

    public const int StarterPackGold = 50000;
    public const int StarterPackGems = 1000;
    public const int StarterPackTickets = 10;
    public const int SmallGemPackGems = 1200;
    public const int LargeGemPackGems = 6500;

    // Natural rewards are balance baseline. No prototype currency floor.
    public const int PrototypeMinimumGold = 0;
    public const int PrototypeMinimumGems = 0;
}
