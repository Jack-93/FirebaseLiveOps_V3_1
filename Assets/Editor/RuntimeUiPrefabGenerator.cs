using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class RuntimeUiPrefabGenerator
{
    private const string PrefabFolder = "Assets/Resources/Prefabs/UI";

    [MenuItem("Tools/UI/Regenerate Runtime UI Prefabs")]
    public static void RegenerateAll()
    {
        Directory.CreateDirectory(PrefabFolder);

        CompanionManager companionManager = CreateCompanionManager();

        SavePrefab(
            "WorldBackdrop",
            root => new WorldBackdropUI(root, 1, false).GameObject);
        SavePrefab(
            "TopBar",
            root => new TopBarUI(root, null, null, false).GameObject);
        SavePrefab(
            "BottomNavigation",
            root => new BottomNavigationUI(
                root,
                null,
                null,
                null,
                null,
                null,
                false).GameObject);
        SavePrefab(
            "MorePanel",
            root => new MorePanelUI(
                root,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                false).GameObject);
        SavePrefab(
            "CollectionPanel",
            root => new CollectionPanelUI(
                root,
                companionManager,
                null,
                null,
                null,
                null,
                false).GameObject);
        SavePrefab(
            "EquipmentPanel",
            root => new EquipmentPanelUI(
                root,
                null,
                null,
                null,
                false).GameObject);
        SavePrefab(
            "QuestPanel",
            root => new QuestPanelUI(
                root,
                null,
                null,
                null,
                false).GameObject);
        SavePrefab(
            "ShopPanel",
            root => new ShopPanelUI(
                root,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                false).GameObject);
        SavePrefab(
            "EventPanel",
            root => new EventPanelUI(root, null, null, false).GameObject);
        SavePrefab(
            "SettingsPanel",
            root => new SettingsPanelUI(
                root,
                null,
                null,
                null,
                null,
                null,
                null,
                false).GameObject);
        SavePrefab(
            "AccountPanel",
            root => new AccountPanelUI(
                root,
                null,
                null,
                null,
                false).GameObject);
        SavePrefab(
            "TutorialPanel",
            root => new TutorialPanelUI(root, null, false).GameObject);
        SavePrefab(
            "StoryIntroOverlay",
            root => new StoryIntroUI(root, null, null, false).GameObject);
        SavePrefab(
            "OfflineOverlay",
            root => new OfflineRewardUI(root, false).GameObject);
        SavePrefab(
            "ToastPanel",
            root => new ToastUI(root, false).GameObject);
        SavePrefab(
            "TitleOverlay",
            root => new TitleScreenUI(root, null, null, false).GameObject);
        SavePrefab(
            "LoadingOverlay",
            root => new LoadingOverlayUI(root, null, false).GameObject);

        if (companionManager != null)
            UnityEngine.Object.DestroyImmediate(
                companionManager.gameObject);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[UI] Runtime UI prefabs regenerated.");
    }

    private static CompanionManager CreateCompanionManager()
    {
        GameObject managerObject = new GameObject(
            "CompanionManagerPrefabBuild",
            typeof(CompanionManager));
        CompanionManager manager =
            managerObject.GetComponent<CompanionManager>();
        manager.Initialize();
        return manager;
    }

    private static void SavePrefab(
        string prefabName,
        Func<RectTransform, GameObject> build)
    {
        GameObject container = CreateContainer(prefabName + "BuildRoot");
        RectTransform containerRect =
            container.GetComponent<RectTransform>();

        GameObject prefabRoot = build(containerRect);
        if (prefabRoot == null)
        {
            Debug.LogError("[UI] Failed to build prefab: " + prefabName);
            UnityEngine.Object.DestroyImmediate(container);
            return;
        }

        prefabRoot.transform.SetParent(null, false);

        string prefabPath = PrefabFolder + "/" + prefabName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(
            prefabRoot,
            prefabPath,
            out bool success);

        UnityEngine.Object.DestroyImmediate(prefabRoot);
        UnityEngine.Object.DestroyImmediate(container);

        if (!success)
        {
            Debug.LogError(
                "[UI] Failed to regenerate prefab: " + prefabPath);
        }
    }

    private static GameObject CreateContainer(string name)
    {
        GameObject container = new GameObject(
            name,
            typeof(RectTransform));
        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return container;
    }
}
