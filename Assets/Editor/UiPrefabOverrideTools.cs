using UnityEditor;
using UnityEngine;

public static class UiPrefabOverrideTools
{
    private const string UiPrefabFolder = "Assets/Resources/Prefabs/UI/";

    [MenuItem("Tools/UI/Apply Selected Preview UI Override To Prefab")]
    public static void ApplySelectedPreviewOverride()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("[UI] Select a UI prefab instance first.");
            return;
        }

        GameObject prefabRoot =
            PrefabUtility.GetOutermostPrefabInstanceRoot(selected);
        if (prefabRoot == null)
        {
            Debug.LogWarning(
                "[UI] Selected object is not a prefab instance.");
            return;
        }

        Object source = PrefabUtility.GetCorrespondingObjectFromSource(
            prefabRoot);
        string assetPath = AssetDatabase.GetAssetPath(source);
        if (string.IsNullOrEmpty(assetPath) ||
            !assetPath.StartsWith(UiPrefabFolder))
        {
            Debug.LogWarning(
                "[UI] Selected prefab is not under " + UiPrefabFolder);
            return;
        }

        PrefabUtility.ApplyPrefabInstance(
            prefabRoot,
            InteractionMode.UserAction);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[UI] Applied preview override to prefab: " + assetPath);
    }

    [MenuItem("Tools/UI/Ping Runtime UI Prefab Folder")]
    public static void PingRuntimeUiPrefabFolder()
    {
        Object folder = AssetDatabase.LoadAssetAtPath<Object>(
            UiPrefabFolder.TrimEnd('/'));
        if (folder == null)
        {
            Debug.LogWarning("[UI] Runtime UI prefab folder is missing.");
            return;
        }

        EditorGUIUtility.PingObject(folder);
        Selection.activeObject = folder;
    }
}
