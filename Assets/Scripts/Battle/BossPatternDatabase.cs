using System;
using System.Collections.Generic;
using UnityEngine;

public enum BossPatternType
{
    TargetedThunder,
    TripleFireBreath,
    SpiritVolley
}

[Serializable]
public class BossPatternDefinition
{
    public string patternName = "Targeted Thunder";
    public BossPatternType patternType;
    [Min(1f)] public float cooldown = 4f;
    [Min(0.1f)] public float warningSeconds = 1.5f;
    [Min(0.05f)] public float impactDelay = 0.35f;
    [Min(0.1f)] public float damageMultiplier = 1f;
    [Min(1)] public int hitCount = 1;
    [Range(0.01f, 0.5f)] public float targetRadiusX = 0.12f;
    [Range(0.01f, 0.5f)] public float targetRadiusY = 0.15f;
}

public sealed class BossPatternRuntime
{
    public BossPatternDefinition Pattern { get; }
    public Vector2[] TargetPositions { get; }
    public int SafeLaneIndex { get; }
    public int Sequence { get; }

    public BossPatternRuntime(
        BossPatternDefinition pattern,
        Vector2[] targetPositions,
        int safeLaneIndex,
        int sequence)
    {
        Pattern = pattern;
        TargetPositions = targetPositions ?? new Vector2[0];
        SafeLaneIndex = safeLaneIndex;
        Sequence = sequence;
    }
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

        return new List<BossPatternDefinition>
        {
            new BossPatternDefinition
            {
                patternName = "추적 낙뢰탄",
                patternType = BossPatternType.TargetedThunder,
                cooldown = 3.5f,
                warningSeconds = 1.6f,
                impactDelay = 0.35f,
                damageMultiplier = 2.2f,
                hitCount = 1,
                targetRadiusX = 0.13f,
                targetRadiusY = 0.16f
            },
            new BossPatternDefinition
            {
                patternName = "삼중 화염 숨결",
                patternType = BossPatternType.TripleFireBreath,
                cooldown = 4f,
                warningSeconds = 1.8f,
                impactDelay = 0.25f,
                damageMultiplier = 0.95f,
                hitCount = 3,
                targetRadiusX = 0.5f,
                targetRadiusY = 0.16f
            },
            new BossPatternDefinition
            {
                patternName = "유령탄 연사",
                patternType = BossPatternType.SpiritVolley,
                cooldown = 4f,
                warningSeconds = 1.5f,
                impactDelay = 0.55f,
                damageMultiplier = 1.05f,
                hitCount = 3,
                targetRadiusX = 0.1f,
                targetRadiusY = 0.13f
            }
        };
    }
}
