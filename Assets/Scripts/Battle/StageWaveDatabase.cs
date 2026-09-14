using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EnemyDefinition
{
    public string enemyId;
    public string displayName;
    public EnemyAttackType attackType = EnemyAttackType.Melee;
    public float attackRange = 110f;
    public float approachSpeed = 500f;
    public float attackInterval = 2f;
    public float projectileDuration = 0.25f;
    public float damageMultiplier = 1f;
    public BattleActorVisualSet visual;

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(enemyId) &&
        (attackType == EnemyAttackType.Melee ||
         attackType == EnemyAttackType.Ranged ||
         attackType == EnemyAttackType.Dash);

    public EnemyCombatProfile CreateCombatProfile()
    {
        return new EnemyCombatProfile(
            attackType,
            attackRange,
            approachSpeed,
            attackInterval,
            projectileDuration,
            damageMultiplier);
    }
}

[Serializable]
public sealed class StageWaveRule
{
    [Min(1)] public int chapterFrom = 1;
    [Min(0)] public int chapterTo;
    [Range(1, 9)] public int chapterStageFrom = 1;
    [Range(1, 9)] public int chapterStageTo = 3;
    public int priority;
    [Min(1)] public int enemyCount = 2;
    [Min(1)] public int maxConsecutiveSameType = 2;
    public bool guaranteeEveryAllowedType;
    [Min(0.1f)] public float stageHealthBudgetMultiplier = 1f;
    public List<EnemyAttackType> allowedAttackTypes =
        new List<EnemyAttackType>();

    public bool Matches(int stage)
    {
        int normalizedStage = Mathf.Max(1, stage);
        int chapter = (normalizedStage - 1) / 10 + 1;
        int chapterStage = (normalizedStage - 1) % 10 + 1;
        if (chapterStage == 10)
            return false;

        if (chapter < Mathf.Max(1, chapterFrom))
            return false;
        if (chapterTo > 0 && chapter > chapterTo)
            return false;

        int from = Mathf.Clamp(chapterStageFrom, 1, 9);
        int to = Mathf.Clamp(chapterStageTo, from, 9);
        return chapterStage >= from && chapterStage <= to;
    }
}

[CreateAssetMenu(
    fileName = "StageWaveDatabase",
    menuName = "Game/Stage Wave Database")]
public sealed class StageWaveDatabase : ScriptableObject
{
    public List<EnemyDefinition> enemies = new List<EnemyDefinition>();
    public List<StageWaveRule> rules = new List<StageWaveRule>();

    public StageWaveRule GetRule(int stage)
    {
        StageWaveRule selected = null;
        if (rules == null)
            return null;

        foreach (StageWaveRule rule in rules)
        {
            if (rule == null || !rule.Matches(stage))
                continue;

            if (selected == null || rule.priority > selected.priority)
                selected = rule;
        }

        return selected;
    }

    public List<EnemyDefinition> GetEnemies(EnemyAttackType attackType)
    {
        List<EnemyDefinition> matches = new List<EnemyDefinition>();
        if (enemies == null)
            return matches;

        foreach (EnemyDefinition enemy in enemies)
        {
            if (enemy != null &&
                enemy.IsValid &&
                enemy.attackType == attackType)
            {
                matches.Add(enemy);
            }
        }

        return matches;
    }
}

public static class StageWaveResolver
{
    private const string DatabaseResourcePath = "StageWaveDatabase";

    private static readonly EnemyAttackType[] DefaultAttackTypes =
    {
        EnemyAttackType.Melee,
        EnemyAttackType.Ranged,
        EnemyAttackType.Dash
    };

    private static readonly Dictionary<EnemyAttackType, EnemyDefinition>
        FallbackEnemies =
            new Dictionary<EnemyAttackType, EnemyDefinition>
            {
                {
                    EnemyAttackType.Melee,
                    new EnemyDefinition
                    {
                        enemyId = "CatMelee_1",
                        displayName = "\uADFC\uC811 \uACE0\uC591\uC774",
                        attackType = EnemyAttackType.Melee,
                        attackRange = 110f,
                        approachSpeed = 500f,
                        attackInterval = 2f,
                        projectileDuration = 0.25f,
                        damageMultiplier = 1f
                    }
                },
                {
                    EnemyAttackType.Ranged,
                    new EnemyDefinition
                    {
                        enemyId = "CatMage_1",
                        displayName = "\uB9C8\uBC95\uC0AC \uACE0\uC591\uC774",
                        attackType = EnemyAttackType.Ranged,
                        attackRange = 225f,
                        approachSpeed = 420f,
                        attackInterval = 2.5f,
                        projectileDuration = 0.62f,
                        damageMultiplier = 0.9f
                    }
                },
                {
                    EnemyAttackType.Dash,
                    new EnemyDefinition
                    {
                        enemyId = "CatDash_1",
                        displayName = "\uB3CC\uC9C4 \uACE0\uC591\uC774",
                        attackType = EnemyAttackType.Dash,
                        attackRange = 96f,
                        approachSpeed = 900f,
                        attackInterval = 2.6f,
                        projectileDuration = 0.2f,
                        damageMultiplier = 1.1f
                    }
                }
            };

