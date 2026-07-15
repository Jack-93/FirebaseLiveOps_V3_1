using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleVisualDatabase))]
public sealed class BattleVisualDatabaseEditor : Editor
{
    private SerializedProperty heroProperty;
    private SerializedProperty normalEnemiesProperty;
    private SerializedProperty bossesProperty;
    private int previewStage = 1;
    private bool showSummary = true;

    private void OnEnable()
    {
        heroProperty = serializedObject.FindProperty("hero");
        normalEnemiesProperty = serializedObject.FindProperty("normalEnemies");
        bossesProperty = serializedObject.FindProperty("bosses");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        BattleVisualDatabase database =
            (BattleVisualDatabase)target;

        DrawTools();
        DrawPreview(database);
        DrawSummary(database);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(
            "Raw Data",
            EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(heroProperty, true);
        EditorGUILayout.PropertyField(normalEnemiesProperty, true);
        EditorGUILayout.PropertyField(bossesProperty, true);

        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawTools()
    {
        EditorGUILayout.LabelField(
            "Battle Art Tools",
            EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Put production art under Assets/Art/Battle, then run Auto Link Visuals.",
            MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Sync Pipeline"))
                BattleArtPipelineTools.SyncBattleArtPipeline();

            if (GUILayout.Button("Prepare Folders"))
                BattleArtPipelineTools.PrepareProductionArtFolders();

            if (GUILayout.Button("Auto Link Visuals"))
                BattleArtPipelineTools.AutoLinkBattleVisuals();

            if (GUILayout.Button("Normalize Rules"))
                BattleArtPipelineTools.NormalizeVisualStageRules();
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Create Sample Stage Profiles"))
                BattleArtPipelineTools.CreateSampleStageProfiles();

            if (GUILayout.Button("Create Sample Stage Themes"))
                BattleArtPipelineTools.CreateSampleStageThemes();
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Create Character Art Folders"))
                BattleArtPipelineTools.CreateCharacterArtFolders();

            if (GUILayout.Button("Write Art Readiness Report"))
                BattleArtPipelineTools.WriteArtReadinessReport();
        }
    }

    private void DrawPreview(BattleVisualDatabase database)
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(
            "Stage Rule Preview",
            EditorStyles.boldLabel);

        previewStage = EditorGUILayout.IntSlider(
            "Preview Stage",
            previewStage,
            1,
            500);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            DrawResolvedProfile(
                "Normal Enemy",
                database.GetEnemy(previewStage, false));
            DrawResolvedProfile(
                "Boss",
                database.GetEnemy(previewStage, true));
        }
    }

    private static void DrawResolvedProfile(
        string label,
        BattleVisualProfile profile)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(
                label,
                GUILayout.Width(110f));

            if (profile == null)
            {
                EditorGUILayout.LabelField("No matching profile");
                return;
            }

            EditorGUILayout.LabelField(
                string.IsNullOrWhiteSpace(profile.profileName)
                    ? "(unnamed)"
                    : profile.profileName);
            EditorGUILayout.LabelField(
                FormatStageRule(profile),
                EditorStyles.miniLabel,
                GUILayout.Width(230f));
        }
    }

    private void DrawSummary(BattleVisualDatabase database)
    {
        EditorGUILayout.Space(8f);
        showSummary = EditorGUILayout.Foldout(
            showSummary,
            "Profile Summary",
            true);
        if (!showSummary)
            return;

        DrawProfileCard("Hero", database.hero);
        DrawProfileList(
            "Normal Enemies",
            database.normalEnemies);
        DrawProfileList(
            "Bosses",
            database.bosses);
    }

    private static void DrawProfileList(
        string title,
        List<BattleVisualProfile> profiles)
    {
        EditorGUILayout.LabelField(
            title,
            EditorStyles.boldLabel);

        if (profiles == null || profiles.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No profiles. Add production art, then run Auto Link Visuals.",
                MessageType.None);
            return;
        }

        for (int index = 0; index < profiles.Count; index++)
        {
            DrawProfileCard(
                (index + 1) + ". " + title,
                profiles[index]);
        }
    }

    private static void DrawProfileCard(
        string title,
        BattleVisualProfile profile)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            if (profile == null)
            {
                EditorGUILayout.LabelField(title, "Missing profile");
                return;
            }

            BattleActorVisualSet visual = profile.Resolve();
            Sprite sprite = visual?.sprite ?? profile.sprite;
            Texture2D preview = sprite == null
                ? null
                : AssetPreview.GetAssetPreview(sprite) ??
                  AssetPreview.GetMiniThumbnail(sprite);
            Texture previewTexture =
                preview == null ? Texture2D.grayTexture : preview;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(
                    previewTexture,
                    GUILayout.Width(56f),
                    GUILayout.Height(56f));

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(
                        string.IsNullOrWhiteSpace(profile.profileName)
                            ? title + "  (unnamed)"
                            : title + "  " + profile.profileName,
                        EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(
                        FormatStageRule(profile),
                        EditorStyles.miniLabel);
                    EditorGUILayout.LabelField(
                        FormatVisualState(profile, visual),
                        EditorStyles.miniLabel);
                }
            }
        }
    }

    private static string FormatStageRule(BattleVisualProfile profile)
    {
        if (profile == null)
            return string.Empty;

        string range = "Stage " + Mathf.Max(1, profile.stageFrom);
        range += profile.stageTo > 0
            ? "-" + profile.stageTo
            : "+";

        string cycle = profile.stageCycle > 1
            ? " / cycle " + profile.stageCycle +
              " offset " + profile.stageCycleOffset
            : string.Empty;

        string priority = profile.priority != 0
            ? " / priority " + profile.priority
            : string.Empty;

        return range + cycle + priority;
    }

    private static string FormatVisualState(
        BattleVisualProfile profile,
        BattleActorVisualSet visual)
    {
        bool hasActor =
            visual != null && visual.HasActorVisual;
        bool hasBasicProjectile =
            visual?.basicProjectile != null &&
            visual.basicProjectile.HasSprite;
        bool hasSkillProjectile =
            visual?.skillProjectile != null &&
            visual.skillProjectile.HasSprite;
        int animationCount =
            visual?.spriteAnimations == null
                ? 0
                : visual.spriteAnimations.Count;

        return
            "actor " + (hasActor ? "OK" : "missing") +
            " / animations " + animationCount +
            " / basic projectile " +
            (hasBasicProjectile ? "OK" : "missing") +
            " / skill projectile " +
            (hasSkillProjectile ? "OK" : "missing") +
            " / legacy " +
            (profile.sprite != null || profile.animatorController != null
                ? "yes"
                : "no");
    }
}
