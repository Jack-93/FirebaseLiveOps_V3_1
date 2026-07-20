using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameBalance
{
    public const int EnemiesPerStage =
        GameBalanceConfig.EnemiesPerStage;
    public const int BossStageInterval =
        GameBalanceConfig.BossStageInterval;
    public const int MaxOfflineHours =
        GameBalanceConfig.MaxOfflineHours;
    public const float BossTimeLimit =
        GameBalanceConfig.BossTimeLimit;
    public const float BossPatternWarningSeconds =
        GameBalanceConfig.BossPatternWarningSeconds;

    private static readonly string[] EnemyNames =
    {
        "Alley Cat",
        "Roof Cat",
        "Storm Cat",
        "Wire Cat"
    };

    private static readonly string[] BossNames =
    {
        "Boss Cat",
        "Iron Claw Cat",
        "Old Tree Cat",
        "Gear Cat"
    };

    public static bool IsBossStage(int stage)
    {
        int normalizedStage = Math.Max(1, stage);
        return normalizedStage % BossStageInterval == 0;
    }

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
        bonusPercent += Mathf.RoundToInt(
            EquipmentManager.GetAttackPercent(data));

        return Mathf.RoundToInt(
            baseAttack * (1f + bonusPercent / 100f));
    }

    public static int GetPlayerMaxHealth(PlayerData data)
    {
        return GetHeroMaxHealth(data);
    }

    public static int GetHeroMaxHealth(PlayerData data)
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
        synergyPercent +=
            GetEquippedCompanionHeroHealthPercentBonus();
        synergyPercent += Mathf.RoundToInt(
            EquipmentManager.GetHeroHealthPercent(data));
        return Mathf.RoundToInt(
            baseHealth * (1f + synergyPercent / 100f));
    }

    public static int GetHeroDamageAfterArmor(
        PlayerData data,
        int incomingDamage)
    {
        int damage = Mathf.Max(0, incomingDamage);
        if (damage <= 0)
            return 0;

        float reductionPercent =
            EquipmentManager.GetHeroDamageReductionPercent(data);
        return Mathf.Max(
            1,
            Mathf.RoundToInt(damage * (1f - reductionPercent / 100f)));
    }

    private static int GetEquippedCompanionHeroHealthPercentBonus()
    {
        List<CharacterData> party =
            CompanionManager.Instance?.GetEquippedParty();
        if (party == null)
            return 0;

        int bonusPercent = 0;
        foreach (CharacterData character in party)
        {
            if (character == null)
                continue;

            bonusPercent += Mathf.RoundToInt(
                Mathf.Max(0f, character.heroHealthPercentBonus));
        }

        return bonusPercent;
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
        double heroHealthScore =
            GetHeroMaxHealth(data) *
            GetHeroEffectiveHealthMultiplier(data) *
            0.35d;
        return ClampToInt(attackScore * 10d + heroHealthScore);
    }

    private static double GetHeroEffectiveHealthMultiplier(PlayerData data)
    {
        float reductionPercent =
            EquipmentManager.GetHeroDamageReductionPercent(data);
        float damageTakenMultiplier =
            Mathf.Clamp(1f - reductionPercent / 100f, 0.1f, 1f);
        return 1d / damageTakenMultiplier;
    }

    public static int GetOfflineGoldPerMinute(PlayerData data)
    {
        return Math.Max(
            5,
            GameBalance.GetEnemyGold(data.highestStage, false) / 2);
    }

    public static string GetEnemyName(int stage, bool isBoss)
    {
        if (isBoss && stage == 10)
            return "캣베로스";

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
