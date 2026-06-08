using UnityEngine;
using UnityEngine.UI;

public sealed class MobileScreenLayout : MonoBehaviour
{
    public const float ReferenceWidth = 1080f;
    public const float ReferenceHeight = 1920f;

    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private int lastScreenWidth;
    private int lastScreenHeight;

    public static RectTransform CreateSafeAreaCanvas(
        string canvasName,
        Color backgroundColor)
    {
        ApplyPortraitSettings();

        GameObject canvasObject = new GameObject(
            canvasName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution =
            new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;

        RectTransform canvasRoot =
            canvasObject.GetComponent<RectTransform>();

        CreateImagePanel(
            "OuterBackground",
            canvasRoot,
            Color.black,
            Vector2.zero,
            Vector2.one);

        RectTransform safeAreaRoot = CreateImagePanel(
            "SafeAreaRoot",
            canvasRoot,
            backgroundColor,
            Vector2.zero,
            Vector2.one);
        safeAreaRoot.gameObject.AddComponent<MobileScreenLayout>();

        return safeAreaRoot;
    }

    public static void ApplyPortraitSettings()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
    }

    public static void ConfigureCamera(Camera camera, Color backgroundColor)
    {
        if (camera == null)
            return;

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;
        camera.orthographic = true;
    }

    public static Camera EnsureMainCamera(Color backgroundColor)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject(
                "Main Camera",
                typeof(Camera),
                typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            camera = cameraObject.GetComponent<Camera>();
        }

        ConfigureCamera(camera, backgroundColor);
        return camera;
    }

    private static RectTransform CreateImagePanel(
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
        panel.GetComponent<Image>().color = color;
        return rect;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea(true);
    }

    private void LateUpdate()
    {
        ApplySafeArea(false);
    }

    private void ApplySafeArea(bool force)
    {
        if (rectTransform == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safeArea = Screen.safeArea;
        if (!force &&
            safeArea == lastSafeArea &&
            Screen.width == lastScreenWidth &&
            Screen.height == lastScreenHeight)
        {
            return;
        }

        lastSafeArea = safeArea;
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
