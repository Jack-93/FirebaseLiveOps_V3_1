using UnityEngine;
using UnityEngine.UI;

public sealed class WorldBackdropUI
{
    private const string DefaultStageMapPrefabResourcePath =
        "Prefabs/Maps/StageMap";

    private RectTransform backdrop;
    private Image backgroundImage;
    private Image farBackgroundImage;
    private Image midgroundImage;
    private Image groundImage;
    private Image foregroundImage;
    private BattleManager battleManager;
    private bool usesStageMapPrefab;
    private string stageMapPrefabPath;
    private int themeIndex = -1;

    public GameObject GameObject => backdrop == null ? null : backdrop.gameObject;

    public WorldBackdropUI(
        RectTransform root,
        int stage,
        BattleManager battle = null,
        bool usePrefab = true)
    {
        battleManager = battle;
        Image rootImage = root.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.color = Color.clear;
            rootImage.raycastTarget = false;
        }

        themeIndex = BattleStageThemeResolver.GetThemeIndex(stage);

        string mapPrefabPath = GetStageMapPrefabPath(stage);
        if (usePrefab &&
            TryInstantiateStageMap(root, mapPrefabPath, out backdrop))
        {
            usesStageMapPrefab = true;
            stageMapPrefabPath = mapPrefabPath;
            BindBattlePoleViews();
            return;
        }

        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "WorldBackdrop",
                root,
                out backdrop))
        {
            Bind();
            RefreshLayers(stage);
            BindBattlePoleViews();
            return;
        }

        BuildGenerated(root, stage);
        BindBattlePoleViews();
    }

    public void BuildGenerated(RectTransform root, int stage)
    {
        backdrop = RuntimeUiFactory.CreatePanel(
            "WorldBackdrop",
            root,
            Color.clear,
            Vector2.zero,
            Vector2.one);
        Image backdropImage = backdrop.GetComponent<Image>();
        if (backdropImage != null)
            backdropImage.raycastTarget = false;

        backgroundImage = CreateWorldLayer(
            "WorldBackgroundLayer",
            backdrop,
            BattleStageThemeResolver.GetStageBackground(stage),
            BattleStageThemeResolver.GetFallbackColor(stage));
        farBackgroundImage = CreateWorldLayer(
            "WorldFarBackgroundLayer",
            backdrop,
            BattleStageThemeResolver.GetStageFarBackground(stage),
            Color.clear);
        midgroundImage = CreateWorldLayer(
            "WorldMidgroundLayer",
            backdrop,
            BattleStageThemeResolver.GetStageMidground(stage),
            Color.clear);
        groundImage = CreateWorldLayer(
            "WorldGroundLayer",
            backdrop,
            BattleStageThemeResolver.GetStageGround(stage),
            Color.clear);
        foregroundImage = CreateWorldLayer(
            "WorldForegroundLayer",
            backdrop,
            BattleStageThemeResolver.GetStageForeground(stage),
            Color.clear);
    }

    public void Refresh(int stage)
    {
        if (usesStageMapPrefab)
        {
            RefreshStageMapPrefab(stage);
            return;
        }

        int nextThemeIndex = BattleStageThemeResolver.GetThemeIndex(stage);
        if (nextThemeIndex == themeIndex)
            return;

        themeIndex = nextThemeIndex;
        RefreshLayers(stage);
    }

    private void Bind()
    {
        Image backdropImage = backdrop.GetComponent<Image>();
        if (backdropImage != null)
        {
            backdropImage.color = Color.clear;
            backdropImage.raycastTarget = false;
        }

        backgroundImage =
            RuntimeUiBinder.FindImage(backdrop, "WorldBackgroundLayer");
        farBackgroundImage = FindOrCreateWorldLayer(
            "WorldFarBackgroundLayer",
            1);
        midgroundImage =
            RuntimeUiBinder.FindImage(backdrop, "WorldMidgroundLayer");
        groundImage = FindOrCreateWorldLayer("WorldGroundLayer", 3);
        foregroundImage =
            RuntimeUiBinder.FindImage(backdrop, "WorldForegroundLayer");
        SetLayerOrder();
    }

    private void RefreshLayers(int stage)
    {
        SetWorldLayer(
            backgroundImage,
            BattleStageThemeResolver.GetStageBackground(stage),
            BattleStageThemeResolver.GetFallbackColor(stage));
        SetWorldLayer(
            farBackgroundImage,
            BattleStageThemeResolver.GetStageFarBackground(stage),
            Color.clear);
        SetWorldLayer(
            midgroundImage,
            BattleStageThemeResolver.GetStageMidground(stage),
            Color.clear);
        SetWorldLayer(
            groundImage,
            BattleStageThemeResolver.GetStageGround(stage),
            Color.clear);
        SetWorldLayer(
            foregroundImage,
            BattleStageThemeResolver.GetStageForeground(stage),
            Color.clear);
    }

    private Image FindOrCreateWorldLayer(string name, int siblingIndex)
    {
        Image image = RuntimeUiBinder.FindImage(backdrop, name);
        if (image != null)
            return image;

        image = CreateWorldLayer(name, backdrop, null, Color.clear);
        image.transform.SetSiblingIndex(siblingIndex);
        return image;
    }

    private void SetLayerOrder()
    {
        SetSibling(backgroundImage, 0);
        SetSibling(farBackgroundImage, 1);
        SetSibling(midgroundImage, 2);
        SetSibling(groundImage, 3);
        SetSibling(foregroundImage, 4);
    }

    private static void SetSibling(Image image, int index)
    {
        if (image != null)
            image.transform.SetSiblingIndex(index);
    }

    private static Image CreateWorldLayer(
        string name,
        RectTransform root,
        Sprite sprite,
        Color fallbackColor)
    {
        RectTransform layer = RuntimeUiFactory.CreatePanel(
            name,
            root,
            sprite == null ? fallbackColor : Color.white,
            Vector2.zero,
            Vector2.one);
        Image image = layer.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.raycastTarget = false;
        return image;
    }

    private static void SetWorldLayer(
        Image image,
        Sprite sprite,
        Color fallbackColor)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.color = sprite == null ? fallbackColor : Color.white;
        image.raycastTarget = false;
    }

    private static bool TryInstantiateStageMap(
        RectTransform root,
        string prefabPath,
        out RectTransform rect)
    {
        rect = null;
        if (root == null || string.IsNullOrWhiteSpace(prefabPath))
            return false;

        GameObject prefab =
            Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
            return false;

        GameObject instance = Object.Instantiate(prefab, root, false);
        instance.name = "StageMap";
        rect = instance.GetComponent<RectTransform>();
        if (rect == null)
            return false;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsFirstSibling();

        foreach (Graphic graphic in
            rect.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
        }

        return true;
    }

    private void RefreshStageMapPrefab(int stage)
    {
        string nextPrefabPath = GetStageMapPrefabPath(stage);
        if (nextPrefabPath == stageMapPrefabPath)
            return;

        RectTransform parent = backdrop == null
            ? null
            : backdrop.parent as RectTransform;
        if (parent == null)
            return;

        if (!TryInstantiateStageMap(parent, nextPrefabPath, out RectTransform next))
            return;

        UnbindBattlePoleViews();
        Object.Destroy(backdrop.gameObject);
        backdrop = next;
        stageMapPrefabPath = nextPrefabPath;
        themeIndex = BattleStageThemeResolver.GetThemeIndex(stage);
        BindBattlePoleViews();
    }

    private static string GetStageMapPrefabPath(int stage)
    {
        string configuredPath =
            BattleStageThemeResolver.GetStageMapPrefabPath(stage);
        return string.IsNullOrWhiteSpace(configuredPath)
            ? DefaultStageMapPrefabResourcePath
            : configuredPath;
    }

    private void BindBattlePoleViews()
    {
        if (backdrop == null)
            return;

        BattlePoleView[] poleViews =
            backdrop.GetComponentsInChildren<BattlePoleView>(true);
        if (poleViews == null || poleViews.Length == 0)
        {
            CreateDefaultBattlePoleView();
            poleViews = backdrop.GetComponentsInChildren<BattlePoleView>(true);
        }

        foreach (BattlePoleView poleView in poleViews)
            poleView.Bind(battleManager);
    }

    private void UnbindBattlePoleViews()
    {
        if (backdrop == null)
            return;

        foreach (BattlePoleView poleView in
            backdrop.GetComponentsInChildren<BattlePoleView>(true))
        {
            poleView.Unbind();
        }
    }

    private void CreateDefaultBattlePoleView()
    {
        Image poleImage = RuntimeUiFactory.CreateSpriteImage(
            "BattlePole",
            backdrop,
            null,
            new Vector2(0.08f, 0.12f),
            new Vector2(0.34f, 0.5f));
        poleImage.raycastTarget = false;
        poleImage.preserveAspect = true;
        poleImage.gameObject.AddComponent<BattlePoleView>();
        poleImage.transform.SetAsLastSibling();
    }
}
