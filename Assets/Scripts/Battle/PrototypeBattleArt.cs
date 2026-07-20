using System;
using System.Collections.Generic;
using UnityEngine;

public static class PrototypeBattleArt
{
    private const string BackgroundRoot = "PrototypeArt/Backgrounds/";
    private const string EnemyRoot = "PrototypeArt/Enemies/";
    private const string EnemyAnimationRoot =
        EnemyRoot + "Animations/";
    private const string ProductionEnemyAnimationRoot =
        "Battle/Enemies/Animations/";
    private const string ProductionEnemyAnimatorRoot =
        "Battle/Enemies/";
    private const string ProductionEnemyProjectileRoot =
        "Battle/Enemies/Projectiles/";
    private const string SupportHeroPath =
        "PrototypeArt/Heroes/SupportSparrow";
    private const string FallbackBackground =
        BackgroundRoot + "StageSunset";
    private const string FallbackEnemy = EnemyRoot + "CatScout";

    private static readonly ThemeDefinition[] Themes =
    {
        new ThemeDefinition("Sunset", "StageSunset", "CatScout"),
        new ThemeDefinition("Forest", "StageForest", "CatForest"),
        new ThemeDefinition("Rooftop", "StageRooftop", "CatRooftop"),
        new ThemeDefinition("Rain", "StageRain", "CatRain")
    };

    private static readonly Dictionary<string, Sprite> SpriteCache =
        new Dictionary<string, Sprite>();
    private static readonly Dictionary<
        string,
        Dictionary<BattleAnimationCue, Sprite[]>> AnimationCache =
            new Dictionary<string, Dictionary<BattleAnimationCue, Sprite[]>>();

    public static int GetThemeIndex(int stage)
    {
        int index = Mathf.Max(0, stage - 1) / 10;
        return Mathf.Clamp(index, 0, Themes.Length - 1);
    }

    public static string GetThemeName(int stage)
    {
        return Themes[GetThemeIndex(stage)].Name;
    }

    public static Sprite GetStageBackground(int stage = 1)
    {
        ThemeDefinition theme = Themes[GetThemeIndex(stage)];
        return LoadSprite(
            BackgroundRoot + theme.BackgroundName,
            FallbackBackground);
    }

    public static Sprite GetSupportHeroSprite()
    {
        return LoadSprite(SupportHeroPath, null);
    }

    public static Sprite GetStageMidground(int stage = 1)
    {
        ThemeDefinition theme = Themes[GetThemeIndex(stage)];
        return LoadSprite(
            BackgroundRoot + theme.BackgroundName + "_Midground",
            null);
    }

    public static Sprite GetStageForeground(int stage = 1)
    {
        ThemeDefinition theme = Themes[GetThemeIndex(stage)];
        return LoadSprite(
            BackgroundRoot + theme.BackgroundName + "_Foreground",
            null);
    }

    public static Sprite GetEnemySprite(int stage, bool boss)
    {
        ThemeDefinition theme = Themes[GetThemeIndex(stage)];
        string suffix = boss ? "Boss" : "";
        string requestedPath = EnemyRoot + theme.EnemyName + suffix;
        Sprite sprite = LoadSprite(requestedPath, null);

        if (sprite != null)
            return sprite;

        return LoadSprite(
            EnemyRoot + theme.EnemyName,
            FallbackEnemy);
    }

    public static Dictionary<BattleAnimationCue, Sprite[]> GetEnemyAnimations(
        int stage,
        bool boss)
    {
        ThemeDefinition theme = Themes[GetThemeIndex(stage)];
        string suffix = boss ? "Boss" : "";
        string requestedKey = theme.EnemyName + suffix;
        Dictionary<BattleAnimationCue, Sprite[]> animations =
            LoadEnemyAnimationSet(requestedKey);

        if (HasIdleAnimation(animations))
            return animations;

        if (boss)
        {
            animations = LoadEnemyAnimationSet(theme.EnemyName);
            if (HasIdleAnimation(animations))
                return animations;
        }

        animations = LoadEnemyAnimationSet("CatScout");
        return HasIdleAnimation(animations) ? animations : null;
    }

    public static BattleActorVisualSet GetMeleeCatVisual()
    {
        Dictionary<BattleAnimationCue, Sprite[]> animations =
            LoadProductionEnemyAnimationSet("CatMelee_1");
        if (!HasIdleAnimation(animations))
            return null;

        Sprite[] idleFrames = animations[BattleAnimationCue.Idle];
        RuntimeAnimatorController controller =
            Resources.Load<RuntimeAnimatorController>(
                ProductionEnemyAnimatorRoot + "CatMelee_1/CatMelee_1");
        if (controller != null)
        {
            return new BattleActorVisualSet
            {
                sprite = idleFrames[0],
                animatorController = controller
            };
        }

        return BattleActorVisualSet.FromPrototype(idleFrames[0], animations);
    }

    public static BattleActorVisualSet GetMageCatVisual()
    {
        Dictionary<BattleAnimationCue, Sprite[]> animations =
            LoadProductionEnemyAnimationSet("CatMage_1");
        if (!HasIdleAnimation(animations))
            return null;

        Sprite[] idleFrames = animations[BattleAnimationCue.Idle];
        RuntimeAnimatorController controller =
            Resources.Load<RuntimeAnimatorController>(
                ProductionEnemyAnimatorRoot + "CatMage_1/CatMage_1");
        BattleActorVisualSet visual = controller == null
            ? BattleActorVisualSet.FromPrototype(idleFrames[0], animations)
            : new BattleActorVisualSet
            {
                sprite = idleFrames[0],
                animatorController = controller
            };
        Sprite projectile = Resources.Load<Sprite>(
            ProductionEnemyProjectileRoot + "CatMage_1_ArcaneBolt");
        visual.basicProjectile = new BattleProjectileVisual
        {
            sprite = projectile,
            tint = Color.white,
            duration = 0.62f,
            size = 112f
        };
        return visual;
    }

