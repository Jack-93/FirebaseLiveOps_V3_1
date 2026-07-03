using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UiPreviewSceneBuilder
{
    private const string PreviewScenePath =
        "Assets/Scenes/UiPrefabPreviewScene.unity";
    private const string IndividualPreviewSceneFolder =
        "Assets/Scenes/UIPreviews";
    private const string PrefabRoot = "Assets/Resources/Prefabs/UI/";
    private const float ScreenWidth = 1080f;
    private const float ScreenHeight = 1920f;
    private const float ScreenGapX = 140f;
    private const float ScreenGapY = 220f;
    private const float CanvasScale = 0.01f;
    private const int Columns = 3;

    [MenuItem("Tools/UI/Open UI Preview Scene")]
    public static void Open()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PreviewScenePath) == null)
            Build();

        EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Single);
        Selection.activeObject =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(PreviewScenePath);
    }

    [MenuItem("Tools/UI/Rebuild UI Preview Scene")]
    public static void Build()
    {
        UiPrefabOverrideTools.UpgradeBattleHudPrefabLayout();

        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        CreateCamera();
        RectTransform canvas = CreateCanvas();
        CreateGlobalGuide(canvas);

        List<PreviewScreen> screens = GetPreviewScreens();

        for (int index = 0; index < screens.Count; index++)
            BuildScreen(canvas, screens[index], index);

        EditorSceneManager.SaveScene(scene, PreviewScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[UI] Preview scene rebuilt: " + PreviewScenePath);
    }

    [MenuItem("Tools/UI/Rebuild Individual UI Preview Scenes")]
    public static void BuildIndividualScenes()
    {
        UiPrefabOverrideTools.UpgradeBattleHudPrefabLayout();
        EnsureIndividualPreviewFolder();

        List<PreviewScreen> screens = GetPreviewScreens();
        foreach (PreviewScreen screen in screens)
            BuildIndividualScene(screen);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "[UI] Individual preview scenes rebuilt: " +
            IndividualPreviewSceneFolder);
    }

    [MenuItem("Tools/UI/Open Battle UI Preview Scene")]
    public static void OpenBattlePreview()
    {
        UiPrefabOverrideTools.UpgradeBattleHudPrefabLayout();

        string path = GetIndividualScenePath("00 Battle");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            BuildBattlePreviewScene();

        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        Selection.activeObject =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
    }

    [MenuItem("Tools/UI/Fix Battle Preview Now")]
    public static void FixBattlePreviewNow()
    {
        BuildBattlePreviewScene();

        string path = GetIndividualScenePath("00 Battle");
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        Selection.activeObject =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
    }

    [MenuItem("Tools/UI/Rebuild Battle UI Preview Scene")]
    public static void BuildBattlePreviewScene()
    {
        UiPrefabOverrideTools.UpgradeBattleHudPrefabLayout();
        EnsureIndividualPreviewFolder();
        BuildIndividualScene(GetPreviewScreens()[0]);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "[UI] Battle preview scene rebuilt: " +
            GetIndividualScenePath("00 Battle"));
    }

    private static List<PreviewScreen> GetPreviewScreens()
    {
        return new List<PreviewScreen>
        {
            new PreviewScreen(
                "00 Battle",
                "WorldBackdrop",
                "TopBar",
                "BattleHud",
                "TutorialPanel",
                "BottomNavigation",
                "ToastPanel"),
            new PreviewScreen(
                "01 Gacha",
                "WorldBackdrop",
                "TopBar",
                "GachaPanel",
                "BottomNavigation"),
            new PreviewScreen(
                "02 Growth",
                "WorldBackdrop",
                "TopBar",
                "GrowthPanel",
                "BottomNavigation"),
            new PreviewScreen(
                "03 More",
                "WorldBackdrop",
                "TopBar",
                "MorePanel",
                "BottomNavigation"),
            new PreviewScreen(
                "04 Collection",
                "WorldBackdrop",
                "TopBar",
                "CollectionPanel",
                "BottomNavigation"),
            new PreviewScreen(
                "05 Equipment",
                "WorldBackdrop",
                "TopBar",
                "EquipmentPanel",
                "BottomNavigation"),
            new PreviewScreen(
                "06 Quest",
                "WorldBackdrop",
                "TopBar",
                "QuestPanel",
                "BottomNavigation"),
            new PreviewScreen(
                "07 Shop",
                "WorldBackdrop",
                "TopBar",
                "ShopPanel",
                "BottomNavigation"),
            new PreviewScreen(
                "08 Event",
                "WorldBackdrop",
                "TopBar",
                "EventPanel",
                "BottomNavigation"),
            new PreviewScreen(
                "09 Settings",
                "WorldBackdrop",
                "TopBar",
                "SettingsPanel",
                "BottomNavigation"),
            new PreviewScreen(
                "10 Account",
                "WorldBackdrop",
                "TopBar",
                "AccountPanel",
                "BottomNavigation"),
            new PreviewScreen(
                "11 Story Intro",
                "StoryIntroOverlay"),
            new PreviewScreen(
                "12 Title",
                "TitleOverlay"),
            new PreviewScreen(
                "13 Loading",
                "LoadingOverlay"),
            new PreviewScreen(
                "14 Offline Reward",
                "WorldBackdrop",
                "OfflineOverlay"),
        };
    }

    private static RectTransform CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "UiPrefabPreviewCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform rect = canvasObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(
            Columns * (ScreenWidth + ScreenGapX),
            5f * (ScreenHeight + ScreenGapY));
        rect.localScale = Vector3.one * CanvasScale;
        rect.position = Vector3.zero;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution =
            new Vector2(ScreenWidth, ScreenHeight);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;
        return rect;
    }

    private static RectTransform CreateSingleCanvas()
    {
        GameObject canvasObject = new GameObject(
            "UiPrefabPreviewCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform rect = canvasObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(ScreenWidth, ScreenHeight);
        rect.localScale = Vector3.one * CanvasScale;
        rect.position = Vector3.zero;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution =
            new Vector2(ScreenWidth, ScreenHeight);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;
        return rect;
    }

    private static void BuildIndividualScene(PreviewScreen screen)
    {
        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        CreateCamera();
        RectTransform canvas = CreateSingleCanvas();

        RectTransform root = CreatePanel(
            "Preview_" + screen.Name,
            canvas,
            new Color32(5, 8, 14, 255),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f));
        root.sizeDelta = new Vector2(ScreenWidth, ScreenHeight);
        root.pivot = new Vector2(0f, 1f);
        root.anchoredPosition = Vector2.zero;

        CreateLabel(root, screen.Name);
        CreateScreenGuide(root);
        CreateScreenOutline(root);

        foreach (string prefabName in screen.Prefabs)
            AddPrefab(root, prefabName);

        EditorSceneManager.SaveScene(scene, GetIndividualScenePath(screen.Name));
    }

    private static void EnsureIndividualPreviewFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        if (!AssetDatabase.IsValidFolder(IndividualPreviewSceneFolder))
            AssetDatabase.CreateFolder("Assets/Scenes", "UIPreviews");
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

    private static void BuildScreen(
        RectTransform canvas,
        PreviewScreen screen,
        int index)
    {
        int column = index % Columns;
        int row = index / Columns;
        RectTransform root = CreatePanel(
            "Preview_" + screen.Name,
            canvas,
            new Color32(5, 8, 14, 255),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f));
        root.sizeDelta = new Vector2(ScreenWidth, ScreenHeight);
        root.pivot = new Vector2(0f, 1f);
        root.anchoredPosition = new Vector2(
            column * (ScreenWidth + ScreenGapX),
            -row * (ScreenHeight + ScreenGapY));

        CreateLabel(root, screen.Name);
        CreateScreenGuide(root);
        CreateScreenOutline(root);

        foreach (string prefabName in screen.Prefabs)
            AddPrefab(root, prefabName);
    }

    private static void CreateGlobalGuide(RectTransform canvas)
    {
        RectTransform guide = CreatePanel(
            "PreviewGlobalGuide",
            canvas,
            new Color32(8, 12, 22, 210),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f));
        guide.sizeDelta = new Vector2(ScreenWidth, 92f);
        guide.pivot = new Vector2(0f, 1f);
        guide.anchoredPosition = new Vector2(0f, 92f);
        guide.GetComponent<Image>().raycastTarget = false;

        TMP_Text text = CreateGuideText(
            guide,
            "PreviewGlobalGuideText",
            "UI Prefab Preview: select a prefab instance, move/resize it, then use Overrides > Apply All or Tools/UI/Apply Selected Preview UI Override To Prefab.",
            22f,
            TextAlignmentOptions.Center);
        text.color = new Color32(205, 235, 255, 255);
    }

    private static void CreateScreenGuide(RectTransform parent)
    {
        RectTransform guide = CreatePanel(
            "PreviewEditGuide",
            parent,
            new Color32(5, 10, 18, 150),
            new Vector2(0.03f, 0.905f),
            new Vector2(0.97f, 0.955f));
        guide.GetComponent<Image>().raycastTarget = false;

        TMP_Text text = CreateGuideText(
            guide,
            "PreviewEditGuideText",
            "Prefab instance edit area. Battle slot guides are Scene-view gizmos only.",
            18f,
            TextAlignmentOptions.Center);
        text.color = new Color32(170, 225, 255, 255);
    }

    private static void CreateScreenOutline(RectTransform parent)
    {
        Image outlineImage = RuntimeUiFactory.CreateSpriteImage(
            "PreviewScreenOutline",
            parent,
            null,
            Vector2.zero,
            Vector2.one);
        outlineImage.color = new Color32(50, 190, 255, 35);
        Outline outline = outlineImage.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color32(70, 210, 255, 180);
        outline.effectDistance = new Vector2(4f, -4f);
        outlineImage.transform.SetAsFirstSibling();
    }

    private static TMP_Text CreateGuideText(
        RectTransform parent,
        string name,
        string value,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.12f);
        rect.anchorMax = new Vector2(0.98f, 0.88f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = 10f;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        GameFont.Apply(text, name);
        return text;
    }

    private static void AddPrefab(RectTransform parent, string prefabName)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            PrefabRoot + prefabName + ".prefab");
        if (prefab == null)
        {
            Debug.LogWarning("[UI] Preview prefab missing: " + prefabName);
            return;
        }

        GameObject instance =
            PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
        if (instance == null)
            return;

        instance.name = prefabName;
        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect == null)
            return;

        // Keep each prefab's own anchors so preview sizes match runtime.
        rect.localScale = Vector3.one;
    }

    private static RectTransform CreatePanel(
        string name,
        Transform parent,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject panel = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private static void CreateLabel(RectTransform parent, string value)
    {
        GameObject textObject = new GameObject(
            "PreviewLabel",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.965f);
        rect.anchorMax = new Vector2(0.48f, 0.997f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = 28f;
        text.enableAutoSizing = true;
        text.fontSizeMax = 28f;
        text.fontSizeMin = 12f;
        text.alignment = TextAlignmentOptions.Left;
        text.color = new Color32(255, 230, 130, 255);
        text.raycastTarget = false;
        GameFont.Apply(text, "PreviewLabel");
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject(
            "Main Camera",
            typeof(Camera),
            typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(
            ScreenWidth * CanvasScale * 0.5f,
            -ScreenHeight * CanvasScale * 0.5f,
            -10f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = ScreenHeight * CanvasScale * 0.54f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(20, 28, 45, 255);
    }

    private readonly struct PreviewScreen
    {
        public readonly string Name;
        public readonly string[] Prefabs;

        public PreviewScreen(string name, params string[] prefabs)
        {
            Name = name;
            Prefabs = prefabs;
        }
    }
}
