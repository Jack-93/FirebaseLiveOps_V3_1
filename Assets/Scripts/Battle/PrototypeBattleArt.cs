using System.Collections.Generic;
using UnityEngine;

public static class PrototypeBattleArt
{
    private const string BackgroundRoot = "PrototypeArt/Backgrounds/";
    private const string EnemyRoot = "PrototypeArt/Enemies/";
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
