using UnityEngine;
using UnityEngine.UI;

public sealed class WorldBackdropUI
{
    private readonly Image backgroundImage;
    private readonly Image midgroundImage;
    private readonly Image foregroundImage;
    private int themeIndex = -1;

    private static readonly Color Background =
        new Color32(17, 24, 39, 255);

    public WorldBackdropUI(RectTransform root, int stage)
    {
        Image rootImage = root.GetComponent<Image>();
        rootImage.color = Color.clear;
        rootImage.raycastTarget = false;

        themeIndex = PrototypeBattleArt.GetThemeIndex(stage);
        backgroundImage = CreateWorldLayer(
            "WorldBackgroundLayer",
            root,
            PrototypeBattleArt.GetStageBackground(stage),
            Background);
        midgroundImage = CreateWorldLayer(
            "WorldMidgroundLayer",
            root,
            PrototypeBattleArt.GetStageMidground(stage),
            Color.clear);
        foregroundImage = CreateWorldLayer(
            "WorldForegroundLayer",
            root,
            PrototypeBattleArt.GetStageForeground(stage),
            Color.clear);
    }

    public void Refresh(int stage)
    {
        int nextThemeIndex = PrototypeBattleArt.GetThemeIndex(stage);
        if (nextThemeIndex == themeIndex)
            return;

        themeIndex = nextThemeIndex;
        SetWorldLayer(
            backgroundImage,
            PrototypeBattleArt.GetStageBackground(stage),
            Background);
        SetWorldLayer(
            midgroundImage,
            PrototypeBattleArt.GetStageMidground(stage),
            Color.clear);
        SetWorldLayer(
            foregroundImage,
            PrototypeBattleArt.GetStageForeground(stage),
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
    }
}
