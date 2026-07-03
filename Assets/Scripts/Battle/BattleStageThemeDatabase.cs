using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BattleStageThemeProfile
{
    public string themeName;
    public int stageFrom = 1;
    public int stageTo;
    public int priority;
    public Sprite background;
    public Sprite midground;
    public Sprite foreground;
    public Color fallbackColor = new Color32(17, 24, 39, 255);

    public bool HasVisual =>
        background != null ||
        midground != null ||
        foreground != null;

    public bool MatchesStage(int stage)
    {
        int normalizedStage = Mathf.Max(1, stage);
        int start = Mathf.Max(1, stageFrom);
        if (normalizedStage < start)
            return false;

        return stageTo <= 0 || normalizedStage <= stageTo;
    }
}

[CreateAssetMenu(
    fileName = "BattleStageThemeDatabase",
    menuName = "Game/Battle Stage Theme Database")]
public sealed class BattleStageThemeDatabase : ScriptableObject
{
    public List<BattleStageThemeProfile> themes =
        new List<BattleStageThemeProfile>();

    public BattleStageThemeProfile GetTheme(int stage)
    {
        if (themes == null || themes.Count == 0)
            return null;

        List<BattleStageThemeProfile> candidates =
            new List<BattleStageThemeProfile>();
        foreach (BattleStageThemeProfile theme in themes)
        {
            if (theme != null && theme.MatchesStage(stage))
                candidates.Add(theme);
        }

        if (candidates.Count == 0)
        {
            foreach (BattleStageThemeProfile theme in themes)
            {
                if (theme != null)
                    candidates.Add(theme);
            }
        }
        if (candidates.Count == 0)
            return null;

        candidates.Sort(CompareThemes);
        int bestPriority = candidates[0].priority;
        candidates.RemoveAll(theme => theme.priority < bestPriority);
        candidates.Sort(CompareThemes);

        int firstStage = GetFirstStage(candidates);
        int selected = Mathf.Abs(stage - firstStage) % candidates.Count;
        return candidates[selected];
    }

    private static int GetFirstStage(List<BattleStageThemeProfile> themes)
    {
        int firstStage = int.MaxValue;
        foreach (BattleStageThemeProfile theme in themes)
        {
            firstStage = Mathf.Min(
                firstStage,
                Mathf.Max(1, theme.stageFrom));
        }

        return firstStage == int.MaxValue ? 1 : firstStage;
    }

    private static int CompareThemes(
        BattleStageThemeProfile left,
        BattleStageThemeProfile right)
    {
        int priorityCompare = right.priority.CompareTo(left.priority);
        if (priorityCompare != 0)
            return priorityCompare;

        int stageCompare =
            Mathf.Max(1, left.stageFrom)
                .CompareTo(Mathf.Max(1, right.stageFrom));
        if (stageCompare != 0)
            return stageCompare;

        return string.Compare(
            left.themeName,
            right.themeName,
            StringComparison.Ordinal);
    }
}

public static class BattleStageThemeResolver
{
    private static BattleStageThemeDatabase database;

    public static int GetThemeIndex(int stage)
    {
        BattleStageThemeProfile profile = GetConfiguredTheme(stage);
        if (profile == null)
            return PrototypeBattleArt.GetThemeIndex(stage);

        return Mathf.Max(0, profile.stageFrom - 1) / 10;
    }

    public static string GetThemeName(int stage)
    {
        BattleStageThemeProfile profile = GetConfiguredTheme(stage);
        if (profile != null &&
            !string.IsNullOrWhiteSpace(profile.themeName))
        {
            return profile.themeName;
        }

        return PrototypeBattleArt.GetThemeName(stage);
    }

    public static Sprite GetStageBackground(int stage)
    {
        BattleStageThemeProfile profile = GetConfiguredTheme(stage);
        return profile?.background ??
            PrototypeBattleArt.GetStageBackground(stage);
    }

    public static Sprite GetStageMidground(int stage)
    {
        BattleStageThemeProfile profile = GetConfiguredTheme(stage);
        return profile?.midground ??
            PrototypeBattleArt.GetStageMidground(stage);
    }

    public static Sprite GetStageForeground(int stage)
    {
        BattleStageThemeProfile profile = GetConfiguredTheme(stage);
        return profile?.foreground ??
            PrototypeBattleArt.GetStageForeground(stage);
    }

    public static Color GetFallbackColor(int stage)
    {
        BattleStageThemeProfile profile = GetConfiguredTheme(stage);
        if (profile != null)
            return profile.fallbackColor;

        return new Color32(17, 24, 39, 255);
    }

    private static BattleStageThemeProfile GetConfiguredTheme(int stage)
    {
        LoadDatabase();
        BattleStageThemeProfile profile = database?.GetTheme(stage);
        return profile != null && profile.HasVisual ? profile : null;
    }

    private static void LoadDatabase()
    {
        if (database == null)
        {
            database = Resources.Load<BattleStageThemeDatabase>(
                "BattleStageThemeDatabase");
        }
    }
}
