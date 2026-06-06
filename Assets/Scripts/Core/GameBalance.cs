using System;
using UnityEngine;

public static class GameBalance
{
    public const int EnemiesPerStage = 5;
    public const int MaxOfflineHours = 8;

    public static int GetPlayerAttack(PlayerData data)
    {
        return 8 + data.level * 2 + data.attackLevel * 6;
    }

    public static int GetPlayerMaxHealth(PlayerData data)
    {
        return 90 + data.level * 10 + data.healthLevel * 30;
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
        if (isBoss)
            value *= 5d;

        return ClampToInt(value);
    }

    public static int GetEnemyAttack(int stage, bool isBoss)
    {
        double value = 5d * Math.Pow(1.11d, stage - 1);
        if (isBoss)
            value *= 1.8d;

        return Math.Max(1, ClampToInt(value));
    }

    public static int GetEnemyGold(int stage, bool isBoss)
    {
        double value = 12d * Math.Pow(1.12d, stage - 1);
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

    private static int ClampToInt(double value)
    {
        return (int)Math.Min(
            int.MaxValue,
            Math.Max(0d, Math.Round(value)));
    }
}
