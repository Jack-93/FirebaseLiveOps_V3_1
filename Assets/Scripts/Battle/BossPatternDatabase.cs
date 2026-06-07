using System;
using System.Collections.Generic;
using UnityEngine;

public enum BossPatternType
{
    HeavyStrike,
    RapidStrikes,
    DrainStrike
}

[Serializable]
public class BossPatternDefinition
{
    public string patternName = "Heavy Strike";
    public BossPatternType patternType;
    [Min(1f)] public float cooldown = 6f;
    [Min(0.1f)] public float damageMultiplier = 1.8f;
    [Min(1)] public int hitCount = 1;
    [Range(0f, 1f)] public float healPercent;
}

[Serializable]
public class BossPatternSet
{
    [Min(1)] public int stage = 1;
    public List<BossPatternDefinition> patterns =
        new List<BossPatternDefinition>();
}

[CreateAssetMenu(
    fileName = "BossPatternDatabase",
    menuName = "Game/Boss Pattern Database")]
public class BossPatternDatabase : ScriptableObject
{
    public List<BossPatternSet> bosses = new List<BossPatternSet>();

    public List<BossPatternDefinition> GetPatterns(int stage)
    {
        if (bosses == null || bosses.Count == 0)
            return null;

        BossPatternSet exact =
            bosses.Find(entry => entry != null && entry.stage == stage);
        if (exact != null && exact.patterns.Count > 0)
            return exact.patterns;

        int index = Mathf.Abs(stage - 1) % bosses.Count;
        return bosses[index]?.patterns;
    }
}

public static class BossPatternResolver
{
    private static BossPatternDatabase database;

    public static List<BossPatternDefinition> GetPatterns(int stage)
    {
        if (database == null)
        {
            database = Resources.Load<BossPatternDatabase>(
                "BossPatternDatabase");
        }

        List<BossPatternDefinition> configured =
            database?.GetPatterns(stage);
        if (configured != null && configured.Count > 0)
            return configured;

        switch (Mathf.Abs(stage - 1) % 3)
        {
            case 1:
                return new List<BossPatternDefinition>
                {
                    new BossPatternDefinition
                    {
                        patternName = "Rapid Claws",
                        patternType = BossPatternType.RapidStrikes,
                        cooldown = 7f,
                        damageMultiplier = 0.65f,
                        hitCount = 3
                    }
                };
            case 2:
                return new List<BossPatternDefinition>
                {
                    new BossPatternDefinition
                    {
                        patternName = "Life Drain",
                        patternType = BossPatternType.DrainStrike,
                        cooldown = 8f,
                        damageMultiplier = 1.25f,
                        healPercent = 0.08f
                    }
                };
            default:
                return new List<BossPatternDefinition>
                {
                    new BossPatternDefinition
                    {
                        patternName = "Heavy Strike",
                        patternType = BossPatternType.HeavyStrike,
                        cooldown = 6f,
                        damageMultiplier = 1.8f
                    }
                };
        }
    }
}
