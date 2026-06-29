using System.IO;
using UnityEditor;
using UnityEngine;

public static class GrowthPanelPrefabGenerator
{
    private const string PrefabFolder = "Assets/Resources/Prefabs/UI";
    private const string PrefabPath = PrefabFolder + "/GrowthPanel.prefab";

    [MenuItem("Tools/UI/Regenerate Growth Panel Prefab")]
    public static void Regenerate()
    {
        Directory.CreateDirectory(PrefabFolder);

        GameObject container = new GameObject(
            "GrowthPanelPrefabBuildRoot",
            typeof(RectTransform));
        RectTransform containerRect =
            container.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        GrowthPanelUI growthPanel = new GrowthPanelUI(
            containerRect,
            null,
            null,
            null,
            false);
        GameObject panelObject = growthPanel.GameObject;
        panelObject.transform.SetParent(null, false);

        PrefabUtility.SaveAsPrefabAsset(
            panelObject,
            PrefabPath,
            out bool success);

        Object.DestroyImmediate(panelObject);
        Object.DestroyImmediate(container);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!success)
        {
            Debug.LogError(
                "[UI] Failed to regenerate Growth Panel prefab: " +
                PrefabPath);
            return;
        }

        Debug.Log("[UI] Growth Panel prefab regenerated: " + PrefabPath);
    }
}
