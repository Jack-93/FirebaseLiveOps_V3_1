using System;
using UnityEngine;

public static class GameBalance
{
    public const int EnemiesPerStage =
        GameBalanceConfig.EnemiesPerStage;
    public const int MaxOfflineHours =
        GameBalanceConfig.MaxOfflineHours;
    public const float BossTimeLimit =
        GameBalanceConfig.BossTimeLimit;

    private static readonly string[] EnemyNames =
    {
        "Slime Scout",
        "Cave Fang",
        "Forest Wisp",
        "Clockwork Bug"
    };

    private static readonly string[] BossNames =
    {
        "Slime Monarch",
        "Ironjaw",
        "Ancient Treant",
        "Gear Colossus"
    };

    public static int GetPlayerAttack(PlayerData data)
    {
        int baseAttack =
            GameBalanceConfig.PlayerBaseAttack +
            data.level * GameBalanceConfig.PlayerAttackPerLevel +
            data.attackLevel *
            GameBalanceConfig.PlayerAttackPerUpgrade +
            EquipmentManager.GetWeaponAttack(data);
        int bonusPercent = 0;
        if (data.equippedCompanionRarities != null)
        {
            for (int index = 0;
                 index < data.equippedCompanionRarities.Count;
                 index++)
            {
                string rarity = data.equippedCompanionRarities[index];
                string characterName =
                    index < data.equippedCompanions.Count
                        ? data.equippedCompanions[index]
                        : "";
                int stars = 1;
                if (!string.IsNullOrEmpty(characterName) &&
                    data.companionStars.TryGetValue(
                        characterName,
                        out int savedStars))
                {
                    stars = savedStars;
                }

                bonusPercent +=
                    CompanionManager.GetAttackBonusPercent(
                        rarity,
                        stars);
            }
        }
        else
        {
            bonusPercent =
                CompanionManager.GetAttackBonusPercent(
                    data.equippedCompanionRarity);
        }

        CompanionSynergyResult synergy =
            CompanionManager.Instance?.GetSynergyResult();
        bonusPercent += synergy?.AttackPercent ?? 0;

        return Mathf.RoundToInt(
            baseAttack * (1f + bonusPercent / 100f));
    }

    public static int GetPlayerMaxHealth(PlayerData data)
    {
        int baseHealth =
            GameBalanceConfig.PlayerBaseHealth +
            data.level * GameBalanceConfig.PlayerHealthPerLevel +
            data.healthLevel *
            GameBalanceConfig.PlayerHealthPerUpgrade +
            EquipmentManager.GetArmorHealth(data);
        int synergyPercent =
            CompanionManager.Instance
                ?.GetSynergyResult()
                .HealthPercent ?? 0;
        return Mathf.RoundToInt(
            baseHealth * (1f + synergyPercent / 100f));
    }

    public static float GetPlayerAttackInterval(PlayerData data)
    {
        float baseInterval = Mathf.Max(
            GameBalanceConfig.PlayerMinAttackInterval,
            GameBalanceConfig.PlayerBaseAttackInterval -
            (data.attackSpeedLevel - 1) *
            GameBalanceConfig.PlayerAttackIntervalReduction);
        int synergyPercent =
            CompanionManager.Instance
                ?.GetSynergyResult()
            .AttackSpeedPercent ?? 0;
        return Mathf.Max(
            GameBalanceConfig.PlayerAbsoluteMinAttackInterval,
            baseInterval / (1f + synergyPercent / 100f));
    }

    public static int GetEnemyMaxHealth(int stage, bool isBoss)
    {
        double value =
            GameBalanceConfig.EnemyBaseHealth *
            Math.Pow(GameBalanceConfig.EnemyHealthGrowth, stage - 1);
        value *= 1d + (Math.Max(1, stage) - 1) %
            4 * GameBalanceConfig.EnemyStageHealthCycleBonus;
        if (isBoss)
            value *= GameBalanceConfig.BossHealthMultiplier;

        return ClampToInt(value);
    }

    public static int GetEnemyAttack(int stage, bool isBoss)
    {
        double value =
            GameBalanceConfig.EnemyBaseAttack *
            Math.Pow(GameBalanceConfig.EnemyAttackGrowth, stage - 1);
        value *= 1d + (Math.Max(1, stage) - 1) %
            4 * GameBalanceConfig.EnemyStageAttackCycleBonus;
        if (isBoss)
            value *= GameBalanceConfig.BossAttackMultiplier;

        return Math.Max(1, ClampToInt(value));
    }

    public static int GetEnemyGold(int stage, bool isBoss)
    {
        double value =
            GameBalanceConfig.EnemyBaseGold *
            Math.Pow(GameBalanceConfig.EnemyGoldGrowth, stage - 1);
        value *= 1d + (Math.Max(1, stage) - 1) %
            4 * GameBalanceConfig.EnemyStageGoldCycleBonus;
        if (isBoss)
            value *= GameBalanceConfig.BossGoldMultiplier;

        return Math.Max(1, ClampToInt(value));
    }

    public static int GetUpgradeCost(UpgradeType type, int currentLevel)
    {
        int baseCost = type == UpgradeType.AttackSpeed
            ? GameBalanceConfig.AttackSpeedUpgradeBaseCost
            : GameBalanceConfig.HeroUpgradeBaseCost;
        return ClampToInt(
            baseCost *
            Math.Pow(
                GameBalanceConfig.HeroUpgradeCostGrowth,
                currentLevel - 1));
    }

    public static int GetCombatPower(PlayerData data)
    {
        double attackScore =
            GetPlayerAttack(data) / GetPlayerAttackInterval(data);
        double healthScore = GetPlayerMaxHealth(data) * 0.35d;
        return ClampToInt(attackScore * 10d + healthScore);
    }

    public static int GetOfflineGoldPerMinute(PlayerData data)
    {
        return Math.Max(
            5,
            GameBalance.GetEnemyGold(data.highestStage, false) / 2);
    }

    public static string GetEnemyName(int stage, bool isBoss)
    {
        int index = Math.Max(0, stage - 1) % EnemyNames.Length;
        return isBoss ? BossNames[index] : EnemyNames[index];
    }

    private static int ClampToInt(double value)
    {
        return (int)Math.Min(
            int.MaxValue,
            Math.Max(0d, Math.Round(value)));
    }
}
