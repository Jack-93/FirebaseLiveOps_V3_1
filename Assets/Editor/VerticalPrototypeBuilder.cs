using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VerticalPrototypeBuilder
{
    private const string MainScenePath =
        "Assets/Scenes/MainGameScene.unity";

    [MenuItem("Tools/Build Vertical Prototype Scene")]
    public static void Build()
    {
        BuildMainScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainScenePath, true)
        };

        PlayerSettings.defaultInterfaceOrientation =
            UIOrientation.Portrait;
        PlayerSettings.defaultScreenWidth =
            (int)MobileScreenLayout.ReferenceWidth;
        PlayerSettings.defaultScreenHeight =
            (int)MobileScreenLayout.ReferenceHeight;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "[Prototype] Vertical main scene created.");
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

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject(
            "Main Camera",
            typeof(Camera),
            typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        MobileScreenLayout.ConfigureCamera(
            camera,
            new Color32(20, 28, 45, 255));
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }
}
