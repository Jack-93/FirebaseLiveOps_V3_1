using System.IO;
using UnityEditor;
using UnityEngine;

public static class GachaPanelPrefabGenerator
{
    private const string PrefabFolder = "Assets/Resources/Prefabs/UI";
    private const string PrefabPath = PrefabFolder + "/GachaPanel.prefab";

    [MenuItem("Tools/UI/Regenerate Gacha Panel Prefab")]
    public static void Regenerate()
    {
        Directory.CreateDirectory(PrefabFolder);

        GameObject container = new GameObject(
            "GachaPanelPrefabBuildRoot",
            typeof(RectTransform));
        RectTransform containerRect =
            container.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        GachaPanelUI gachaPanel = new GachaPanelUI(
            containerRect,
            null,
            null,
            false);
        GameObject panelObject = gachaPanel.GameObject;
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
                "[UI] Failed to regenerate Gacha Panel prefab: " +
                PrefabPath);
            return;
        }

        Debug.Log("[UI] Gacha Panel prefab regenerated: " + PrefabPath);
    }
}