    private static StageWaveDatabase database;

    public static List<EnemyDefinition> BuildWave(
        int stage,
        int seed,
        out float healthBudgetMultiplier)
    {
        StageWaveRule rule = GetRule(stage);
        healthBudgetMultiplier = Mathf.Max(
            0.1f,
            rule.stageHealthBudgetMultiplier);

        List<EnemyAttackType> allowedTypes = GetAllowedTypes(rule);
        int count = Mathf.Max(1, rule.enemyCount);
        int maxConsecutive = Mathf.Max(1, rule.maxConsecutiveSameType);
        System.Random random = new System.Random(seed);
        List<EnemyAttackType> requiredTypes =
            rule.guaranteeEveryAllowedType && count >= allowedTypes.Count
                ? new List<EnemyAttackType>(allowedTypes)
                : new List<EnemyAttackType>();
        List<EnemyDefinition> wave = new List<EnemyDefinition>(count);

        EnemyAttackType previousType = EnemyAttackType.Boss;
        int consecutiveCount = 0;
        for (int index = 0; index < count; index++)
        {
            List<EnemyAttackType> selectable = new List<EnemyAttackType>();
            foreach (EnemyAttackType attackType in allowedTypes)
            {
                if (attackType != previousType ||
                    consecutiveCount < maxConsecutive)
                {
                    selectable.Add(attackType);
                }
            }

            if (selectable.Count == 0)
                selectable.AddRange(allowedTypes);

            int remainingSlots = count - index;
            List<EnemyAttackType> forced = requiredTypes.FindAll(
                attackType => selectable.Contains(attackType));
            List<EnemyAttackType> source =
                forced.Count > 0 && remainingSlots <= requiredTypes.Count
                    ? forced
                    : selectable;
            EnemyAttackType selectedType = source[random.Next(source.Count)];
            requiredTypes.Remove(selectedType);

            List<EnemyDefinition> variants = GetEnemies(selectedType);
            EnemyDefinition selectedEnemy =
                variants[random.Next(variants.Count)];
            wave.Add(selectedEnemy);

            if (selectedType == previousType)
                consecutiveCount++;
            else
            {
                previousType = selectedType;
                consecutiveCount = 1;
            }
        }

        return wave;
    }

    public static int GetEnemyCount(int stage)
    {
        if (GameBalance.IsBossStage(stage))
            return 1;

        return Mathf.Max(1, GetRule(stage).enemyCount);
    }

    private static StageWaveRule GetRule(int stage)
    {
        LoadDatabase();
        StageWaveRule configured = database?.GetRule(stage);
        if (configured != null)
            return configured;

        int chapterStage = (Mathf.Max(1, stage) - 1) % 10 + 1;
        return new StageWaveRule
        {
            chapterStageFrom = chapterStage,
            chapterStageTo = Mathf.Min(9, chapterStage),
            enemyCount = chapterStage <= 3
                ? 2
                : chapterStage <= 6
                    ? 3
                    : 4,
            maxConsecutiveSameType = 2,
            guaranteeEveryAllowedType = chapterStage >= 7,
            stageHealthBudgetMultiplier = 1f,
            allowedAttackTypes = new List<EnemyAttackType>(DefaultAttackTypes)
        };
    }

    private static List<EnemyAttackType> GetAllowedTypes(StageWaveRule rule)
    {
        List<EnemyAttackType> allowed = new List<EnemyAttackType>();
        if (rule.allowedAttackTypes != null)
        {
            foreach (EnemyAttackType attackType in rule.allowedAttackTypes)
            {
                if (attackType == EnemyAttackType.Boss ||
                    allowed.Contains(attackType) ||
                    GetEnemies(attackType).Count == 0)
                {
                    continue;
                }

                allowed.Add(attackType);
            }
        }

        if (allowed.Count == 0)
            allowed.AddRange(DefaultAttackTypes);

        return allowed;
    }

    private static List<EnemyDefinition> GetEnemies(
        EnemyAttackType attackType)
    {
        LoadDatabase();
        List<EnemyDefinition> configured = database?.GetEnemies(attackType);
        if (configured != null && configured.Count > 0)
            return configured;

        return new List<EnemyDefinition>
        {
            FallbackEnemies[attackType]
        };
    }

    private static void LoadDatabase()
    {
        if (database == null)
        {
            database = Resources.Load<StageWaveDatabase>(
                DatabaseResourcePath);
        }
    }
}
