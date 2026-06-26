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
    private RectTransform bossWarningPanel;
    private RectTransform enemyDamagePopup;
    private RectTransform playerDamagePopup;
    private RectTransform rewardPopup;

    private TMP_Text autoAdvanceText;
    private TMP_Text enemyDamageText;
    private TMP_Text playerDamageText;
    private TMP_Text rewardPopupText;
    private TMP_Text bossWarningText;

    private Image enemyVisualImage;
    private Image playerVisualImage;
    private Image attackTrailImage;
    private Image skillProjectileImage;
    private Image battleFlashImage;

    private BattleActorView enemyActorView;
    private BattleActorView playerActorView;
    private readonly BattleActorView[] companionActorViews =
        new BattleActorView[CompanionManager.PartySize];
    private readonly RectTransform[] companionVisualRects =
        new RectTransform[CompanionManager.PartySize];
    private BattleStatusHudUI statusHud;
    private BattleSkillControlsUI skillControls;
    private BattleQuickButtonsUI quickButtons;

    private float enemyAnimationTimer;
    private float playerAnimationTimer;
    private float playerDefeatTimer;
    private float attackTrailTimer;
    private float skillProjectileTimer;
    private float battleFlashTimer;
    private float bossWarningTimer;
    private float enemyDamagePopupTimer;
    private float playerDamagePopupTimer;
    private float rewardPopupTimer;
    private int skillProjectileSlot = -1;
    private Color battleFlashColor = Color.white;
    private readonly float[] companionSkillTimers =
        new float[CompanionManager.PartySize];

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
    }

    public void RefreshAutoAdvance(PlayerData data)
    {
        if (autoAdvanceText == null || data == null)
            return;

        autoAdvanceText.text = data.autoAdvance
            ? LocalizationManager.Text("AUTO ON", "AUTO ON")
            : LocalizationManager.Text("REPEAT", "REPEAT");
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

        BattleVisualProfile enemy =
            BattleVisualResolver.GetEnemy(
                data.currentStage,
                battleManager.IsBoss);
        enemyActorView?.SetVisual(
            enemy?.sprite ?? PrototypeBattleArt.GetEnemySprite(
                data.currentStage,
                battleManager.IsBoss),
            enemy?.animatorController);

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
        attackTrailTimer = 0.18f;
        StartBattleFlash(Accent, 0.09f);
        ShowEnemyDamage(damage, false);
        playerActorView?.Play(BattleAnimationCue.Attack);
        enemyActorView?.Play(BattleAnimationCue.Hit);
    }

    public void HandleCompanionBasicAttackVisual(
        int slot,
        CharacterData character,
        int damage)
    {
        enemyAnimationTimer = 0.22f;
        attackTrailTimer = 0.14f;
        StartBattleFlash(Gold, 0.06f);
        ShowEnemyDamage(damage, false);
        if (slot >= 0 && slot < companionActorViews.Length)
        {
            companionSkillTimers[slot] = 0.22f;
            companionActorViews[slot]?.Play(BattleAnimationCue.Attack);
            StartSkillProjectile(slot);
        }
        enemyActorView?.Play(BattleAnimationCue.Hit);
    }

    public void HandlePowerChargedVisual(float current, float max)
    {
        playerAnimationTimer = 0.14f;
        StartBattleFlash(Success, 0.05f);
        playerActorView?.Play(BattleAnimationCue.Skill);
        RefreshSkillStatus();
    }

    public void HandleEnemyAttackVisual(int damage)
    {
        enemyAnimationTimer = 0.18f;
        playerAnimationTimer = 0.25f;
        StartBattleFlash(Danger, 0.13f);
        ShowPlayerDamage(damage);
        enemyActorView?.Play(BattleAnimationCue.Attack);
        playerActorView?.Play(BattleAnimationCue.Hit);
    }

    public void HandleEnemyDefeatedVisual(int reward)
    {
        enemyAnimationTimer = 0.4f;
        StartBattleFlash(Success, 0.16f);
        ShowRewardPopup(reward);
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
        attackTrailTimer = 0.24f;
        StartBattleFlash(Accent, 0.18f);
        ShowEnemyDamage(damage, true);
        if (slot >= 0 && slot < companionActorViews.Length)
        {
            companionSkillTimers[slot] = 0.36f;
            companionActorViews[slot]?.Play(BattleAnimationCue.Skill);
            StartSkillProjectile(slot);
        }
        enemyActorView?.Play(BattleAnimationCue.Hit);
        showToast?.Invoke(
            $"{character?.skillName ?? "Skill"}  DMG {damage:N0}");
    }

    public void HandleBossChallengeFailed()
    {
        enemyAnimationTimer = 0.4f;
        ShowBossWarning(LocalizationManager.Text("BOSS FAILED", "BOSS FAILED"));
        showToast?.Invoke("Boss time expired. Retrying.");
    }

    public void HandleBossPatternVisual(
        BossPatternDefinition pattern,
        int damage)
    {
        enemyAnimationTimer = 0.3f;
        playerAnimationTimer = 0.3f;
        StartBattleFlash(Danger, 0.2f);
        ShowBossWarning(
            $"{LocalizationManager.Text("BOSS SKILL", "BOSS SKILL")}  " +
            $"{pattern.patternName}");
        ShowPlayerDamage(damage);
        enemyActorView?.Play(BattleAnimationCue.Skill);
        playerActorView?.Play(BattleAnimationCue.Hit);
        showToast?.Invoke($"{pattern.patternName}  DMG {damage:N0}");
    }

    public void UpdateAnimations(float deltaTime)
    {
        enemyAnimationTimer = Mathf.Max(0f, enemyAnimationTimer - deltaTime);
        playerAnimationTimer = Mathf.Max(0f, playerAnimationTimer - deltaTime);
        playerDefeatTimer = Mathf.Max(0f, playerDefeatTimer - deltaTime);
        attackTrailTimer = Mathf.Max(0f, attackTrailTimer - deltaTime);
        skillProjectileTimer = Mathf.Max(0f, skillProjectileTimer - deltaTime);
        battleFlashTimer = Mathf.Max(0f, battleFlashTimer - deltaTime);
        bossWarningTimer = Mathf.Max(0f, bossWarningTimer - deltaTime);
        enemyDamagePopupTimer =
            Mathf.Max(0f, enemyDamagePopupTimer - deltaTime);
        playerDamagePopupTimer =
            Mathf.Max(0f, playerDamagePopupTimer - deltaTime);
        rewardPopupTimer = Mathf.Max(0f, rewardPopupTimer - deltaTime);

        for (int slot = 0; slot < companionSkillTimers.Length; slot++)
        {
            companionSkillTimers[slot] =
                Mathf.Max(0f, companionSkillTimers[slot] - deltaTime);
        }

        UpdateAttackTrail();
        UpdateSkillProjectile();
        UpdateBattleFlash();
        UpdateBossWarning();
        UpdateCompanionAnimations();
        BattleHudUiFactory.UpdateFloatingPopup(
            enemyDamagePopup,
            enemyDamageText,
            enemyDamagePopupTimer,
            0.72f,
            new Vector2(0f, 34f));
        BattleHudUiFactory.UpdateFloatingPopup(
            playerDamagePopup,
            playerDamageText,
            playerDamagePopupTimer,
            0.55f,
            new Vector2(0f, 22f));
        BattleHudUiFactory.UpdateFloatingPopup(
            rewardPopup,
            rewardPopupText,
            rewardPopupTimer,
            0.9f,
            new Vector2(0f, 28f));
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

    private void BuildEnemyPopups(RectTransform enemyCard)
    {
        enemyDamagePopup = RuntimeUiFactory.CreatePanel(
            "EnemyDamagePopup",
            enemyCard,
            new Color32(0, 0, 0, 0),
            new Vector2(0.52f, 0.56f),
            new Vector2(0.94f, 0.78f));
        enemyDamageText = RuntimeUiFactory.CreateText(
            "EnemyDamageText",
            enemyDamagePopup,
            "",
            44,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center,
            Gold);
        enemyDamagePopup.gameObject.SetActive(false);

        rewardPopup = RuntimeUiFactory.CreatePanel(
            "RewardPopup",
            enemyCard,
            new Color32(0, 0, 0, 0),
            new Vector2(0.1f, 0.58f),
            new Vector2(0.48f, 0.76f));
        rewardPopupText = RuntimeUiFactory.CreateText(
            "RewardPopupText",
            rewardPopup,
            "",
            35,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center,
            Success);
        rewardPopup.gameObject.SetActive(false);
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
        skillProjectileImage.sprite = PrototypeUiArt.SkillFrame;
        skillProjectileImage.preserveAspect = true;
        skillProjectileImage.raycastTarget = false;
        skillProjectile.gameObject.SetActive(false);

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

        playerDamagePopup = RuntimeUiFactory.CreatePanel(
            "PlayerDamagePopup",
            enemyCard,
            new Color32(0, 0, 0, 0),
            new Vector2(0.14f, 0.38f),
            new Vector2(0.4f, 0.5f));
        playerDamageText = RuntimeUiFactory.CreateText(
            "PlayerDamageText",
            playerDamagePopup,
            "",
            30,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center,
            Danger);
        playerDamagePopup.gameObject.SetActive(false);

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

    private void ShowEnemyDamage(int damage, bool skill)
    {
        if (enemyDamagePopup == null || enemyDamageText == null)
            return;

        enemyDamagePopupTimer = skill ? 0.72f : 0.55f;
        enemyDamageText.text = skill
            ? $"SKILL\n-{damage:N0}"
            : $"-{damage:N0}";
        enemyDamageText.color = skill ? Accent : Gold;
        enemyDamagePopup.gameObject.SetActive(true);
    }

    private void ShowPlayerDamage(int damage)
    {
        if (playerDamagePopup == null || playerDamageText == null)
            return;

        playerDamagePopupTimer = 0.55f;
        playerDamageText.text = $"-{damage:N0}";
        playerDamageText.color = Danger;
        playerDamagePopup.gameObject.SetActive(true);
    }

    private void ShowRewardPopup(int reward)
    {
        if (rewardPopup == null || rewardPopupText == null)
            return;

        rewardPopupTimer = 0.9f;
        rewardPopupText.text = $"+{reward:N0} GOLD";
        rewardPopupText.color = Success;
        rewardPopup.gameObject.SetActive(true);
    }

    private void StartBattleFlash(Color color, float intensity)
    {
        if (battleFlash == null || battleFlashImage == null)
            return;

        battleFlashColor = color;
        battleFlashTimer = Mathf.Clamp(intensity, 0.05f, 0.3f);
        battleFlash.gameObject.SetActive(true);
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

    private void StartSkillProjectile(int slot)
    {
        if (skillProjectile == null)
            return;

        skillProjectileSlot = slot;
        skillProjectileTimer = 0.32f;
        skillProjectile.gameObject.SetActive(true);
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

        float progress = 1f - Mathf.Clamp01(skillProjectileTimer / 0.32f);
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
            Accent.r,
            Accent.g,
            Accent.b,
            Mathf.Lerp(0.3f, 0.95f, pulse));
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

        if (playerVisual == null || playerVisualImage == null)
            return;

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
