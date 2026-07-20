using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class MainGameUI : MonoBehaviour
{
    private MainGameBootstrap bootstrap;
    private BattleManager battleManager;
    private GrowthManager growthManager;
    private TutorialManager tutorialManager;
    private CompanionManager companionManager;
    private MainGameEventSubscriptions eventSubscriptions;

    private GameObject battlePanel;
    private GameObject growthPanel;
    private GameObject gachaPanel;
    private GameObject morePanel;
    private GameObject collectionPanel;
    private GameObject equipmentPanel;
    private GameObject suppliesPanel;
    private GameObject questPanel;
    private GameObject shopPanel;
    private GameObject eventPanel;
    private GameObject settingsPanel;
    private GameObject accountPanel;
    private TMP_Text autoAdvanceText;
    private BattleHudUI battleHud;
    private BattleActionController battleActions;
    private GrowthActionController growthActions;
    private TopBarUI topBarUI;
    private ToastUI toastUI;
    private WorldBackdropUI worldBackdropUI;
    private BottomNavigationUI bottomNavigationUI;
    private MainGameNavigationController navigation;
    private SessionActionController sessionActions;
    private OfflineRewardUI offlineRewardUI;
    private TitleScreenUI titleScreenUI;
    private LoadingOverlayUI loadingOverlayUI;
    private TutorialPanelUI tutorialPanelUI;
    private TutorialFlowController tutorialFlow;
    private StoryIntroUI storyIntroUI;
    private GrowthPanelUI growthPanelUI;
    private GachaPanelUI gachaPanelUI;
    private GachaFlowController gachaFlow;
    private CollectionPanelUI collectionPanelUI;
    private CompanionActionController companionActions;
    private QuestPanelUI questPanelUI;
    private EquipmentPanelUI equipmentPanelUI;
    private EquipmentActionController equipmentActions;
    private EquipmentCubeModalUI equipmentCubeModalUI;
    private EquipmentActionFlowModalUI equipmentActionFlowModalUI;
    private EquipmentEnhancementResultModalUI equipmentEnhancementResultModalUI;
    private EquipmentInventoryModalUI equipmentInventoryModalUI;
    private EquipmentItemActionModalUI equipmentItemActionModalUI;
    private FlightSuppliesPanelUI flightSuppliesPanelUI;
    private MorePanelUI morePanelUI;
    private ShopPanelUI shopPanelUI;
    private ShopActionController shopActions;
    private EventPanelUI eventPanelUI;
    private SettingsPanelUI settingsPanelUI;
    private SettingsActionController settingsActions;
    private AccountPanelUI accountPanelUI;
    private AccountActionController accountActions;
    private RewardActionController rewardActions;
    private RectTransform portraitRoot;
    private readonly NotificationBadgeController notificationBadges =
        new NotificationBadgeController();

    private static readonly Color Background =
        new Color32(20, 28, 45, 255);

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
        sessionActions = new SessionActionController(bootstrap);

        BuildInterface();
        BindEvents();
        LocalizationManager.ApplyTo(portraitRoot);
        ShowBattle();
    }

    public void RefreshAll()
    {
        RefreshTopBar();
        RefreshBattle();
        RefreshGrowth();
        RefreshGacha();
        RefreshMore();
        RefreshCollection();
        RefreshEquipment();
        RefreshSupplies();
        RefreshQuests();
        RefreshShop();
        RefreshEvent();
        RefreshSettings();
        RefreshAccount();
        RefreshTutorial();
        RefreshStoryIntro();
        LocalizationManager.ApplyTo(portraitRoot);
        RefreshNotificationBadges();
    }

    public void SetLoading(bool visible, string message)
    {
        loadingOverlayUI?.SetLoading(visible, message);
    }

    public void ShowInitializationError(string message)
    {
        loadingOverlayUI?.ShowError(message);
    }

    public void ShowTitleScreen(string status)
    {
        titleScreenUI?.Show(status);
    }

    public void HideTitleScreen()
    {
        titleScreenUI?.Hide();
    }

    public void SetTitleBusy(bool busy, string status)
    {
        titleScreenUI?.SetBusy(busy, status);
    }

    public void ShowOfflineReward(long seconds, int gold)
    {
        offlineRewardUI?.Show(seconds, gold);
    }

    public void ShowToast(string message)
    {
        toastUI?.Show(message);
    }

    private void Update()
    {
        toastUI?.Update(Time.unscaledDeltaTime);
        battleHud?.UpdateAnimations(
            BattleTempo.ScaleDeltaTime(Time.unscaledDeltaTime));
    }

    private void BuildInterface()
    {
        EnsureEventSystem();

        portraitRoot =
            MobileScreenLayout.CreateSafeAreaCanvas(
                "MainGameCanvas",
                Background);

        BuildWorldBackdrop(portraitRoot);
        BuildTopBar(portraitRoot);
        BuildBattlePanel(portraitRoot);
        BuildGrowthPanel(portraitRoot);
        BuildGachaPanel(portraitRoot);
        BuildMorePanel(portraitRoot);
        BuildCollectionPanel(portraitRoot);
        BuildEquipmentPanel(portraitRoot);
        BuildFlightSuppliesPanel(portraitRoot);
        BuildQuestPanel(portraitRoot);
        BuildShopPanel(portraitRoot);
        BuildEventPanel(portraitRoot);
        BuildSettingsPanel(portraitRoot);
        BuildAccountPanel(portraitRoot);
        BuildBottomNavigation(portraitRoot);
        BuildTutorial(portraitRoot);
        BuildOfflinePopup(portraitRoot);
        BuildToast(portraitRoot);
        BuildTitleScreen(portraitRoot);
        BuildStoryIntro(portraitRoot);
        BuildLoadingOverlay(portraitRoot);
        LocalizationManager.ApplyTo(portraitRoot);
    }

    private void BuildWorldBackdrop(RectTransform root)
    {
        int stage = PlayerDataManager.Instance?.playerData?.currentStage ?? 1;
        worldBackdropUI = new WorldBackdropUI(root, stage);
    }

    private void BuildTopBar(RectTransform root)
    {
        battleActions = new BattleActionController(
            battleManager,
            bootstrap,
            ShowToast);
        topBarUI = new TopBarUI(
            root,
            () => ChangeStage(-1),
            () => ChangeStage(1));
    }

    private void BuildBattlePanel(RectTransform root)
    {
        battleHud = new BattleHudUI(
            battleManager,
            companionManager,
            ToggleAutoAdvance,
            ShowQuests,
            ShowEvent,
            ShowShop,
            ShowEquipment,
            ShowToast);
        battlePanel = battleHud.Build(root).gameObject;
        autoAdvanceText = battleHud.AutoAdvanceText;
        notificationBadges.RegisterBattleHud(battleHud);
    }

    private void BuildGrowthPanel(RectTransform root)
    {
        growthActions = new GrowthActionController(
            growthManager,
            ShowToast,
            RefreshGrowth,
            RefreshBattle,
            RefreshTopBar);
        growthPanelUI = new GrowthPanelUI(
            root,
            () => growthActions?.Upgrade(UpgradeType.Attack),
            () => growthActions?.Upgrade(UpgradeType.Health),
            () => growthActions?.Upgrade(UpgradeType.AttackSpeed));
        growthPanel = growthPanelUI.GameObject;
    }

    private void BuildGachaPanel(RectTransform root)
    {
        gachaPanelUI = new GachaPanelUI(
            root,
            count => gachaFlow?.Roll(count),
            () => gachaFlow?.ClearResult());
        gachaPanel = gachaPanelUI.GameObject;
        gachaFlow = new GachaFlowController(
            gachaPanelUI,
            companionManager,
            RefreshGacha,
            ShowBattle);
    }

    private void BuildMorePanel(RectTransform root)
    {
        rewardActions = new RewardActionController(
            ShowToast,
            RefreshTopBar,
            RefreshMore,
            RefreshQuests,
            RefreshEvent);
        morePanelUI = new MorePanelUI(
            root,
            rewardActions.ClaimAllMail,
            ShowCollection,
            () => companionActions?.AutoEquip(),
            rewardActions.ClaimDailyReward,
            ShowQuests,
            ShowEvent,
            ShowShop,
            sessionActions.SaveNow,
            ShowSettings,
            ShowAccount);
        morePanel = morePanelUI.GameObject;
        notificationBadges.RegisterMorePanel(morePanelUI);
    }

    private void BuildCollectionPanel(RectTransform root)
    {
        companionActions = new CompanionActionController(
            companionManager,
            battleManager,
            bootstrap,
            ShowToast,
            RefreshCollection,
            RefreshBattle);
        collectionPanelUI = new CollectionPanelUI(
            root,
            companionManager,
            ShowMore,
            companionActions.Select,
            companionActions.PromoteSelected,
            companionActions.ToggleSelectedSlot);
        collectionPanel = collectionPanelUI.GameObject;
    }

    private void BuildEquipmentPanel(RectTransform root)
    {
        equipmentEnhancementResultModalUI =
            new EquipmentEnhancementResultModalUI(root);
        equipmentCubeModalUI = new EquipmentCubeModalUI(
            root,
            applyNew =>
            {
                equipmentActions?.ResolveCubePreview(applyNew);
                equipmentActionFlowModalUI?.RefreshCurrentSelection();
            });
        equipmentActions = new EquipmentActionController(
            battleManager,
            ShowToast,
            RefreshEquipment,
            preview => equipmentCubeModalUI?.Show(preview));
        equipmentItemActionModalUI = new EquipmentItemActionModalUI(
            root,
            instanceId => equipmentActions?.Equip(instanceId) ?? false,
            instanceId => equipmentActions?.Dismantle(instanceId) ?? false,
            () => equipmentInventoryModalUI?.Refresh());
        equipmentInventoryModalUI = new EquipmentInventoryModalUI(
            root,
            instance => equipmentItemActionModalUI?.Show(instance));
        equipmentActionFlowModalUI = new EquipmentActionFlowModalUI(
            root,
            (action, slot) =>
            {
                if (action == EquipmentActionKind.Enhancement)
                {
                    return equipmentActions?.Upgrade(
                        slot,
                        result => equipmentEnhancementResultModalUI?.Show(
                            result)) ?? false;
                }

                return equipmentActions?.RerollOptions(slot) ?? false;
            });
        equipmentPanelUI = new EquipmentPanelUI(
            root,
            ShowMore,
            () => equipmentActionFlowModalUI?.ShowSelection(
                EquipmentActionKind.Enhancement),
            () => equipmentActionFlowModalUI?.ShowSelection(
                EquipmentActionKind.OptionReset),
            () => equipmentInventoryModalUI?.Show(),
            () => equipmentInventoryModalUI?.Show());
        equipmentPanel = equipmentPanelUI.GameObject;
    }

    private void BuildQuestPanel(RectTransform root)
    {
        questPanelUI = new QuestPanelUI(
            root,
            ShowMore,
            () => rewardActions?.ClaimCurrentQuest(),
            () => rewardActions?.ClaimAchievements());
        questPanel = questPanelUI.GameObject;
    }

    private void BuildFlightSuppliesPanel(RectTransform root)
    {
        flightSuppliesPanelUI = new FlightSuppliesPanelUI(
            root,
            () => equipmentInventoryModalUI?.Show());
        suppliesPanel = flightSuppliesPanelUI.GameObject;
    }
    private void BuildShopPanel(RectTransform root)
    {
        shopActions = new ShopActionController(
            ShowToast,
            RefreshTopBar,
            RefreshMore,
            RefreshShop);
        shopPanelUI = new ShopPanelUI(
            root,
            ShowMore,
            () => shopActions?.BuyRealMoneyProduct(
                RealMoneyProduct.StarterPack),
            () => shopActions?.BuyRealMoneyProduct(
                RealMoneyProduct.GemPackSmall),
            () => shopActions?.BuyRealMoneyProduct(
                RealMoneyProduct.GemPackLarge),
            shopActions.WatchRewardedAd,
            () => shopActions?.BuyShopProduct(ShopProduct.GoldPouch),
            () => shopActions?.BuyShopProduct(ShopProduct.TicketBundle),
            () => shopActions?.BuyShopProduct(ShopProduct.GrowthChest));
        shopPanel = shopPanelUI.GameObject;
    }

    private void BuildEventPanel(RectTransform root)
    {
        eventPanelUI = new EventPanelUI(
            root,
            ShowMore,
            () => rewardActions?.ClaimEventReward());
        eventPanel = eventPanelUI.GameObject;
    }
    private void BuildSettingsPanel(RectTransform root)
    {
        settingsActions = new SettingsActionController(
            RefreshSettings,
            RefreshAll,
            ShowToast);
        settingsPanelUI = new SettingsPanelUI(
            root,
            ShowMore,
            settingsActions.ToggleSound,
            settingsActions.ToggleVibration,
            settingsActions.ToggleNotifications,
            settingsActions.ToggleFrameRate,
            settingsActions.ToggleLanguage);
        settingsPanel = settingsPanelUI.GameObject;
    }
    private void BuildAccountPanel(RectTransform root)
    {
        accountActions = new AccountActionController(
            bootstrap,
            RefreshAccount,
            RefreshMore,
            ShowToast);
        accountPanelUI = new AccountPanelUI(
            root,
            ShowMore,
            accountActions.LinkGoogle,
            sessionActions.Logout);
        accountPanel = accountPanelUI.GameObject;
    }

    private void BuildBottomNavigation(RectTransform root)
    {
        bottomNavigationUI = new BottomNavigationUI(
            root,
            ShowBattle,
            ShowGrowth,
            ShowGacha,
            ShowCollection,
            ShowEquipment,
            ShowSupplies,
            ShowMore);
        notificationBadges.RegisterBottomNavigation(bottomNavigationUI);
        navigation = new MainGameNavigationController(
            bottomNavigationUI,
            battlePanel,
            growthPanel,
            gachaPanel,
            morePanel,
            collectionPanel,
            equipmentPanel,
            suppliesPanel,
            questPanel,
            shopPanel,
            eventPanel,
            settingsPanel,
            accountPanel);
    }

    private void BuildTutorial(RectTransform root)
    {
        tutorialPanelUI = new TutorialPanelUI(
            root,
            GetTutorialFlow().HandleTutorialAction);
    }

    private void BuildStoryIntro(RectTransform root)
    {
        storyIntroUI = new StoryIntroUI(
            root,
            GetTutorialFlow().HandleStoryIntroNext,
            GetTutorialFlow().HandleStoryIntroPrevious);
    }

    private void BuildOfflinePopup(RectTransform root)
    {
        offlineRewardUI = new OfflineRewardUI(root);
    }

    private void BuildToast(RectTransform root)
    {
        toastUI = new ToastUI(root);
    }

    private void BuildTitleScreen(RectTransform root)
    {
        titleScreenUI = new TitleScreenUI(
            root,
            sessionActions.StartGoogleLogin,
            sessionActions.StartGuestLogin);
    }

    private void BuildLoadingOverlay(RectTransform root)
    {
        loadingOverlayUI = new LoadingOverlayUI(
            root,
            sessionActions.RetryInitialization);
    }

    private void BindEvents()
    {
        eventSubscriptions = new MainGameEventSubscriptions(
            battleManager,
            growthManager,
            tutorialManager,
            companionManager,
            battleHud,
            RefreshBattle,
            HandleEquipmentDropped,
            HandleEquipmentChanged,
            HandleGrowthUpdated,
            RefreshTutorial,
            HandleCompanionChanged,
            RefreshAll,
            RefreshAccount,
            RefreshShop);
        eventSubscriptions.Bind();
    }

    private void RefreshTopBar()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        topBarUI?.Refresh(data);
        if (autoAdvanceText != null)
        {
            autoAdvanceText.text =
                data != null && data.autoAdvance
                    ? LocalizationManager.Text("AUTO ON", "자동 진행")
                    : LocalizationManager.Text("REPEAT", "반복");
        }
    }

    private void RefreshBattle()
    {
        if (battleManager == null || !battleManager.IsInitialized)
            return;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        RefreshWorldBackdrop(data.currentStage);
        battleHud?.Refresh();
        RefreshTopBar();
    }

    private void RefreshWorldBackdrop(int stage)
    {
        worldBackdropUI?.Refresh(stage);
    }

    private void RefreshGrowth()
    {
        growthPanelUI?.Refresh(
            growthManager,
            battleManager,
            PlayerDataManager.Instance?.playerData);
    }

    private void RefreshGacha()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        gachaPanelUI?.Refresh(data);
    }
    private void RefreshMore()
    {
        morePanelUI?.Refresh(
            PlayerDataManager.Instance?.playerData,
            companionManager,
            AccountLinkManager.Instance,
            DailyRewardManager.Instance);
    }

    private void RefreshCollection()
    {
        if (companionManager == null)
            return;

        companionActions?.EnsureSelected();
        CharacterData selectedCharacter =
            companionActions?.SelectedCharacter;
        collectionPanelUI?.Refresh(selectedCharacter, companionManager);
    }

    private void RefreshEquipment()
    {
        equipmentPanelUI?.Refresh(PlayerDataManager.Instance?.playerData);
    }

    private void RefreshSupplies()
    {
        flightSuppliesPanelUI?.Refresh(
            PlayerDataManager.Instance?.playerData);
    }

    private void RefreshQuests()
    {
        questPanelUI?.Refresh(
            QuestManager.Instance,
            PlayerDataManager.Instance?.playerData);
    }
    private void RefreshShop()
    {
        shopPanelUI?.Refresh(
            PlayerDataManager.Instance?.playerData,
            MonetizationManager.Instance);
    }

    private void RefreshEvent()
    {
        eventPanelUI?.Refresh(
            EventMissionManager.Instance,
            PlayerDataManager.Instance?.playerData);
    }
    private void RefreshNotificationBadges()
    {
        notificationBadges.Refresh(
            NotificationBadgePolicy.Evaluate(
                PlayerDataManager.Instance?.playerData,
                growthManager,
                companionManager));
    }

    private void RefreshSettings()
    {
        settingsPanelUI?.Refresh(GameSettingsManager.Instance);
    }
    private void RefreshTutorial()
    {
        if (tutorialManager == null)
            return;

        RefreshStoryIntro();
        tutorialPanelUI?.Refresh(
            tutorialManager,
            tutorialManager.ShouldShowStoryIntro);
    }

    private void RefreshStoryIntro()
    {
        storyIntroUI?.Refresh(tutorialManager);
    }

    private void RefreshAccount()
    {
        accountPanelUI?.Refresh(AccountLinkManager.Instance);
    }

    private void HandleCompanionChanged()
    {
        battleManager?.RefreshPlayerStats();
        RefreshCollection();
        battleHud?.RefreshVisuals();
        battleHud?.RefreshSkillStatus();
        RefreshTopBar();
    }

    private void HandleEquipmentDropped(string itemName)
    {
        equipmentActions?.HandleDropped(itemName);
    }

    private void HandleEquipmentChanged()
    {
        battleManager?.RefreshPlayerStats();
        RefreshGrowth();
        RefreshTopBar();
        RefreshSupplies();
    }

    private void HandleGrowthUpdated(UpgradeType type)
    {
        RefreshGrowth();
        RefreshTopBar();
    }

    private void ChangeStage(int direction)
    {
        battleActions?.ChangeStage(direction);
    }

    private void ToggleAutoAdvance()
    {
        battleActions?.ToggleAutoAdvance();
    }

    private void ShowBattle()
    {
        navigation?.ShowBattle();
        RefreshBattle();
    }

    private void ShowGrowth()
    {
        navigation?.ShowGrowth();
        RefreshGrowth();
    }

    private void ShowGacha()
    {
        navigation?.ShowGacha();
        RefreshGacha();
    }

    private void ShowMore()
    {
        navigation?.ShowMore();
        RefreshMore();
    }

    private void ShowCollection()
    {
        navigation?.ShowCollection();
        RefreshCollection();
    }

    private void ShowEquipment()
    {
        navigation?.ShowEquipment();
        RefreshEquipment();
    }

    private void ShowSupplies()
    {
        navigation?.ShowSupplies();
        RefreshSupplies();
    }

    private void ShowQuests()
    {
        navigation?.ShowQuests();
        RefreshQuests();
    }

    private void ShowShop()
    {
        navigation?.ShowShop();
        RefreshShop();
    }

    private void ShowEvent()
    {
        navigation?.ShowEvent();
        RefreshEvent();
    }

    private void ShowSettings()
    {
        navigation?.ShowSettings();
        RefreshSettings();
    }

    private void ShowAccount()
    {
        navigation?.ShowAccount();
        RefreshAccount();
    }

    private TutorialFlowController GetTutorialFlow()
    {
        if (tutorialFlow == null)
        {
            tutorialFlow = new TutorialFlowController(
                tutorialManager,
                ShowBattle,
                ShowGrowth,
                ShowGacha,
                RefreshTutorial);
        }

        return tutorialFlow;
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
        eventSubscriptions?.Dispose();
    }
}
