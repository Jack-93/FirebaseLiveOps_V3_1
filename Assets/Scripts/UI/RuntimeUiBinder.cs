using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class RuntimeUiBinder
{
    private const string UiPrefabRoot = "Prefabs/UI/";

    public static bool TryInstantiatePrefab(
        string prefabName,
        RectTransform parent,
        out RectTransform rect)
    {
        rect = null;
        if (string.IsNullOrWhiteSpace(prefabName) || parent == null)
            return false;

        GameObject prefab =
            Resources.Load<GameObject>(UiPrefabRoot + prefabName);
        if (prefab == null)
            return false;

        GameObject instance = Object.Instantiate(prefab, parent, false);
        instance.name = prefabName;
        rect = instance.GetComponent<RectTransform>();
        GameFont.ApplyToHierarchy(rect);
        return rect != null;
    }

    public static SpriteNumberText BindNumber(
        Transform root,
        string name,
        string resourceRoot,
        float characterHeight)
    {
        return new SpriteNumberText(
            FindRect(root, name),
            resourceRoot,
            characterHeight);
    }

    public static RectTransform FindRect(Transform root, string name)
    {
        Transform transform = FindTransform(root, name);
        return transform == null
            ? null
            : transform.GetComponent<RectTransform>();
    }

    public static RectTransform FindChildRect(Transform root, string name)
    {
        if (root == null)
            return null;

        for (int index = 0; index < root.childCount; index++)
        {
            Transform child = root.GetChild(index);
            if (child.name == name)
                return child.GetComponent<RectTransform>();
        }

        return null;
    }

    public static RectTransform FindProgressFill(
        Transform root,
        string progressBarName)
    {
        RectTransform progressBar = FindRect(root, progressBarName);
        return FindChildRect(progressBar, "Fill");
    }

    public static TMP_Text FindText(Transform root, string name)
    {
        Transform transform = FindTransform(root, name);
        TMP_Text text = transform == null
            ? null
            : transform.GetComponent<TMP_Text>();
        GameFont.Apply(text, name);
        return text;
    }

    public static Image FindImage(Transform root, string name)
    {
        Transform transform = FindTransform(root, name);
        return transform == null
            ? null
            : transform.GetComponent<Image>();
    }

    public static Button FindButton(Transform root, string name)
    {
        Transform transform = FindTransform(root, name);
        if (transform == null)
            return null;

        GameFont.ApplyToHierarchy(transform);
        return transform.GetComponent<Button>();
    }

    public static Transform FindTransform(Transform root, string name)
    {
        if (root == null || string.IsNullOrEmpty(name))
            return null;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(
            true))
        {
            if (child.name == name)
                return child;
        }

        return null;
    }

    public static void ReplaceButtonAction(
        Button button,
        UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(
            () => AudioManager.Instance?.PlayButtonClick());
        if (action != null)
            button.onClick.AddListener(action);
    }
}
