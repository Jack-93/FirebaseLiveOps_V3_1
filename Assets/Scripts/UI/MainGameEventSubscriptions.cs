using System;

public sealed class MainGameEventSubscriptions : IDisposable
{
    private readonly BattleManager battleManager;
    private readonly GrowthManager growthManager;
    private readonly TutorialManager tutorialManager;
    private readonly CompanionManager companionManager;
    private readonly BattleHudUI battleHud;
    private readonly EquipmentManager equipmentManager;
    private readonly PlayerDataManager playerDataManager;
    private readonly AccountLinkManager accountLinkManager;
    private readonly MonetizationManager monetizationManager;
    private readonly Action refreshBattle;
    private readonly Action<string> handleEquipmentDropped;
    private readonly Action handleEquipmentChanged;
    private readonly Action<UpgradeType> handleGrowthUpdated;
    private readonly Action refreshTutorial;
    private readonly Action handleCompanionChanged;
    private readonly Action refreshAll;
    private readonly Action refreshAccount;
    private readonly Action refreshShop;
    private bool isBound;

    public MainGameEventSubscriptions(
        BattleManager battleManager,
        GrowthManager growthManager,
        TutorialManager tutorialManager,
        CompanionManager companionManager,
        BattleHudUI battleHud,
        Action refreshBattle,
        Action<string> handleEquipmentDropped,
        Action handleEquipmentChanged,
        Action<UpgradeType> handleGrowthUpdated,
        Action refreshTutorial,
        Action handleCompanionChanged,
        Action refreshAll,
        Action refreshAccount,
        Action refreshShop)
    {
        this.battleManager = battleManager;
        this.growthManager = growthManager;
        this.tutorialManager = tutorialManager;
        this.companionManager = companionManager;
        this.battleHud = battleHud;
        this.refreshBattle = refreshBattle;
        this.handleEquipmentDropped = handleEquipmentDropped;
        this.handleEquipmentChanged = handleEquipmentChanged;
        this.handleGrowthUpdated = handleGrowthUpdated;
        this.refreshTutorial = refreshTutorial;
        this.handleCompanionChanged = handleCompanionChanged;
        this.refreshAll = refreshAll;
        this.refreshAccount = refreshAccount;
        this.refreshShop = refreshShop;

        equipmentManager = EquipmentManager.Instance;
        playerDataManager = PlayerDataManager.Instance;
        accountLinkManager = AccountLinkManager.Instance;
        monetizationManager = MonetizationManager.Instance;
    }

    public void Bind()
    {
        if (isBound)
            return;

        if (battleManager != null)
        {
            battleManager.OnBattleStateChanged += refreshBattle;
            BindBattleHud();
        }

        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentDropped += handleEquipmentDropped;
            equipmentManager.OnEquipmentChanged += handleEquipmentChanged;
        }

        if (growthManager != null)
            growthManager.OnUpgraded += handleGrowthUpdated;

        if (tutorialManager != null)
            tutorialManager.OnTutorialChanged += refreshTutorial;

        if (companionManager != null)
            companionManager.OnCompanionChanged += handleCompanionChanged;

        if (playerDataManager != null)
            playerDataManager.OnPlayerDataChanged += refreshAll;

        if (accountLinkManager != null)
            accountLinkManager.OnAccountChanged += refreshAccount;

        if (monetizationManager != null)
            monetizationManager.OnMonetizationChanged += refreshShop;

        isBound = true;
    }

    public void Dispose()
    {
        if (!isBound)
            return;

        if (battleManager != null)
        {
            battleManager.OnBattleStateChanged -= refreshBattle;
            UnbindBattleHud();
        }

        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentDropped -= handleEquipmentDropped;
            equipmentManager.OnEquipmentChanged -= handleEquipmentChanged;
        }

        if (growthManager != null)
            growthManager.OnUpgraded -= handleGrowthUpdated;

        if (tutorialManager != null)
            tutorialManager.OnTutorialChanged -= refreshTutorial;

        if (companionManager != null)
            companionManager.OnCompanionChanged -= handleCompanionChanged;

        if (playerDataManager != null)
            playerDataManager.OnPlayerDataChanged -= refreshAll;

        if (accountLinkManager != null)
            accountLinkManager.OnAccountChanged -= refreshAccount;

        if (monetizationManager != null)
            monetizationManager.OnMonetizationChanged -= refreshShop;

        isBound = false;
    }

    private void BindBattleHud()
    {
        if (battleHud == null)
            return;

        battleManager.OnCompanionBasicAttackPerformed +=
            battleHud.HandleCompanionBasicAttackVisual;
        battleManager.OnEnemyAttackPerformed +=
            battleHud.HandleEnemyAttackVisual;
        battleManager.OnEnemyDefeatedVisual +=
            battleHud.HandleEnemyDefeatedVisual;
        battleManager.OnPlayerDefeated +=
            battleHud.HandlePlayerDefeatedVisual;
        battleManager.OnCompanionSkillUsed +=
            battleHud.HandleCompanionSkillVisual;
        battleManager.OnBossPatternUsed +=
            battleHud.HandleBossPatternVisual;
        battleManager.OnBossPatternWarning +=
            battleHud.HandleBossPatternWarningVisual;
        battleManager.OnBossChallengeFailed +=
            battleHud.HandleBossChallengeFailed;
        battleManager.OnPowerCharged +=
            battleHud.HandlePowerChargedVisual;
    }

    private void UnbindBattleHud()
    {
        if (battleHud == null)
            return;

        battleManager.OnCompanionBasicAttackPerformed -=
            battleHud.HandleCompanionBasicAttackVisual;
        battleManager.OnEnemyAttackPerformed -=
            battleHud.HandleEnemyAttackVisual;
        battleManager.OnEnemyDefeatedVisual -=
            battleHud.HandleEnemyDefeatedVisual;
        battleManager.OnPlayerDefeated -=
            battleHud.HandlePlayerDefeatedVisual;
        battleManager.OnCompanionSkillUsed -=
            battleHud.HandleCompanionSkillVisual;
        battleManager.OnBossPatternUsed -=
            battleHud.HandleBossPatternVisual;
        battleManager.OnBossPatternWarning -=
            battleHud.HandleBossPatternWarningVisual;
        battleManager.OnBossChallengeFailed -=
            battleHud.HandleBossChallengeFailed;
        battleManager.OnPowerCharged -=
            battleHud.HandlePowerChargedVisual;
    }
}
