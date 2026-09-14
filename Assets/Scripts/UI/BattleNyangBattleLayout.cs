using TMPro;
using UnityEngine;

/// <summary>
/// Runtime layout profile for the first Nyang battle pass.
/// The battlefield keeps visual priority while controls, status bars, popups,
/// and actor roots use fixed touch-safe sizes inside the portrait canvas.
/// </summary>
public sealed class BattleNyangBattleLayout
{
    private const float BattlefieldCardBottom = 0.30f;
    private const float BattlefieldCardTop = 0.94f;
    private const float ControlCardBottom = 0.02f;
    private const float ControlCardTop = 0.30f;

    private const float HeroScale = 1.12f;
    private const float CompanionScale = 0.92f;
    private const float NormalEnemyScale = 0.92f;
    private const float BossScale = 1.14f;

    private readonly RectTransform panel;
    private readonly RectTransform enemyCard;
    private readonly RectTransform gameplayArea;
    private readonly RectTransform controlCard;
    private readonly RectTransform powerChargeGroup;
    private readonly RectTransform skillButtonGroup;
    private readonly RectTransform powerChargeButton;
    private readonly RectTransform[] skillButtons =
        new RectTransform[CompanionManager.PartySize];
    private readonly RectTransform[] actorRoots =
        new RectTransform[CompanionManager.PartySize + 2];
    private readonly RectTransform[] quickButtons =
        new RectTransform[4];

    private bool enemyIsBoss;
    private bool applied;
    private Vector2 lastPanelSize;
    private float controlScale = 1f;

    private BattleNyangBattleLayout(RectTransform panel)
    {
        this.panel = panel;
        enemyCard = Find(panel, "EnemyCard");
        gameplayArea = Find(panel, "GamePlayLine");
        controlCard = Find(panel, "BattleControlCard");
        powerChargeGroup = Find(panel, "PowerChargeGroup");
        skillButtonGroup = Find(panel, "SkillButtonGroup");
        powerChargeButton = Find(panel, "PowerChargeButton");

        for (int slot = 0; slot < skillButtons.Length; slot++)
        {
            skillButtons[slot] = Find(
                panel,
                $"CompanionSkillButton{slot + 1}");
        }

        actorRoots[0] = Find(panel, "EnemyActorRoot");
        actorRoots[1] = Find(panel, "SupportActorRoot");
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            actorRoots[slot + 2] = Find(
                panel,
                $"CompanionActorRoot{slot + 1}");
        }

