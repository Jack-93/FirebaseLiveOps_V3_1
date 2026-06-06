using System;
using UnityEngine;

public static class GameBalance
{
    public const int EnemiesPerStage = 5;
    public const int MaxOfflineHours = 8;
    public const float BossTimeLimit = 30f;

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
            8 + data.level * 2 + data.attackLevel * 6 +
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

        return Mathf.RoundToInt(
            baseAttack * (1f + bonusPercent / 100f));
    }

    public static int GetPlayerMaxHealth(PlayerData data)
    {
        return 90 + data.level * 10 + data.healthLevel * 30 +
            EquipmentManager.GetArmorHealth(data);
    }

    public static float GetPlayerAttackInterval(PlayerData data)
    {
        return Mathf.Max(
            0.25f,
            1.2f - (data.attackSpeedLevel - 1) * 0.045f);
    }

    public static int GetEnemyMaxHealth(int stage, bool isBoss)
    {
        double value = 42d * Math.Pow(1.17d, stage - 1);
        value *= 1d + (Math.Max(1, stage) - 1) % 4 * 0.08d;
        if (isBoss)
            value *= 5d;

        return ClampToInt(value);
    }

    public static int GetEnemyAttack(int stage, bool isBoss)
    {
        double value = 5d * Math.Pow(1.11d, stage - 1);
        value *= 1d + (Math.Max(1, stage) - 1) % 4 * 0.05d;
        if (isBoss)
            value *= 1.8d;

        return Math.Max(1, ClampToInt(value));
    }

    public static int GetEnemyGold(int stage, bool isBoss)
    {
        double value = 12d * Math.Pow(1.12d, stage - 1);
        value *= 1d + (Math.Max(1, stage) - 1) % 4 * 0.06d;
        if (isBoss)
            value *= 8d;

        return Math.Max(1, ClampToInt(value));
    }

    public static int GetUpgradeCost(UpgradeType type, int currentLevel)
    {
        int baseCost = type == UpgradeType.AttackSpeed ? 180 : 100;
        return ClampToInt(baseCost * Math.Pow(1.34d, currentLevel - 1));
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
