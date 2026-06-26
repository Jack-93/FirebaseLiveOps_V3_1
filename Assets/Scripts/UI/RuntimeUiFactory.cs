using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class RuntimeUiFactory
{
    public static RectTransform CreatePanel(
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

        if (PrototypeUiArt.ShouldDecoratePanel(name))
        {
            Image frame = CreateSpriteImage(
                "PanelArt",
                rect,
                PrototypeUiArt.PanelFrame,
                Vector2.zero,
                Vector2.one);
            frame.type = Image.Type.Sliced;
            frame.preserveAspect = false;
            frame.fillCenter = false;
        }
        return rect;
    }

    public static Image CreateSpriteImage(
        string name,
        Transform parent,
        Sprite sprite,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject imageObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = sprite == null ? Color.clear : Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    public static TMP_Text CreateText(
        string name,
        Transform parent,
        string value,
        float fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax,
        TextAlignmentOptions alignment,
        Color color)
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text =
            textObject.GetComponent<TextMeshProUGUI>();
        text.text = LocalizationManager.Translate(value);
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = Mathf.Max(12f, fontSize * 0.58f);
        text.lineSpacing = -8f;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        GameFont.Apply(text);
        return text;
    }

    public static Button CreateButton(
        string name,
        Transform parent,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        UnityAction action)
    {
        RectTransform rect = CreatePanel(
            name,
            parent,
            color,
            anchorMin,
            anchorMax);

        Button button = rect.gameObject.AddComponent<Button>();
        button.onClick.AddListener(
            () => AudioManager.Instance?.PlayButtonClick());
        button.onClick.AddListener(action);

        bool isSkillButton = PrototypeUiArt.IsSkillButton(name);
        Image art = CreateSpriteImage(
            "ButtonArt",
            rect,
            isSkillButton
                ? PrototypeUiArt.SkillFrame
                : PrototypeUiArt.ButtonNormal,
            Vector2.zero,
            Vector2.one);
        art.type = isSkillButton
            ? Image.Type.Simple
            : Image.Type.Sliced;
        art.preserveAspect = isSkillButton;
        rect.GetComponent<Image>().color = Color.clear;
        button.targetGraphic = art;

        CreateText(
            "Label",
            rect,
            label,
            27,
            new Vector2(0.05f, 0.04f),
            new Vector2(0.95f, 0.93f),
            TextAlignmentOptions.Center,
            Color.white);
        return button;
    }
}
