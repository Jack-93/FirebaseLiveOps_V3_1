using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BattleArtPipelineTools
{
    private const string ArtRoot = "Assets/Art";
    private const string BattleRoot = ArtRoot + "/Battle";
    private const string DatabasePath =
        "Assets/Resources/BattleVisualDatabase.asset";
    private const string ThemeDatabasePath =
        "Assets/Resources/BattleStageThemeDatabase.asset";

    private static readonly string[] RequiredFolders =
    {
        "Assets/Art",
        "Assets/Art/Battle",
        "Assets/Art/Battle/Heroes",
        "Assets/Art/Battle/Companions",
        "Assets/Art/Battle/Enemies",
        "Assets/Art/Battle/Bosses",
        "Assets/Art/Battle/Projectiles",
        "Assets/Art/Battle/Backgrounds",
        "Assets/Art/UI",
        "Assets/Art/UI/Icons",
        "Assets/Art/UI/Frames",
        "Assets/Art/Fonts",
        "Assets/Art/Audio",
        "Assets/Art/Audio/BGM",
        "Assets/Art/Audio/SFX"
    };

    private static readonly BattleVisualSampleProfile[] SampleEnemies =
    {
        new BattleVisualSampleProfile("CatScout", 1, 20),
        new BattleVisualSampleProfile("CatStray", 21, 50),
        new BattleVisualSampleProfile("CatWireRunner", 51, 90),
        new BattleVisualSampleProfile("CatStormClaw", 91, 0)
    };

    private static readonly BattleVisualSampleProfile[] SampleBosses =
    {
        new BattleVisualSampleProfile("CatPoleCaptain", 1, 30),
        new BattleVisualSampleProfile("CatRooftopRaider", 31, 70),
        new BattleVisualSampleProfile("CatStormGeneral", 71, 0)
    };

    private static readonly BattleStageThemeSample[] SampleThemes =
    {
        new BattleStageThemeSample("SunsetPole", 1, 20),
        new BattleStageThemeSample("ForestPole", 21, 50),
        new BattleStageThemeSample("RooftopPole", 51, 90),
        new BattleStageThemeSample("RainPole", 91, 0)
    };

    [MenuItem("Tools/Battle Art/Prepare Production Art Folders")]
    public static void PrepareProductionArtFolders()
    {
        foreach (string folder in RequiredFolders)
            EnsureFolder(folder);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BattleArt] Production art folders prepared.");
    }

    [MenuItem("Tools/Battle Art/Auto Link Battle Visuals")]
    public static void AutoLinkBattleVisuals()
    {
        PrepareProductionArtFolders();

        int changed = 0;
        changed += LinkHeroAndEnemies();
        changed += LinkStageThemes();
        changed += LinkCompanions();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BattleArt] Auto-linked battle visuals: " + changed);
    }

    [MenuItem("Tools/Battle Art/Normalize Visual Stage Rules")]
    public static void NormalizeVisualStageRules()
    {
        BattleVisualDatabase database = LoadOrCreateDatabase();
        int changed = 0;
        changed += NormalizeProfile(database.hero, "SupportSparrow");
        changed += NormalizeProfiles(database.normalEnemies, "Enemy");
        changed += NormalizeProfiles(database.bosses, "Boss");

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BattleArt] Normalized visual stage rules: " + changed);
    }

    [MenuItem("Tools/Battle Art/Create Sample Stage Profiles")]
    public static void CreateSampleStageProfiles()
    {
        PrepareProductionArtFolders();

        BattleVisualDatabase database = LoadOrCreateDatabase();
        int changed = 0;
        if (database.hero == null)
        {
            database.hero = new BattleVisualProfile();
            changed++;
        }
        if (database.normalEnemies == null)
        {
            database.normalEnemies = new List<BattleVisualProfile>();
            changed++;
        }
        if (database.bosses == null)
        {
            database.bosses = new List<BattleVisualProfile>();
            changed++;
        }

        database.hero.profileName = "SupportSparrow";
        database.hero.stageFrom = 1;
        if (EnsureActorFolders(
            BattleRoot + "/Heroes",
            "SupportSparrow"))
        {
            changed++;
        }

        changed += CreateSampleProfiles(
            database.normalEnemies,
            BattleRoot + "/Enemies",
            SampleEnemies);
        changed += CreateSampleProfiles(
            database.bosses,
            BattleRoot + "/Bosses",
            SampleBosses);
        changed += CreateCharacterArtFoldersInternal();
        changed += CreateSampleThemeProfilesInternal();
        changed += NormalizeProfile(database.hero, "SupportSparrow");
        changed += NormalizeProfiles(database.normalEnemies, "Enemy");
        changed += NormalizeProfiles(database.bosses, "Boss");

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BattleArt] Created sample stage profiles: " + changed);
    }

    [MenuItem("Tools/Battle Art/Create Sample Stage Themes")]
    public static void CreateSampleStageThemes()
    {
        PrepareProductionArtFolders();

        int changed = CreateSampleThemeProfilesInternal();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BattleArt] Created sample stage themes: " + changed);
    }

    [MenuItem("Tools/Battle Art/Create Character Art Folders")]
    public static void CreateCharacterArtFolders()
    {
        PrepareProductionArtFolders();

        int changed = CreateCharacterArtFoldersInternal();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BattleArt] Created character art folders: " + changed);
    }

    [MenuItem("Tools/Battle Art/Write Art Readiness Report")]
    public static void WriteArtReadinessReport()
    {
        PrepareProductionArtFolders();

        BattleVisualDatabase database = LoadOrCreateDatabase();
        List<string> lines = new List<string>
        {
            "Battle Art Readiness Report",
            "Generated by Tools > Battle Art > Write Art Readiness Report",
            string.Empty
        };

        AppendActorReport(
            lines,
            "Hero",
            BattleRoot + "/Heroes",
            "SupportSparrow");
        AppendCharacterReports(lines);
        AppendProfileReports(
            lines,
            "Enemy",
            BattleRoot + "/Enemies",
            database.normalEnemies);
        AppendProfileReports(
            lines,
            "Boss",
            BattleRoot + "/Bosses",
            database.bosses);
        AppendStageThemeReport(lines);

        Directory.CreateDirectory("Logs");
        File.WriteAllLines(
            "Logs/BattleArtReadinessReport.txt",
            lines);
        Debug.Log(
            "[BattleArt] Wrote Logs/BattleArtReadinessReport.txt\n" +
            string.Join("\n", lines));
    }

    private static int LinkHeroAndEnemies()
    {
        BattleVisualDatabase database = LoadOrCreateDatabase();
        int changed = 0;

        BattleActorVisualSet heroVisual =
            BuildVisualSet(
                BattleRoot + "/Heroes/SupportSparrow",
                BattleRoot + "/Heroes",
                "SupportSparrow");
        if (BattleActorVisualSet.IsConfigured(heroVisual))
        {
            database.hero.profileName = "SupportSparrow";
            NormalizeProfile(database.hero, "SupportSparrow");
            database.hero.visual = heroVisual;
            changed++;
        }

        changed += LinkProfiles(
            database.normalEnemies,
            BattleRoot + "/Enemies");
        changed += LinkProfiles(
            database.bosses,
            BattleRoot + "/Bosses");

        EditorUtility.SetDirty(database);
        return changed;
    }

    private static int LinkStageThemes()
    {
        BattleStageThemeDatabase database = LoadOrCreateThemeDatabase();
        if (database.themes == null)
            database.themes = new List<BattleStageThemeProfile>();

        int changed = 0;
        foreach (Sprite sprite in LoadSprites(BattleRoot + "/Backgrounds"))
        {
            string themeName = sprite.name;
            BattleStageThemeProfile theme =
                FindOrCreateTheme(database.themes, themeName);
            if (string.IsNullOrWhiteSpace(theme.themeName))
            {
                theme.themeName = themeName;
                changed++;
            }

            if (theme.background != sprite)
            {
                theme.background = sprite;
                changed++;
            }

            Sprite midground = LoadSprite(
                BattleRoot + "/Backgrounds/" + themeName + "_Midground");
            if (theme.midground != midground)
            {
                theme.midground = midground;
                changed++;
            }

            Sprite foreground = LoadSprite(
                BattleRoot + "/Backgrounds/" + themeName + "_Foreground");
            if (theme.foreground != foreground)
            {
                theme.foreground = foreground;
                changed++;
            }
        }

        database.themes.Sort(CompareThemeRules);
        EditorUtility.SetDirty(database);
        return changed;
    }

    private static int LinkCompanions()
    {
        int changed = 0;
        string[] guids = AssetDatabase.FindAssets("t:CharacterData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterData character =
                AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (character == null ||
                string.IsNullOrWhiteSpace(character.characterName))
            {
                continue;
            }

            string assetName = ToAssetName(character.characterName);
            BattleActorVisualSet visual = BuildVisualSet(
                BattleRoot + "/Companions/" + assetName,
                BattleRoot + "/Companions",
                assetName);
            if (!BattleActorVisualSet.IsConfigured(visual))
                continue;

            character.battleVisual = visual;
            if (character.icon == null && visual.sprite != null)
                character.icon = visual.sprite;

            EditorUtility.SetDirty(character);
            changed++;
        }

        return changed;
    }

    private static int LinkProfiles(
        List<BattleVisualProfile> profiles,
        string root)
    {
        if (profiles == null)
            return 0;

        int changed = 0;
        foreach (string folder in GetChildFolders(root))
        {
            string profileName = Path.GetFileName(folder);
            BattleActorVisualSet visual =
                BuildVisualSet(folder, root, profileName);
            if (!BattleActorVisualSet.IsConfigured(visual))
                continue;

            BattleVisualProfile profile =
                FindOrCreateProfile(profiles, profileName);
            profile.profileName = profileName;
            NormalizeProfile(profile, profileName);
            profile.visual = visual;
            changed++;
        }

        foreach (Sprite sprite in LoadSprites(root))
        {
            string profileName = sprite.name;
            BattleActorVisualSet visual =
                BuildVisualSet(root + "/" + profileName, root, profileName);
            if (!BattleActorVisualSet.IsConfigured(visual))
                continue;

            BattleVisualProfile profile =
                FindOrCreateProfile(profiles, profileName);
            profile.profileName = profileName;
            NormalizeProfile(profile, profileName);
            profile.visual = visual;
            changed++;
        }

        profiles.Sort(
            CompareProfileRules);
        return changed;
    }

    private static BattleActorVisualSet BuildVisualSet(
        string folder,
        string flatRoot,
        string assetName)
    {
        BattleActorVisualSet visual = new BattleActorVisualSet
        {
            sprite = LoadSprite(
                folder + "/" + assetName,
                flatRoot + "/" + assetName),
            animatorController = LoadAnimator(
                folder + "/" + assetName,
                flatRoot + "/" + assetName),
            basicProjectile = new BattleProjectileVisual
            {
                sprite = LoadSprite(
                    folder + "/BasicProjectile",
                    flatRoot + "/" + assetName + "_BasicProjectile",
                    BattleRoot + "/Projectiles/" +
                    assetName + "_BasicProjectile")
            },
            skillProjectile = new BattleProjectileVisual
            {
                sprite = LoadSprite(
                    folder + "/SkillProjectile",
                    flatRoot + "/" + assetName + "_SkillProjectile",
                    BattleRoot + "/Projectiles/" +
                    assetName + "_SkillProjectile")
            }
        };

        foreach (BattleAnimationCue cue in
                 Enum.GetValues(typeof(BattleAnimationCue)))
        {
            Sprite[] frames = LoadAnimationFrames(folder + "/" + cue);
            if (frames == null || frames.Length == 0)
                continue;

            visual.spriteAnimations.Add(new BattleSpriteAnimation
            {
                cue = cue,
                frames = frames
            });
        }

        return visual;
    }

    private static BattleVisualProfile FindOrCreateProfile(
        List<BattleVisualProfile> profiles,
        string profileName)
    {
        BattleVisualProfile profile = profiles.Find(
            item => item != null &&
                item.profileName == profileName);
        if (profile != null)
            return profile;

        profile = new BattleVisualProfile
        {
            profileName = profileName
        };
        profiles.Add(profile);
        return profile;
    }

    private static BattleStageThemeProfile FindOrCreateTheme(
        List<BattleStageThemeProfile> themes,
        string themeName)
    {
        BattleStageThemeProfile theme = themes.Find(
            item => item != null &&
                item.themeName == themeName);
        if (theme != null)
            return theme;

        theme = new BattleStageThemeProfile
        {
            themeName = themeName
        };
        themes.Add(theme);
        return theme;
    }

    private static int CreateSampleProfiles(
        List<BattleVisualProfile> profiles,
        string root,
        BattleVisualSampleProfile[] samples)
    {
        if (profiles == null || samples == null)
            return 0;

        int changed = 0;
        foreach (BattleVisualSampleProfile sample in samples)
        {
            BattleVisualProfile profile =
                FindOrCreateProfile(profiles, sample.profileName);
            changed += ApplySampleProfile(profile, sample);
            if (EnsureActorFolders(root, sample.profileName))
                changed++;
        }

        profiles.Sort(
            CompareProfileRules);
        return changed;
    }

    private static int CreateSampleThemeProfilesInternal()
    {
        BattleStageThemeDatabase database = LoadOrCreateThemeDatabase();
        if (database.themes == null)
            database.themes = new List<BattleStageThemeProfile>();

        int changed = 0;
        foreach (BattleStageThemeSample sample in SampleThemes)
        {
            BattleStageThemeProfile theme =
                FindOrCreateTheme(database.themes, sample.themeName);
            changed += ApplyThemeSample(theme, sample);
        }

        database.themes.Sort(CompareThemeRules);
        EditorUtility.SetDirty(database);
        return changed;
    }

    private static int CreateCharacterArtFoldersInternal()
    {
        int changed = 0;
        string[] guids = AssetDatabase.FindAssets("t:CharacterData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterData character =
                AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (character == null ||
                string.IsNullOrWhiteSpace(character.characterName))
            {
                continue;
            }

            string assetName = ToAssetName(character.characterName);
            if (EnsureActorFolders(
                    BattleRoot + "/Companions",
                    assetName))
            {
                changed++;
            }
        }

        return changed;
    }

    private static void AppendCharacterReports(List<string> lines)
    {
        lines.Add("Companions");
        string[] guids = AssetDatabase.FindAssets("t:CharacterData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterData character =
                AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (character == null ||
                string.IsNullOrWhiteSpace(character.characterName))
            {
                continue;
            }

            AppendActorReport(
                lines,
                character.rarity,
                BattleRoot + "/Companions",
                ToAssetName(character.characterName));
        }

        lines.Add(string.Empty);
    }

    private static void AppendProfileReports(
        List<string> lines,
        string label,
        string root,
        List<BattleVisualProfile> profiles)
    {
        lines.Add(label + " Profiles");
        if (profiles == null || profiles.Count == 0)
        {
            lines.Add("- none");
            lines.Add(string.Empty);
            return;
        }

        foreach (BattleVisualProfile profile in profiles)
        {
            if (profile == null ||
                string.IsNullOrWhiteSpace(profile.profileName))
            {
                lines.Add("- missing profile name");
                continue;
            }

            AppendActorReport(
                lines,
                label,
                root,
                profile.profileName);
        }

        lines.Add(string.Empty);
    }

    private static void AppendStageThemeReport(List<string> lines)
    {
        BattleStageThemeDatabase database = LoadOrCreateThemeDatabase();
        lines.Add("Stage Themes");
        if (database.themes == null || database.themes.Count == 0)
        {
            lines.Add("- none");
            lines.Add(string.Empty);
            return;
        }

        foreach (BattleStageThemeProfile theme in database.themes)
        {
            if (theme == null)
                continue;

            lines.Add(
                "- " + theme.themeName +
                " | stage " + Mathf.Max(1, theme.stageFrom) +
                (theme.stageTo > 0 ? "-" + theme.stageTo : "+") +
                " | background: " +
                FormatReady(theme.background != null) +
                " | midground: " +
                FormatReady(theme.midground != null) +
                " | foreground: " +
                FormatReady(theme.foreground != null));
        }

        lines.Add(string.Empty);
    }

    private static void AppendActorReport(
        List<string> lines,
        string label,
        string root,
        string assetName)
    {
        string folder = root + "/" + assetName;
        bool mainSprite =
            LoadSprite(folder + "/" + assetName, root + "/" + assetName) !=
            null;
        bool basicProjectile =
            LoadSprite(
                folder + "/BasicProjectile",
                root + "/" + assetName + "_BasicProjectile",
                BattleRoot + "/Projectiles/" +
                assetName + "_BasicProjectile") != null;
        bool skillProjectile =
            LoadSprite(
                folder + "/SkillProjectile",
                root + "/" + assetName + "_SkillProjectile",
                BattleRoot + "/Projectiles/" +
                assetName + "_SkillProjectile") != null;
        int animationFolderCount = CountAnimationFoldersWithSprites(folder);

        lines.Add(
            "- " + label + " " + assetName +
            " | sprite: " + FormatReady(mainSprite) +
            " | animations: " + animationFolderCount + "/5" +
            " | basic projectile: " + FormatReady(basicProjectile) +
            " | skill projectile: " + FormatReady(skillProjectile));
    }

    private static int CountAnimationFoldersWithSprites(string folder)
    {
        int count = 0;
        foreach (BattleAnimationCue cue in
                 Enum.GetValues(typeof(BattleAnimationCue)))
        {
            string animationFolder = folder + "/" + cue;
            if (!AssetDatabase.IsValidFolder(animationFolder))
                continue;

            string[] guids = AssetDatabase.FindAssets(
                "t:Sprite",
                new[] { animationFolder });
            if (guids != null && guids.Length > 0)
                count++;
        }

        return count;
    }

    private static string FormatReady(bool ready)
    {
        return ready ? "ready" : "missing";
    }

    private static int ApplySampleProfile(
        BattleVisualProfile profile,
        BattleVisualSampleProfile sample)
    {
        if (profile == null)
            return 0;

        bool changed = false;
        changed |= SetIfDifferent(
            ref profile.profileName,
            sample.profileName);
        changed |= SetIfDifferent(
            ref profile.stageFrom,
            sample.stageFrom);
        changed |= SetIfDifferent(
            ref profile.stageTo,
            sample.stageTo);
        changed |= SetIfDifferent(
            ref profile.stageCycle,
            0);
        changed |= SetIfDifferent(
            ref profile.stageCycleOffset,
            0);
        changed |= SetIfDifferent(
            ref profile.priority,
            0);
        return changed ? 1 : 0;
    }

    private static int ApplyThemeSample(
        BattleStageThemeProfile theme,
        BattleStageThemeSample sample)
    {
        if (theme == null)
            return 0;

        bool changed = false;
        changed |= SetIfDifferent(
            ref theme.themeName,
            sample.themeName);
        changed |= SetIfDifferent(
            ref theme.stageFrom,
            sample.stageFrom);
        changed |= SetIfDifferent(
            ref theme.stageTo,
            sample.stageTo);
        changed |= SetIfDifferent(
            ref theme.priority,
            0);
        return changed ? 1 : 0;
    }

    private static int NormalizeProfiles(
        List<BattleVisualProfile> profiles,
        string fallbackPrefix)
    {
        if (profiles == null)
            return 0;

        int changed = 0;
        for (int index = 0; index < profiles.Count; index++)
        {
            changed += NormalizeProfile(
                profiles[index],
                fallbackPrefix + (index + 1).ToString("00"));
        }

        profiles.Sort(
            CompareProfileRules);
        return changed;
    }

    private static int CompareProfileRules(
        BattleVisualProfile left,
        BattleVisualProfile right)
    {
        if (ReferenceEquals(left, right))
            return 0;
        if (left == null)
            return 1;
        if (right == null)
            return -1;

        int stageCompare =
            Mathf.Max(1, left.stageFrom)
                .CompareTo(Mathf.Max(1, right.stageFrom));
        if (stageCompare != 0)
            return stageCompare;

        int priorityCompare = right.priority.CompareTo(left.priority);
        if (priorityCompare != 0)
            return priorityCompare;

        return string.Compare(
            left.profileName,
            right.profileName,
            StringComparison.Ordinal);
    }

    private static int CompareThemeRules(
        BattleStageThemeProfile left,
        BattleStageThemeProfile right)
    {
        if (ReferenceEquals(left, right))
            return 0;
        if (left == null)
            return 1;
        if (right == null)
            return -1;

        int stageCompare =
            Mathf.Max(1, left.stageFrom)
                .CompareTo(Mathf.Max(1, right.stageFrom));
        if (stageCompare != 0)
            return stageCompare;

        int priorityCompare = right.priority.CompareTo(left.priority);
        if (priorityCompare != 0)
            return priorityCompare;

        return string.Compare(
            left.themeName,
            right.themeName,
            StringComparison.Ordinal);
    }

    private static int NormalizeProfile(
        BattleVisualProfile profile,
        string fallbackName)
    {
        if (profile == null)
            return 0;

        bool changed = false;
        if (string.IsNullOrWhiteSpace(profile.profileName) &&
            profile.HasVisual)
        {
            profile.profileName = fallbackName;
            changed = true;
        }

        if (profile.stageFrom <= 0)
        {
            profile.stageFrom = 1;
            changed = true;
        }

        if (profile.stageTo > 0 && profile.stageTo < profile.stageFrom)
        {
            profile.stageTo = profile.stageFrom;
            changed = true;
        }

        if (profile.stageCycle < 0)
        {
            profile.stageCycle = 0;
            changed = true;
        }

        if (profile.stageCycle <= 1 && profile.stageCycleOffset != 0)
        {
            profile.stageCycleOffset = 0;
            changed = true;
        }
        else if (profile.stageCycle > 1)
        {
            int offset =
                Mathf.Clamp(
                    profile.stageCycleOffset,
                    0,
                    profile.stageCycle - 1);
            if (offset != profile.stageCycleOffset)
            {
                profile.stageCycleOffset = offset;
                changed = true;
            }
        }

        return changed ? 1 : 0;
    }

    private static BattleVisualDatabase LoadOrCreateDatabase()
    {
        BattleVisualDatabase database =
            AssetDatabase.LoadAssetAtPath<BattleVisualDatabase>(DatabasePath);
        if (database != null)
            return database;

        EnsureFolder("Assets/Resources");
        database = ScriptableObject.CreateInstance<BattleVisualDatabase>();
        AssetDatabase.CreateAsset(database, DatabasePath);
        return database;
    }

    private static BattleStageThemeDatabase LoadOrCreateThemeDatabase()
    {
        BattleStageThemeDatabase database =
            AssetDatabase.LoadAssetAtPath<BattleStageThemeDatabase>(
                ThemeDatabasePath);
        if (database != null)
            return database;

        EnsureFolder("Assets/Resources");
        database = ScriptableObject.CreateInstance<BattleStageThemeDatabase>();
        AssetDatabase.CreateAsset(database, ThemeDatabasePath);
        return database;
    }

    private static Sprite LoadSprite(params string[] resourceFreePaths)
    {
        foreach (string path in resourceFreePaths)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                path + ".png");
            if (sprite != null)
                return sprite;
        }

        return null;
    }

    private static RuntimeAnimatorController LoadAnimator(
        params string[] resourceFreePaths)
    {
        foreach (string path in resourceFreePaths)
        {
            RuntimeAnimatorController controller =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    path + ".controller");
            if (controller != null)
                return controller;
        }

        return null;
    }

    private static Sprite[] LoadAnimationFrames(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            return Array.Empty<Sprite>();

        List<Sprite> frames = new List<Sprite>();
        string[] guids = AssetDatabase.FindAssets(
            "t:Sprite",
            new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                frames.Add(sprite);
        }

        frames.Sort(
            (left, right) => string.CompareOrdinal(
                left.name,
                right.name));
        return frames.ToArray();
    }

    private static IEnumerable<Sprite> LoadSprites(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            yield break;

        string[] guids = AssetDatabase.FindAssets(
            "t:Sprite",
            new[] { folder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetDirectoryName(path)?.Replace("\\", "/") != folder)
                continue;

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                yield return sprite;
        }
    }

    private static IEnumerable<string> GetChildFolders(string root)
    {
        if (!AssetDatabase.IsValidFolder(root))
            yield break;

        string[] guids = AssetDatabase.FindAssets("", new[] { root });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path == root || !AssetDatabase.IsValidFolder(path))
                continue;

            if (Path.GetDirectoryName(path)?.Replace("\\", "/") == root)
                yield return path;
        }
    }

    private static bool EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return false;

        string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
        if (string.IsNullOrEmpty(parent))
            return false;

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        return true;
    }

    private static bool EnsureActorFolders(string root, string profileName)
    {
        bool changed = false;
        string actorRoot = root + "/" + profileName;
        changed |= EnsureFolder(actorRoot);
        foreach (BattleAnimationCue cue in
                 Enum.GetValues(typeof(BattleAnimationCue)))
        {
            changed |= EnsureFolder(actorRoot + "/" + cue);
        }

        return changed;
    }

    private static bool SetIfDifferent(ref string current, string value)
    {
        if (current == value)
            return false;

        current = value;
        return true;
    }

    private static bool SetIfDifferent(ref int current, int value)
    {
        if (current == value)
            return false;

        current = value;
        return true;
    }

    private static string ToAssetName(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars())
            value = value.Replace(invalid.ToString(), "");

        return value.Replace(" ", "");
    }

    private readonly struct BattleVisualSampleProfile
    {
        public readonly string profileName;
        public readonly int stageFrom;
        public readonly int stageTo;

        public BattleVisualSampleProfile(
            string profileName,
            int stageFrom,
            int stageTo)
        {
            this.profileName = profileName;
            this.stageFrom = stageFrom;
            this.stageTo = stageTo;
        }
    }

    private readonly struct BattleStageThemeSample
    {
        public readonly string themeName;
        public readonly int stageFrom;
        public readonly int stageTo;

        public BattleStageThemeSample(
            string themeName,
            int stageFrom,
            int stageTo)
        {
            this.themeName = themeName;
            this.stageFrom = stageFrom;
            this.stageTo = stageTo;
        }
    }
}
