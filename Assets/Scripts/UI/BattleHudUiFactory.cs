using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class BattleHudUiFactory
{
    public static RectTransform CreateHealthBar(
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

    public static RectTransform CreateBadge(
        Button button,
        string name,
        Color color)
    {
        if (button == null)
            return null;

        RectTransform badge = RuntimeUiFactory.CreatePanel(
            name,
            button.transform,
            color,
            new Vector2(0.74f, 0.68f),
            new Vector2(0.98f, 0.98f));
        badge.GetComponent<Image>().raycastTarget = false;
        RuntimeUiFactory.CreateText(
            "BadgeText",
            badge,
            "!",
            22,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center,
            Color.white);
        badge.SetAsLastSibling();
        badge.gameObject.SetActive(false);
        return badge;
    }

    public static void SetBar(
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

    public static void SetBar(
        RectTransform fill,
        float current,
        float maximum)
    {
        if (fill == null)
            return;

        float ratio = maximum <= 0f
            ? 0f
            : Mathf.Clamp01(current / maximum);
        fill.anchorMax = new Vector2(ratio, 1f);
    }

    public static void SetAnchoredPoint(
        RectTransform target,
        RectTransform parent,
        Vector2 normalizedPoint)
    {
        if (target == null || parent == null)
            return;

        Rect rect = parent.rect;
        target.anchorMin = Vector2.zero;
        target.anchorMax = Vector2.zero;
        target.anchoredPosition = new Vector2(
            rect.width * normalizedPoint.x,
            rect.height * normalizedPoint.y);
    }

    public static void UpdateFloatingPopup(
        RectTransform popup,
        TMP_Text text,
        float timer,
        float duration,
        Vector2 floatOffset)
    {
        if (popup == null)
            return;

        bool active = timer > 0f;
        popup.gameObject.SetActive(active);
        if (!active)
            return;

        float progress = 1f - Mathf.Clamp01(timer / duration);
        popup.anchoredPosition = Vector2.Lerp(
            Vector2.zero,
            floatOffset,
            progress);
        popup.localScale =
            Vector3.one * Mathf.Lerp(1.18f, 0.92f, progress);

        if (text == null)
            return;

        Color textColor = text.color;
        textColor.a = Mathf.Lerp(1f, 0f, progress);
        text.color = textColor;
    }
}
