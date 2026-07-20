using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleVisualProfile
{
    public string profileName;
    public int stageFrom = 1;
    public int stageTo;
    public int stageCycle;
    public int stageCycleOffset;
    public int priority;
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;
    public BattleActorVisualSet visual = new BattleActorVisualSet();

    public bool HasVisual =>
        BattleActorVisualSet.IsConfigured(visual) ||
        sprite != null ||
        animatorController != null;

    public BattleActorVisualSet Resolve()
    {
        return BattleActorVisualSet.IsConfigured(visual)
            ? visual
            : BattleActorVisualSet.FromLegacy(sprite, animatorController);
    }

    public bool MatchesStage(int stage)
    {
        int normalizedStage = Mathf.Max(1, stage);
        int start = Mathf.Max(1, stageFrom);
        if (normalizedStage < start)
            return false;

        if (stageTo > 0 && normalizedStage > stageTo)
            return false;

        int cycle = Mathf.Max(0, stageCycle);
        if (cycle <= 1)
            return true;

        int offset = Mathf.Abs(stageCycleOffset) % cycle;
        return (normalizedStage - 1) % cycle == offset;
    }
}

[CreateAssetMenu(
    fileName = "BattleVisualDatabase",
    menuName = "Game/Battle Visual Database")]
public class BattleVisualDatabase : ScriptableObject
{
    public BattleVisualProfile hero = new BattleVisualProfile();
    public List<BattleVisualProfile> normalEnemies =
        new List<BattleVisualProfile>();
    public List<BattleVisualProfile> bosses =
        new List<BattleVisualProfile>();

    public BattleVisualProfile GetEnemy(int stage, bool boss)
    {
        List<BattleVisualProfile> profiles =
            boss ? bosses : normalEnemies;
        if (profiles == null || profiles.Count == 0)
            return null;

        List<BattleVisualProfile> candidates =
            GetStageCandidates(profiles, stage);
        if (candidates.Count == 0)
            candidates = GetValidProfiles(profiles);
        if (candidates.Count == 0)
            return null;

        int bestPriority = candidates[0].priority;
        for (int index = 1; index < candidates.Count; index++)
            bestPriority = Mathf.Max(bestPriority, candidates[index].priority);

        candidates.RemoveAll(profile => profile.priority < bestPriority);
        candidates.Sort(CompareProfiles);

        int firstStage = GetFirstStage(candidates);
        int selected = Mathf.Abs(stage - firstStage) % candidates.Count;
        return candidates[selected];
    }

    private static List<BattleVisualProfile> GetStageCandidates(
        List<BattleVisualProfile> profiles,
        int stage)
    {
        List<BattleVisualProfile> candidates =
            new List<BattleVisualProfile>();
        foreach (BattleVisualProfile profile in profiles)
        {
            if (profile != null && profile.MatchesStage(stage))
                candidates.Add(profile);
        }

        return candidates;
    }

    private static List<BattleVisualProfile> GetValidProfiles(
        List<BattleVisualProfile> profiles)
    {
        List<BattleVisualProfile> candidates =
            new List<BattleVisualProfile>();
        foreach (BattleVisualProfile profile in profiles)
        {
            if (profile != null)
                candidates.Add(profile);
        }

        return candidates;
    }

    private static int GetFirstStage(List<BattleVisualProfile> profiles)
    {
        int firstStage = int.MaxValue;
        foreach (BattleVisualProfile profile in profiles)
        {
            firstStage = Mathf.Min(
                firstStage,
                Mathf.Max(1, profile.stageFrom));
        }

        return firstStage == int.MaxValue ? 1 : firstStage;
    }

    private static int CompareProfiles(
        BattleVisualProfile left,
        BattleVisualProfile right)
    {
        int stageCompare =
            Mathf.Max(1, left.stageFrom)
                .CompareTo(Mathf.Max(1, right.stageFrom));
        if (stageCompare != 0)
            return stageCompare;

        return string.Compare(
            left.profileName,
            right.profileName,
            StringComparison.Ordinal);
    }
}

public static class BattleVisualResolver
{
    private static BattleVisualDatabase database;

    public static BattleActorVisualSet GetHero()
    {
        LoadDatabase();
        if (database?.hero != null && database.hero.HasVisual)
            return database.hero.Resolve();

        BattleActorVisualSet resource = LoadProfile("Battle/Hero");
        if (resource.HasActorVisual)
            return resource;

        return BattleActorVisualSet.FromLegacy(
            PrototypeBattleArt.GetSupportHeroSprite(),
            null);
    }

    public static BattleActorVisualSet GetEnemy(
        int stage,
        bool boss,
        EnemyAttackType attackType)
    {
        if (boss)
        {
            BattleActorVisualSet cerberus =
                PrototypeBattleArt.GetBossCatCerberusVisual();
            if (cerberus != null && cerberus.HasActorVisual)
                return cerberus;
        }

        if (!boss && attackType == EnemyAttackType.Melee)
        {
            BattleActorVisualSet meleeCat =
                PrototypeBattleArt.GetMeleeCatVisual();
            if (meleeCat != null && meleeCat.HasActorVisual)
                return meleeCat;
        }

        if (!boss && attackType == EnemyAttackType.Ranged)
        {
            BattleActorVisualSet mageCat =
                PrototypeBattleArt.GetMageCatVisual();
            if (mageCat != null && mageCat.HasActorVisual)
                return mageCat;
        }

        if (!boss && attackType == EnemyAttackType.Dash)
        {
            BattleActorVisualSet dashCat =
                PrototypeBattleArt.GetDashCatVisual();
            if (dashCat != null && dashCat.HasActorVisual)
                return dashCat;
        }

        LoadDatabase();
        BattleVisualProfile profile =
            database?.GetEnemy(stage, boss);
        if (profile != null && profile.HasVisual)
            return profile.Resolve();

        string type = boss ? "Bosses/Boss" : "Enemies/Enemy";
        int index = Mathf.Abs(stage - 1) % 4 + 1;
        BattleActorVisualSet resource =
            LoadProfile($"Battle/{type}_{index}");
        if (resource.HasActorVisual)
            return resource;

        return BattleActorVisualSet.FromPrototype(
            PrototypeBattleArt.GetEnemySprite(stage, boss),
            PrototypeBattleArt.GetEnemyAnimations(stage, boss));
    }

    private static BattleActorVisualSet LoadProfile(string resourcePath)
    {
        return BattleActorVisualSet.FromLegacy(
            Resources.Load<Sprite>(resourcePath),
                Resources.Load<RuntimeAnimatorController>(
                    resourcePath + "_Animator"));
    }

    private static void LoadDatabase()
    {
        if (database == null)
        {
            database = Resources.Load<BattleVisualDatabase>(
                "BattleVisualDatabase");
        }
    }
}
