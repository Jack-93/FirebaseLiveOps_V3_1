using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MainGameUI : MonoBehaviour
{
    private MainGameBootstrap bootstrap;
    private BattleManager battleManager;
    private GrowthManager growthManager;
    private TutorialManager tutorialManager;

    private GameObject battlePanel;
    private GameObject growthPanel;
    private GameObject morePanel;
    private GameObject tutorialPanel;
    private GameObject loadingOverlay;
    private GameObject offlineOverlay;
    private GameObject toastPanel;

    private TMP_Text goldText;
    private TMP_Text stageText;
    private TMP_Text powerText;
    private TMP_Text enemyNameText;
    private TMP_Text enemyHealthText;
    private TMP_Text playerHealthText;
    private TMP_Text combatStatusText;
    private TMP_Text attackGrowthText;
    private TMP_Text healthGrowthText;
    private TMP_Text speedGrowthText;
    private TMP_Text inventoryText;
    private TMP_Text accountText;
    private TMP_Text dailyRewardText;
    private TMP_Text tutorialText;
    private TMP_Text tutorialButtonText;
    private TMP_Text loadingText;
    private TMP_Text offlineText;
    private TMP_Text toastText;

    private RectTransform enemyHealthFill;
    private RectTransform playerHealthFill;
    private Button tutorialButton;
    private Button retryButton;
    private float toastTimer;

    private static readonly Color Background =
        new Color32(20, 28, 45, 255);
    private static readonly Color Panel =
        new Color32(37, 49, 73, 245);
    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color Danger =
        new Color32(238, 91, 103, 255);
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);

    public void Configure(
        MainGameBootstrap sessionBootstrap,
        BattleManager battle,
        GrowthManager growth,
        TutorialManager tutorial)
    {
        bootstrap = sessionBootstrap;
        battleManager = battle;
        growthManager = growth;
        tutorialManager = tutorial;

        BuildInterface();
        BindEvents();
        ShowBattle();
    }

    public void RefreshAll()
    {
        RefreshTopBar();
        RefreshBattle();
        RefreshGrowth();
        RefreshMore();
        RefreshTutorial();
    }

    public void SetLoading(bool visible, string message)
    {
        if (loadingOverlay == null)
            return;

        loadingOverlay.SetActive(visible);
        if (loadingText != null)
            loadingText.text = message;

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
    }

    public void ShowInitializationError(string message)
    {
        loadingOverlay.SetActive(true);
        loadingText.text =
            "Connection failed\n\n" + message;
        retryButton.gameObject.SetActive(true);
    }

    public void ShowOfflineReward(long seconds, int gold)
    {
        long minutes = Math.Max(1, seconds / 60);
        offlineText.text =
            $"Welcome back!\n\nAway: {minutes} min\nGold earned: {gold:N0}";
        offlineOverlay.SetActive(true);
    }

    public void ShowToast(string message)
    {
        toastText.text = message;
        toastPanel.SetActive(true);
        toastTimer = 2f;
    }

    private void Update()
    {
        if (toastTimer <= 0f)
            return;

        toastTimer -= Time.unscaledDeltaTime;
        if (toastTimer <= 0f && toastPanel != null)
            toastPanel.SetActive(false);
    }

    private void BuildInterface()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject(
            "MainGameCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 1f;

        RectTransform root =
            canvasObject.GetComponent<RectTransform>();

        CreatePanel(
            "OuterBackground",
            root,
            Color.black,
            Vector2.zero,
            Vector2.one);

        RectTransform portraitRoot = CreatePanel(
            "PortraitViewport",
            root,
            Background,
            Vector2.zero,
            Vector2.one);

        AspectRatioFitter fitter =
            portraitRoot.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 9f / 16f;

        BuildTopBar(portraitRoot);
        BuildBattlePanel(portraitRoot);
        BuildGrowthPanel(portraitRoot);
        BuildMorePanel(portraitRoot);
        BuildBottomNavigation(portraitRoot);
        BuildTutorial(portraitRoot);
        BuildOfflinePopup(portraitRoot);
        BuildToast(portraitRoot);
        BuildLoadingOverlay(portraitRoot);
    }

    private void BuildTopBar(RectTransform root)
    {
        RectTransform top = CreatePanel(
            "TopBar",
            root,
            Panel,
            new Vector2(0f, 0.9f),
            Vector2.one);

        stageText = CreateText(
            "StageText",
            top,
            "Stage 1",
            42,
            new Vector2(0.04f, 0.12f),
            new Vector2(0.34f, 0.88f),
            TextAlignmentOptions.Left);

        goldText = CreateText(
            "GoldText",
            top,
            "Gold 0",
            40,
            new Vector2(0.36f, 0.12f),
            new Vector2(0.68f, 0.88f),
            TextAlignmentOptions.Center,
            Gold);

        powerText = CreateText(
            "PowerText",
            top,
            "Power 0",
            36,
            new Vector2(0.7f, 0.12f),
            new Vector2(0.96f, 0.88f),
            TextAlignmentOptions.Right);
    }

    private void BuildBattlePanel(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "BattlePanel",
            root,
            Background,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));
        battlePanel = panel.gameObject;

        CreateText(
            "BattleTitle",
            panel,
            "AUTO BATTLE",
            48,
            new Vector2(0.05f, 0.9f),
            new Vector2(0.95f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RectTransform enemyCard = CreatePanel(
            "EnemyCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.47f),
            new Vector2(0.94f, 0.88f));

        enemyNameText = CreateText(
            "EnemyName",
            enemyCard,
            "Enemy",
            48,
            new Vector2(0.08f, 0.8f),
            new Vector2(0.92f, 0.96f),
            TextAlignmentOptions.Center);

        RectTransform enemyVisual = CreatePanel(
            "EnemyVisual",
            enemyCard,
            Danger,
            new Vector2(0.3f, 0.28f),
            new Vector2(0.7f, 0.75f));

        CreateText(
            "EnemyGlyph",
            enemyVisual,
            "BOSS",
            46,
            new Vector2(0f, 0f),
            Vector2.one,
            TextAlignmentOptions.Center);

        enemyHealthFill = CreateHealthBar(
            enemyCard,
            "EnemyHealthBar",
            Danger,
            new Vector2(0.08f, 0.12f),
            new Vector2(0.92f, 0.22f));

        enemyHealthText = CreateText(
            "EnemyHealthText",
            enemyCard,
            "0 / 0",
            28,
            new Vector2(0.08f, 0.11f),
            new Vector2(0.92f, 0.23f),
            TextAlignmentOptions.Center);

        RectTransform playerCard = CreatePanel(
            "PlayerCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.2f),
            new Vector2(0.94f, 0.44f));

        CreateText(
            "PlayerName",
            playerCard,
            "HERO",
            42,
            new Vector2(0.06f, 0.65f),
            new Vector2(0.35f, 0.93f),
            TextAlignmentOptions.Left,
            Accent);

        combatStatusText = CreateText(
            "CombatStatus",
            playerCard,
            "Preparing...",
            31,
            new Vector2(0.37f, 0.62f),
            new Vector2(0.94f, 0.93f),
            TextAlignmentOptions.Right);

        playerHealthFill = CreateHealthBar(
            playerCard,
            "PlayerHealthBar",
            Success,
            new Vector2(0.06f, 0.34f),
            new Vector2(0.94f, 0.51f));

        playerHealthText = CreateText(
            "PlayerHealthText",
            playerCard,
            "0 / 0",
            28,
            new Vector2(0.06f, 0.34f),
            new Vector2(0.94f, 0.51f),
            TextAlignmentOptions.Center);

        CreateText(
            "AutoNotice",
            playerCard,
            "Your hero attacks automatically.",
            27,
            new Vector2(0.06f, 0.05f),
            new Vector2(0.94f, 0.27f),
            TextAlignmentOptions.Center,
            new Color32(190, 203, 225, 255));
    }

    private void BuildGrowthPanel(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "GrowthPanel",
            root,
            Background,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));
        growthPanel = panel.gameObject;

        CreateText(
            "GrowthTitle",
            panel,
            "HERO GROWTH",
            48,
            new Vector2(0.05f, 0.88f),
            new Vector2(0.95f, 0.97f),
            TextAlignmentOptions.Center,
            Accent);

        attackGrowthText = CreateUpgradeRow(
            panel,
            "Attack",
            "Increase damage",
            new Vector2(0.06f, 0.64f),
            growthManager.UpgradeAttack);

        healthGrowthText = CreateUpgradeRow(
            panel,
            "Health",
            "Increase maximum HP",
            new Vector2(0.06f, 0.4f),
            growthManager.UpgradeHealth);

        speedGrowthText = CreateUpgradeRow(
            panel,
            "Attack Speed",
            "Attack more frequently",
            new Vector2(0.06f, 0.16f),
            growthManager.UpgradeAttackSpeed);
    }

    private TMP_Text CreateUpgradeRow(
        RectTransform parent,
        string title,
        string description,
        Vector2 anchorMin,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform row = CreatePanel(
            title + "Row",
            parent,
            Panel,
            anchorMin,
            new Vector2(0.94f, anchorMin.y + 0.2f));

        TMP_Text info = CreateText(
            title + "Info",
            row,
            title,
            38,
            new Vector2(0.05f, 0.36f),
            new Vector2(0.66f, 0.9f),
            TextAlignmentOptions.Left);

        CreateText(
            title + "Description",
            row,
            description,
            25,
            new Vector2(0.05f, 0.08f),
            new Vector2(0.66f, 0.38f),
            TextAlignmentOptions.Left,
            new Color32(180, 194, 218, 255));

        CreateButton(
            title + "Button",
            row,
            "UPGRADE",
            new Vector2(0.69f, 0.2f),
            new Vector2(0.95f, 0.8f),
            Accent,
            action);

        return info;
    }

    private void BuildMorePanel(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "MorePanel",
            root,
            Background,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));
        morePanel = panel.gameObject;

        CreateText(
            "MoreTitle",
            panel,
            "PLAYER HUB",
            48,
            new Vector2(0.05f, 0.9f),
            new Vector2(0.95f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RectTransform inventoryCard = CreatePanel(
            "InventoryCard",
            panel,
            Panel,
            new Vector2(0.05f, 0.49f),
            new Vector2(0.95f, 0.87f));

        inventoryText = CreateText(
            "InventoryText",
            inventoryCard,
            "Inventory",
            31,
            new Vector2(0.05f, 0.08f),
            new Vector2(0.95f, 0.92f),
            TextAlignmentOptions.TopLeft);

        RectTransform rewardCard = CreatePanel(
            "RewardCard",
            panel,
            Panel,
            new Vector2(0.05f, 0.25f),
            new Vector2(0.95f, 0.46f));

        dailyRewardText = CreateText(
            "DailyRewardText",
            rewardCard,
            "Daily reward",
            32,
            new Vector2(0.05f, 0.45f),
            new Vector2(0.64f, 0.9f),
            TextAlignmentOptions.Left);

        CreateButton(
            "DailyRewardButton",
            rewardCard,
            "CLAIM",
            new Vector2(0.69f, 0.2f),
            new Vector2(0.95f, 0.8f),
            Success,
            ClaimDailyReward);

        accountText = CreateText(
            "AccountText",
            panel,
            "Account",
            24,
            new Vector2(0.06f, 0.14f),
            new Vector2(0.94f, 0.23f),
            TextAlignmentOptions.Center,
            new Color32(174, 189, 214, 255));

        CreateButton(
            "SaveButton",
            panel,
            "SAVE",
            new Vector2(0.06f, 0.03f),
            new Vector2(0.46f, 0.12f),
            PanelLight,
            HandleSaveAction);

        CreateButton(
            "LogoutButton",
            panel,
            "NEW GUEST",
            new Vector2(0.54f, 0.03f),
            new Vector2(0.94f, 0.12f),
            Danger,
            HandleLogoutAction);
    }

    private void BuildBottomNavigation(RectTransform root)
    {
        RectTransform bottom = CreatePanel(
            "BottomNavigation",
            root,
            Panel,
            Vector2.zero,
            new Vector2(1f, 0.12f));

        CreateButton(
            "BattleNav",
            bottom,
            "BATTLE",
            new Vector2(0.02f, 0.12f),
            new Vector2(0.24f, 0.88f),
            PanelLight,
            ShowBattle);

        CreateButton(
            "GrowthNav",
            bottom,
            "GROWTH",
            new Vector2(0.26f, 0.12f),
            new Vector2(0.48f, 0.88f),
            PanelLight,
            ShowGrowth);

        CreateButton(
            "GachaNav",
            bottom,
            "GACHA",
            new Vector2(0.5f, 0.12f),
            new Vector2(0.72f, 0.88f),
            Accent,
            HandleGachaAction);

        CreateButton(
            "MoreNav",
            bottom,
            "MORE",
            new Vector2(0.74f, 0.12f),
            new Vector2(0.98f, 0.88f),
            PanelLight,
            ShowMore);
    }

    private void BuildTutorial(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "TutorialPanel",
            root,
            new Color32(26, 38, 61, 250),
            new Vector2(0.04f, 0.125f),
            new Vector2(0.96f, 0.235f));
        tutorialPanel = panel.gameObject;

        tutorialText = CreateText(
            "TutorialText",
            panel,
            "Tutorial",
            29,
            new Vector2(0.04f, 0.15f),
            new Vector2(0.72f, 0.85f),
            TextAlignmentOptions.Left);

        tutorialButton = CreateButton(
            "TutorialAction",
            panel,
            "START",
            new Vector2(0.75f, 0.18f),
            new Vector2(0.97f, 0.82f),
            Gold,
            HandleTutorialAction);
        tutorialButtonText =
            tutorialButton.GetComponentInChildren<TMP_Text>();
    }

    private void BuildOfflinePopup(RectTransform root)
    {
        RectTransform overlay = CreatePanel(
            "OfflineOverlay",
            root,
            new Color(0f, 0f, 0f, 0.75f),
            Vector2.zero,
            Vector2.one);
        offlineOverlay = overlay.gameObject;

        RectTransform card = CreatePanel(
            "OfflineCard",
            overlay,
            Panel,
            new Vector2(0.1f, 0.32f),
            new Vector2(0.9f, 0.68f));

        offlineText = CreateText(
            "OfflineText",
            card,
            "Welcome back!",
            42,
            new Vector2(0.08f, 0.3f),
            new Vector2(0.92f, 0.9f),
            TextAlignmentOptions.Center,
            Gold);

        CreateButton(
            "OfflineConfirm",
            card,
            "COLLECT",
            new Vector2(0.22f, 0.08f),
            new Vector2(0.78f, 0.27f),
            Success,
            () => offlineOverlay.SetActive(false));

        offlineOverlay.SetActive(false);
    }

    private void BuildToast(RectTransform root)
    {
        RectTransform toast = CreatePanel(
            "ToastPanel",
            root,
            new Color32(10, 15, 26, 235),
            new Vector2(0.2f, 0.82f),
            new Vector2(0.8f, 0.88f));
        toastPanel = toast.gameObject;

        toastText = CreateText(
            "ToastText",
            toast,
            "",
            29,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center);

        toastPanel.SetActive(false);
    }

    private void BuildLoadingOverlay(RectTransform root)
    {
        RectTransform overlay = CreatePanel(
            "LoadingOverlay",
            root,
            Background,
            Vector2.zero,
            Vector2.one);
        loadingOverlay = overlay.gameObject;

        CreateText(
            "GameTitle",
            overlay,
            "IDLE RPG PROTOTYPE",
            62,
            new Vector2(0.08f, 0.58f),
            new Vector2(0.92f, 0.72f),
            TextAlignmentOptions.Center,
            Accent);

        loadingText = CreateText(
            "LoadingText",
            overlay,
            "Loading...",
            34,
            new Vector2(0.08f, 0.4f),
            new Vector2(0.92f, 0.57f),
            TextAlignmentOptions.Center);

        retryButton = CreateButton(
            "RetryButton",
            overlay,
            "RETRY",
            new Vector2(0.3f, 0.31f),
            new Vector2(0.7f, 0.39f),
            Accent,
            HandleRetryAction);
        retryButton.gameObject.SetActive(false);
    }

    private void BindEvents()
    {
        battleManager.OnBattleStateChanged += RefreshBattle;
        growthManager.OnUpgraded += HandleGrowthUpdated;
        tutorialManager.OnTutorialChanged += RefreshTutorial;

        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnPlayerDataChanged += RefreshAll;
    }

    private void RefreshTopBar()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        goldText.text = $"Gold  {data.gold:N0}";
        stageText.text = $"Stage {data.currentStage}";
        powerText.text =
            $"Power {GameBalance.GetCombatPower(data):N0}";
    }

    private void RefreshBattle()
    {
        if (battleManager == null || !battleManager.IsInitialized)
            return;

        PlayerData data = PlayerDataManager.Instance.playerData;
        enemyNameText.text =
            battleManager.IsBoss
                ? $"STAGE {data.currentStage} BOSS"
                : $"Enemy {data.stageEnemyIndex + 1}/" +
                  $"{GameBalance.EnemiesPerStage - 1}";

        enemyHealthText.text =
            $"{battleManager.EnemyHealth:N0} / " +
            $"{battleManager.EnemyMaxHealth:N0}";
        playerHealthText.text =
            $"{battleManager.PlayerHealth:N0} / " +
            $"{battleManager.PlayerMaxHealth:N0}";

        SetBar(
            enemyHealthFill,
            battleManager.EnemyHealth,
            battleManager.EnemyMaxHealth);
        SetBar(
            playerHealthFill,
            battleManager.PlayerHealth,
            battleManager.PlayerMaxHealth);

        if (!battleManager.IsRunning)
        {
            combatStatusText.text = "Paused";
        }
        else
        {
            combatStatusText.text =
                $"DMG {battleManager.LastPlayerDamage:N0}  " +
                $"ATK {GameBalance.GetPlayerAttack(data):N0}";
        }

        RefreshTopBar();
    }

    private void RefreshGrowth()
    {
        if (growthManager == null)
            return;

        attackGrowthText.text =
            $"Attack Lv.{growthManager.GetLevel(UpgradeType.Attack)}\n" +
            $"Cost {growthManager.GetCost(UpgradeType.Attack):N0}";
        healthGrowthText.text =
            $"Health Lv.{growthManager.GetLevel(UpgradeType.Health)}\n" +
            $"Cost {growthManager.GetCost(UpgradeType.Health):N0}";
        speedGrowthText.text =
            $"Speed Lv.{growthManager.GetLevel(UpgradeType.AttackSpeed)}\n" +
            $"Cost {growthManager.GetCost(UpgradeType.AttackSpeed):N0}";
    }

    private void RefreshMore()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("INVENTORY");
        foreach (var item in data.inventory.items)
            builder.AppendLine($"{item.Key}   x{item.Value}");

        builder.AppendLine();
        builder.AppendLine($"Mailbox   {data.mailbox.Count} waiting");
        builder.AppendLine(
            $"Monsters defeated   {data.totalMonstersDefeated:N0}");
        inventoryText.text = builder.ToString();

        string uid = string.IsNullOrEmpty(data.uid) ? "-" : data.uid;
        string shortUid = uid.Length > 16 ? uid.Substring(0, 16) + "..." : uid;
        accountText.text =
            $"Guest account  {shortUid}  |  Highest {data.highestStage}";

        if (DailyRewardManager.Instance != null)
        {
            int day = DailyRewardManager.Instance.GetNextRewardDay();
            dailyRewardText.text =
                DailyRewardManager.Instance.CanClaimReward()
                    ? $"Daily Reward Day {day} is ready"
                    : $"Daily Reward Day {day} already claimed";
        }
    }

    private void RefreshTutorial()
    {
        if (tutorialManager == null)
            return;

        tutorialPanel.SetActive(!tutorialManager.IsComplete);
        if (tutorialManager.IsComplete)
            return;

        tutorialText.text = tutorialManager.CurrentMessage;

        switch (tutorialManager.CurrentStep)
        {
            case 0:
                tutorialButtonText.text = "START";
                break;
            case 1:
                tutorialButtonText.text = "GROWTH";
                break;
            default:
                tutorialButtonText.text = "BATTLE";
                break;
        }
    }

    private void HandleTutorialAction()
    {
        switch (tutorialManager.CurrentStep)
        {
            case 0:
                tutorialManager.BeginTutorial();
                ShowGrowth();
                break;
            case 1:
                ShowGrowth();
                break;
            default:
                ShowBattle();
                break;
        }
    }

    private void HandleSaveAction()
    {
        bootstrap?.SaveNow();
    }

    private void HandleLogoutAction()
    {
        bootstrap?.Logout();
    }

    private void HandleGachaAction()
    {
        bootstrap?.OpenGacha();
    }

    private void HandleRetryAction()
    {
        bootstrap?.RetryInitialization();
    }

    private async void ClaimDailyReward()
    {
        try
        {
            if (DailyRewardManager.Instance == null)
                return;

            bool claimed =
                await DailyRewardManager.Instance.ClaimRewardAsync();
            ShowToast(claimed
                ? "Daily reward collected."
                : "Daily reward is not available.");
            RefreshMore();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            ShowToast("Reward claim failed.");
        }
    }

    private void HandleGrowthUpdated(UpgradeType type)
    {
        RefreshGrowth();
        RefreshTopBar();
        ShowToast($"{type} upgraded.");
    }

    private void ShowBattle()
    {
        SetActiveView(battlePanel);
    }

    private void ShowGrowth()
    {
        SetActiveView(growthPanel);
        RefreshGrowth();
    }

    private void ShowMore()
    {
        SetActiveView(morePanel);
        RefreshMore();
    }

    private void SetActiveView(GameObject active)
    {
        if (battlePanel != null)
            battlePanel.SetActive(active == battlePanel);
        if (growthPanel != null)
            growthPanel.SetActive(active == growthPanel);
        if (morePanel != null)
            morePanel.SetActive(active == morePanel);
    }

    private static void SetBar(
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

    private static RectTransform CreateHealthBar(
        RectTransform parent,
        string name,
        Color fillColor,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform background = CreatePanel(
            name,
            parent,
            new Color32(12, 18, 30, 255),
            anchorMin,
            anchorMax);

        return CreatePanel(
            "Fill",
            background,
            fillColor,
            Vector2.zero,
            Vector2.one);
    }

    private static RectTransform CreatePanel(
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

        panel.GetComponent<Image>().color = color;
        return rect;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string value,
        float fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax,
        TextAlignmentOptions alignment,
        Color? color = null)
    {
        GameObject textObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect =
            textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text =
            textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color ?? Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        return text;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = CreatePanel(
            name,
            parent,
            color,
            anchorMin,
            anchorMax);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.onClick.AddListener(action);

        ColorBlock colors = button.colors;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.15f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        colors.disabledColor = new Color(
            color.r,
            color.g,
            color.b,
            0.4f);
        button.colors = colors;

        CreateText(
            "Label",
            rect,
            label,
            29,
            new Vector2(0.03f, 0.05f),
            new Vector2(0.97f, 0.95f),
            TextAlignmentOptions.Center,
            Color.white);

        return button;
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystem = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(InputSystemUIInputModule));
    }

    private void OnDestroy()
    {
        if (battleManager != null)
            battleManager.OnBattleStateChanged -= RefreshBattle;

        if (growthManager != null)
            growthManager.OnUpgraded -= HandleGrowthUpdated;

        if (tutorialManager != null)
            tutorialManager.OnTutorialChanged -= RefreshTutorial;

        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnPlayerDataChanged -= RefreshAll;
    }
}
