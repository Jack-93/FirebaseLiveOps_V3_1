using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class UiPreviewSceneBuilder
{
    private const string PreviewScenePath =
        "Assets/Scenes/UiPrefabPreviewScene.unity";
    private const string IndividualPreviewSceneFolder =
        "Assets/Scenes/UIPreviews";

    [MenuItem("Tools/UI/Open UI Preview Scene")]
    public static void Open()
    {
        OpenScene(PreviewScenePath, "UI preview scene");
    }

    [MenuItem("Tools/UI/Open Battle UI Preview Scene")]
    public static void OpenBattlePreview()
    {
        OpenIndividualPreview("00 Battle");
    }

    [MenuItem("Tools/UI/Open Gacha UI Preview Scene")]
    public static void OpenGachaPreview()
    {
        OpenIndividualPreview("01 Gacha");
    }

    [MenuItem("Tools/UI/Open Growth UI Preview Scene")]
    public static void OpenGrowthPreview()
    {
        OpenIndividualPreview("02 Growth");
    }

    [MenuItem("Tools/UI/Open More UI Preview Scene")]
    public static void OpenMorePreview()
    {
        OpenIndividualPreview("03 More");
    }

    [MenuItem("Tools/UI/Open Collection UI Preview Scene")]
    public static void OpenCollectionPreview()
    {
        OpenIndividualPreview("04 Collection");
    }

    [MenuItem("Tools/UI/Open Equipment UI Preview Scene")]
    public static void OpenEquipmentPreview()
    {
        OpenIndividualPreview("05 Equipment");
    }

    [MenuItem("Tools/UI/Open Quest UI Preview Scene")]
    public static void OpenQuestPreview()
    {
        OpenIndividualPreview("06 Quest");
    }

    [MenuItem("Tools/UI/Open Shop UI Preview Scene")]
    public static void OpenShopPreview()
    {
        OpenIndividualPreview("07 Shop");
    }

    [MenuItem("Tools/UI/Open Event UI Preview Scene")]
    public static void OpenEventPreview()
    {
        OpenIndividualPreview("08 Event");
    }

    [MenuItem("Tools/UI/Open Settings UI Preview Scene")]
    public static void OpenSettingsPreview()
    {
        OpenIndividualPreview("09 Settings");
    }

    [MenuItem("Tools/UI/Open Account UI Preview Scene")]
    public static void OpenAccountPreview()
    {
        OpenIndividualPreview("10 Account");
    }

    [MenuItem("Tools/UI/Open Story Intro UI Preview Scene")]
    public static void OpenStoryIntroPreview()
    {
        OpenIndividualPreview("11 Story Intro");
    }

    [MenuItem("Tools/UI/Open Title UI Preview Scene")]
    public static void OpenTitlePreview()
    {
        OpenIndividualPreview("12 Title");
    }

    [MenuItem("Tools/UI/Open Loading UI Preview Scene")]
    public static void OpenLoadingPreview()
    {
        OpenIndividualPreview("13 Loading");
    }

    [MenuItem("Tools/UI/Open Offline Reward UI Preview Scene")]
    public static void OpenOfflineRewardPreview()
    {
        OpenIndividualPreview("14 Offline Reward");
    }

    [MenuItem("Tools/UI/Open Tutorial UI Preview Scene")]
    public static void OpenTutorialPreview()
    {
        OpenIndividualPreview("15 Tutorial");
    }

    [MenuItem("Tools/UI/Open Toast UI Preview Scene")]
    public static void OpenToastPreview()
    {
        OpenIndividualPreview("16 Toast");
    }

    private static void OpenIndividualPreview(string screenName)
    {
        OpenScene(GetIndividualScenePath(screenName), screenName);
    }

    private static void OpenScene(string path, string label)
    {
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        if (scene == null)
        {
            Debug.LogWarning("[UI] Missing " + label + ": " + path);
            return;
        }

        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        Selection.activeObject = scene;
    }

    private static string GetIndividualScenePath(string screenName)
    {
        return IndividualPreviewSceneFolder + "/Preview_" +
            SanitizeSceneName(screenName) + ".unity";
    }

    private static string SanitizeSceneName(string screenName)
    {
        return screenName
            .Replace(" ", "_")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(":", "_");
    }
}
