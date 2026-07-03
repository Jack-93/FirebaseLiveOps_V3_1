using UnityEditor;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UiPrefabOverrideTools
{
    private const string UiPrefabFolder = "Assets/Resources/Prefabs/UI/";
    private const string BattleHudPrefabPath =
        UiPrefabFolder + "BattleHud.prefab";
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);
    private const string BattleHudLayoutUpgradeSessionKey =
        "FirebaseLiveOps.BattleHudLayoutUpgrade.20260703V2";

    [InitializeOnLoadMethod]
    private static void UpgradeBattleHudPrefabLayoutOnce()
    {
        if (SessionState.GetBool(BattleHudLayoutUpgradeSessionKey, false))
            return;

        EditorApplication.delayCall += RunDelayedBattleHudPrefabUpgrade;
    }

    private static void RunDelayedBattleHudPrefabUpgrade()
    {
        if (SessionState.GetBool(BattleHudLayoutUpgradeSessionKey, false))
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += RunDelayedBattleHudPrefabUpgrade;
            return;
        }

        SessionState.SetBool(BattleHudLayoutUpgradeSessionKey, true);
        UpgradeBattleHudPrefabLayout();
    }

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
            "Assets/Resources/Prefabs/UI");
        if (folder == null)
        {
            Debug.LogWarning("[UI] Runtime UI prefab folder is missing.");
            return;
        }

        EditorGUIUtility.PingObject(folder);
        Selection.activeObject = folder;
    }

    [MenuItem("Tools/UI/Upgrade Battle Hud Prefab Layout")]
    public static void UpgradeBattleHudPrefabLayout()
    {
        GameObject prefab = PrefabUtility.LoadPrefabContents(
            BattleHudPrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("[UI] BattleHud prefab is missing.");
            return;
        }

        try
        {
            RectTransform root = prefab.GetComponent<RectTransform>();
            if (root == null)
            {
                Debug.LogWarning("[UI] BattleHud prefab has no RectTransform.");
                return;
            }

            UpgradeBattleHudRoot(root);
            GameFont.ApplyToHierarchy(root);
            PrefabUtility.SaveAsPrefabAsset(prefab, BattleHudPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[UI] BattleHud prefab layout upgraded.");
    }

    [MenuItem("Tools/UI/Remove Obsolete Battle Slot Pads")]
    public static void RemoveObsoleteBattleSlotPads()
    {
        GameObject prefab = PrefabUtility.LoadPrefabContents(
            BattleHudPrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("[UI] BattleHud prefab is missing.");
            return;
        }

        string[] obsoleteNames =
        {
            "EnemySlotPad",
            "SupportChargePad",
            "CompanionSlotPad1",
            "CompanionSlotPad2",
            "CompanionSlotPad3"
        };

        int removed = 0;
        foreach (string obsoleteName in obsoleteNames)
        {
            Transform child = FindDescendant(prefab.transform, obsoleteName);
            if (child == null)
                continue;

            Object.DestroyImmediate(child.gameObject);
            removed++;
        }

        PrefabUtility.SaveAsPrefabAsset(prefab, BattleHudPrefabPath);
        PrefabUtility.UnloadPrefabContents(prefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[UI] Removed obsolete Battle slot pads: " + removed);
    }

    private static Transform FindDescendant(
        Transform root,
        string childName)
    {
        if (root == null)
            return null;

        if (root.name == childName)
            return root;

        foreach (Transform child in root)
        {
            Transform match = FindDescendant(child, childName);
            if (match != null)
                return match;
        }

        return null;
    }

    private static RectTransform FindRect(Transform root, string childName)
    {
        Transform transform = FindDescendant(root, childName);
        return transform == null
            ? null
            : transform.GetComponent<RectTransform>();
    }

    private static Button FindButton(Transform root, string childName)
    {
        Transform transform = FindDescendant(root, childName);
        return transform == null
            ? null
            : transform.GetComponent<Button>();
    }

    private static TMP_Text FindText(Transform root, string childName)
    {
        Transform transform = FindDescendant(root, childName);
        return transform == null
            ? null
            : transform.GetComponent<TMP_Text>();
    }

    private static void UpgradeBattleHudRoot(RectTransform root)
    {
        RectTransform enemyCard = FindRect(root, "EnemyCard");
        SetAnchors(
            enemyCard,
            new Vector2(0.02f, 0.34f),
            new Vector2(0.98f, 0.91f));

        RectTransform controlPanel = FindRect(root, "BattleControlCard") ??
            RuntimeUiFactory.CreatePanel(
                "BattleControlCard",
                root,
                new Color32(63, 48, 36, 245),
                new Vector2(0.02f, 0.02f),
                new Vector2(0.98f, 0.325f));
        SetAnchors(
            controlPanel,
            new Vector2(0.02f, 0.02f),
            new Vector2(0.98f, 0.325f));
        controlPanel.SetAsLastSibling();

        EnsureText(
            controlPanel,
            "PowerChargeHeader",
            "\uC804\uB825 \uCDA9\uC804",
            20f,
            new Vector2(0.04f, 0.86f),
            new Vector2(0.36f, 0.98f));
        EnsureText(
            controlPanel,
            "SkillHeader",
            "\uCE90\uB9AD\uD130 \uC2A4\uD0AC",
            20f,
            new Vector2(0.42f, 0.86f),
            new Vector2(0.96f, 0.98f));
        RectTransform divider = FindRect(controlPanel, "BattleControlDivider") ??
            RuntimeUiFactory.CreatePanel(
                "BattleControlDivider",
                controlPanel,
                new Color32(22, 18, 16, 180),
                new Vector2(0.385f, 0.08f),
                new Vector2(0.395f, 0.92f));
        SetAnchors(
            divider,
            new Vector2(0.385f, 0.08f),
            new Vector2(0.395f, 0.92f));

        Button powerButton = FindButton(root, "PowerChargeButton") ??
            RuntimeUiFactory.CreateButton(
                "PowerChargeButton",
                controlPanel,
                "CHARGE POWER",
                new Vector2(0.04f, 0.16f),
                new Vector2(0.36f, 0.88f),
                Success,
                EmptyAction);
        ReparentAndAnchor(
            powerButton.GetComponent<RectTransform>(),
            controlPanel,
            new Vector2(0.04f, 0.16f),
            new Vector2(0.36f, 0.88f));
        ConfigureButtonText(powerButton, 22f);
        EnsurePowerNumberRoots(root, powerButton.transform);

        TMP_Text skillStatus = FindText(root, "SkillStatus") ??
            RuntimeUiFactory.CreateText(
                "SkillStatus",
                controlPanel,
                "\uB3D9\uB8CC \uC2A4\uD0AC",
                21f,
                new Vector2(0.41f, 0.76f),
                new Vector2(0.96f, 0.86f),
                TextAlignmentOptions.Right,
                Gold);
        ReparentAndAnchor(
            skillStatus.GetComponent<RectTransform>(),
            controlPanel,
            new Vector2(0.41f, 0.76f),
            new Vector2(0.96f, 0.86f));

        for (int slot = 0; slot < 3; slot++)
            EnsureSkillButton(root, controlPanel, slot);
    }

    private static void EnsureSkillButton(
        RectTransform searchRoot,
        RectTransform controlPanel,
        int slot)
    {
        string buttonName = "CompanionSkillButton" + (slot + 1);
        float left = 0.42f + slot * 0.18f;
        Button button = FindButton(searchRoot, buttonName) ??
            RuntimeUiFactory.CreateButton(
                buttonName,
                controlPanel,
                "S" + (slot + 1),
                new Vector2(left, 0.16f),
                new Vector2(left + 0.16f, 0.78f),
                PanelLight,
                EmptyAction);
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        ReparentAndAnchor(
            buttonRect,
            controlPanel,
            new Vector2(left, 0.16f),
            new Vector2(left + 0.16f, 0.78f));

        ConfigureButtonText(button, 16f);

        Image portrait = FindImage(buttonRect, "Portrait") ??
            RuntimeUiFactory.CreateSpriteImage(
                "Portrait",
                buttonRect,
                null,
                new Vector2(0.13f, 0.34f),
                new Vector2(0.87f, 0.9f));
        SetAnchors(
            portrait.GetComponent<RectTransform>(),
            new Vector2(0.13f, 0.34f),
            new Vector2(0.87f, 0.9f));

        RectTransform cooldown = FindRect(buttonRect, "CooldownOverlay") ??
            RuntimeUiFactory.CreatePanel(
                "CooldownOverlay",
                buttonRect,
                new Color32(5, 8, 16, 145),
                Vector2.zero,
                Vector2.one);
        SetAnchors(cooldown, Vector2.zero, Vector2.one);

        Image glow = FindImage(buttonRect, "ReadyGlow") ??
            RuntimeUiFactory.CreateSpriteImage(
                "ReadyGlow",
                buttonRect,
                PrototypeUiArt.SkillFrame,
                new Vector2(-0.08f, -0.08f),
                new Vector2(1.08f, 1.08f));
        SetAnchors(
            glow.GetComponent<RectTransform>(),
            new Vector2(-0.08f, -0.08f),
            new Vector2(1.08f, 1.08f));
        glow.transform.SetAsLastSibling();

        RectTransform skillNumber =
            FindRect(buttonRect, "SkillStateNumberText") ??
            RuntimeUiFactory.CreatePanel(
                "SkillStateNumberText",
                buttonRect,
                new Color32(0, 0, 0, 0),
                new Vector2(0.12f, 0.03f),
                new Vector2(0.88f, 0.28f));
        SetAnchors(
            skillNumber,
            new Vector2(0.12f, 0.03f),
            new Vector2(0.88f, 0.28f));
    }

    private static Image FindImage(Transform root, string childName)
    {
        Transform transform = FindDescendant(root, childName);
        return transform == null
            ? null
            : transform.GetComponent<Image>();
    }

    private static void EnsurePowerNumberRoots(
        Transform searchRoot,
        Transform powerButton)
    {
        EnsureNumberRoot(
            searchRoot,
            powerButton,
            "PowerChargeCurrentNumberText",
            new Vector2(0.08f, 0.12f),
            new Vector2(0.31f, 0.42f));
        EnsureTextFromSearchRoot(
            searchRoot,
            powerButton,
            "PowerChargeSlashText",
            "/",
            18f,
            new Vector2(0.31f, 0.12f),
            new Vector2(0.38f, 0.42f));
        EnsureNumberRoot(
            searchRoot,
            powerButton,
            "PowerChargeMaxNumberText",
            new Vector2(0.38f, 0.12f),
            new Vector2(0.61f, 0.42f));
        EnsureNumberRoot(
            searchRoot,
            powerButton,
            "PowerChargeTapNumberText",
            new Vector2(0.64f, 0.12f),
            new Vector2(0.94f, 0.42f));
    }

    private static TMP_Text EnsureTextFromSearchRoot(
        Transform searchRoot,
        Transform parent,
        string name,
        string value,
        float fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        TMP_Text text = FindText(searchRoot, name) ??
            RuntimeUiFactory.CreateText(
                name,
                parent,
                value,
                fontSize,
                anchorMin,
                anchorMax,
                TextAlignmentOptions.Center,
                Gold);
        ReparentAndAnchor(
            text.GetComponent<RectTransform>(),
            parent,
            anchorMin,
            anchorMax);
        text.text = value;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = Mathf.Max(10f, fontSize * 0.58f);
        text.alignment = TextAlignmentOptions.Center;
        GameFont.Apply(text, name);
        RemoveDuplicateTexts(searchRoot, name, text);
        return text;
    }

    private static void EnsureNumberRoot(
        Transform searchRoot,
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform rect = FindRect(searchRoot, name) ??
            RuntimeUiFactory.CreatePanel(
                name,
                parent,
                new Color32(0, 0, 0, 0),
                anchorMin,
                anchorMax);
        ReparentAndAnchor(rect, parent, anchorMin, anchorMax);
        RemoveDuplicateRects(searchRoot, name, rect);
        rect.GetComponent<Image>().raycastTarget = false;
    }

    private static TMP_Text EnsureText(
        Transform parent,
        string name,
        string value,
        float fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        TMP_Text text = FindText(parent, name) ??
            RuntimeUiFactory.CreateText(
                name,
                parent,
                value,
                fontSize,
                anchorMin,
                anchorMax,
                TextAlignmentOptions.Center,
                Gold);
        ReparentAndAnchor(
            text.GetComponent<RectTransform>(),
            parent,
            anchorMin,
            anchorMax);
        text.text = value;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = Mathf.Max(10f, fontSize * 0.58f);
        text.alignment = TextAlignmentOptions.Center;
        GameFont.Apply(text, name);
        return text;
    }

    private static void ConfigureButtonText(Button button, float maxFontSize)
    {
        if (button == null)
            return;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text == null)
            return;

        RectTransform rect = text.GetComponent<RectTransform>();
        if (button.name == "PowerChargeButton")
        {
            SetAnchors(
                rect,
                new Vector2(0.08f, 0.48f),
                new Vector2(0.92f, 0.94f));
        }
        else
        {
            SetAnchors(
                rect,
                new Vector2(0.06f, 0.03f),
                new Vector2(0.94f, 0.32f));
        }

        text.fontSizeMax = maxFontSize;
        text.fontSizeMin = Mathf.Max(10f, maxFontSize * 0.58f);
        text.alignment = TextAlignmentOptions.Center;
        GameFont.Apply(text, button.name);
        text.transform.SetAsLastSibling();
    }

    private static void ReparentAndAnchor(
        RectTransform rect,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        if (rect == null || parent == null)
            return;

        if (rect.parent != parent)
            rect.SetParent(parent, false);

        SetAnchors(rect, anchorMin, anchorMax);
    }

    private static void SetAnchors(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        if (rect == null)
            return;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void RemoveDuplicateRects(
        Transform root,
        string name,
        RectTransform keep)
    {
        if (root == null || keep == null)
            return;

        RectTransform[] rects =
            root.GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform rect in rects)
        {
            if (rect == keep || rect.name != name)
                continue;

            Object.DestroyImmediate(rect.gameObject);
        }
    }

    private static void RemoveDuplicateTexts(
        Transform root,
        string name,
        TMP_Text keep)
    {
        if (root == null || keep == null)
            return;

        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text == keep || text.name != name)
                continue;

            Object.DestroyImmediate(text.gameObject);
        }
    }

    private static void EmptyAction()
    {
    }
}
