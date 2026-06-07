using System;
using System.Collections.Generic;
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
    private CompanionManager companionManager;

    private GameObject battlePanel;
    private GameObject growthPanel;
    private GameObject morePanel;
    private GameObject collectionPanel;
    private GameObject equipmentPanel;
    private GameObject questPanel;
    private GameObject shopPanel;
    private GameObject eventPanel;
    private GameObject settingsPanel;
    private GameObject accountPanel;
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
    private TMP_Text skillStatusText;
    private TMP_Text autoAdvanceText;
    private TMP_Text attackGrowthText;
    private TMP_Text healthGrowthText;
    private TMP_Text speedGrowthText;
    private TMP_Text inventoryText;
    private TMP_Text companionText;
    private TMP_Text characterDetailText;
    private TMP_Text equipmentText;
    private TMP_Text questText;
    private TMP_Text shopText;
    private TMP_Text eventText;
    private TMP_Text settingsText;
    private TMP_Text accountText;
    private TMP_Text accountDetailText;
    private TMP_Text dailyRewardText;
    private TMP_Text tutorialText;
    private TMP_Text tutorialButtonText;
    private TMP_Text loadingText;
    private TMP_Text offlineText;
    private TMP_Text toastText;

    private RectTransform enemyHealthFill;
    private RectTransform playerHealthFill;
    private RectTransform enemyVisual;
    private RectTransform playerVisual;
    private Image enemyVisualImage;
    private Image playerVisualImage;
    private BattleActorView enemyActorView;
    private BattleActorView playerActorView;
    private readonly List<BattleActorView> companionActorViews =
        new List<BattleActorView>();
    private Button tutorialButton;
    private Button retryButton;
    private Button googleLinkButton;
    private Button appleLinkButton;
    private Button starterPackButton;
    private Button smallGemPackButton;
    private Button largeGemPackButton;
    private Button rewardedAdButton;
    private CharacterData selectedCharacter;
    private float toastTimer;
    private float enemyAnimationTimer;
    private float playerAnimationTimer;
    private float playerDefeatTimer;

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
        TutorialManager tutorial,
        CompanionManager companion)
    {
        bootstrap = sessionBootstrap;
        battleManager = battle;
        growthManager = growth;
        tutorialManager = tutorial;
        companionManager = companion;

        BuildInterface();
        BindEvents();
        LocalizationManager.ApplyTo(transform);
        ShowBattle();
    }

    public void RefreshAll()
    {
        RefreshTopBar();
        RefreshBattle();
        RefreshGrowth();
        RefreshMore();
        RefreshCollection();
        RefreshEquipment();
        RefreshQuests();
        RefreshShop();
        RefreshEvent();
        RefreshSettings();
        RefreshAccount();
        RefreshTutorial();
        LocalizationManager.ApplyTo(transform);
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
        if (toastTimer > 0f)
        {
            toastTimer -= Time.unscaledDeltaTime;
            if (toastTimer <= 0f && toastPanel != null)
                toastPanel.SetActive(false);
        }

        UpdateBattleAnimations(Time.unscaledDeltaTime);
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
        BuildCollectionPanel(portraitRoot);
        BuildEquipmentPanel(portraitRoot);
        BuildQuestPanel(portraitRoot);
        BuildShopPanel(portraitRoot);
        BuildEventPanel(portraitRoot);
        BuildSettingsPanel(portraitRoot);
        BuildAccountPanel(portraitRoot);
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

        CreateButton(
            "PreviousStageButton",
            top,
            "<",
            new Vector2(0.005f, 0.18f),
            new Vector2(0.04f, 0.82f),
            PanelLight,
            () => ChangeStage(-1));

        CreateButton(
            "NextStageButton",
            top,
            ">",
            new Vector2(0.34f, 0.18f),
            new Vector2(0.375f, 0.82f),
            PanelLight,
            () => ChangeStage(1));

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

        Button autoButton = CreateButton(
            "AutoAdvanceButton",
            panel,
            "AUTO ON",
            new Vector2(0.72f, 0.9f),
            new Vector2(0.94f, 0.97f),
            PanelLight,
            ToggleAutoAdvance);
        autoAdvanceText =
            autoButton.GetComponentInChildren<TMP_Text>();

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

        enemyVisual = CreatePanel(
            "EnemyVisual",
            enemyCard,
            Danger,
            new Vector2(0.3f, 0.28f),
            new Vector2(0.7f, 0.75f));
        enemyVisualImage = enemyVisual.GetComponent<Image>();

        TMP_Text enemyGlyph = CreateText(
            "EnemyGlyph",
            enemyVisual,
            "BOSS",
            46,
            new Vector2(0f, 0f),
            Vector2.one,
            TextAlignmentOptions.Center);
        enemyActorView =
            enemyVisual.gameObject.AddComponent<BattleActorView>();
        enemyActorView.Initialize(enemyGlyph, Danger);

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

        playerVisual = CreatePanel(
            "PlayerVisual",
            playerCard,
            Accent,
            new Vector2(0.06f, 0.55f),
            new Vector2(0.18f, 0.92f));
        playerVisualImage = playerVisual.GetComponent<Image>();

        TMP_Text playerGlyph = CreateText(
            "PlayerGlyph",
            playerVisual,
            "HERO",
            22,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center);
        playerActorView =
            playerVisual.gameObject.AddComponent<BattleActorView>();
        playerActorView.Initialize(playerGlyph, Accent);

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

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            float minX = 0.06f + slot * 0.1f;
            RectTransform companionVisual = CreatePanel(
                $"CompanionVisual{slot + 1}",
                playerCard,
                PanelLight,
                new Vector2(minX, 0.05f),
                new Vector2(minX + 0.08f, 0.28f));
            TMP_Text companionGlyph = CreateText(
                "Glyph",
                companionVisual,
                (slot + 1).ToString(),
                20,
                Vector2.zero,
                Vector2.one,
                TextAlignmentOptions.Center);
            BattleActorView actorView =
                companionVisual.gameObject.AddComponent<BattleActorView>();
            actorView.Initialize(companionGlyph, PanelLight);
            companionActorViews.Add(actorView);
        }

        CreateText(
            "AutoNotice",
            playerCard,
            "Your hero attacks automatically.",
            27,
            new Vector2(0.39f, 0.05f),
            new Vector2(0.94f, 0.27f),
            TextAlignmentOptions.Center,
            new Color32(190, 203, 225, 255));

        skillStatusText = CreateText(
            "SkillStatus",
            panel,
            "Companion skills preparing...",
            27,
            new Vector2(0.06f, 0.08f),
            new Vector2(0.94f, 0.16f),
            TextAlignmentOptions.Center,
            Gold);
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
            new Vector2(0.05f, 0.64f),
            new Vector2(0.95f, 0.87f));

        inventoryText = CreateText(
            "InventoryText",
            inventoryCard,
            "Inventory",
            31,
            new Vector2(0.05f, 0.08f),
            new Vector2(0.68f, 0.92f),
            TextAlignmentOptions.TopLeft);

        CreateButton(
            "ClaimMailButton",
            inventoryCard,
            "MAIL",
            new Vector2(0.71f, 0.53f),
            new Vector2(0.95f, 0.88f),
            Gold,
            ClaimAllMail);

        CreateButton(
            "EquipmentButton",
            inventoryCard,
            "EQUIPMENT",
            new Vector2(0.71f, 0.12f),
            new Vector2(0.95f, 0.47f),
            PanelLight,
            ShowEquipment);

        RectTransform companionCard = CreatePanel(
            "CompanionCard",
            panel,
            Panel,
            new Vector2(0.05f, 0.42f),
            new Vector2(0.95f, 0.61f));

        companionText = CreateText(
            "CompanionText",
            companionCard,
            "Companion",
            29,
            new Vector2(0.05f, 0.08f),
            new Vector2(0.68f, 0.92f),
            TextAlignmentOptions.TopLeft);

        CreateButton(
            "CollectionButton",
            companionCard,
            "COLLECTION",
            new Vector2(0.71f, 0.53f),
            new Vector2(0.95f, 0.88f),
            PanelLight,
            ShowCollection);

        CreateButton(
            "BestCompanionButton",
            companionCard,
            "BEST",
            new Vector2(0.71f, 0.12f),
            new Vector2(0.95f, 0.47f),
            Accent,
            HandleAutoEquip);

        RectTransform rewardCard = CreatePanel(
            "RewardCard",
            panel,
            Panel,
            new Vector2(0.05f, 0.25f),
            new Vector2(0.95f, 0.39f));

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
            rewardCard,
            "Account",
            22,
            new Vector2(0.05f, 0.05f),
            new Vector2(0.64f, 0.4f),
            TextAlignmentOptions.Left,
            new Color32(174, 189, 214, 255));

        CreateButton(
            "QuestButton",
            panel,
            "QUESTS",
            new Vector2(0.06f, 0.14f),
            new Vector2(0.31f, 0.23f),
            Gold,
            ShowQuests);

        CreateButton(
            "EventButton",
            panel,
            "EVENT",
            new Vector2(0.36f, 0.14f),
            new Vector2(0.64f, 0.23f),
            Success,
            ShowEvent);

        CreateButton(
            "ShopButton",
            panel,
            "SHOP",
            new Vector2(0.69f, 0.14f),
            new Vector2(0.94f, 0.23f),
            Accent,
            ShowShop);

        CreateButton(
            "SaveButton",
            panel,
            "SAVE",
            new Vector2(0.06f, 0.03f),
            new Vector2(0.31f, 0.12f),
            PanelLight,
            HandleSaveAction);

        CreateButton(
            "SettingsButton",
            panel,
            "SETTINGS",
            new Vector2(0.36f, 0.03f),
            new Vector2(0.64f, 0.12f),
            PanelLight,
            ShowSettings);

        CreateButton(
            "AccountButton",
            panel,
            "ACCOUNT",
            new Vector2(0.69f, 0.03f),
            new Vector2(0.94f, 0.12f),
            Accent,
            ShowAccount);
    }

    private void BuildCollectionPanel(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "CollectionPanel",
            root,
            Background,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));
        collectionPanel = panel.gameObject;

        CreateButton(
            "CollectionBackButton",
            panel,
            "BACK",
            new Vector2(0.04f, 0.9f),
            new Vector2(0.22f, 0.97f),
            PanelLight,
            ShowMore);

        CreateText(
            "CollectionTitle",
            panel,
            "COMPANION COLLECTION",
            42,
            new Vector2(0.24f, 0.9f),
            new Vector2(0.96f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        List<CharacterData> characters =
            companionManager.GetAllCharacters();
        for (int index = 0; index < characters.Count; index++)
        {
            CharacterData character = characters[index];
            int column = index % 3;
            int row = index / 3;
            float xMin = 0.04f + column * 0.32f;
            float yMax = 0.87f - row * 0.12f;

            CreateButton(
                "Character_" + character.characterName,
                panel,
                $"[{character.rarity}]\n{character.characterName}",
                new Vector2(xMin, yMax - 0.09f),
                new Vector2(xMin + 0.28f, yMax),
                GetRarityColor(character.rarity),
                () => SelectCharacter(character));
        }

        RectTransform detailCard = CreatePanel(
            "CharacterDetailCard",
            panel,
            Panel,
            new Vector2(0.04f, 0.05f),
            new Vector2(0.96f, 0.37f));

        characterDetailText = CreateText(
            "CharacterDetailText",
            detailCard,
            "Select a companion.",
            28,
            new Vector2(0.05f, 0.28f),
            new Vector2(0.67f, 0.94f),
            TextAlignmentOptions.TopLeft);

        CreateButton(
            "PromoteButton",
            detailCard,
            "PROMOTE",
            new Vector2(0.7f, 0.55f),
            new Vector2(0.95f, 0.88f),
            Gold,
            PromoteSelectedCharacter);

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            int capturedSlot = slot;
            float xMin = 0.05f + slot * 0.32f;
            CreateButton(
                "EquipSlot" + (slot + 1),
                detailCard,
                "SLOT " + (slot + 1),
                new Vector2(xMin, 0.05f),
                new Vector2(xMin + 0.27f, 0.23f),
                Accent,
                () => ToggleSelectedCharacterSlot(capturedSlot));
        }
    }

    private void BuildEquipmentPanel(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "EquipmentPanel",
            root,
            Background,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));
        equipmentPanel = panel.gameObject;

        CreateButton(
            "EquipmentBackButton",
            panel,
            "BACK",
            new Vector2(0.04f, 0.9f),
            new Vector2(0.22f, 0.97f),
            PanelLight,
            ShowMore);

        CreateText(
            "EquipmentTitle",
            panel,
            "EQUIPMENT",
            46,
            new Vector2(0.24f, 0.9f),
            new Vector2(0.96f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RectTransform card = CreatePanel(
            "EquipmentCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.35f),
            new Vector2(0.94f, 0.85f));

        equipmentText = CreateText(
            "EquipmentText",
            card,
            "No equipment.",
            34,
            new Vector2(0.07f, 0.34f),
            new Vector2(0.93f, 0.92f),
            TextAlignmentOptions.TopLeft);

        CreateButton(
            "UpgradeWeaponButton",
            card,
            "UPGRADE WEAPON",
            new Vector2(0.07f, 0.08f),
            new Vector2(0.47f, 0.27f),
            Accent,
            UpgradeWeapon);

        CreateButton(
            "UpgradeArmorButton",
            card,
            "UPGRADE ARMOR",
            new Vector2(0.53f, 0.08f),
            new Vector2(0.93f, 0.27f),
            Success,
            UpgradeArmor);
    }

    private void BuildQuestPanel(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "QuestPanel",
            root,
            Background,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));
        questPanel = panel.gameObject;

        CreateButton(
            "QuestBackButton",
            panel,
            "BACK",
            new Vector2(0.04f, 0.9f),
            new Vector2(0.22f, 0.97f),
            PanelLight,
            ShowMore);

        CreateText(
            "QuestTitle",
            panel,
            "QUESTS & ACHIEVEMENTS",
            42,
            new Vector2(0.24f, 0.9f),
            new Vector2(0.96f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RectTransform card = CreatePanel(
            "QuestCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.35f),
            new Vector2(0.94f, 0.85f));

        questText = CreateText(
            "QuestText",
            card,
            "Quest data unavailable.",
            36,
            new Vector2(0.07f, 0.35f),
            new Vector2(0.93f, 0.92f),
            TextAlignmentOptions.TopLeft);

        CreateButton(
            "ClaimQuestButton",
            card,
            "CLAIM QUEST",
            new Vector2(0.07f, 0.08f),
            new Vector2(0.47f, 0.27f),
            Success,
            ClaimCurrentQuest);

        CreateButton(
            "ClaimAchievementButton",
            card,
            "CLAIM ACHIEVEMENTS",
            new Vector2(0.53f, 0.08f),
            new Vector2(0.93f, 0.27f),
            Gold,
            ClaimAchievements);
    }

    private void BuildShopPanel(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "ShopPanel",
            root,
            Background,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));
        shopPanel = panel.gameObject;

        CreateButton(
            "ShopBackButton",
            panel,
            "BACK",
            new Vector2(0.05f, 0.9f),
            new Vector2(0.25f, 0.98f),
            PanelLight,
            ShowMore);

        CreateText(
            "ShopTitle",
            panel,
            "SHOP",
            48,
            new Vector2(0.3f, 0.9f),
            new Vector2(0.7f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RectTransform card = CreatePanel(
            "ShopCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.18f),
            new Vector2(0.94f, 0.86f));

        shopText = CreateText(
            "ShopText",
            card,
            "Shop data unavailable.",
            28,
            new Vector2(0.07f, 0.78f),
            new Vector2(0.93f, 0.93f),
            TextAlignmentOptions.TopLeft);

        starterPackButton = CreateButton(
            "StarterPackButton",
            card,
            "STARTER PACK",
            new Vector2(0.07f, 0.62f),
            new Vector2(0.93f, 0.75f),
            Gold,
            BuyStarterPack);

        smallGemPackButton = CreateButton(
            "SmallGemPackButton",
            card,
            "1,200 GEMS",
            new Vector2(0.07f, 0.47f),
            new Vector2(0.93f, 0.59f),
            Accent,
            BuySmallGemPack);

        largeGemPackButton = CreateButton(
            "LargeGemPackButton",
            card,
            "6,500 GEMS",
            new Vector2(0.07f, 0.32f),
            new Vector2(0.93f, 0.44f),
            Success,
            BuyLargeGemPack);

        rewardedAdButton = CreateButton(
            "RewardedAdButton",
            card,
            "WATCH AD  +100 GEMS",
            new Vector2(0.07f, 0.18f),
            new Vector2(0.93f, 0.29f),
            PanelLight,
            WatchRewardedAd);

        CreateButton(
            "BuyGoldPouchButton",
            card,
            "5K GOLD\n100 GEM",
            new Vector2(0.03f, 0.02f),
            new Vector2(0.32f, 0.14f),
            Gold,
            BuyGoldPouch);

        CreateButton(
            "BuyTicketBundleButton",
            card,
            "3 TICKETS\n250 GEM",
            new Vector2(0.35f, 0.02f),
            new Vector2(0.65f, 0.14f),
            Accent,
            BuyTicketBundle);

        CreateButton(
            "BuyGrowthChestButton",
            card,
            "30K GOLD\n500 GEM",
            new Vector2(0.68f, 0.02f),
            new Vector2(0.97f, 0.14f),
            Success,
            BuyGrowthChest);
    }

    private void BuildEventPanel(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "EventPanel",
            root,
            Background,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));
        eventPanel = panel.gameObject;

        CreateButton(
            "EventBackButton",
            panel,
            "BACK",
            new Vector2(0.05f, 0.9f),
            new Vector2(0.25f, 0.98f),
            PanelLight,
            ShowMore);

        CreateText(
            "EventTitle",
            panel,
            "EVENT",
            48,
            new Vector2(0.3f, 0.9f),
            new Vector2(0.7f, 0.98f),
            TextAlignmentOptions.Center,
            Success);

        RectTransform card = CreatePanel(
            "EventCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.35f),
            new Vector2(0.94f, 0.85f));

        eventText = CreateText(
            "EventText",
            card,
            "Event data unavailable.",
            34,
            new Vector2(0.07f, 0.3f),
            new Vector2(0.93f, 0.92f),
            TextAlignmentOptions.TopLeft);

        CreateButton(
            "ClaimEventRewardButton",
            card,
            "CLAIM EVENT REWARD",
            new Vector2(0.07f, 0.06f),
            new Vector2(0.93f, 0.24f),
            Success,
            ClaimEventReward);
    }

    private void BuildSettingsPanel(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "SettingsPanel",
            root,
            Background,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));
        settingsPanel = panel.gameObject;

        CreateButton(
            "SettingsBackButton",
            panel,
            "BACK",
            new Vector2(0.05f, 0.9f),
            new Vector2(0.25f, 0.98f),
            PanelLight,
            ShowMore);

        CreateText(
            "SettingsTitle",
            panel,
            "SETTINGS",
            48,
            new Vector2(0.3f, 0.9f),
            new Vector2(0.7f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RectTransform card = CreatePanel(
            "SettingsCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.3f),
            new Vector2(0.94f, 0.85f));

        settingsText = CreateText(
            "SettingsText",
            card,
            "Settings unavailable.",
            36,
            new Vector2(0.08f, 0.74f),
            new Vector2(0.92f, 0.92f),
            TextAlignmentOptions.TopLeft);

        CreateButton(
            "ToggleSoundButton",
            card,
            "TOGGLE SOUND",
            new Vector2(0.08f, 0.58f),
            new Vector2(0.92f, 0.7f),
            Accent,
            ToggleSound);

        CreateButton(
            "ToggleVibrationButton",
            card,
            "TOGGLE VIBRATION",
            new Vector2(0.08f, 0.43f),
            new Vector2(0.92f, 0.55f),
            Success,
            ToggleVibration);

        CreateButton(
            "ToggleNotificationsButton",
            card,
            "TOGGLE NOTIFICATIONS",
            new Vector2(0.08f, 0.28f),
            new Vector2(0.92f, 0.4f),
            PanelLight,
            ToggleNotifications);

        CreateButton(
            "ToggleFrameRateButton",
            card,
            "SWITCH 30 / 60 FPS",
            new Vector2(0.08f, 0.13f),
            new Vector2(0.92f, 0.25f),
            Gold,
            ToggleFrameRate);

        CreateButton(
            "ToggleLanguageButton",
            card,
            "SWITCH LANGUAGE",
            new Vector2(0.08f, 0.01f),
            new Vector2(0.92f, 0.1f),
            Danger,
            ToggleLanguage);
    }

    private void BuildAccountPanel(RectTransform root)
    {
        RectTransform panel = CreatePanel(
            "AccountPanel",
            root,
            Background,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));
        accountPanel = panel.gameObject;

        CreateButton(
            "AccountBackButton",
            panel,
            "BACK",
            new Vector2(0.05f, 0.9f),
            new Vector2(0.25f, 0.98f),
            PanelLight,
            ShowMore);

        CreateText(
            "AccountTitle",
            panel,
            "ACCOUNT LINK",
            48,
            new Vector2(0.27f, 0.9f),
            new Vector2(0.82f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RectTransform card = CreatePanel(
            "AccountCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.24f),
            new Vector2(0.94f, 0.85f));

        accountDetailText = CreateText(
            "AccountDetailText",
            card,
            "Account status",
            33,
            new Vector2(0.08f, 0.67f),
            new Vector2(0.92f, 0.93f),
            TextAlignmentOptions.TopLeft);

        googleLinkButton = CreateButton(
            "GoogleLinkButton",
            card,
            "LINK GOOGLE",
            new Vector2(0.08f, 0.46f),
            new Vector2(0.92f, 0.61f),
            Accent,
            HandleGoogleLink);

        appleLinkButton = CreateButton(
            "AppleLinkButton",
            card,
            "LINK APPLE",
            new Vector2(0.08f, 0.27f),
            new Vector2(0.92f, 0.42f),
            PanelLight,
            HandleAppleLink);

        CreateText(
            "AccountNotice",
            card,
            "Linking keeps the current UID and all guest progress.",
            25,
            new Vector2(0.08f, 0.14f),
            new Vector2(0.92f, 0.24f),
            TextAlignmentOptions.Center,
            new Color32(174, 189, 214, 255));

        CreateButton(
            "NewGuestButton",
            card,
            "START NEW GUEST",
            new Vector2(0.2f, 0.02f),
            new Vector2(0.8f, 0.12f),
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
        battleManager.OnPlayerAttackPerformed += HandlePlayerAttackVisual;
        battleManager.OnEnemyAttackPerformed += HandleEnemyAttackVisual;
        battleManager.OnEnemyDefeated += HandleEnemyDefeatedVisual;
        battleManager.OnPlayerDefeated += HandlePlayerDefeatedVisual;
        battleManager.OnCompanionSkillUsed += HandleCompanionSkillVisual;
        battleManager.OnBossPatternUsed += HandleBossPatternVisual;
        battleManager.OnBossChallengeFailed += HandleBossChallengeFailed;
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentDropped +=
                HandleEquipmentDropped;
        growthManager.OnUpgraded += HandleGrowthUpdated;
        tutorialManager.OnTutorialChanged += RefreshTutorial;

        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnPlayerDataChanged += RefreshAll;

        if (AccountLinkManager.Instance != null)
            AccountLinkManager.Instance.OnAccountChanged += RefreshAccount;

        if (MonetizationManager.Instance != null)
        {
            MonetizationManager.Instance.OnMonetizationChanged +=
                RefreshShop;
        }
    }

    private void RefreshTopBar()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        goldText.text = $"Gold  {data.gold:N0}";
        stageText.text = $"Stage {data.currentStage}";
        if (autoAdvanceText != null)
        {
            autoAdvanceText.text =
                data.autoAdvance ? "AUTO ON" : "REPEAT";
        }
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
                ? $"{battleManager.EnemyName}  " +
                  $"{battleManager.BossTimeRemaining:0.0}s"
                : $"{battleManager.EnemyName}  " +
                  $"{data.stageEnemyIndex + 1}/" +
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
        RefreshSkillStatus();
        RefreshBattleVisuals();
    }

    private void RefreshBattleVisuals()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        BattleVisualProfile hero = BattleVisualResolver.GetHero();
        playerActorView?.SetVisual(
            hero?.sprite,
            hero?.animatorController);

        BattleVisualProfile enemy =
            BattleVisualResolver.GetEnemy(
                data.currentStage,
                battleManager.IsBoss);
        enemyActorView?.SetVisual(
            enemy?.sprite,
            enemy?.animatorController);

        for (int slot = 0;
             slot < companionActorViews.Count;
             slot++)
        {
            CharacterData character =
                companionManager?.GetEquippedAtSlot(slot);
            companionActorViews[slot].SetVisual(
                character == null
                    ? null
                    : character.battleSprite ?? character.icon,
                character?.battleAnimator);
        }
    }

    private void RefreshSkillStatus()
    {
        if (skillStatusText == null || companionManager == null)
            return;

        StringBuilder builder = new StringBuilder();
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            CharacterData character =
                companionManager.GetEquippedAtSlot(slot);
            if (character == null)
                continue;

            if (builder.Length > 0)
                builder.Append("   ");

            float cooldown =
                slot < battleManager.SkillCooldowns.Count
                    ? battleManager.SkillCooldowns[slot]
                    : 0f;
            builder.Append(
                cooldown <= 0f
                    ? $"{character.characterName}: READY"
                    : $"{character.characterName}: {cooldown:0.0}s");
        }

        skillStatusText.text = builder.Length > 0
            ? builder.ToString()
            : "No companion skills equipped.";
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
        builder.AppendLine("RESOURCES");
        foreach (var item in data.inventory.items)
        {
            if (companionManager == null ||
                !companionManager.IsCharacterItem(item.Key))
            {
                builder.AppendLine($"{item.Key}   x{item.Value}");
            }
        }

        builder.AppendLine($"Mailbox   {data.mailbox.Count} waiting");
        builder.AppendLine(
            $"Monsters defeated   {data.totalMonstersDefeated:N0}");
        inventoryText.text = builder.ToString();

        StringBuilder companionBuilder = new StringBuilder();
        companionBuilder.AppendLine("COMPANION");

        var party = companionManager?.GetEquippedParty();
        if (party == null || party.Count == 0)
        {
            companionBuilder.AppendLine("Party 0/3");
            companionBuilder.Append("Recruit one in Gacha.");
        }
        else
        {
            int bonus = 0;
            companionBuilder.AppendLine(
                $"PARTY {party.Count}/{CompanionManager.PartySize}");
            for (int i = 0; i < party.Count; i++)
            {
                CharacterData character = party[i];
                if (i > 0)
                    companionBuilder.Append(", ");

                companionBuilder.Append(
                    $"[{character.rarity}] {character.characterName}");
                bonus += CompanionManager.GetAttackBonusPercent(
                    character.rarity);
            }

            companionBuilder.AppendLine();
            companionBuilder.Append($"Team Attack +{bonus}%");
            CompanionSynergyResult synergy =
                companionManager.GetSynergyResult();
            companionBuilder.AppendLine();
            companionBuilder.Append(synergy.GetSummary());
        }

        companionText.text = companionBuilder.ToString();

        AccountLinkManager accounts = AccountLinkManager.Instance;
        string accountType = accounts != null &&
            (accounts.IsLinked(AccountLinkProvider.Google) ||
             accounts.IsLinked(AccountLinkProvider.Apple))
                ? "Linked account"
                : "Guest account";
        accountText.text =
            $"{accountType}  |  Highest {data.highestStage}";

        if (DailyRewardManager.Instance != null)
        {
            int day = DailyRewardManager.Instance.GetNextRewardDay();
            dailyRewardText.text =
                DailyRewardManager.Instance.CanClaimReward()
                    ? $"Daily Reward Day {day} is ready"
                    : $"Daily Reward Day {day} already claimed";
        }
    }

    private void RefreshCollection()
    {
        if (characterDetailText == null || companionManager == null)
            return;

        if (selectedCharacter == null)
        {
            List<CharacterData> characters =
                companionManager.GetAllCharacters();
            if (characters.Count > 0)
                selectedCharacter = characters[0];
        }

        if (selectedCharacter == null)
            return;

        int owned =
            companionManager.GetOwnedCount(
                selectedCharacter.characterName);
        int bonus =
            CompanionManager.GetAttackBonusPercent(
                selectedCharacter.rarity,
                companionManager.GetStars(
                    selectedCharacter.characterName));
        int stars =
            companionManager.GetStars(selectedCharacter.characterName);
        int promotionCost =
            companionManager.GetPromotionCost(
                selectedCharacter.characterName);

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(
            $"[{selectedCharacter.rarity}] " +
            $"{selectedCharacter.characterName}");
        builder.AppendLine(
            owned > 0 ? $"Owned x{owned}" : "LOCKED");
        builder.AppendLine(
            $"Stars {stars}/5  |  Attack +{bonus}%");
        builder.AppendLine(
            $"{selectedCharacter.element} / {selectedCharacter.role}");
        if (owned > 0 && stars < 5)
            builder.AppendLine($"Promotion needs {promotionCost} duplicate(s)");
        builder.AppendLine(selectedCharacter.description);
        builder.Append("Party: ");

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            if (slot > 0)
                builder.Append("  |  ");

            CharacterData equipped =
                companionManager.GetEquippedAtSlot(slot);
            builder.Append(
                $"{slot + 1}. " +
                $"{(equipped == null ? "Empty" : equipped.characterName)}");
        }

        characterDetailText.text = builder.ToString();
    }

    private void RefreshEquipment()
    {
        if (equipmentText == null)
            return;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        string weapon = string.IsNullOrEmpty(data.equippedWeapon)
            ? "None"
            : data.equippedWeapon;
        string armor = string.IsNullOrEmpty(data.equippedArmor)
            ? "None"
            : data.equippedArmor;

        equipmentText.text =
            $"WEAPON\n{weapon}  Lv.{data.weaponUpgradeLevel}\n" +
            $"Attack +{EquipmentManager.GetWeaponAttack(data)}\n" +
            $"Next cost {(weapon == "None" ? "-" : EquipmentManager.GetUpgradeCost(data.weaponUpgradeLevel).ToString("N0"))}\n\n" +
            $"ARMOR\n{armor}  Lv.{data.armorUpgradeLevel}\n" +
            $"Health +{EquipmentManager.GetArmorHealth(data)}\n" +
            $"Next cost {(armor == "None" ? "-" : EquipmentManager.GetUpgradeCost(data.armorUpgradeLevel).ToString("N0"))}";
    }

    private void RefreshQuests()
    {
        if (questText != null && QuestManager.Instance != null)
            questText.text = QuestManager.Instance.GetStatusText();
    }

    private void RefreshShop()
    {
        if (shopText == null)
            return;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
        {
            shopText.text = "Shop data unavailable.";
            return;
        }

        int gems = GachaEconomy.GetItemCount(data, "Gem");
        int tickets = GachaEconomy.GetItemCount(data, "GachaTicket");
        MonetizationManager monetization = MonetizationManager.Instance;
        shopText.text =
            $"Gems {gems:N0}  |  Gold {data.gold:N0}  |  " +
            $"Tickets {tickets:N0}\n" +
            (monetization?.GetStoreStatus() ??
             "Monetization service unavailable");

        if (monetization == null)
            return;

        bool busy = monetization.IsBusy;
        bool starterOwned = data.ownedPurchaseProducts.Contains(
            MonetizationManager.GetProductId(
                RealMoneyProduct.StarterPack));
        SetButtonLabel(
            starterPackButton,
            starterOwned
                ? "STARTER PACK  OWNED"
                : "STARTER PACK  " +
                  monetization.GetPriceLabel(
                      RealMoneyProduct.StarterPack) +
                  "\n1K GEMS + 10 TICKETS + 50K GOLD");
        SetButtonLabel(
            smallGemPackButton,
            "1,200 GEMS  " +
            monetization.GetPriceLabel(
                RealMoneyProduct.GemPackSmall));
        SetButtonLabel(
            largeGemPackButton,
            "6,500 GEMS  " +
            monetization.GetPriceLabel(
                RealMoneyProduct.GemPackLarge));

        if (starterPackButton != null)
            starterPackButton.interactable = !busy && !starterOwned;
        if (smallGemPackButton != null)
            smallGemPackButton.interactable = !busy;
        if (largeGemPackButton != null)
            largeGemPackButton.interactable = !busy;

        if (rewardedAdButton != null)
        {
            bool canWatch =
                monetization.CanWatchRewardedAd(out string reason);
            rewardedAdButton.interactable =
                !busy && monetization.RewardedAdReady && canWatch;
            SetButtonLabel(
                rewardedAdButton,
                !monetization.AdProviderReady
                    ? "AD SDK PENDING"
                    : canWatch
                    ? $"WATCH AD  +{MonetizationManager.RewardedAdGemAmount} GEMS"
                    : reason.ToUpperInvariant());
        }
    }

    private void RefreshEvent()
    {
        if (eventText != null && EventMissionManager.Instance != null)
            eventText.text =
                EventMissionManager.Instance.GetStatusText();
    }

    private void RefreshSettings()
    {
        if (settingsText == null)
            return;

        GameSettingsManager settings = GameSettingsManager.Instance;
        if (settings == null)
        {
            settingsText.text = "Settings unavailable.";
            return;
        }

        settingsText.text =
            LocalizationManager.Text(
                $"Sound   {(settings.SoundEnabled ? "ON" : "OFF")}\n" +
                $"Vibration   {(settings.VibrationEnabled ? "ON" : "OFF")}\n" +
                $"Notifications   " +
                $"{(settings.NotificationsEnabled ? "ON" : "OFF")}\n" +
                $"Frame Rate   {settings.TargetFrameRate} FPS\n" +
                "Language   English",
                $"사운드   {(settings.SoundEnabled ? "켜짐" : "꺼짐")}\n" +
                $"진동   {(settings.VibrationEnabled ? "켜짐" : "꺼짐")}\n" +
                $"알림   " +
                $"{(settings.NotificationsEnabled ? "켜짐" : "꺼짐")}\n" +
                $"프레임   {settings.TargetFrameRate} FPS\n" +
                "언어   한국어");
    }

    private void PromoteSelectedCharacter()
    {
        if (selectedCharacter == null || companionManager == null)
            return;

        if (!companionManager.TryPromote(selectedCharacter))
        {
            ShowToast("Not enough duplicate copies.");
            return;
        }

        battleManager?.RefreshPlayerStats();
        PlayerDataManager.Instance?.NotifyPlayerDataChanged();
        if (bootstrap != null)
            _ = bootstrap.SaveNowAsync();

        RefreshCollection();
        ShowToast($"{selectedCharacter.characterName} promoted.");
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

    private void RefreshAccount()
    {
        if (accountDetailText == null)
            return;

        AccountLinkManager accounts = AccountLinkManager.Instance;
        if (accounts == null)
        {
            accountDetailText.text = "Account service is unavailable.";
            return;
        }

        accountDetailText.text = accounts.GetAccountSummary();

        bool busy = accounts.IsBusy;
        if (googleLinkButton != null)
        {
            googleLinkButton.interactable =
                !busy &&
                !accounts.IsLinked(AccountLinkProvider.Google);
        }

        if (appleLinkButton != null)
        {
            appleLinkButton.interactable =
                !busy &&
                !accounts.IsLinked(AccountLinkProvider.Apple);
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

    private async void HandleGoogleLink()
    {
        await HandleAccountLink(AccountLinkProvider.Google);
    }

    private async void HandleAppleLink()
    {
        await HandleAccountLink(AccountLinkProvider.Apple);
    }

    private async System.Threading.Tasks.Task HandleAccountLink(
        AccountLinkProvider provider)
    {
        if (bootstrap == null)
            return;

        RefreshAccount();
        AccountLinkResult result =
            await bootstrap.LinkAccountAsync(provider);
        RefreshAccount();
        RefreshMore();
        ShowToast(result.Message);
    }

    private void HandleGachaAction()
    {
        bootstrap?.OpenGacha();
    }

    private void HandleRetryAction()
    {
        bootstrap?.RetryInitialization();
    }

    private void HandleAutoEquip()
    {
        if (companionManager == null)
            return;

        companionManager.TryEquipBestOwned(
            out CharacterData equipped);
        ApplyCompanionSelection(equipped);
    }

    private void SelectCharacter(CharacterData character)
    {
        selectedCharacter = character;
        RefreshCollection();
    }

    private void ToggleSelectedCharacterSlot(int slotIndex)
    {
        if (selectedCharacter == null || companionManager == null)
            return;

        CharacterData equipped =
            companionManager.GetEquippedAtSlot(slotIndex);
        bool changed;

        if (equipped == selectedCharacter)
        {
            changed = companionManager.TryUnequipSlot(slotIndex);
        }
        else
        {
            changed = companionManager.TryEquipToSlot(
                selectedCharacter,
                slotIndex);
        }

        if (!changed)
        {
            ShowToast("This companion is not owned.");
            return;
        }

        battleManager?.RefreshPlayerStats();
        PlayerDataManager.Instance?.NotifyPlayerDataChanged();
        if (bootstrap != null)
            _ = bootstrap.SaveNowAsync();

        RefreshCollection();
        ShowToast(
            equipped == selectedCharacter
                ? $"{selectedCharacter.characterName} removed."
                : $"{selectedCharacter.characterName} set to slot " +
                  $"{slotIndex + 1}.");
    }

    private void ApplyCompanionSelection(CharacterData equipped)
    {
        if (equipped == null)
        {
            ShowToast("Recruit a companion first.");
            return;
        }

        battleManager?.RefreshPlayerStats();
        PlayerDataManager.Instance?.NotifyPlayerDataChanged();
        if (bootstrap != null)
            _ = bootstrap.SaveNowAsync();

        int bonus =
            CompanionManager.GetAttackBonusPercent(equipped.rarity);
        ShowToast(
            $"{equipped.characterName} equipped. Attack +{bonus}%.");
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

    private async void ClaimAllMail()
    {
        try
        {
            if (MailboxManager.Instance == null)
                return;

            int claimed =
                await MailboxManager.Instance.ClaimAllMailsAsync();
            ShowToast(claimed > 0
                ? $"{claimed} mail reward(s) collected."
                : "No mail rewards available.");
            RefreshMore();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            ShowToast("Mail claim failed.");
        }
    }

    private async void UpgradeWeapon()
    {
        await UpgradeEquipment(EquipmentSlot.Weapon);
    }

    private async void UpgradeArmor()
    {
        await UpgradeEquipment(EquipmentSlot.Armor);
    }

    private async System.Threading.Tasks.Task UpgradeEquipment(
        EquipmentSlot slot)
    {
        if (EquipmentManager.Instance == null)
            return;

        bool upgraded =
            await EquipmentManager.Instance.TryUpgradeAsync(slot);
        if (!upgraded)
        {
            ShowToast("Equipment missing or not enough Gold.");
            return;
        }

        battleManager.RefreshPlayerStats();
        RefreshEquipment();
        ShowToast($"{slot} upgraded.");
    }

    private void HandleEquipmentDropped(string itemName)
    {
        battleManager?.RefreshPlayerStats();
        ShowToast($"Equipment found: {itemName}");
        RefreshEquipment();
    }

    private async void ClaimCurrentQuest()
    {
        bool claimed = QuestManager.Instance != null &&
            await QuestManager.Instance.ClaimCurrentQuestAsync();
        ShowToast(claimed
            ? "Quest reward collected. Next quest started."
            : "Current quest is not complete.");
        RefreshQuests();
    }

    private async void ClaimAchievements()
    {
        int claimed = QuestManager.Instance == null
            ? 0
            : await QuestManager.Instance
                .ClaimAvailableAchievementsAsync();
        ShowToast(claimed > 0
            ? $"{claimed} achievement reward(s) collected."
            : "No achievement rewards available.");
        RefreshQuests();
    }

    private async void BuyGoldPouch()
    {
        await BuyShopProduct(ShopProduct.GoldPouch);
    }

    private async void BuyTicketBundle()
    {
        await BuyShopProduct(ShopProduct.TicketBundle);
    }

    private async void BuyGrowthChest()
    {
        await BuyShopProduct(ShopProduct.GrowthChest);
    }

    private async void BuyStarterPack()
    {
        await BuyRealMoneyProduct(RealMoneyProduct.StarterPack);
    }

    private async void BuySmallGemPack()
    {
        await BuyRealMoneyProduct(RealMoneyProduct.GemPackSmall);
    }

    private async void BuyLargeGemPack()
    {
        await BuyRealMoneyProduct(RealMoneyProduct.GemPackLarge);
    }

    private async void WatchRewardedAd()
    {
        MonetizationManager monetization = MonetizationManager.Instance;
        if (monetization == null)
            return;

        string message = await monetization.ShowRewardedAdAsync(
            RewardedAdPlacement.ShopFreeGems);
        ShowToast(message);
        RefreshTopBar();
        RefreshMore();
        RefreshShop();
    }

    private async System.Threading.Tasks.Task BuyRealMoneyProduct(
        RealMoneyProduct product)
    {
        MonetizationManager monetization = MonetizationManager.Instance;
        if (monetization == null)
            return;

        string message = await monetization.PurchaseAsync(product);
        ShowToast(message);
        RefreshTopBar();
        RefreshMore();
        RefreshShop();
    }

    private async System.Threading.Tasks.Task BuyShopProduct(
        ShopProduct product)
    {
        try
        {
            bool purchased = ShopManager.Instance != null &&
                await ShopManager.Instance.TryPurchaseAsync(product);
            ShowToast(purchased
                ? $"Purchased: {ShopManager.GetDescription(product)}."
                : "Not enough Gems.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            ShowToast("Purchase failed. Gems were restored.");
        }

        RefreshTopBar();
        RefreshMore();
        RefreshShop();
    }

    private async void ClaimEventReward()
    {
        bool claimed = EventMissionManager.Instance != null &&
            await EventMissionManager.Instance.ClaimRewardAsync();
        ShowToast(claimed
            ? "Event reward collected."
            : "Event missions are not complete.");
        RefreshTopBar();
        RefreshMore();
        RefreshEvent();
    }

    private void ToggleSound()
    {
        GameSettingsManager.Instance?.ToggleSound();
        RefreshSettings();
    }

    private void ToggleVibration()
    {
        GameSettingsManager.Instance?.ToggleVibration();
        RefreshSettings();
    }

    private void ToggleNotifications()
    {
        GameSettingsManager.Instance?.ToggleNotifications();
        RefreshSettings();
    }

    private void ToggleFrameRate()
    {
        GameSettingsManager.Instance?.ToggleFrameRate();
        RefreshSettings();
    }

    private void ToggleLanguage()
    {
        GameSettingsManager.Instance?.ToggleLanguage();
        RefreshAll();
        ShowToast(LocalizationManager.Text(
            "Language changed to English.",
            "언어가 한국어로 변경되었습니다."));
    }

    private void HandleGrowthUpdated(UpgradeType type)
    {
        RefreshGrowth();
        RefreshTopBar();
        ShowToast($"{type} upgraded.");
    }

    private void ChangeStage(int direction)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        if (!battleManager.SelectStage(data.currentStage + direction))
        {
            ShowToast(direction < 0
                ? "This is the first stage."
                : "Clear the current highest stage first.");
            return;
        }

        if (bootstrap != null)
            _ = bootstrap.SaveNowAsync();
    }

    private void ToggleAutoAdvance()
    {
        battleManager.ToggleAutoAdvance();
        if (bootstrap != null)
            _ = bootstrap.SaveNowAsync();

        PlayerData data = PlayerDataManager.Instance.playerData;
        ShowToast(data.autoAdvance
            ? "Auto stage advance enabled."
            : "Current stage repeat enabled.");
    }

    private void HandlePlayerAttackVisual(int damage)
    {
        playerAnimationTimer = 0.18f;
        enemyAnimationTimer = 0.25f;
        playerActorView?.Play(BattleAnimationCue.Attack);
        enemyActorView?.Play(BattleAnimationCue.Hit);
    }

    private void HandleEnemyAttackVisual(int damage)
    {
        enemyAnimationTimer = 0.18f;
        playerAnimationTimer = 0.25f;
        enemyActorView?.Play(BattleAnimationCue.Attack);
        playerActorView?.Play(BattleAnimationCue.Hit);
    }

    private void HandleEnemyDefeatedVisual(int reward)
    {
        enemyAnimationTimer = 0.4f;
        enemyActorView?.Play(BattleAnimationCue.Death);
    }

    private void HandlePlayerDefeatedVisual()
    {
        playerDefeatTimer = 1.8f;
        playerActorView?.Play(BattleAnimationCue.Death);
    }

    private void HandleCompanionSkillVisual(
        int slot,
        CharacterData character,
        int damage)
    {
        enemyAnimationTimer = 0.35f;
        if (slot >= 0 && slot < companionActorViews.Count)
            companionActorViews[slot].Play(BattleAnimationCue.Skill);
        enemyActorView?.Play(BattleAnimationCue.Hit);
        ShowToast(
            $"{character.skillName}  DMG {damage:N0}");
    }

    private void HandleBossChallengeFailed()
    {
        enemyAnimationTimer = 0.4f;
        ShowToast("Boss time expired. Retrying.");
    }

    private void HandleBossPatternVisual(
        BossPatternDefinition pattern,
        int damage)
    {
        enemyAnimationTimer = 0.3f;
        playerAnimationTimer = 0.3f;
        enemyActorView?.Play(BattleAnimationCue.Skill);
        playerActorView?.Play(BattleAnimationCue.Hit);
        ShowToast($"{pattern.patternName}  DMG {damage:N0}");
    }

    private void UpdateBattleAnimations(float deltaTime)
    {
        enemyAnimationTimer = Mathf.Max(
            0f,
            enemyAnimationTimer - deltaTime);
        playerAnimationTimer = Mathf.Max(
            0f,
            playerAnimationTimer - deltaTime);
        playerDefeatTimer = Mathf.Max(
            0f,
            playerDefeatTimer - deltaTime);

        if (enemyVisual != null)
        {
            float pulse = enemyAnimationTimer > 0f
                ? Mathf.Sin(enemyAnimationTimer * 70f) * 12f
                : 0f;
            enemyVisual.anchoredPosition = new Vector2(pulse, 0f);
            enemyVisual.localScale = enemyAnimationTimer > 0.3f
                ? Vector3.one * 0.65f
                : Vector3.one;
            enemyVisualImage.color =
                enemyActorView != null && enemyActorView.HasSprite
                    ? enemyAnimationTimer > 0f
                        ? Color.Lerp(Color.white, Danger, 0.4f)
                        : Color.white
                    : enemyAnimationTimer > 0f
                        ? Color.Lerp(Danger, Color.white, 0.45f)
                        : Danger;
        }

        if (playerVisual != null)
        {
            float lunge = playerAnimationTimer > 0.12f ? 18f : 0f;
            playerVisual.anchoredPosition = new Vector2(lunge, 0f);
            playerVisual.localScale = playerDefeatTimer > 0f
                ? Vector3.one * 0.55f
                : Vector3.one;
            playerVisualImage.color =
                playerActorView != null && playerActorView.HasSprite
                    ? playerAnimationTimer > 0f
                        ? Color.Lerp(Color.white, Danger, 0.45f)
                        : Color.white
                    : playerAnimationTimer > 0f
                        ? Color.Lerp(Accent, Danger, 0.55f)
                        : Accent;
        }
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

    private void ShowCollection()
    {
        SetActiveView(collectionPanel);
        RefreshCollection();
    }

    private void ShowEquipment()
    {
        SetActiveView(equipmentPanel);
        RefreshEquipment();
    }

    private void ShowQuests()
    {
        SetActiveView(questPanel);
        RefreshQuests();
    }

    private void ShowShop()
    {
        SetActiveView(shopPanel);
        RefreshShop();
    }

    private void ShowEvent()
    {
        SetActiveView(eventPanel);
        RefreshEvent();
    }

    private void ShowSettings()
    {
        SetActiveView(settingsPanel);
        RefreshSettings();
    }

    private void ShowAccount()
    {
        SetActiveView(accountPanel);
        RefreshAccount();
    }

    private void SetActiveView(GameObject active)
    {
        if (battlePanel != null)
            battlePanel.SetActive(active == battlePanel);
        if (growthPanel != null)
            growthPanel.SetActive(active == growthPanel);
        if (morePanel != null)
            morePanel.SetActive(active == morePanel);
        if (collectionPanel != null)
            collectionPanel.SetActive(active == collectionPanel);
        if (equipmentPanel != null)
            equipmentPanel.SetActive(active == equipmentPanel);
        if (questPanel != null)
            questPanel.SetActive(active == questPanel);
        if (shopPanel != null)
            shopPanel.SetActive(active == shopPanel);
        if (eventPanel != null)
            eventPanel.SetActive(active == eventPanel);
        if (settingsPanel != null)
            settingsPanel.SetActive(active == settingsPanel);
        if (accountPanel != null)
            accountPanel.SetActive(active == accountPanel);
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

    private static void SetButtonLabel(Button button, string value)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = value;
    }

    private static Color GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "SSR":
                return new Color32(184, 112, 255, 255);
            case "SR":
                return new Color32(77, 137, 235, 255);
            default:
                return PanelLight;
        }
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
        {
            battleManager.OnBattleStateChanged -= RefreshBattle;
            battleManager.OnPlayerAttackPerformed -=
                HandlePlayerAttackVisual;
            battleManager.OnEnemyAttackPerformed -=
                HandleEnemyAttackVisual;
            battleManager.OnEnemyDefeated -=
                HandleEnemyDefeatedVisual;
            battleManager.OnPlayerDefeated -=
                HandlePlayerDefeatedVisual;
            battleManager.OnCompanionSkillUsed -=
                HandleCompanionSkillVisual;
            battleManager.OnBossPatternUsed -=
                HandleBossPatternVisual;
            battleManager.OnBossChallengeFailed -=
                HandleBossChallengeFailed;
        }

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentDropped -=
                HandleEquipmentDropped;

        if (growthManager != null)
            growthManager.OnUpgraded -= HandleGrowthUpdated;

        if (tutorialManager != null)
            tutorialManager.OnTutorialChanged -= RefreshTutorial;

        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnPlayerDataChanged -= RefreshAll;

        if (AccountLinkManager.Instance != null)
            AccountLinkManager.Instance.OnAccountChanged -= RefreshAccount;

        if (MonetizationManager.Instance != null)
        {
            MonetizationManager.Instance.OnMonetizationChanged -=
                RefreshShop;
        }
    }
}
