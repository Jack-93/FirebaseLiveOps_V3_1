using System.IO;
using UnityEditor;
using UnityEngine;

public static class BattleHudPrefabGenerator
{
    private const string PrefabFolder = "Assets/Resources/Prefabs/UI";
    private const string PrefabPath = PrefabFolder + "/BattleHud.prefab";

    [MenuItem("Tools/UI/Regenerate Battle HUD Prefab")]
    public static void Regenerate()
    {
        Directory.CreateDirectory(PrefabFolder);

        GameObject container = new GameObject(
            "BattleHudPrefabBuildRoot",
            typeof(RectTransform));
        RectTransform containerRect =
            container.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        BattleHudUI battleHud = new BattleHudUI(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        RectTransform panel = battleHud.BuildGenerated(containerRect);
        panel.SetParent(null, false);

        PrefabUtility.SaveAsPrefabAsset(
            panel.gameObject,
            PrefabPath,
            out bool success);

        Object.DestroyImmediate(panel.gameObject);
        Object.DestroyImmediate(container);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!success)
        {
            Debug.LogError(
                "[UI] Failed to regenerate Battle HUD prefab: " +
                PrefabPath);
            return;
        }

        Debug.Log("[UI] Battle HUD prefab regenerated: " + PrefabPath);
    }
}
