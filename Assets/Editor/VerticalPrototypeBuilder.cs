using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VerticalPrototypeBuilder
{
    private const string MainScenePath =
        "Assets/Scenes/MainGameScene.unity";
    private const string GachaScenePath =
        "Assets/Scenes/VerticalGachaScene.unity";

    [MenuItem("Tools/Build Vertical Prototype Scene")]
    public static void Build()
    {
        BuildMainScene();
        BuildGachaScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainScenePath, true),
            new EditorBuildSettingsScene(GachaScenePath, true)
        };

        PlayerSettings.defaultInterfaceOrientation =
            UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "[Prototype] Vertical main and gacha scenes created.");
    }

    private static void BuildMainScene()
    {
        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        GameObject bootstrap = new GameObject("MainGameBootstrap");
        bootstrap.AddComponent<MainGameBootstrap>();

        CreateCamera();
        EditorSceneManager.SaveScene(scene, MainScenePath);
    }

    private static void BuildGachaScene()
    {
        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        CharacterDatabase database =
            AssetDatabase.LoadAssetAtPath<CharacterDatabase>(
                "Assets/Characters/CharacterDatabase.asset");

        if (database == null)
        {
            throw new MissingReferenceException(
                "CharacterDatabase.asset is missing.");
        }

        GameObject systems = new GameObject("GachaManager");
        GachaManager gachaManager =
            systems.AddComponent<GachaManager>();
        gachaManager.database = database;

        GameObject interfaceObject =
            new GameObject("VerticalGachaUI");
        interfaceObject.AddComponent<VerticalGachaUI>();

        CreateCamera();
        EditorSceneManager.SaveScene(scene, GachaScenePath);
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject(
            "Main Camera",
            typeof(Camera),
            typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(20, 28, 45, 255);
        camera.orthographic = true;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }
}
