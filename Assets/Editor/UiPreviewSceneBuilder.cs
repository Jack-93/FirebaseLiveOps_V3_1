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
    private const string PrefabRoot = "Assets/Resources/Prefabs/UI/";
    private const float ScreenWidth = 1080f;
    private const float ScreenHeight = 1920f;
    private const float ScreenGapX = 140f;
    private const float ScreenGapY = 220f;
    private const int Columns = 3;

    [MenuItem("Tools/UI/Rebuild UI Preview Scene")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        CreateCamera();
        RectTransform canvas = CreateCanvas();

        List<PreviewScreen> screens = new List<PreviewScreen>
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

        for (int index = 0; index < screens.Count; index++)
            BuildScreen(canvas, screens[index], index);

        EditorSceneManager.SaveScene(scene, PreviewScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[UI] Preview scene rebuilt: " + PreviewScenePath);
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
        rect.localScale = Vector3.one * 0.01f;
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

        foreach (string prefabName in screen.Prefabs)
            AddPrefab(root, prefabName);
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

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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
        cameraObject.transform.position = new Vector3(16f, -36f, -10f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 16f;
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
