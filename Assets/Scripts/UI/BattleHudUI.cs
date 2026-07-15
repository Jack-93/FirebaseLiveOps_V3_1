using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleHudUI
{
    private readonly BattleManager battleManager;
    private readonly CompanionManager companionManager;
    private readonly Action onAutoAdvance;
    private readonly Action onQuest;
    private readonly Action onEvent;
    private readonly Action onShop;
    private readonly Action onEquipment;
    private readonly Action<string> showToast;

    private RectTransform panel;
    private RectTransform battlefieldLayer;
    private RectTransform battlefieldBackgroundLayer;
    private RectTransform battlefieldActorLayer;
    private RectTransform battleEffectLayer;
    private RectTransform enemyActorRoot;
    private RectTransform playerActorRoot;
    private RectTransform enemyVisual;
    private RectTransform playerVisual;
    private Vector2 enemyVisualBasePosition;
    private Vector2 playerVisualBasePosition;
    private RectTransform attackTrail;
    private RectTransform skillProjectile;
    private RectTransform battleFlash;
    private readonly RectTransform[] sparkleRects =
        new RectTransform[SparkleCount];
    private RectTransform bossWarningPanel;
    private RectTransform enemyDamagePopup;
    private RectTransform playerDamagePopup;
    private RectTransform powerChargePopup;
    private RectTransform rewardPopup;

    private TMP_Text autoAdvanceText;
    private TMP_Text enemyDamageText;
    private TMP_Text playerDamageText;
    private TMP_Text powerChargePopupText;
    private TMP_Text rewardPopupText;
    private TMP_Text bossWarningText;
    private TMP_Text enemyDamageNumberText;
    private TMP_Text playerDamageNumberText;
    private SpriteNumberText rewardNumberText;

    private Image enemyVisualImage;
    private Image playerVisualImage;
    private Image battlefieldBackgroundImage;
    private Image battlefieldFarBackgroundImage;
    private Image battlefieldMidgroundImage;
    private Image battlefieldGroundImage;
    private Image battlefieldForegroundImage;
    private Image attackTrailImage;
    private Image skillProjectileImage;
    private Image battleFlashImage;
    private Image rewardIconImage;
    private readonly Image[] sparkleImages =
        new Image[SparkleCount];

    private BattleActorView enemyActorView;
    private BattleActorView playerActorView;
    private readonly BattleActorView[] companionActorViews =
        new BattleActorView[CompanionManager.PartySize];
    private readonly RectTransform[] companionActorRoots =
        new RectTransform[CompanionManager.PartySize];
    private readonly RectTransform[] companionVisualRects =
        new RectTransform[CompanionManager.PartySize];
    private readonly Vector2[] companionVisualBasePositions =
        new Vector2[CompanionManager.PartySize];
    private readonly RectTransform[] companionProjectileRects =
        new RectTransform[CompanionManager.PartySize];
    private readonly RectTransform[] companionProjectileTrailRects =
        new RectTransform[CompanionManager.PartySize];
    private readonly Image[] companionProjectileImages =
        new Image[CompanionManager.PartySize];
    private readonly Image[] companionProjectileTrailImages =
        new Image[CompanionManager.PartySize];
    private readonly Sprite[] companionProjectileSprites =
        new Sprite[CompanionManager.PartySize];
    private BattleStatusHudUI statusHud;
    private BattleSkillControlsUI skillControls;
    private BattleQuickButtonsUI quickButtons;

    private float enemyAnimationTimer;
    private float playerAnimationTimer;
    private float playerDefeatTimer;
    private float enemyHitShakeTimer;
    private float enemyDefeatPopTimer;
    private float playerHitShakeTimer;
    private float attackTrailTimer;
    private float skillProjectileTimer;
    private float battleFlashTimer;
    private float bossWarningTimer;
    private float enemyDamagePopupTimer;
    private float playerDamagePopupTimer;
    private float powerChargePopupTimer;
    private float rewardPopupTimer;
    private float rewardPopupDuration = 0.9f;
    private float lastObservedPowerCharge;
    private int skillProjectileSlot = -1;
    private Color battleFlashColor = Color.white;
    private Color skillProjectileColor = Accent;
    private float skillProjectileDuration = 0.32f;
    private readonly float[] companionSkillTimers =
        new float[CompanionManager.PartySize];
    private readonly float[] companionAnimationDurations =
        new float[CompanionManager.PartySize];
    private readonly float[] companionProjectileTimers =
        new float[CompanionManager.PartySize];
    private readonly float[] companionProjectileDurations =
        new float[CompanionManager.PartySize];
    private readonly Color[] companionProjectileColors =
        new Color[CompanionManager.PartySize];
    private readonly float[] companionProjectileSizes =
        new float[CompanionManager.PartySize];
    private readonly float[] sparkleTimers =
        new float[SparkleCount];
    private readonly float[] sparkleDurations =
        new float[SparkleCount];
    private readonly float[] sparkleDistances =
        new float[SparkleCount];
    private readonly Vector2[] sparkleAnchors =
        new Vector2[SparkleCount];
    private readonly Vector2[] sparkleDirections =
        new Vector2[SparkleCount];
    private readonly Color[] sparkleColors =
        new Color[SparkleCount];
    private readonly Vector3[] actorWorldCorners = new Vector3[4];

    private const int SparkleCount = 8;
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

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

    private const string BattleHudPrefabResourcePath =
        "Prefabs/UI/BattleHud";

    public GameObject GameObject => panel == null ? null : panel.gameObject;
    public TMP_Text AutoAdvanceText => autoAdvanceText;
    public RectTransform QuestQuickBadge => quickButtons?.QuestQuickBadge;
    public RectTransform EventQuickBadge => quickButtons?.EventQuickBadge;
    public RectTransform ShopQuickBadge => quickButtons?.ShopQuickBadge;
    public RectTransform EquipmentQuickBadge =>
        quickButtons?.EquipmentQuickBadge;

    public BattleHudUI(
        BattleManager battleManager,
        CompanionManager companionManager,
        Action onAutoAdvance,
        Action onQuest,
        Action onEvent,
        Action onShop,
        Action onEquipment,
        Action<string> showToast)
    {
        this.battleManager = battleManager;
        this.companionManager = companionManager;
        this.onAutoAdvance = onAutoAdvance;
        this.onQuest = onQuest;
        this.onEvent = onEvent;
        this.onShop = onShop;
        this.onEquipment = onEquipment;
        this.showToast = showToast;
    }

    public RectTransform Build(RectTransform root)
    {
        if (TryBuildFromPrefab(root))
            return panel;

        return BuildGenerated(root);
    }

    public RectTransform BuildGenerated(RectTransform root)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "BattlePanel",
            root,
            new Color32(0, 0, 0, 0),
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateText(
            "BattleTitle",
            panel,
            "BATTLE",
            30,
            new Vector2(0.06f, 0.91f),
            new Vector2(0.3f, 0.97f),
            TextAlignmentOptions.Left,
            Accent);

        Button autoButton = RuntimeUiFactory.CreateButton(
            "AutoAdvanceButton",
            panel,
            "AUTO ON",
            new Vector2(0.75f, 0.91f),
            new Vector2(0.94f, 0.97f),
            PanelLight,
            () => onAutoAdvance?.Invoke());
        autoAdvanceText = autoButton.GetComponentInChildren<TMP_Text>();

        RectTransform enemyCard = RuntimeUiFactory.CreatePanel(
            "EnemyCard",
            panel,
            new Color32(0, 0, 0, 0),
            new Vector2(0.02f, 0.34f),
            new Vector2(0.98f, 0.91f));
        BuildBattlefieldLayers(enemyCard);

        statusHud = new BattleStatusHudUI(Danger, Accent, Success);
        statusHud.BuildEnemyName(enemyCard);

        BuildBossWarning(enemyCard);
        BuildEnemyActor(battlefieldActorLayer);
        BuildEnemyPopups(battleEffectLayer);
        BuildCombatEffects(battleEffectLayer);
        statusHud.BuildEnemyHealth(enemyCard);
        BuildPlayerSide(battlefieldActorLayer, battleEffectLayer);
        statusHud.BuildPlayerStatus(enemyCard);
        BuildCompanionActors(battlefieldActorLayer);
        SortActorRoots();
        BuildSkillControls();
        BuildQuickButtons();

        return panel;
    }

    private bool TryBuildFromPrefab(RectTransform root)
    {
        GameObject prefab =
            Resources.Load<GameObject>(BattleHudPrefabResourcePath);
        if (prefab == null)
            return false;

        GameObject instance = UnityEngine.Object.Instantiate(
            prefab,
            root,
            false);
        instance.name = "BattlePanel";
        panel = instance.GetComponent<RectTransform>();
        if (panel == null)
            return false;

        GameFont.ApplyToHierarchy(panel);
        BindPrefab(panel);
        return true;
    }

    private void BindPrefab(RectTransform prefabPanel)
    {
        panel = prefabPanel;

        Button autoButton =
            RuntimeUiBinder.FindButton(panel, "AutoAdvanceButton");
        RuntimeUiBinder.ReplaceButtonAction(
            autoButton,
            () => onAutoAdvance?.Invoke());
        autoAdvanceText = autoButton == null
            ? null
            : autoButton.GetComponentInChildren<TMP_Text>(true);
        GameFont.Apply(autoAdvanceText, "AutoAdvanceButton");

        RectTransform enemyCard =
            RuntimeUiBinder.FindRect(panel, "EnemyCard");
        if (enemyCard == null)
        {
            enemyCard = RuntimeUiFactory.CreatePanel(
                "EnemyCard",
                panel,
                new Color32(0, 0, 0, 0),
                new Vector2(0.02f, 0.34f),
                new Vector2(0.98f, 0.91f));
        }
        BindBattlefieldLayers(enemyCard);

        statusHud = new BattleStatusHudUI(Danger, Accent, Success);
        statusHud.Bind(enemyCard);

        BindBossWarning(enemyCard);
        BindEnemyActor(battlefieldActorLayer);
        BindEnemyPopups(battleEffectLayer);
        BindCombatEffects(battleEffectLayer);
        BindPlayerSide(battlefieldActorLayer, battleEffectLayer);
        BindCompanionActors(battlefieldActorLayer);
        SortActorRoots();
        BindSkillControls();
        BindQuickButtons();
    }

    public void Refresh()
    {
        if (battleManager == null || !battleManager.IsInitialized)
            return;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        statusHud?.Refresh(battleManager, data);

        RefreshAutoAdvance(data);
        RefreshSkillStatus();
        RefreshVisuals();
        lastObservedPowerCharge = battleManager.PowerCharge;
    }

    public void RefreshAutoAdvance(PlayerData data)
    {
        if (autoAdvanceText == null || data == null)
            return;

        autoAdvanceText.text = data.autoAdvance
            ? LocalizationManager.Translate("AUTO ON")
            : LocalizationManager.Translate("REPEAT");
    }

    public void RefreshVisuals()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        RefreshBattlefieldTheme(data.currentStage);

        ApplyActorVisual(
            playerActorView,
            BattleVisualResolver.GetHero());

        ApplyActorVisual(
            enemyActorView,
            BattleVisualResolver.GetEnemy(
                data.currentStage,
                battleManager.IsBoss));

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            CharacterData character =
                companionManager?.GetEquippedAtSlot(slot);
            ApplyActorVisual(
                companionActorViews[slot],
                character?.ResolveBattleVisual());
        }
    }

    private static void ApplyActorVisual(
        BattleActorView actorView,
        BattleActorVisualSet visual)
    {
        if (actorView == null)
            return;

        actorView.SetVisual(
            visual?.sprite,
            visual?.animatorController);
        actorView.SetSpriteAnimations(
            visual?.animatorController == null
                ? visual?.CreateAnimationLookup()
                : null);
    }

    public void RefreshSkillStatus()
    {
        skillControls?.Refresh();
    }

    public void HandlePlayerAttackVisual(int damage)
    {
        playerAnimationTimer = 0.18f;
        enemyAnimationTimer = 0.25f;
        enemyHitShakeTimer = 0.22f;
        attackTrailTimer = 0.18f;
        StartSparkles(
            GetEnemyImpactAnchor(),
            Success,
            0.28f,
            0.075f);
        StartBattleFlash(Accent, 0.09f);
        ShowEnemyDamage(
            damage,
            LocalizationManager.Text("SUPPORT", "\uC9C0\uC6D0"),
            Success,
            0.58f);
        playerActorView?.Play(BattleAnimationCue.Attack);
    }

    public void HandleCompanionBasicAttackVisual(
        int slot,
        CharacterData character,
        int damage)
    {
        enemyAnimationTimer = 0.22f;
        enemyHitShakeTimer = 0.2f;
        attackTrailTimer = 0.14f;
        StartSparkles(
            GetEnemyImpactAnchor(),
            Gold,
            0.28f,
            0.075f);
        StartBattleFlash(Gold, 0.06f);
        ShowEnemyDamage(
            damage,
            character?.characterName ?? $"S{slot + 1}",
            Gold,
            0.55f);
        if (slot >= 0 && slot < companionActorViews.Length)
        {
            companionSkillTimers[slot] = 0.56f;
            companionAnimationDurations[slot] = 0.56f;
            companionActorViews[slot]?.Play(BattleAnimationCue.Attack);
            StartSkillProjectile(
                slot,
                character,
                false,
                Gold,
                0.42f,
                42f);
        }
    }

    public void HandlePowerChargedVisual(float current, float max)
    {
        playerAnimationTimer = 0.14f;
        StartBattleFlash(Success, 0.05f);
        ShowPowerChargePopup(current, max);
        playerActorView?.Play(BattleAnimationCue.Skill);
        RefreshSkillStatus();
        lastObservedPowerCharge = current;
    }

    public void HandleEnemyAttackVisual(int damage)
    {
        enemyAnimationTimer = 0.18f;
        playerAnimationTimer = 0.25f;
        playerHitShakeTimer = 0.24f;
        StartSparkles(
            GetSupportImpactAnchor(),
            Danger,
            0.25f,
            0.055f);
        StartBattleFlash(Danger, 0.13f);
        ShowPlayerDamage(damage);
        enemyActorView?.Play(BattleAnimationCue.Attack);
    }

    public void HandleEnemyDefeatedVisual(int reward)
    {
        bool defeatedBoss =
            battleManager != null &&
            battleManager.LastDefeatedEnemyWasBoss;

        enemyAnimationTimer = 0.4f;
        enemyDefeatPopTimer = 0.52f;
        StartSparkles(
            GetEnemyImpactAnchor(),
            defeatedBoss ? Gold : Success,
            0.58f,
            0.15f);
        StartBattleFlash(Success, 0.16f);
        ShowRewardPopup(reward, defeatedBoss);
        enemyActorView?.Play(BattleAnimationCue.Death);
    }

    public void HandlePlayerDefeatedVisual()
    {
        playerDefeatTimer = 1.8f;
        StartBattleFlash(Danger, 0.22f);
        playerActorView?.Play(BattleAnimationCue.Death);
        foreach (BattleActorView companionActorView in companionActorViews)
        {
            companionActorView?.Play(BattleAnimationCue.Death);
        }
    }

    public void HandleCompanionSkillVisual(
        int slot,
        CharacterData character,
        int effectValue)
    {
        if (character != null &&
            character.skillEffect == CompanionSkillEffect.RepairPole)
        {
            HandleCompanionPoleRepairSkillVisual(
                slot,
                character,
                effectValue);
            return;
        }
        if (character != null &&
            character.skillEffect == CompanionSkillEffect.PartyDamageBuff)
        {
            HandleCompanionPartyDamageBuffSkillVisual(
                slot,
                character,
                effectValue);
            return;
        }

        enemyAnimationTimer = 0.35f;
        enemyHitShakeTimer = 0.3f;
        attackTrailTimer = 0.24f;
        StartSparkles(
            GetEnemyImpactAnchor(),
            Accent,
            0.38f,
            0.11f);
        StartBattleFlash(Accent, 0.18f);
        ShowEnemyDamage(
            effectValue,
            character?.skillName ?? "SKILL",
            Accent,
            0.72f);
        if (slot >= 0 && slot < companionActorViews.Length)
        {
            companionSkillTimers[slot] = 0.84f;
            companionAnimationDurations[slot] = 0.84f;
            companionActorViews[slot]?.Play(BattleAnimationCue.Skill);
            StartSkillProjectile(
                slot,
                character,
                true,
                Accent,
                0.56f,
                60f);
        }
        showToast?.Invoke(
            $"{character?.skillName ?? "Skill"}  DMG " +
            CompactNumberFormatter.Format(effectValue));
    }

    private void HandleCompanionPoleRepairSkillVisual(
        int slot,
        CharacterData character,
        int repairAmount)
    {
        playerAnimationTimer = 0.35f;
        attackTrailTimer = 0.18f;
        StartSparkles(
            GetSupportImpactAnchor(),
            Success,
            0.48f,
            0.12f);
        StartSparkles(
            GetCompanionImpactAnchor(slot),
            Gold,
            0.42f,
            0.1f);
        StartBattleFlash(Success, 0.14f);
        ShowPoleRepair(
            repairAmount,
            character?.skillName ?? "REPAIR",
            0.72f);
        if (slot >= 0 && slot < companionActorViews.Length)
        {
            companionSkillTimers[slot] = 0.84f;
            companionAnimationDurations[slot] = 0.84f;
            companionActorViews[slot]?.Play(BattleAnimationCue.Skill);
        }

        playerActorView?.Play(BattleAnimationCue.Skill);
        showToast?.Invoke(
            $"{character?.skillName ?? "Skill"}  \uC804\uBD07\uB300 \uC218\uB9AC +" +
            CompactNumberFormatter.Format(repairAmount));
    }

    private void HandleCompanionPartyDamageBuffSkillVisual(
        int slot,
        CharacterData character,
        int buffPercent)
    {
        playerAnimationTimer = 0.35f;
        attackTrailTimer = 0.18f;
        StartSparkles(
            GetSupportImpactAnchor(),
            Accent,
            0.58f,
            0.1f);
        StartSparkles(
            GetCompanionImpactAnchor(slot),
            Gold,
            0.46f,
            0.1f);
        StartBattleFlash(Accent, 0.14f);
        if (slot >= 0 && slot < companionActorViews.Length)
        {
            companionSkillTimers[slot] = 0.84f;
            companionAnimationDurations[slot] = 0.84f;
            companionActorViews[slot]?.Play(BattleAnimationCue.Skill);
            StartSkillProjectile(
                slot,
                character,
                true,
                Accent,
                0.56f,
                60f);
        }

        playerActorView?.Play(BattleAnimationCue.Skill);
        showToast?.Invoke(
            $"{character?.skillName ?? "Skill"}  DMG +" +
            buffPercent + "%  " +
            Mathf.RoundToInt(
                Mathf.Max(
                    0f,
                    character == null
                        ? 0f
                        : character.skillDamageBuffDuration)) +
            "s");
    }

    public void HandleBossChallengeFailed()
    {
        enemyAnimationTimer = 0.4f;
        ShowBossWarning(LocalizationManager.Translate("BOSS FAILED"));
        showToast?.Invoke("Boss time expired. Retrying.");
    }

    public void HandleBossPatternWarningVisual(
        BossPatternDefinition pattern,
        float warningSeconds)
    {
        enemyAnimationTimer = Mathf.Max(enemyAnimationTimer, 0.35f);
        enemyHitShakeTimer = Mathf.Max(enemyHitShakeTimer, 0.18f);
        StartSparkles(
            GetEnemyImpactAnchor(),
            Gold,
            Mathf.Max(0.35f, warningSeconds),
            0.1f);
        StartBattleFlash(Gold, 0.12f);
        ShowBossWarning(
            $"{LocalizationManager.Translate("BOSS WARNING")}  " +
            $"{pattern.patternName}");
        enemyActorView?.Play(BattleAnimationCue.Skill);
        showToast?.Invoke(
            $"{pattern.patternName}  " +
            LocalizationManager.Translate("Incoming"));
    }

    public void HandleBossPatternVisual(
        BossPatternDefinition pattern,
        int damage)
    {
        enemyAnimationTimer = 0.3f;
        playerAnimationTimer = 0.3f;
        playerHitShakeTimer = 0.28f;
        StartSparkles(
            GetSupportImpactAnchor(),
            Danger,
            0.34f,
            0.08f);
        StartBattleFlash(Danger, 0.2f);
        ShowBossWarning(
            $"{LocalizationManager.Translate("BOSS SKILL")}  " +
            $"{pattern.patternName}");
        ShowPlayerDamage(damage);
        enemyActorView?.Play(BattleAnimationCue.Skill);
        showToast?.Invoke(
            $"{pattern.patternName}  DMG " +
            CompactNumberFormatter.Format(damage));
    }

    public void UpdateAnimations(float deltaTime)
    {
        enemyAnimationTimer = Mathf.Max(0f, enemyAnimationTimer - deltaTime);
        playerAnimationTimer = Mathf.Max(0f, playerAnimationTimer - deltaTime);
        playerDefeatTimer = Mathf.Max(0f, playerDefeatTimer - deltaTime);
        enemyHitShakeTimer = Mathf.Max(0f, enemyHitShakeTimer - deltaTime);
        enemyDefeatPopTimer =
            Mathf.Max(0f, enemyDefeatPopTimer - deltaTime);
        playerHitShakeTimer = Mathf.Max(0f, playerHitShakeTimer - deltaTime);
        attackTrailTimer = Mathf.Max(0f, attackTrailTimer - deltaTime);
        skillProjectileTimer = Mathf.Max(0f, skillProjectileTimer - deltaTime);
        battleFlashTimer = Mathf.Max(0f, battleFlashTimer - deltaTime);
        bossWarningTimer = Mathf.Max(0f, bossWarningTimer - deltaTime);
        enemyDamagePopupTimer =
            Mathf.Max(0f, enemyDamagePopupTimer - deltaTime);
        playerDamagePopupTimer =
            Mathf.Max(0f, playerDamagePopupTimer - deltaTime);
        powerChargePopupTimer =
            Mathf.Max(0f, powerChargePopupTimer - deltaTime);
        rewardPopupTimer = Mathf.Max(0f, rewardPopupTimer - deltaTime);

        for (int slot = 0; slot < companionSkillTimers.Length; slot++)
        {
            companionSkillTimers[slot] =
                Mathf.Max(0f, companionSkillTimers[slot] - deltaTime);
            companionProjectileTimers[slot] =
                Mathf.Max(0f, companionProjectileTimers[slot] - deltaTime);
        }
        for (int index = 0; index < sparkleTimers.Length; index++)
        {
            sparkleTimers[index] =
                Mathf.Max(0f, sparkleTimers[index] - deltaTime);
        }

        UpdateAttackTrail();
        UpdateSkillProjectile();
        UpdateCompanionProjectiles();
        UpdateSparkles();
        UpdateBattleFlash();
        UpdateBossWarning();
        UpdateCompanionAnimations();
        BattleHudUiFactory.UpdateFloatingPopup(
            enemyDamagePopup,
            enemyDamageText,
            enemyDamagePopupTimer,
            0.72f,
            new Vector2(0f, 72f));
        UpdateTextAlpha(
            enemyDamageNumberText,
            enemyDamagePopupTimer,
            0.72f);
        BattleHudUiFactory.UpdateFloatingPopup(
            playerDamagePopup,
            playerDamageText,
            playerDamagePopupTimer,
            0.55f,
            new Vector2(0f, 46f));
        UpdateTextAlpha(
            playerDamageNumberText,
            playerDamagePopupTimer,
            0.55f);
        BattleHudUiFactory.UpdateFloatingPopup(
            powerChargePopup,
            powerChargePopupText,
            powerChargePopupTimer,
            0.8f,
            new Vector2(0f, 30f));
        BattleHudUiFactory.UpdateFloatingPopup(
            rewardPopup,
            rewardPopupText,
            rewardPopupTimer,
            rewardPopupDuration,
            new Vector2(0f, 44f));
        UpdateRewardPopupVisuals();
        skillControls?.Refresh();
        UpdateActorPulses();
    }

    private void BuildBossWarning(RectTransform enemyCard)
    {
        bossWarningPanel = RuntimeUiFactory.CreatePanel(
            "BossWarningPanel",
            enemyCard,
            new Color32(80, 16, 26, 225),
            new Vector2(0.24f, 0.76f),
            new Vector2(0.76f, 0.84f));
        bossWarningPanel.GetComponent<Image>().raycastTarget = false;
        bossWarningText = RuntimeUiFactory.CreateText(
            "BossWarningText",
            bossWarningPanel,
            "",
            24,
            new Vector2(0.04f, 0.08f),
            new Vector2(0.96f, 0.92f),
            TextAlignmentOptions.Center,
            Gold);
        bossWarningPanel.gameObject.SetActive(false);
    }

    private void BindBossWarning(RectTransform enemyCard)
    {
        bossWarningPanel =
            RuntimeUiBinder.FindRect(enemyCard, "BossWarningPanel");
        bossWarningText =
            RuntimeUiBinder.FindText(enemyCard, "BossWarningText");
    }

    private void BuildBattlefieldLayers(RectTransform enemyCard)
    {
        battlefieldLayer = CreateNonInteractivePanel(
            "BattlefieldLayer",
            enemyCard,
            Vector2.zero,
            Vector2.one);
        battlefieldBackgroundLayer = CreateNonInteractivePanel(
            "BattlefieldBackgroundLayer",
            battlefieldLayer,
            Vector2.zero,
            Vector2.one);
        battlefieldBackgroundImage = CreateBattlefieldImage(
            "BattlefieldBackground",
            battlefieldBackgroundLayer,
            Vector2.zero,
            Vector2.one);
        battlefieldFarBackgroundImage = CreateBattlefieldImage(
            "BattlefieldFarBackground",
            battlefieldBackgroundLayer,
            Vector2.zero,
            Vector2.one);
        battlefieldMidgroundImage = CreateBattlefieldImage(
            "BattlefieldMidground",
            battlefieldBackgroundLayer,
            Vector2.zero,
            Vector2.one);
        battlefieldGroundImage = CreateBattlefieldImage(
            "BattlefieldGround",
            battlefieldBackgroundLayer,
            Vector2.zero,
            Vector2.one);
        CreateNonInteractivePanel(
            "BattlefieldGuideLayer",
            battlefieldLayer,
            Vector2.zero,
            Vector2.one);
        battlefieldActorLayer = CreateNonInteractivePanel(
            "BattlefieldActorLayer",
            battlefieldLayer,
            Vector2.zero,
            Vector2.one);
        battleEffectLayer = CreateNonInteractivePanel(
            "BattlefieldEffectLayer",
            battlefieldLayer,
            Vector2.zero,
            Vector2.one);
        battlefieldForegroundImage = CreateBattlefieldImage(
            "BattlefieldForeground",
            battleEffectLayer,
            Vector2.zero,
            Vector2.one);
    }

    private void BindBattlefieldLayers(RectTransform enemyCard)
    {
        battlefieldLayer =
            RuntimeUiBinder.FindRect(enemyCard, "BattlefieldLayer") ??
            enemyCard;
        battlefieldBackgroundLayer =
            RuntimeUiBinder.FindRect(
                battlefieldLayer,
                "BattlefieldBackgroundLayer");
        battlefieldActorLayer =
            RuntimeUiBinder.FindRect(battlefieldLayer, "BattlefieldActorLayer")
            ?? battlefieldLayer;
        battleEffectLayer =
            RuntimeUiBinder.FindRect(battlefieldLayer, "BattlefieldEffectLayer")
            ?? battlefieldLayer;
        battlefieldBackgroundImage =
            RuntimeUiBinder.FindImage(
                battlefieldLayer,
                "BattlefieldBackground");
        battlefieldMidgroundImage =
            RuntimeUiBinder.FindImage(
                battlefieldLayer,
                "BattlefieldMidground");
        battlefieldFarBackgroundImage =
            FindOrCreateBattlefieldImage(
                "BattlefieldFarBackground",
                battlefieldBackgroundLayer,
                1);
        battlefieldGroundImage =
            FindOrCreateBattlefieldImage(
                "BattlefieldGround",
                battlefieldBackgroundLayer,
                3);
        battlefieldForegroundImage =
            RuntimeUiBinder.FindImage(
                battlefieldLayer,
                "BattlefieldForeground");
        SetBattlefieldLayerOrder();
    }

    private void RefreshBattlefieldTheme(int stage)
    {
        SetBattlefieldLayer(
            battlefieldBackgroundImage,
            BattleStageThemeResolver.GetStageBackground(stage),
            BattleStageThemeResolver.GetFallbackColor(stage),
            0.38f);
        SetBattlefieldLayer(
            battlefieldFarBackgroundImage,
            BattleStageThemeResolver.GetStageFarBackground(stage),
            Color.clear,
            0.28f);
        SetBattlefieldLayer(
            battlefieldMidgroundImage,
            BattleStageThemeResolver.GetStageMidground(stage),
            Color.clear,
            0.24f);
        SetBattlefieldLayer(
            battlefieldGroundImage,
            BattleStageThemeResolver.GetStageGround(stage),
            Color.clear,
            0.42f);
        SetBattlefieldLayer(
            battlefieldForegroundImage,
            BattleStageThemeResolver.GetStageForeground(stage),
            Color.clear,
            0.18f);
    }

    private Image FindOrCreateBattlefieldImage(
        string name,
        RectTransform parent,
        int siblingIndex)
    {
        Image image = RuntimeUiBinder.FindImage(battlefieldLayer, name);
        if (image != null)
            return image;

        if (parent == null)
            return null;

        image = CreateBattlefieldImage(
            name,
            parent,
            Vector2.zero,
            Vector2.one);
        image.transform.SetSiblingIndex(siblingIndex);
        return image;
    }

    private void SetBattlefieldLayerOrder()
    {
        SetSibling(battlefieldBackgroundImage, 0);
        SetSibling(battlefieldFarBackgroundImage, 1);
        SetSibling(battlefieldMidgroundImage, 2);
        SetSibling(battlefieldGroundImage, 3);
    }

    private static void SetSibling(Image image, int index)
    {
        if (image != null)
            image.transform.SetSiblingIndex(index);
    }

    private static Image CreateBattlefieldImage(
        string name,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform rect = RuntimeUiFactory.CreatePanel(
            name,
            parent,
            Color.clear,
            anchorMin,
            anchorMax);
        Image image = rect.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        return image;
    }

    private static void SetBattlefieldLayer(
        Image image,
        Sprite sprite,
        Color fallback,
        float alpha)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        if (sprite == null)
        {
            image.color = fallback;
        }
        else
        {
            image.color = new Color(1f, 1f, 1f, alpha);
        }
        image.raycastTarget = false;
    }

    private static RectTransform CreateNonInteractivePanel(
        string name,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform panel = RuntimeUiFactory.CreatePanel(
            name,
            parent,
            new Color32(0, 0, 0, 0),
            anchorMin,
            anchorMax);
        panel.GetComponent<Image>().raycastTarget = false;
        return panel;
    }

    private static void ApplyAnchors(
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
    }

    private static RectTransform CreateActorRoot(
        RectTransform parent,
        string rootName,
        string shadowName,
        Vector2 anchor,
        Vector2 normalizedSize,
        Color guideColor)
    {
        Vector2 halfSize = new Vector2(normalizedSize.x * 0.5f, 0f);
        RectTransform root = CreateNonInteractivePanel(
            rootName,
            parent,
            anchor - halfSize,
            anchor + new Vector2(halfSize.x, normalizedSize.y));

        Image shadowImage = RuntimeUiFactory.CreateSpriteImage(
            shadowName,
            root,
            PrototypeUiArt.ActorShadow,
            new Vector2(0.12f, -0.03f),
            new Vector2(0.88f, 0.15f));
        shadowImage.raycastTarget = false;
        shadowImage.preserveAspect = false;
        if (shadowImage.sprite == null)
            shadowImage.color = new Color32(0, 0, 0, 85);

        CreateActorAnchor(root, "DamageAnchor", 0.84f);
        CreateActorAnchor(root, "HealthAnchor", 0.98f);

        BattleSlotGuide guide = root.gameObject.AddComponent<BattleSlotGuide>();
        guide.Configure(rootName, guideColor);
        return root;
    }

    private static void CreateActorAnchor(
        RectTransform root,
        string name,
        float y)
    {
        GameObject anchor = new GameObject(name, typeof(RectTransform));
        anchor.transform.SetParent(root, false);
        RectTransform rect = anchor.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, y);
        rect.anchorMax = new Vector2(0.5f, y);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = Vector2.zero;
    }

    private void BuildEnemyActor(RectTransform enemyCard)
    {
        Vector2 enemyAnchor = BattleLayoutConfig.EnemyAnchor;
        enemyActorRoot = CreateActorRoot(
            enemyCard,
            "EnemyActorRoot",
            "EnemyShadow",
            enemyAnchor,
            new Vector2(0.22f, 0.25f),
            Danger);
        enemyVisual = RuntimeUiFactory.CreatePanel(
            "EnemyVisual",
            enemyActorRoot,
            Danger,
            new Vector2(0.08f, 0.12f),
            new Vector2(0.92f, 1f));
        enemyVisualImage = enemyVisual.GetComponent<Image>();
        enemyVisualImage.raycastTarget = false;
        enemyVisualBasePosition = enemyVisual.anchoredPosition;

        TMP_Text enemyGlyph = RuntimeUiFactory.CreateText(
            "EnemyGlyph",
            enemyVisual,
            "BOSS",
            30,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center,
            Color.white);
        enemyActorView = enemyVisual.gameObject.AddComponent<BattleActorView>();
        enemyActorView.Initialize(enemyGlyph, Danger);
    }

    private void BindEnemyActor(RectTransform enemyCard)
    {
        enemyActorRoot =
            RuntimeUiBinder.FindRect(enemyCard, "EnemyActorRoot");
        enemyVisual = RuntimeUiBinder.FindRect(enemyCard, "EnemyVisual");
        enemyVisualImage = enemyVisual == null
            ? null
            : enemyVisual.GetComponent<Image>();
        if (enemyVisualImage != null)
            enemyVisualImage.raycastTarget = false;
        enemyVisualBasePosition = enemyVisual == null
            ? Vector2.zero
            : enemyVisual.anchoredPosition;
        TMP_Text enemyGlyph =
            RuntimeUiBinder.FindText(enemyVisual, "EnemyGlyph");
        enemyActorView = enemyVisual == null
            ? null
            : enemyVisual.GetComponent<BattleActorView>();
        if (enemyActorView == null && enemyVisual != null)
            enemyActorView =
                enemyVisual.gameObject.AddComponent<BattleActorView>();
        enemyActorView?.Initialize(enemyGlyph, Danger);
    }

    private void BuildEnemyPopups(RectTransform enemyCard)
    {
        enemyDamagePopup = RuntimeUiFactory.CreatePanel(
            "EnemyDamagePopup",
            enemyCard,
            new Color32(0, 0, 0, 0),
            BattleLayoutConfig.EnemyAnchor + new Vector2(-0.28f, 0.12f),
            BattleLayoutConfig.EnemyAnchor + new Vector2(0.28f, 0.34f));
        enemyDamageText = RuntimeUiFactory.CreateText(
            "EnemyDamageLabel",
            enemyDamagePopup,
            "",
            18,
            new Vector2(0f, 0.64f),
            new Vector2(1f, 1f),
            TextAlignmentOptions.Center,
            Gold);
        enemyDamageNumberText = RuntimeUiFactory.CreateText(
            "EnemyDamageNumber",
            enemyDamagePopup,
            "",
            58,
            new Vector2(0f, 0.04f),
            new Vector2(1f, 0.78f),
            TextAlignmentOptions.Center,
            Gold);
        GameFont.ApplyDamage(enemyDamageNumberText);
        enemyDamagePopup.gameObject.SetActive(false);

        rewardPopup = RuntimeUiFactory.CreatePanel(
            "RewardPopup",
            enemyCard,
            new Color32(0, 0, 0, 0),
            BattleLayoutConfig.EnemyAnchor + new Vector2(-0.24f, -0.13f),
            BattleLayoutConfig.EnemyAnchor + new Vector2(0.24f, 0.07f));
        rewardPopupText = RuntimeUiFactory.CreateText(
            "RewardPopupLabel",
            rewardPopup,
            "",
            18,
            new Vector2(0f, 0.68f),
            Vector2.one,
            TextAlignmentOptions.Center,
            Success);
        rewardIconImage = RuntimeUiFactory.CreateSpriteImage(
            "RewardGoldIcon",
            rewardPopup,
            PrototypeUiArt.GoldIcon,
            new Vector2(0.08f, 0.16f),
            new Vector2(0.28f, 0.64f));
        rewardNumberText = new SpriteNumberText(
            rewardPopup,
            "RewardNumber",
            NumberResourceRoot,
            58f,
            new Vector2(0.18f, 0f),
            new Vector2(1f, 0.72f));
        rewardPopup.gameObject.SetActive(false);
    }

    private void BindEnemyPopups(RectTransform enemyCard)
    {
        enemyDamagePopup =
            RuntimeUiBinder.FindRect(enemyCard, "EnemyDamagePopup");
        enemyDamageText =
            RuntimeUiBinder.FindText(enemyCard, "EnemyDamageLabel");
        enemyDamageNumberText =
            RuntimeUiBinder.FindText(enemyCard, "EnemyDamageNumber");

        rewardPopup =
            RuntimeUiBinder.FindRect(enemyCard, "RewardPopup");
        rewardPopupText =
            RuntimeUiBinder.FindText(enemyCard, "RewardPopupLabel");
        rewardIconImage =
            RuntimeUiBinder.FindImage(enemyCard, "RewardGoldIcon");
        rewardNumberText = new SpriteNumberText(
            RuntimeUiBinder.FindRect(enemyCard, "RewardNumber"),
            NumberResourceRoot,
            58f);
    }

    private void BuildCombatEffects(RectTransform enemyCard)
    {
        attackTrail = RuntimeUiFactory.CreatePanel(
            "AttackTrail",
            enemyCard,
            new Color32(255, 255, 255, 0),
            new Vector2(0.2f, 0.48f),
            new Vector2(0.8f, 0.52f));
        attackTrailImage = attackTrail.GetComponent<Image>();
        attackTrail.localRotation = Quaternion.Euler(0f, 0f, -10f);
        attackTrail.gameObject.SetActive(false);

        skillProjectile = RuntimeUiFactory.CreatePanel(
            "SkillProjectile",
            enemyCard,
            Accent,
            Vector2.zero,
            Vector2.zero);
        skillProjectile.pivot = new Vector2(0.5f, 0.5f);
        skillProjectile.sizeDelta = new Vector2(48f, 48f);
        skillProjectileImage = skillProjectile.GetComponent<Image>();
        skillProjectileImage.sprite = null;
        skillProjectileImage.preserveAspect = true;
        skillProjectileImage.raycastTarget = false;
        skillProjectile.gameObject.SetActive(false);

        BuildCompanionProjectiles(enemyCard);
        BuildSparkles(enemyCard);

        battleFlash = RuntimeUiFactory.CreatePanel(
            "BattleFlash",
            enemyCard,
            new Color32(255, 255, 255, 0),
            Vector2.zero,
            Vector2.one);
        battleFlashImage = battleFlash.GetComponent<Image>();
        battleFlashImage.raycastTarget = false;
        battleFlash.gameObject.SetActive(false);
    }

    private void BindCombatEffects(RectTransform enemyCard)
    {
        attackTrail =
            RuntimeUiBinder.FindRect(enemyCard, "AttackTrail");
        attackTrailImage = attackTrail == null
            ? null
            : attackTrail.GetComponent<Image>();

        skillProjectile =
            RuntimeUiBinder.FindRect(enemyCard, "SkillProjectile");
        skillProjectileImage = skillProjectile == null
            ? null
            : skillProjectile.GetComponent<Image>();

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            RectTransform trail = RuntimeUiBinder.FindRect(
                enemyCard,
                $"CompanionProjectileTrail{slot + 1}");
            companionProjectileTrailRects[slot] = trail;
            companionProjectileTrailImages[slot] = trail == null
                ? null
                : trail.GetComponent<Image>();

            RectTransform projectile = RuntimeUiBinder.FindRect(
                enemyCard,
                $"CompanionProjectile{slot + 1}");
            companionProjectileRects[slot] = projectile;
            companionProjectileImages[slot] = projectile == null
                ? null
                : projectile.GetComponent<Image>();
        }

        for (int index = 0; index < SparkleCount; index++)
        {
            RectTransform sparkle = RuntimeUiBinder.FindRect(
                enemyCard,
                $"HitSparkle{index + 1}");
            sparkleRects[index] = sparkle;
            sparkleImages[index] = sparkle == null
                ? null
                : sparkle.GetComponent<Image>();
        }

        battleFlash =
            RuntimeUiBinder.FindRect(enemyCard, "BattleFlash");
        battleFlashImage = battleFlash == null
            ? null
            : battleFlash.GetComponent<Image>();
    }

    private void BuildCompanionProjectiles(RectTransform enemyCard)
    {
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            RectTransform trail = RuntimeUiFactory.CreatePanel(
                $"CompanionProjectileTrail{slot + 1}",
                enemyCard,
                new Color32(255, 255, 255, 0),
                Vector2.zero,
                Vector2.zero);
            trail.anchorMin = Vector2.zero;
            trail.anchorMax = Vector2.zero;
            trail.pivot = new Vector2(0.5f, 0.5f);
            trail.sizeDelta = new Vector2(96f, 8f);
            Image trailImage = trail.GetComponent<Image>();
            trailImage.raycastTarget = false;
            trail.gameObject.SetActive(false);
            companionProjectileTrailRects[slot] = trail;
            companionProjectileTrailImages[slot] = trailImage;

            RectTransform projectile = RuntimeUiFactory.CreatePanel(
                $"CompanionProjectile{slot + 1}",
                enemyCard,
                Accent,
                Vector2.zero,
                Vector2.zero);
            projectile.anchorMin = Vector2.zero;
            projectile.anchorMax = Vector2.zero;
            projectile.pivot = new Vector2(0.5f, 0.5f);
            projectile.sizeDelta = new Vector2(44f, 44f);
            Image projectileImage = projectile.GetComponent<Image>();
            projectileImage.sprite = null;
            projectileImage.preserveAspect = true;
            projectileImage.raycastTarget = false;
            projectile.gameObject.SetActive(false);
            companionProjectileRects[slot] = projectile;
            companionProjectileImages[slot] = projectileImage;
        }
    }

    private void BuildSparkles(RectTransform enemyCard)
    {
        for (int index = 0; index < SparkleCount; index++)
        {
            RectTransform sparkle = RuntimeUiFactory.CreatePanel(
                $"HitSparkle{index + 1}",
                enemyCard,
                new Color32(255, 255, 255, 0),
                Vector2.zero,
                Vector2.zero);
            sparkle.anchorMin = Vector2.zero;
            sparkle.anchorMax = Vector2.zero;
            sparkle.pivot = new Vector2(0.5f, 0.5f);
            sparkle.sizeDelta = Vector2.one * 14f;
            Image image = sparkle.GetComponent<Image>();
            image.raycastTarget = false;
            sparkle.gameObject.SetActive(false);
            sparkleRects[index] = sparkle;
            sparkleImages[index] = image;
        }
    }

    private void BuildPlayerSide(
        RectTransform actorLayer,
        RectTransform effectLayer)
    {
        RuntimeUiFactory.CreateText(
            "PlayerName",
            actorLayer,
            "SUPPORT SPARROW",
            24,
            new Vector2(0.03f, 0.35f),
            new Vector2(0.24f, 0.4f),
            TextAlignmentOptions.Left,
            Accent);

        playerActorRoot = CreateActorRoot(
            actorLayer,
            "SupportActorRoot",
            "SupportShadow",
            BattleLayoutConfig.SupportSparrowAnchor,
            new Vector2(0.22f, 0.24f),
            Success);
        playerVisual = RuntimeUiFactory.CreatePanel(
            "PlayerVisual",
            playerActorRoot,
            Accent,
            new Vector2(0.08f, 0.12f),
            new Vector2(0.92f, 1f));
        playerVisualImage = playerVisual.GetComponent<Image>();
        playerVisualImage.raycastTarget = false;
        playerVisualBasePosition = playerVisual.anchoredPosition;

        TMP_Text playerGlyph = RuntimeUiFactory.CreateText(
            "PlayerGlyph",
            playerVisual,
            "PWR",
            18,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center,
            Color.white);
        playerActorView =
            playerVisual.gameObject.AddComponent<BattleActorView>();
        playerActorView.Initialize(playerGlyph, Accent);

        powerChargePopup = RuntimeUiFactory.CreatePanel(
            "PowerChargePopup",
            effectLayer,
            new Color32(0, 0, 0, 0),
            new Vector2(0.03f, 0.42f),
            new Vector2(0.39f, 0.56f));
        powerChargePopupText = RuntimeUiFactory.CreateText(
            "PowerChargePopupText",
            powerChargePopup,
            "",
            28,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center,
            Success);
        powerChargePopup.gameObject.SetActive(false);

        playerDamagePopup = RuntimeUiFactory.CreatePanel(
            "PlayerDamagePopup",
            effectLayer,
            new Color32(0, 0, 0, 0),
            BattleLayoutConfig.SupportSparrowAnchor +
                new Vector2(-0.22f, 0.11f),
            BattleLayoutConfig.SupportSparrowAnchor +
                new Vector2(0.22f, 0.29f));
        playerDamageText = RuntimeUiFactory.CreateText(
            "PlayerDamageLabel",
            playerDamagePopup,
            "",
            14,
            new Vector2(0f, 0.68f),
            new Vector2(1f, 1f),
            TextAlignmentOptions.Center,
            Danger);
        playerDamageNumberText = RuntimeUiFactory.CreateText(
            "PlayerDamageNumber",
            playerDamagePopup,
            "",
            42,
            new Vector2(0f, 0f),
            new Vector2(1f, 0.86f),
            TextAlignmentOptions.Center,
            Danger);
        GameFont.ApplyDamage(playerDamageNumberText);
        playerDamagePopup.gameObject.SetActive(false);

    }

    private void BindPlayerSide(
        RectTransform actorLayer,
        RectTransform effectLayer)
    {
        playerActorRoot =
            RuntimeUiBinder.FindRect(actorLayer, "SupportActorRoot");
        playerVisual =
            RuntimeUiBinder.FindRect(actorLayer, "PlayerVisual");
        playerVisualImage = playerVisual == null
            ? null
            : playerVisual.GetComponent<Image>();
        if (playerVisualImage != null)
            playerVisualImage.raycastTarget = false;
        playerVisualBasePosition = playerVisual == null
            ? Vector2.zero
            : playerVisual.anchoredPosition;
        TMP_Text playerGlyph =
            RuntimeUiBinder.FindText(playerVisual, "PlayerGlyph");
        playerActorView = playerVisual == null
            ? null
            : playerVisual.GetComponent<BattleActorView>();
        if (playerActorView == null && playerVisual != null)
            playerActorView =
                playerVisual.gameObject.AddComponent<BattleActorView>();
        playerActorView?.Initialize(playerGlyph, Accent);

        powerChargePopup =
            RuntimeUiBinder.FindRect(effectLayer, "PowerChargePopup");
        powerChargePopupText =
            RuntimeUiBinder.FindText(effectLayer, "PowerChargePopupText");

        playerDamagePopup =
            RuntimeUiBinder.FindRect(effectLayer, "PlayerDamagePopup");
        playerDamageText =
            RuntimeUiBinder.FindText(effectLayer, "PlayerDamageLabel");
        playerDamageNumberText =
            RuntimeUiBinder.FindText(effectLayer, "PlayerDamageNumber");
    }

    private void BuildCompanionActors(RectTransform enemyCard)
    {
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            Vector2 anchor = BattleLayoutConfig.GetCompanionAnchor(slot);
            RectTransform companionRoot = CreateActorRoot(
                enemyCard,
                $"CompanionActorRoot{slot + 1}",
                $"CompanionShadow{slot + 1}",
                anchor,
                new Vector2(0.2f, 0.21f),
                Accent);
            RectTransform companionVisual = RuntimeUiFactory.CreatePanel(
                $"CompanionVisual{slot + 1}",
                companionRoot,
                new Color32(255, 255, 255, 0),
                new Vector2(0.08f, 0.14f),
                new Vector2(0.92f, 1f));
            companionVisual.GetComponent<Image>().raycastTarget = false;
            companionVisualBasePositions[slot] =
                companionVisual.anchoredPosition;
            TMP_Text companionGlyph = RuntimeUiFactory.CreateText(
                "Glyph",
                companionVisual,
                (slot + 1).ToString(),
                20,
                Vector2.zero,
                Vector2.one,
                TextAlignmentOptions.Center,
                Color.white);
            BattleActorView actorView =
                companionVisual.gameObject.AddComponent<BattleActorView>();
            actorView.Initialize(companionGlyph, PanelLight);
            companionActorRoots[slot] = companionRoot;
            companionActorViews[slot] = actorView;
            companionVisualRects[slot] = companionVisual;
        }
    }

    private void BindCompanionActors(RectTransform enemyCard)
    {
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            companionActorRoots[slot] = RuntimeUiBinder.FindRect(
                enemyCard,
                $"CompanionActorRoot{slot + 1}");
            RectTransform companionVisual = RuntimeUiBinder.FindRect(
                enemyCard,
                $"CompanionVisual{slot + 1}");
            Image companionImage = companionVisual == null
                ? null
                : companionVisual.GetComponent<Image>();
            if (companionImage != null)
                companionImage.raycastTarget = false;
            companionVisualBasePositions[slot] = companionVisual == null
                ? Vector2.zero
                : companionVisual.anchoredPosition;
            TMP_Text companionGlyph =
                RuntimeUiBinder.FindText(companionVisual, "Glyph");
            BattleActorView actorView = companionVisual == null
                ? null
                : companionVisual.GetComponent<BattleActorView>();
            if (actorView == null && companionVisual != null)
                actorView =
                    companionVisual.gameObject.AddComponent<BattleActorView>();
            actorView?.Initialize(companionGlyph, PanelLight);
            companionActorViews[slot] = actorView;
            companionVisualRects[slot] = companionVisual;
        }
    }

    private void SortActorRoots()
    {
        SetActorRootSibling(enemyActorRoot);
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
            SetActorRootSibling(companionActorRoots[slot]);
        SetActorRootSibling(playerActorRoot);

        RectTransform[] roots = new RectTransform[
            CompanionManager.PartySize + 2];
        roots[0] = enemyActorRoot;
        roots[1] = playerActorRoot;
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
            roots[slot + 2] = companionActorRoots[slot];

        Array.Sort(
            roots,
            (left, right) =>
                GetActorSortY(right).CompareTo(GetActorSortY(left)));

        foreach (RectTransform root in roots)
            SetActorRootSibling(root);
    }

    private static float GetActorSortY(RectTransform root)
    {
        if (root == null)
            return float.MaxValue;

        return (root.anchorMin.y + root.anchorMax.y) * 0.5f;
    }

    private static void SetActorRootSibling(RectTransform root)
    {
        if (root != null)
            root.SetAsLastSibling();
    }

    private void BuildSkillControls()
    {
        skillControls = new BattleSkillControlsUI(
            battleManager,
            companionManager,
            showToast,
            PanelLight,
            Accent,
            Gold,
            Success);
        skillControls.Build(panel);
    }

    private void BindSkillControls()
    {
        skillControls = new BattleSkillControlsUI(
            battleManager,
            companionManager,
            showToast,
            PanelLight,
            Accent,
            Gold,
            Success);
        skillControls.Bind(panel);
    }

    private void BuildQuickButtons()
    {
        quickButtons = new BattleQuickButtonsUI(
            onQuest,
            onEvent,
            onShop,
            onEquipment,
            Danger);
        quickButtons.Build(panel);
    }

    private void BindQuickButtons()
    {
        quickButtons = new BattleQuickButtonsUI(
            onQuest,
            onEvent,
            onShop,
            onEquipment,
            Danger);
        quickButtons.Bind(panel);
    }

    private void ShowEnemyDamage(
        int damage,
        string label,
        Color color,
        float duration)
    {
        if (enemyDamagePopup == null ||
            enemyDamageText == null ||
            enemyDamageNumberText == null)
        {
            return;
        }

        enemyDamagePopupTimer = duration;
        SetPopupRect(
            enemyDamagePopup,
            GetEnemyImpactAnchor() + new Vector2(0f, 0.05f),
            new Vector2(0.56f, 0.22f));
        enemyDamageText.text = string.IsNullOrWhiteSpace(label)
            ? string.Empty
            : label;
        enemyDamageText.color = color;
        enemyDamageNumberText.text = FormatCompactNumber(damage, "-");
        enemyDamageNumberText.color = color;
        SetTextAlpha(enemyDamageNumberText, 1f);
        enemyDamagePopup.gameObject.SetActive(true);
    }

    private void ShowPlayerDamage(int damage)
    {
        if (playerDamagePopup == null ||
            playerDamageText == null ||
            playerDamageNumberText == null)
        {
            return;
        }

        playerDamagePopupTimer = 0.55f;
        SetPopupRect(
            playerDamagePopup,
            GetSupportImpactAnchor() + new Vector2(0f, 0.03f),
            new Vector2(0.44f, 0.18f));
        playerDamageText.text = string.Empty;
        playerDamageText.color = Danger;
        playerDamageNumberText.text = FormatCompactNumber(damage, "-");
        playerDamageNumberText.color = Danger;
        SetTextAlpha(playerDamageNumberText, 1f);
        playerDamagePopup.gameObject.SetActive(true);
    }

    private void ShowPoleRepair(
        int repairAmount,
        string label,
        float duration)
    {
        if (playerDamagePopup == null ||
            playerDamageText == null ||
            playerDamageNumberText == null)
        {
            return;
        }

        playerDamagePopupTimer = duration;
        SetPopupRect(
            playerDamagePopup,
            GetSupportImpactAnchor() + new Vector2(0f, 0.08f),
            new Vector2(0.5f, 0.2f));
        playerDamageText.text = string.IsNullOrWhiteSpace(label)
            ? string.Empty
            : label;
        playerDamageText.color = Success;
        playerDamageNumberText.text = FormatCompactNumber(repairAmount, "+");
        playerDamageNumberText.color = Success;
        SetTextAlpha(playerDamageNumberText, 1f);
        playerDamagePopup.gameObject.SetActive(true);
    }

    private static void UpdateTextAlpha(
        TMP_Text text,
        float timer,
        float duration)
    {
        if (text == null)
            return;

        float progress = 1f - Mathf.Clamp01(timer / duration);
        SetTextAlpha(text, Mathf.Lerp(1f, 0f, progress));
    }

    private static void SetTextAlpha(TMP_Text text, float alpha)
    {
        if (text == null)
            return;

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    private static string FormatCompactNumber(int value, string prefix)
    {
        return CompactNumberFormatter.Format(value, prefix);
    }

    private void ShowPowerChargePopup(float current, float max)
    {
        if (powerChargePopup == null || powerChargePopupText == null)
            return;

        float previous = lastObservedPowerCharge;
        bool crossedReady =
            previous < BattleManager.CompanionSkillPowerCost &&
            current >= BattleManager.CompanionSkillPowerCost;
        bool crossedFull = previous < max && current >= max;
        bool increased = current > previous + 0.01f;
        if (!increased && !crossedReady && !crossedFull)
            return;

        powerChargePopupTimer = crossedFull ? 1f : 0.8f;
        if (crossedFull)
        {
            powerChargePopupText.text =
                $"{LocalizationManager.Text("FULL POWER", "\uC804\uB825 \uCD5C\uB300")}\n" +
                $"{LocalizationManager.Text("Cooldown boosted", "\uC7AC\uC0AC\uC6A9 \uAC00\uC18D")}";
            powerChargePopupText.color = Gold;
        }
        else if (crossedReady)
        {
            powerChargePopupText.text =
                $"{LocalizationManager.Text("SKILL READY", "\uC2A4\uD0AC \uC900\uBE44")}\n" +
                $"{LocalizationManager.Text("Tap companion", "\uB3D9\uB8CC \uD130\uCE58")}";
            powerChargePopupText.color = Accent;
        }
        else
        {
            powerChargePopupText.text =
                $"{LocalizationManager.Text("POWER", "\uC804\uB825")}\n" +
                $"{LocalizationManager.Text("Charging", "\uCDA9\uC804 \uC911")}";
            powerChargePopupText.color = Success;
        }

        powerChargePopup.gameObject.SetActive(true);
    }

    private void ShowRewardPopup(int reward, bool bossClear)
    {
        if (rewardPopup == null ||
            rewardPopupText == null ||
            rewardNumberText == null)
        {
            return;
        }

        rewardPopupDuration = bossClear ? 1.15f : 0.9f;
        rewardPopupTimer = rewardPopupDuration;
        SetPopupRect(
            rewardPopup,
            GetEnemyFootAnchor() + new Vector2(0f, 0.03f),
            new Vector2(0.48f, 0.2f));
        rewardPopupText.text = bossClear
            ? LocalizationManager.Translate("BOSS")
            : string.Empty;
        rewardPopupText.color = bossClear ? Gold : Success;
        rewardNumberText.SetText(FormatCompactNumber(reward, "+"));
        rewardNumberText.SetAlpha(1f);
        SetRewardIconAlpha(1f);
        rewardPopup.gameObject.SetActive(true);
    }

    private void UpdateRewardPopupVisuals()
    {
        if (rewardPopupTimer <= 0f)
            return;

        float progress =
            1f - Mathf.Clamp01(rewardPopupTimer / rewardPopupDuration);
        float alpha = Mathf.Lerp(1f, 0f, progress);
        rewardNumberText?.SetAlpha(alpha);
        SetRewardIconAlpha(alpha);
    }

    private void SetRewardIconAlpha(float alpha)
    {
        if (rewardIconImage == null)
            return;

        Color color = rewardIconImage.color;
        color.a = alpha;
        rewardIconImage.color = color;
    }

    private void StartBattleFlash(Color color, float intensity)
    {
        if (battleFlash == null || battleFlashImage == null)
            return;

        battleFlashColor = color;
        battleFlashTimer = Mathf.Clamp(intensity, 0.05f, 0.3f);
        battleFlash.gameObject.SetActive(true);
    }

    private void StartSparkles(
        Vector2 anchor,
        Color color,
        float duration,
        float radius)
    {
        for (int index = 0; index < SparkleCount; index++)
        {
            if (sparkleRects[index] == null)
                continue;

            float angle =
                (-40f + index * (320f / Mathf.Max(1, SparkleCount - 1))) *
                Mathf.Deg2Rad;
            float distance =
                radius *
                Mathf.Lerp(0.65f, 1.15f, (index % 3) / 2f);
            sparkleAnchors[index] = anchor;
            sparkleDirections[index] =
                new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            sparkleDistances[index] = distance;
            sparkleDurations[index] = Mathf.Max(0.12f, duration);
            sparkleTimers[index] =
                sparkleDurations[index] - index * 0.018f;
            sparkleColors[index] = color;
            sparkleRects[index].SetAsLastSibling();
            sparkleRects[index].gameObject.SetActive(true);
        }
    }

    private void ShowBossWarning(string message)
    {
        if (bossWarningPanel == null || bossWarningText == null)
            return;

        bossWarningTimer = 1.1f;
        bossWarningText.text = message;
        bossWarningPanel.SetAsLastSibling();
        bossWarningPanel.gameObject.SetActive(true);
    }

    private Vector2 GetEnemyFootAnchor()
    {
        return GetActorFootAnchor(
            enemyVisual,
            enemyActorRoot,
            BattleLayoutConfig.EnemyAnchor);
    }

    private Vector2 GetSupportFootAnchor()
    {
        return GetActorFootAnchor(
            playerVisual,
            playerActorRoot,
            BattleLayoutConfig.SupportSparrowAnchor);
    }

    private Vector2 GetCompanionFootAnchor(int slot)
    {
        RectTransform root =
            slot >= 0 && slot < companionActorRoots.Length
                ? companionActorRoots[slot]
                : null;
        RectTransform visual =
            slot >= 0 && slot < companionVisualRects.Length
                ? companionVisualRects[slot]
                : null;
        return GetActorFootAnchor(
            visual,
            root,
            BattleLayoutConfig.GetCompanionAnchor(slot));
    }

    private Vector2 GetActorFootAnchor(
        RectTransform visual,
        RectTransform root,
        Vector2 fallback)
    {
        Vector2 rootAnchor = GetAnchorFoot(root, fallback);
        RectTransform target = visual == null ? root : visual;
        return GetRectFootAnchor(target, rootAnchor);
    }

    private Vector2 GetRectFootAnchor(
        RectTransform target,
        Vector2 fallback)
    {
        if (target == null || battleEffectLayer == null)
            return fallback;

        Rect referenceRect = battleEffectLayer.rect;
        if (referenceRect.width <= 0.01f || referenceRect.height <= 0.01f)
            return fallback;

        target.GetWorldCorners(actorWorldCorners);
        Vector3 footWorld =
            (actorWorldCorners[0] + actorWorldCorners[3]) * 0.5f;
        Vector3 footLocal = battleEffectLayer.InverseTransformPoint(footWorld);
        return new Vector2(
            Mathf.InverseLerp(
                referenceRect.xMin,
                referenceRect.xMax,
                footLocal.x),
            Mathf.InverseLerp(
                referenceRect.yMin,
                referenceRect.yMax,
                footLocal.y));
    }

    private static Vector2 GetAnchorFoot(
        RectTransform root,
        Vector2 fallback)
    {
        if (root == null)
            return fallback;

        return new Vector2(
            (root.anchorMin.x + root.anchorMax.x) * 0.5f,
            root.anchorMin.y);
    }

    private static void SetPopupRect(
        RectTransform popup,
        Vector2 center,
        Vector2 size)
    {
        if (popup == null)
            return;

        Vector2 halfSize = size * 0.5f;
        popup.anchorMin = center - halfSize;
        popup.anchorMax = center + halfSize;
        popup.offsetMin = Vector2.zero;
        popup.offsetMax = Vector2.zero;
        popup.anchoredPosition = Vector2.zero;
        popup.localScale = Vector3.one;
    }

    private Vector2 GetEnemyImpactAnchor()
    {
        return GetEnemyFootAnchor() + new Vector2(0f, 0.13f);
    }

    private Vector2 GetSupportImpactAnchor()
    {
        return GetSupportFootAnchor() + new Vector2(0f, 0.1f);
    }

    private Vector2 GetCompanionImpactAnchor(int slot)
    {
        return GetCompanionFootAnchor(slot) + new Vector2(0f, 0.105f);
    }

    private void StartSkillProjectile(
        int slot,
        CharacterData character,
        bool skill,
        Color fallbackColor,
        float duration,
        float size)
    {
        BattleProjectileVisual projectile =
            character?.ResolveBattleVisual()?.GetProjectile(skill);
        Sprite sprite = projectile?.sprite;
        Color color = fallbackColor;
        if (sprite != null && projectile != null)
            color = projectile.ResolveTint(fallbackColor);

        StartSkillProjectile(
            slot,
            sprite,
            color,
            projectile?.ResolveDuration(duration) ?? duration,
            projectile?.ResolveSize(size) ?? size);
    }

    private void StartSkillProjectile(
        int slot,
        Sprite sprite,
        Color color,
        float duration,
        float size)
    {
        if (slot < 0 ||
            slot >= CompanionManager.PartySize ||
            companionProjectileRects[slot] == null)
        {
            return;
        }

        companionProjectileColors[slot] = color;
        companionProjectileSprites[slot] = sprite;
        companionProjectileDurations[slot] = Mathf.Max(0.16f, duration);
        companionProjectileTimers[slot] = companionProjectileDurations[slot];
        companionProjectileSizes[slot] = Mathf.Max(24f, size);
        if (companionProjectileImages[slot] != null)
            companionProjectileImages[slot].sprite =
                companionProjectileSprites[slot];
        companionProjectileTrailRects[slot]?.SetAsLastSibling();
        companionProjectileRects[slot].SetAsLastSibling();
        companionProjectileRects[slot].gameObject.SetActive(true);
        companionProjectileTrailRects[slot]?.gameObject.SetActive(true);
    }

    private void UpdateSkillProjectile()
    {
        if (skillProjectile == null || skillProjectileImage == null)
            return;

        bool active =
            skillProjectileTimer > 0f &&
            skillProjectileSlot >= 0 &&
            skillProjectileSlot < CompanionManager.PartySize;
        skillProjectile.gameObject.SetActive(active);
        if (!active)
            return;

        float progress = 1f - Mathf.Clamp01(
            skillProjectileTimer / skillProjectileDuration);
        progress = Mathf.SmoothStep(0f, 1f, progress);
        Vector2 from = GetCompanionImpactAnchor(skillProjectileSlot);
        Vector2 to = GetEnemyImpactAnchor();
        BattleHudUiFactory.SetAnchoredPoint(
            skillProjectile,
            battleEffectLayer,
            Vector2.Lerp(from, to, progress));

        float pulse = Mathf.Sin(progress * Mathf.PI);
        skillProjectile.localScale =
            Vector3.one * Mathf.Lerp(0.55f, 1.15f, pulse);
        skillProjectileImage.color = new Color(
            skillProjectileColor.r,
            skillProjectileColor.g,
            skillProjectileColor.b,
            Mathf.Lerp(0.3f, 0.95f, pulse));
    }

    // 투사체 특성
    private void UpdateCompanionProjectiles()
    {
        if (battleEffectLayer == null)
            return;

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            RectTransform projectile = companionProjectileRects[slot];
            RectTransform trail = companionProjectileTrailRects[slot];
            Image projectileImage = companionProjectileImages[slot];
            Image trailImage = companionProjectileTrailImages[slot];
            bool active =
                projectile != null &&
                companionProjectileTimers[slot] > 0f;

            if (projectile != null)
                projectile.gameObject.SetActive(active);
            if (trail != null)
                trail.gameObject.SetActive(active);
            if (!active)
                continue;

            float duration = Mathf.Max(
                0.16f,
                companionProjectileDurations[slot]);
            float progress = 1f - Mathf.Clamp01(
                companionProjectileTimers[slot] / duration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            Vector2 from = GetCompanionImpactAnchor(slot);
            Vector2 to = GetEnemyImpactAnchor();
            Vector2 current = Vector2.Lerp(from, to, progress);
            BattleHudUiFactory.SetAnchoredPoint(
                projectile,
                battleEffectLayer,
                current);

            Color color = companionProjectileColors[slot];
            float pulse = Mathf.Sin(progress * Mathf.PI);
            float size = Mathf.Lerp(
                companionProjectileSizes[slot] * 0.72f,
                companionProjectileSizes[slot] * 1.18f,
                pulse);
            projectile.sizeDelta = new Vector2(size, size);
            projectile.localRotation = Quaternion.Euler(0f, 0f, progress * 30f);
            /* 회전 코드
             Quaternion.Euler(0f, 0f, progress * 30f);
            */
            projectileImage.color = new Color(
                color.r,
                color.g,
                color.b,
                Mathf.Lerp(0.25f, 1f, pulse));

            UpdateProjectileTrail(
                trail,
                trailImage,
                from,
                current,
                color,
                progress);
        }
    }

    private void UpdateProjectileTrail(
        RectTransform trail,
        Image trailImage,
        Vector2 from,
        Vector2 current,
        Color color,
        float progress)
    {
        if (trail == null ||
            trailImage == null ||
            battleEffectLayer == null)
        {
            return;
        }

        Rect rect = battleEffectLayer.rect;
        Vector2 fromPixels = new Vector2(
            rect.width * from.x,
            rect.height * from.y);
        Vector2 currentPixels = new Vector2(
            rect.width * current.x,
            rect.height * current.y);
        Vector2 delta = currentPixels - fromPixels;
        float length = Mathf.Clamp(delta.magnitude * 0.72f, 24f, 220f);
        Vector2 midpoint = currentPixels - delta.normalized * length * 0.5f;

        trail.anchorMin = Vector2.zero;
        trail.anchorMax = Vector2.zero;
        trail.anchoredPosition = midpoint;
        trail.sizeDelta = new Vector2(length, Mathf.Lerp(6f, 14f, 1f - progress));
        trail.localRotation = Quaternion.Euler(
            0f,
            0f,
            Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        trailImage.color = new Color(
            color.r,
            color.g,
            color.b,
            Mathf.Lerp(0.55f, 0.02f, progress));
    }

    private void UpdateSparkles()
    {
        if (battleEffectLayer == null)
            return;

        for (int index = 0; index < SparkleCount; index++)
        {
            RectTransform sparkle = sparkleRects[index];
            Image image = sparkleImages[index];
            if (sparkle == null || image == null)
                continue;

            bool active = sparkleTimers[index] > 0f;
            sparkle.gameObject.SetActive(active);
            if (!active)
                continue;

            float duration = Mathf.Max(0.12f, sparkleDurations[index]);
            float progress =
                1f - Mathf.Clamp01(sparkleTimers[index] / duration);
            Vector2 point =
                sparkleAnchors[index] +
                sparkleDirections[index] *
                sparkleDistances[index] *
                Mathf.SmoothStep(0f, 1f, progress);
            BattleHudUiFactory.SetAnchoredPoint(
                sparkle,
                battleEffectLayer,
                point);

            float pulse = Mathf.Sin(progress * Mathf.PI);
            float size = Mathf.Lerp(8f, 24f, pulse);
            sparkle.sizeDelta = new Vector2(size, size);
            sparkle.localRotation =
                Quaternion.Euler(0f, 0f, progress * 180f);

            Color color = sparkleColors[index];
            image.color = new Color(
                color.r,
                color.g,
                color.b,
                Mathf.Lerp(0.95f, 0f, progress));
        }
    }

    private void UpdateBattleFlash()
    {
        if (battleFlash == null || battleFlashImage == null)
            return;

        bool active = battleFlashTimer > 0f;
        battleFlash.gameObject.SetActive(active);
        if (!active)
            return;

        float alpha = Mathf.Clamp01(battleFlashTimer / 0.3f) * 0.28f;
        battleFlashImage.color = new Color(
            battleFlashColor.r,
            battleFlashColor.g,
            battleFlashColor.b,
            alpha);
    }

    private void UpdateBossWarning()
    {
        if (bossWarningPanel == null)
            return;

        bool active = bossWarningTimer > 0f;
        bossWarningPanel.gameObject.SetActive(active);
        if (!active)
        {
            bossWarningPanel.localScale = Vector3.one;
            return;
        }

        float pulse = 1f + Mathf.Sin(bossWarningTimer * 24f) * 0.04f;
        bossWarningPanel.localScale = Vector3.one * pulse;
    }

    private void UpdateCompanionAnimations()
    {
        for (int slot = 0; slot < companionVisualRects.Length; slot++)
        {
            RectTransform visual = companionVisualRects[slot];
            if (visual == null)
                continue;

            float timer = companionSkillTimers[slot];
            if (timer <= 0f)
            {
                visual.anchoredPosition =
                    companionVisualBasePositions[slot];
                visual.localScale = Vector3.one;
                continue;
            }

            float duration = Mathf.Max(
                0.01f,
                companionAnimationDurations[slot]);
            float progress = 1f - Mathf.Clamp01(timer / duration);
            float arc = Mathf.Sin(progress * Mathf.PI);
            visual.anchoredPosition =
                companionVisualBasePositions[slot] +
                new Vector2(arc * 12f, arc * 18f);
            visual.localScale =
                Vector3.one * (1f + arc * 0.12f);
        }
    }

    private void UpdateAttackTrail()
    {
        if (attackTrail == null || attackTrailImage == null)
            return;

        bool active = attackTrailTimer > 0f;
        attackTrail.gameObject.SetActive(active);
        if (!active)
            return;

        float ratio = Mathf.Clamp01(attackTrailTimer / 0.24f);
        attackTrail.localScale = new Vector3(
            Mathf.Lerp(0.4f, 1.2f, ratio),
            Mathf.Lerp(0.5f, 1f, ratio),
            1f);
        attackTrailImage.color = new Color(
            Accent.r,
            Accent.g,
            Accent.b,
            Mathf.Lerp(0f, 0.55f, ratio));
    }

    private void UpdateActorPulses()
    {
        if (enemyVisual != null && enemyVisualImage != null)
        {
            float shake = enemyHitShakeTimer > 0f
                ? Mathf.Sin(enemyHitShakeTimer * 90f) *
                  Mathf.Lerp(0f, 18f, enemyHitShakeTimer / 0.3f)
                : 0f;
            enemyVisual.anchoredPosition =
                enemyVisualBasePosition + new Vector2(shake, 0f);
            float defeatProgress =
                1f - Mathf.Clamp01(enemyDefeatPopTimer / 0.52f);
            float hitProgress =
                1f - Mathf.Clamp01(enemyHitShakeTimer / 0.3f);
            float hitSquash = enemyHitShakeTimer > 0f
                ? Mathf.Sin(hitProgress * Mathf.PI) * 0.1f
                : 0f;
            float defeatScale = enemyDefeatPopTimer > 0f
                ? Mathf.Lerp(1.25f, 0.52f, defeatProgress)
                : 1f;
            enemyVisual.localScale =
                new Vector3(
                    defeatScale + hitSquash,
                    defeatScale - hitSquash * 0.5f,
                    1f);
            enemyVisualImage.color =
                enemyActorView != null && enemyActorView.HasSprite
                    ? enemyHitShakeTimer > 0f
                        ? Color.Lerp(Color.white, Danger, 0.4f)
                        : enemyDefeatPopTimer > 0f
                            ? Color.Lerp(Color.white, Success, 0.35f)
                        : Color.white
                    : enemyHitShakeTimer > 0f
                        ? Color.Lerp(Danger, Color.white, 0.45f)
                        : enemyDefeatPopTimer > 0f
                            ? Color.Lerp(Danger, Success, 0.45f)
                        : Danger;
        }

        if (playerVisual == null || playerVisualImage == null)
            return;

        float lunge = playerAnimationTimer > 0.12f ? 18f : 0f;
        float playerShake = playerHitShakeTimer > 0f
            ? Mathf.Sin(playerHitShakeTimer * 85f) *
              Mathf.Lerp(0f, 11f, playerHitShakeTimer / 0.28f)
            : 0f;
        playerVisual.anchoredPosition =
            playerVisualBasePosition + new Vector2(lunge - playerShake, 0f);
        playerVisual.localScale = playerDefeatTimer > 0f
            ? Vector3.one * 0.55f
            : playerHitShakeTimer > 0f
                ? Vector3.one * 0.92f
                : Vector3.one;
        playerVisualImage.color =
            playerActorView != null && playerActorView.HasSprite
                ? playerHitShakeTimer > 0f
                    ? Color.Lerp(Color.white, Danger, 0.45f)
                    : Color.white
                : playerHitShakeTimer > 0f
                    ? Color.Lerp(Accent, Danger, 0.55f)
                    : Accent;
    }

}
