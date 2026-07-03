using UnityEngine;
using UnityEngine.UI;

public sealed class WorldBackdropUI
{
    private RectTransform backdrop;
    private Image backgroundImage;
    private Image midgroundImage;
    private Image foregroundImage;
    private int themeIndex = -1;

    public GameObject GameObject => backdrop == null ? null : backdrop.gameObject;

    public WorldBackdropUI(
        RectTransform root,
        int stage,
        bool usePrefab = true)
    {
        Image rootImage = root.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.color = Color.clear;
            rootImage.raycastTarget = false;
        }

        themeIndex = BattleStageThemeResolver.GetThemeIndex(stage);

        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "WorldBackdrop",
                root,
                out backdrop))
        {
            Bind();
            RefreshLayers(stage);
            return;
        }

        BuildGenerated(root, stage);
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
        midgroundImage = CreateWorldLayer(
            "WorldMidgroundLayer",
            backdrop,
            BattleStageThemeResolver.GetStageMidground(stage),
            Color.clear);
        foregroundImage = CreateWorldLayer(
            "WorldForegroundLayer",
            backdrop,
            BattleStageThemeResolver.GetStageForeground(stage),
            Color.clear);
    }

    public void Refresh(int stage)
    {
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
        midgroundImage =
            RuntimeUiBinder.FindImage(backdrop, "WorldMidgroundLayer");
        foregroundImage =
            RuntimeUiBinder.FindImage(backdrop, "WorldForegroundLayer");
    }

    private void RefreshLayers(int stage)
    {
        SetWorldLayer(
            backgroundImage,
            BattleStageThemeResolver.GetStageBackground(stage),
            BattleStageThemeResolver.GetFallbackColor(stage));
        SetWorldLayer(
            midgroundImage,
            BattleStageThemeResolver.GetStageMidground(stage),
            Color.clear);
        SetWorldLayer(
            foregroundImage,
            BattleStageThemeResolver.GetStageForeground(stage),
            Color.clear);
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
}
