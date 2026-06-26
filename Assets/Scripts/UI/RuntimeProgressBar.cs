using UnityEngine;

public static class RuntimeProgressBar
{
    public static RectTransform Create(
        RectTransform parent,
        string name,
        Color fillColor,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform background = RuntimeUiFactory.CreatePanel(
            name,
            parent,
            new Color32(12, 18, 30, 255),
            anchorMin,
            anchorMax);

        return RuntimeUiFactory.CreatePanel(
            "Fill",
            background,
            fillColor,
            Vector2.zero,
            Vector2.one);
    }

    public static void Set(
        RectTransform fill,
        int current,
        int maximum)
    {
        if (fill == null)
            return;

        float ratio = maximum <= 0
            ? 0f
            : Mathf.Clamp01(current / (float)maximum);
        fill.anchorMax = new Vector2(ratio, 1f);
    }
}