    public static BattleActorVisualSet GetDashCatVisual()
    {
        Dictionary<BattleAnimationCue, Sprite[]> animations =
            LoadProductionEnemyAnimationSet("CatDash_1");
        if (!HasIdleAnimation(animations))
            return null;

        Sprite[] idleFrames = animations[BattleAnimationCue.Idle];
        RuntimeAnimatorController controller =
            Resources.Load<RuntimeAnimatorController>(
                ProductionEnemyAnimatorRoot + "CatDash_1/CatDash_1");
        if (controller != null)
        {
            return new BattleActorVisualSet
            {
                sprite = idleFrames[0],
                animatorController = controller
            };
        }

        return BattleActorVisualSet.FromPrototype(idleFrames[0], animations);
    }

    public static BattleActorVisualSet GetBossCatCerberusVisual()
    {
        const string enemyKey = "BossCatCerberus";
        Dictionary<BattleAnimationCue, Sprite[]> animations =
            LoadProductionEnemyAnimationSet(enemyKey);
        if (!HasIdleAnimation(animations))
            return null;

        Sprite[] idleFrames = animations[BattleAnimationCue.Idle];
        RuntimeAnimatorController controller =
            Resources.Load<RuntimeAnimatorController>(
                ProductionEnemyAnimatorRoot + enemyKey + "/" + enemyKey);
        if (controller != null)
        {
            return new BattleActorVisualSet
            {
                sprite = idleFrames[0],
                animatorController = controller
            };
        }

        return BattleActorVisualSet.FromPrototype(idleFrames[0], animations);
    }

    private static Sprite LoadSprite(string path, string fallbackPath)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        if (!SpriteCache.TryGetValue(path, out Sprite sprite))
        {
            sprite = Resources.Load<Sprite>(path);
            SpriteCache[path] = sprite;
        }

        if (sprite != null || string.IsNullOrEmpty(fallbackPath))
            return sprite;

        if (!SpriteCache.TryGetValue(fallbackPath, out Sprite fallback))
        {
            fallback = Resources.Load<Sprite>(fallbackPath);
            SpriteCache[fallbackPath] = fallback;
        }

        return fallback;
    }

    private static Dictionary<BattleAnimationCue, Sprite[]>
        LoadEnemyAnimationSet(string enemyKey)
    {
        if (string.IsNullOrEmpty(enemyKey))
            return null;

        if (AnimationCache.TryGetValue(enemyKey, out var cached))
            return cached;

        Dictionary<BattleAnimationCue, Sprite[]> animations =
            new Dictionary<BattleAnimationCue, Sprite[]>();
        Array cueValues = Enum.GetValues(typeof(BattleAnimationCue));
        foreach (BattleAnimationCue cue in cueValues)
        {
            string path =
                EnemyAnimationRoot + enemyKey + "/" + cue;
            Sprite[] frames = Resources.LoadAll<Sprite>(path);
            if (frames == null || frames.Length == 0)
                continue;

            Array.Sort(
                frames,
                (left, right) =>
                    string.CompareOrdinal(left.name, right.name));
            animations[cue] = frames;
        }

        AnimationCache[enemyKey] = animations;
        return animations;
    }

    private static Dictionary<BattleAnimationCue, Sprite[]>
        LoadProductionEnemyAnimationSet(string enemyKey)
    {
        if (string.IsNullOrEmpty(enemyKey))
            return null;

        string cacheKey = "Production/" + enemyKey;
        if (AnimationCache.TryGetValue(cacheKey, out var cached))
            return cached;

        Dictionary<BattleAnimationCue, Sprite[]> animations =
            new Dictionary<BattleAnimationCue, Sprite[]>();
        BattleAnimationCue[] cues =
        {
            BattleAnimationCue.Idle,
            BattleAnimationCue.Move,
            BattleAnimationCue.Attack,
            BattleAnimationCue.Death,
            BattleAnimationCue.SkillLeft,
            BattleAnimationCue.SkillCenter,
            BattleAnimationCue.SkillRight
        };
        foreach (BattleAnimationCue cue in cues)
        {
            Sprite[] frames = Resources.LoadAll<Sprite>(
                ProductionEnemyAnimationRoot + enemyKey + "/" + cue);
            if (frames == null || frames.Length == 0)
                continue;

            Array.Sort(
                frames,
                (left, right) =>
                    string.CompareOrdinal(left.name, right.name));
            animations[cue] = frames;
        }

        AnimationCache[cacheKey] = animations;
        return animations;
    }

    private static bool HasIdleAnimation(
        Dictionary<BattleAnimationCue, Sprite[]> animations)
    {
        return animations != null &&
            animations.TryGetValue(
                BattleAnimationCue.Idle,
                out Sprite[] idleFrames) &&
            idleFrames != null &&
            idleFrames.Length > 0;
    }

    private sealed class ThemeDefinition
    {
        public string Name { get; }
        public string BackgroundName { get; }
        public string EnemyName { get; }

        public ThemeDefinition(
            string name,
            string backgroundName,
            string enemyName)
        {
            Name = name;
            BackgroundName = backgroundName;
            EnemyName = enemyName;
        }
    }
}
