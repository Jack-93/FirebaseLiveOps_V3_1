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
    private RectTransform battleEffectLayer;
    private RectTransform enemyVisual;
    private RectTransform playerVisual;
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
    private readonly RectTransform[] companionVisualRects =
        new RectTransform[CompanionManager.PartySize];
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
            new Vector2(0.02f, 0.16f),
            new Vector2(0.98f, 0.91f));
        battleEffectLayer = enemyCard;

        BattleHudUiFactory.CreateBattlePad(
            enemyCard,
            "EnemySlotPad",
            BattleLayoutConfig.EnemyAnchor,
            new Vector2(0.25f, 0.14f),
            Danger);

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            BattleHudUiFactory.CreateBattlePad(
                enemyCard,
                $"CompanionSlotPad{slot + 1}",
                BattleLayoutConfig.GetCompanionAnchor(slot),
                new Vector2(0.21f, 0.1f),
                Accent);
        }

        statusHud = new BattleStatusHudUI(Danger, Accent, Success);
        statusHud.BuildEnemyName(enemyCard);

        BuildBossWarning(enemyCard);
        BuildEnemyActor(enemyCard);
        BuildEnemyPopups(enemyCard);
        BuildCombatEffects(enemyCard);
        statusHud.BuildEnemyHealth(enemyCard);
        BuildPlayerSide(enemyCard);
        statusHud.BuildPlayerStatus(enemyCard);
        BuildCompanionActors(enemyCard);
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

        RectTransform enemyCard =
            RuntimeUiBinder.FindRect(panel, "EnemyCard");
        battleEffectLayer = enemyCard;

        statusHud = new BattleStatusHudUI(Danger, Accent, Success);
        statusHud.Bind(enemyCard);

        BindBossWarning(enemyCard);
        BindEnemyActor(enemyCard);
        BindEnemyPopups(enemyCard);
        BindCombatEffects(enemyCard);
        BindPlayerSide(enemyCard);
        BindCompanionActors(enemyCard);
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

        BattleVisualProfile hero = BattleVisualResolver.GetHero();
        playerActorView?.SetVisual(
            hero?.sprite ?? PrototypeBattleArt.GetSupportHeroSprite(),
            hero?.animatorController);
        playerActorView?.SetSpriteAnimations(null);

        BattleVisualProfile enemy =
            BattleVisualResolver.GetEnemy(
                data.currentStage,
                battleManager.IsBoss);
        Sprite enemySprite =
            enemy?.sprite ?? PrototypeBattleArt.GetEnemySprite(
                data.currentStage,
                battleManager.IsBoss);
        RuntimeAnimatorController enemyAnimator =
            enemy?.animatorController;
        enemyActorView?.SetVisual(
            enemySprite,
            enemyAnimator);
        enemyActorView?.SetSpriteAnimations(
            enemyAnimator == null &&
                (enemy == null || enemy.sprite == null)
                ? PrototypeBattleArt.GetEnemyAnimations(
                    data.currentStage,
                    battleManager.IsBoss)
                : null);

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            CharacterData character =
                companionManager?.GetEquippedAtSlot(slot);
            Sprite sprite = character == null
                ? null
                : character.battleSprite ?? character.icon;
            companionActorViews[slot]?.SetVisual(
                sprite,
                character?.battleAnimator);
            companionActorViews[slot]?.SetSpriteAnimations(null);
        }
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
            BattleLayoutConfig.EnemyAnchor,
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
        enemyActorView?.Play(BattleAnimationCue.Hit);
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
            BattleLayoutConfig.EnemyAnchor,
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
            companionSkillTimers[slot] = 0.22f;
            companionActorViews[slot]?.Play(BattleAnimationCue.Attack);
            StartSkillProjectile(
                slot,
                character,
                false,
                Gold,
                0.42f,
                42f);
        }
        enemyActorView?.Play(BattleAnimationCue.Hit);
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
            BattleLayoutConfig.SupportSparrowAnchor,
            Danger,
            0.25f,
            0.055f);
        StartBattleFlash(Danger, 0.13f);
        ShowPlayerDamage(damage);
        enemyActorView?.Play(BattleAnimationCue.Attack);
        playerActorView?.Play(BattleAnimationCue.Hit);
    }

    public void HandleEnemyDefeatedVisual(int reward)
    {
        enemyAnimationTimer = 0.4f;
        enemyDefeatPopTimer = 0.52f;
        StartSparkles(
            BattleLayoutConfig.EnemyAnchor,
            battleManager != null && battleManager.IsBoss
                ? Gold
                : Success,
            0.58f,
            0.15f);
        StartBattleFlash(Success, 0.16f);
        ShowRewardPopup(reward, battleManager != null && battleManager.IsBoss);
        enemyActorView?.Play(BattleAnimationCue.Death);
    }

    public void HandlePlayerDefeatedVisual()
    {
        playerDefeatTimer = 1.8f;
        StartBattleFlash(Danger, 0.22f);
        playerActorView?.Play(BattleAnimationCue.Death);
    }

    public void HandleCompanionSkillVisual(
        int slot,
        CharacterData character,
        int damage)
    {
        enemyAnimationTimer = 0.35f;
        enemyHitShakeTimer = 0.3f;
        attackTrailTimer = 0.24f;
        StartSparkles(
            BattleLayoutConfig.EnemyAnchor,
            Accent,
            0.38f,
            0.11f);
        StartBattleFlash(Accent, 0.18f);
        ShowEnemyDamage(
            damage,
            character?.skillName ?? "SKILL",
            Accent,
            0.72f);
        if (slot >= 0 && slot < companionActorViews.Length)
        {
            companionSkillTimers[slot] = 0.36f;
            companionActorViews[slot]?.Play(BattleAnimationCue.Skill);
            StartSkillProjectile(
                slot,
                character,
                true,
                Accent,
                0.56f,
                60f);
        }
        enemyActorView?.Play(BattleAnimationCue.Hit);
        showToast?.Invoke(
            $"{character?.skillName ?? "Skill"}  DMG " +
            CompactNumberFormatter.Format(damage));
    }

    public void HandleBossChallengeFailed()
    {
        enemyAnimationTimer = 0.4f;
        ShowBossWarning(LocalizationManager.Translate("BOSS FAILED"));
        showToast?.Invoke("Boss time expired. Retrying.");
    }

    public void HandleBossPatternVisual(
        BossPatternDefinition pattern,
        int damage)
    {
        enemyAnimationTimer = 0.3f;
        playerAnimationTimer = 0.3f;
        playerHitShakeTimer = 0.28f;
        StartSparkles(
            BattleLayoutConfig.SupportSparrowAnchor,
            Danger,
            0.34f,
            0.08f);
        StartBattleFlash(Danger, 0.2f);
        ShowBossWarning(
            $"{LocalizationManager.Translate("BOSS SKILL")}  " +
            $"{pattern.patternName}");
        ShowPlayerDamage(damage);
        enemyActorView?.Play(BattleAnimationCue.Skill);
        playerActorView?.Play(BattleAnimationCue.Hit);
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

    private void BuildEnemyActor(RectTransform enemyCard)
    {
        Vector2 enemyAnchor = BattleLayoutConfig.EnemyAnchor;
        enemyVisual = RuntimeUiFactory.CreatePanel(
            "EnemyVisual",
            enemyCard,
            Danger,
            enemyAnchor - new Vector2(0.1f, 0.1f),
            enemyAnchor + new Vector2(0.1f, 0.1f));
        enemyVisualImage = enemyVisual.GetComponent<Image>();

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
        enemyVisual = RuntimeUiBinder.FindRect(enemyCard, "EnemyVisual");
        enemyVisualImage = enemyVisual == null
            ? null
            : enemyVisual.GetComponent<Image>();
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

    private void BuildPlayerSide(RectTransform enemyCard)
    {
        BattleHudUiFactory.CreateBattlePad(
            enemyCard,
            "SupportChargePad",
            BattleLayoutConfig.SupportSparrowAnchor,
            new Vector2(0.24f, 0.12f),
            Success);

        RuntimeUiFactory.CreateText(
            "PlayerName",
            enemyCard,
            "SUPPORT SPARROW",
            24,
            new Vector2(0.03f, 0.35f),
            new Vector2(0.24f, 0.4f),
            TextAlignmentOptions.Left,
            Accent);

        playerVisual = RuntimeUiFactory.CreatePanel(
            "PlayerVisual",
            enemyCard,
            Accent,
            BattleLayoutConfig.SupportSparrowAnchor - new Vector2(0.09f, 0.09f),
            BattleLayoutConfig.SupportSparrowAnchor + new Vector2(0.09f, 0.09f));
        playerVisualImage = playerVisual.GetComponent<Image>();

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
            enemyCard,
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
            enemyCard,
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

    private void BindPlayerSide(RectTransform enemyCard)
    {
        playerVisual =
            RuntimeUiBinder.FindRect(enemyCard, "PlayerVisual");
        playerVisualImage = playerVisual == null
            ? null
            : playerVisual.GetComponent<Image>();
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
            RuntimeUiBinder.FindRect(enemyCard, "PowerChargePopup");
        powerChargePopupText =
            RuntimeUiBinder.FindText(enemyCard, "PowerChargePopupText");

        playerDamagePopup =
            RuntimeUiBinder.FindRect(enemyCard, "PlayerDamagePopup");
        playerDamageText =
            RuntimeUiBinder.FindText(enemyCard, "PlayerDamageLabel");
        playerDamageNumberText =
            RuntimeUiBinder.FindText(enemyCard, "PlayerDamageNumber");
    }

    private void BuildCompanionActors(RectTransform enemyCard)
    {
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            Vector2 anchor = BattleLayoutConfig.GetCompanionAnchor(slot);
            RectTransform companionVisual = RuntimeUiFactory.CreatePanel(
                $"CompanionVisual{slot + 1}",
                enemyCard,
                new Color32(255, 255, 255, 0),
                anchor - new Vector2(0.09f, 0.09f),
                anchor + new Vector2(0.09f, 0.09f));
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
            companionActorViews[slot] = actorView;
            companionVisualRects[slot] = companionVisual;
        }
    }

    private void BindCompanionActors(RectTransform enemyCard)
    {
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            RectTransform companionVisual = RuntimeUiBinder.FindRect(
                enemyCard,
                $"CompanionVisual{slot + 1}");
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
        playerDamageText.text = string.Empty;
        playerDamageText.color = Danger;
        playerDamageNumberText.text = FormatCompactNumber(damage, "-");
        playerDamageNumberText.color = Danger;
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

    private void StartSkillProjectile(
        int slot,
        CharacterData character,
        bool skill,
        Color fallbackColor,
        float duration,
        float size)
    {
        Sprite sprite = null;
        Color color = fallbackColor;

        if (character != null)
        {
            if (skill)
            {
                sprite = character.skillProjectileSprite;
                if (sprite != null)
                {
                    color = ResolveProjectileColor(
                        character.skillProjectileTint,
                        fallbackColor);
                }
                if (sprite == null)
                {
                    sprite = character.basicProjectileSprite;
                    if (sprite != null)
                    {
                        color = ResolveProjectileColor(
                            character.basicProjectileTint,
                            fallbackColor);
                    }
                }
            }
            else
            {
                sprite = character.basicProjectileSprite;
                if (sprite != null)
                {
                    color = ResolveProjectileColor(
                        character.basicProjectileTint,
                        fallbackColor);
                }
            }
        }

        StartSkillProjectile(slot, sprite, color, duration, size);
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

    private static Color ResolveProjectileColor(
        Color configured,
        Color fallback)
    {
        return configured.a <= 0.01f ? fallback : configured;
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
        Vector2 from =
            BattleLayoutConfig.GetCompanionAnchor(skillProjectileSlot);
        Vector2 to = BattleLayoutConfig.EnemyAnchor;
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

            Vector2 from =
                BattleLayoutConfig.GetCompanionAnchor(slot);
            Vector2 to = BattleLayoutConfig.EnemyAnchor;
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
            projectile.localRotation =
                Quaternion.Euler(0f, 0f, progress * 420f);
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
                visual.anchoredPosition = Vector2.zero;
                visual.localScale = Vector3.one;
                continue;
            }

            float progress = 1f - Mathf.Clamp01(timer / 0.36f);
            float arc = Mathf.Sin(progress * Mathf.PI);
            visual.anchoredPosition =
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
            enemyVisual.anchoredPosition = new Vector2(shake, 0f);
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
        playerVisual.anchoredPosition = new Vector2(lunge - playerShake, 0f);
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
