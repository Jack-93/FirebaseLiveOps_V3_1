using UnityEngine;
using UnityEngine.UI;

public sealed class WorldBackdropUI
{
    private const string DefaultStageMapPrefabResourcePath =
        "Prefabs/Maps/StageMap";

    private RectTransform root;
    private RectTransform backdrop;
    private string stageMapPrefabPath;

    public GameObject GameObject => backdrop == null ? null : backdrop.gameObject;

    public WorldBackdropUI(
        RectTransform root,
        int stage)
    {
        this.root = root;
        Image rootImage = root.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.color = Color.clear;
            rootImage.raycastTarget = false;
        }

        string mapPrefabPath = GetStageMapPrefabPath(stage);
        if (TryInstantiateStageMap(root, mapPrefabPath, out backdrop))
        {
            stageMapPrefabPath = mapPrefabPath;
            return;
        }

        Debug.LogError("StageMap prefab could not be loaded: " +
            mapPrefabPath);
    }

    public void Refresh(int stage)
    {
        RefreshStageMapPrefab(stage);
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
            ? root
            : backdrop.parent as RectTransform;
        if (parent == null)
            return;

        if (!TryInstantiateStageMap(parent, nextPrefabPath, out RectTransform next))
            return;

        Object.Destroy(backdrop.gameObject);
        backdrop = next;
        stageMapPrefabPath = nextPrefabPath;
    }

    private static string GetStageMapPrefabPath(int stage)
    {
        string configuredPath =
            BattleStageThemeResolver.GetStageMapPrefabPath(stage);
        return string.IsNullOrWhiteSpace(configuredPath)
            ? DefaultStageMapPrefabResourcePath
            : configuredPath;
    }

}