        quickButtons[0] = Find(panel, "QuestQuickButton");
        quickButtons[1] = Find(panel, "EventQuickButton");
        quickButtons[2] = Find(panel, "ShopQuickButton");
        quickButtons[3] = Find(panel, "EquipmentQuickButton");
    }

    public static BattleNyangBattleLayout Attach(RectTransform panel)
    {
        if (panel == null)
            return null;

        BattleNyangBattleLayout layout =
            new BattleNyangBattleLayout(panel);
        layout.Apply();
        return layout;
    }

    public void Refresh()
    {
        if (!applied)
            Apply();
        if (panel == null || panel.rect.size == lastPanelSize)
            return;

        lastPanelSize = panel.rect.size;
        controlScale = Mathf.Min(1f, Mathf.Max(0.1f, panel.rect.height / 1498f));
        ApplyControlLayout();
    }

    public void SetEnemyBoss(bool isBoss)
    {
        if (enemyIsBoss == isBoss && applied)
            return;

        enemyIsBoss = isBoss;
        ApplyActorScales();
    }

    private void Apply()
    {
        if (panel == null)
            return;

        SetNormalizedRect(
            enemyCard,
            new Vector2(0.02f, BattlefieldCardBottom),
            new Vector2(0.98f, BattlefieldCardTop));
        SetNormalizedRect(
            controlCard,
            new Vector2(0.02f, ControlCardBottom),
            new Vector2(0.98f, ControlCardTop));

        // Use the same responsive rectangle for input, spawning and effects.
        SetNormalizedRect(
            gameplayArea,
            new Vector2(0.02f, BattlefieldCardBottom),
            new Vector2(0.98f, BattlefieldCardTop));

        ApplyActorScales();
        ApplyStatusLayout();
        lastPanelSize = panel.rect.size;
        controlScale = Mathf.Min(1f, Mathf.Max(0.1f, panel.rect.height / 1498f));
        ApplyControlLayout();
        ApplyQuickButtonLayout();
        ApplyPresentationLayout();
        ApplyPopupDefaults();
        SetTextMetrics(FindText(panel, "EnemyDamageNumber"), 44f, 24f);
        SetTextMetrics(FindText(panel, "PlayerDamageNumber"), 38f, 22f);
        SetTextMetrics(FindText(panel, "EnemyDamageLabel"), 18f, 12f);
        SetTextMetrics(FindText(panel, "PlayerDamageLabel"), 18f, 12f);
        SetTextMetrics(FindText(panel, "PowerChargePopupText"), 20f, 13f);
        SetTextMetrics(FindText(panel, "RewardPopupLabel"), 18f, 12f);
        SetNormalizedRect(Find(panel, "BattleAnnouncementText"),
            new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));
        // Power totals already live inside the charge button.
        RectTransform powerSeparator = Find(enemyCard, "PowerChargeSeparatorText");
        if (powerSeparator != null) powerSeparator.gameObject.SetActive(false);

        applied = true;
    }

    private void ApplyActorScales()
    {
        SetScale(actorRoots[0], enemyIsBoss ? BossScale : NormalEnemyScale);
        SetScale(actorRoots[1], HeroScale);
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
            SetScale(actorRoots[slot + 2], CompanionScale);
    }

    private void ApplyStatusLayout()
    {
        if (enemyCard == null)
            return;

        SetPixelRect(
            Find(enemyCard, "EnemyName"),
            new Vector2(0.66f, 0.90f),
            new Vector2(230f, 34f));
        SetTextMetrics(
            FindText(enemyCard, "EnemyName"),
            24f,
            15f);

        SetPixelRect(
            Find(enemyCard, "EnemyProgressCurrentNumberText"),
            new Vector2(0.81f, 0.90f),
            new Vector2(56f, 32f));
        SetPixelRect(
            Find(enemyCard, "EnemyProgressSeparatorText"),
            new Vector2(0.865f, 0.90f),
            new Vector2(18f, 32f));
        SetPixelRect(
            Find(enemyCard, "EnemyProgressMaxNumberText"),
            new Vector2(0.915f, 0.90f),
            new Vector2(56f, 32f));
        SetPixelRect(
            Find(enemyCard, "EnemyProgressSuffixText"),
            new Vector2(0.96f, 0.90f),
            new Vector2(26f, 32f));
        SetTextMetrics(
            FindText(enemyCard, "EnemyProgressSeparatorText"),
            18f,
            12f);
        SetTextMetrics(
            FindText(enemyCard, "EnemyProgressSuffixText"),
            18f,
            12f);

        SetPixelRect(
            Find(enemyCard, "EnemyHealthBar"),
            new Vector2(0.80f, 0.845f),
            new Vector2(320f, 38f));
        SetPixelRect(
            Find(enemyCard, "EnemyHealthCurrentNumberText"),
            new Vector2(0.72f, 0.845f),
            new Vector2(90f, 28f));
        SetPixelRect(
            Find(enemyCard, "EnemyHealthSeparatorText"),
            new Vector2(0.80f, 0.845f),
            new Vector2(18f, 28f));
        SetPixelRect(
            Find(enemyCard, "EnemyHealthMaxNumberText"),
            new Vector2(0.88f, 0.845f),
            new Vector2(90f, 28f));

        SetPixelRect(
            Find(enemyCard, "CombatStatus"),
            new Vector2(0.19f, 0.30f),
            new Vector2(300f, 30f));
        SetTextMetrics(
            FindText(enemyCard, "CombatStatus"),
            18f,
            12f);

        SetPixelRect(
            Find(enemyCard, "PlayerHealthBar"),
            new Vector2(0.18f, 0.24f),
            new Vector2(300f, 32f));
        SetPixelRect(
            Find(enemyCard, "PlayerHealthCurrentNumberText"),
            new Vector2(0.08f, 0.24f),
            new Vector2(76f, 26f));
        SetPixelRect(
            Find(enemyCard, "PlayerHealthSeparatorText"),
            new Vector2(0.15f, 0.24f),
            new Vector2(18f, 26f));
        SetPixelRect(
            Find(enemyCard, "PlayerHealthMaxNumberText"),
            new Vector2(0.24f, 0.24f),
            new Vector2(76f, 26f));

        RectTransform heroLabel = Find(enemyCard, "LineDefenseLabelText");
        if (heroLabel == null)
            heroLabel = Find(enemyCard, "HeroHealthLabelText");
        SetPixelRect(heroLabel, new Vector2(0.18f, 0.275f), new Vector2(300f, 22f));
        SetTextMetrics(
            heroLabel == null ? null : heroLabel.GetComponent<TMP_Text>(),
            14f,
            11f);

        SetPixelRect(
            Find(enemyCard, "PowerChargeBar"),
            new Vector2(0.18f, 0.17f),
            new Vector2(300f, 28f));
        RectTransform powerLabel = Find(enemyCard, "PowerChargeLabelText");
        SetPixelRect(powerLabel, new Vector2(0.065f, 0.17f), new Vector2(64f, 22f));
        SetTextMetrics(
            powerLabel == null ? null : powerLabel.GetComponent<TMP_Text>(),
            15f,
            11f);
    }

    private void ApplyControlLayout()
    {
        SetNormalizedRect(
            powerChargeGroup,
            new Vector2(0.04f, 0.10f),
            new Vector2(0.36f, 0.94f));
        SetNormalizedRect(
            skillButtonGroup,
            new Vector2(0.41f, 0.10f),
            new Vector2(0.96f, 0.94f));

        SetPixelRect(
            powerChargeButton,
            new Vector2(0.5f, 0.50f),
            new Vector2(270f, 220f) * controlScale);
        SetButtonLabel(powerChargeButton, 18f, 12f);
        SetNormalizedRect(Find(powerChargeButton, "Label"),
            new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.94f));
        string[] numberNames = { "PowerChargeCurrentNumberText", "PowerChargeSlashText",
            "PowerChargeMaxNumberText", "PowerChargeTapNumberText" };
        float[] left = { 0.08f, 0.31f, 0.38f, 0.64f };
        float[] right = { 0.31f, 0.38f, 0.61f, 0.94f };
        for (int i = 0; i < numberNames.Length; i++)
            SetNormalizedRect(Find(powerChargeButton, numberNames[i]),
                new Vector2(left[i], 0.12f), new Vector2(right[i], 0.42f));

        float[] centers = { 0.17f, 0.50f, 0.83f };
        for (int slot = 0; slot < skillButtons.Length; slot++)
        {
            SetPixelRect(
                skillButtons[slot],
                new Vector2(centers[slot], 0.49f),
                new Vector2(160f, 160f) * controlScale);
            SetButtonLabel(skillButtons[slot], 16f, 11f);
        }

        RectTransform powerHeader = Find(panel, "PowerChargeHeader");
        SetNormalizedRect(
            powerHeader,
            new Vector2(0.04f, 0.84f),
            new Vector2(0.96f, 0.99f));
        SetTextMetrics(
            powerHeader == null ? null : powerHeader.GetComponent<TMP_Text>(),
            17f,
            12f);

        RectTransform skillHeader = Find(panel, "SkillHeader");
        SetNormalizedRect(
            skillHeader,
            new Vector2(0.02f, 0.84f),
            new Vector2(0.98f, 0.99f));
        SetTextMetrics(
            skillHeader == null ? null : skillHeader.GetComponent<TMP_Text>(),
            17f,
            12f);

        RectTransform skillStatus = Find(panel, "SkillStatus");
        SetNormalizedRect(
            skillStatus,
            new Vector2(0.02f, 0.02f),
            new Vector2(0.98f, 0.16f));
        SetTextMetrics(
            skillStatus == null ? null : skillStatus.GetComponent<TMP_Text>(),
            16f,
            11f);
    }

    private void ApplyQuickButtonLayout()
    {
        Vector2[] centers =
        {
            new Vector2(0.10f, 0.82f),
            new Vector2(0.90f, 0.69f),
            new Vector2(0.10f, 0.74f),
            new Vector2(0.90f, 0.61f)
        };

        for (int index = 0; index < quickButtons.Length; index++)
        {
            SetPixelRect(
                quickButtons[index],
                centers[index],
                new Vector2(104f, 72f));
            SetButtonLabel(quickButtons[index], 16f, 11f);
        }

        RectTransform autoButton = Find(panel, "AutoAdvanceButton");
        SetPixelRect(
            autoButton,
            new Vector2(0.91f, 0.955f),
            new Vector2(132f, 52f));
        SetButtonLabel(autoButton, 16f, 11f);
    }

    private void ApplyPresentationLayout()
    {
        SetPixelRect(
            Find(panel, "NyangModeCard"),
            new Vector2(0.41f, 0.965f),
            new Vector2(800f, 62f));
        SetTextMetrics(
            FindText(panel, "NyangModeTitle"),
            20f,
            13f);
        SetTextMetrics(
            FindText(panel, "NyangModeHint"),
            16f,
            11f);
        SetTextMetrics(
            FindText(panel, "NyangStageLabel"),
            17f,
            12f);

        SetPixelRect(
            Find(panel, "NyangCrisisCard"),
            new Vector2(0.5f, 0.77f),
            new Vector2(540f, 72f));
        SetTextMetrics(
            FindText(panel, "NyangCrisisText"),
            22f,
            14f);

        SetPixelRect(
            Find(panel, "NyangComboCard"),
            new Vector2(0.5f, 0.36f),
            new Vector2(360f, 64f));
        SetTextMetrics(
            FindText(panel, "NyangComboText"),
            19f,
            13f);
    }

    private void ApplyPopupDefaults()
    {
        RectTransform effectLayer = Find(panel, "BattlefieldEffectLayer");
        if (effectLayer == null)
            return;

        SetPixelRect(
            Find(effectLayer, "EnemyDamagePopup"),
            new Vector2(0.80f, 0.78f),
            new Vector2(410f, 130f));
        SetPixelRect(
            Find(effectLayer, "PlayerDamagePopup"),
            new Vector2(0.16f, 0.57f),
            new Vector2(300f, 118f));
        SetPixelRect(
            Find(effectLayer, "RewardPopup"),
            new Vector2(0.78f, 0.55f),
            new Vector2(300f, 118f));
        SetPixelRect(
            Find(effectLayer, "PowerChargePopup"),
            new Vector2(0.20f, 0.48f),
            new Vector2(300f, 96f));

        RectTransform announcement = Find(panel, "BattleAnnouncementPanel");
        SetPixelRect(announcement, new Vector2(0.5f, 0.61f), new Vector2(520f, 64f));
        SetTextMetrics(
            FindText(panel, "BattleAnnouncementText"),
            22f,
            14f);
    }

    private static RectTransform Find(Transform root, string name)
    {
        return RuntimeUiBinder.FindRect(root, name);
    }

    private static TMP_Text FindText(Transform root, string name)
    {
        return RuntimeUiBinder.FindText(root, name);
    }

    private static void SetNormalizedRect(
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
        rect.anchoredPosition = Vector2.zero;
    }

    private static void SetPixelRect(
        RectTransform rect,
        Vector2 normalizedCenter,
        Vector2 size)
    {
        if (rect == null)
            return;

        rect.anchorMin = normalizedCenter;
        rect.anchorMax = normalizedCenter;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    private static void SetScale(RectTransform rect, float scale)
    {
        if (rect != null)
            rect.localScale = Vector3.one * scale;
    }

    private static void SetTextMetrics(
        TMP_Text text,
        float fontSizeMax,
        float fontSizeMin)
    {
        if (text == null)
            return;

        text.enableAutoSizing = true;
        text.fontSize = fontSizeMax;
        text.fontSizeMax = fontSizeMax;
        text.fontSizeMin = fontSizeMin;
    }

    private static void SetButtonLabel(
        RectTransform button,
        float fontSizeMax,
        float fontSizeMin)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        SetTextMetrics(label, fontSizeMax, fontSizeMin);
    }
}
