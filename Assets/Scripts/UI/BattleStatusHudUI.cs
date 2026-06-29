using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleStatusHudUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private readonly Color danger;
    private readonly Color accent;
    private readonly Color success;

    private RectTransform enemyHealthFill;
    private RectTransform playerHealthFill;
    private RectTransform powerChargeFill;
    private Image powerChargeFillImage;
    private TMP_Text enemyNameText;
    private TMP_Text enemyProgressSeparatorText;
    private TMP_Text enemyProgressSuffixText;
    private TMP_Text powerChargeLabelText;
    private TMP_Text combatStatusText;
    private SpriteNumberText enemyProgressCurrentNumberText;
    private SpriteNumberText enemyProgressMaxNumberText;
    private SpriteNumberText enemyHealthCurrentNumberText;
    private SpriteNumberText enemyHealthMaxNumberText;
    private SpriteNumberText playerHealthCurrentNumberText;
    private SpriteNumberText playerHealthMaxNumberText;
    private SpriteNumberText powerChargeCurrentNumberText;
    private SpriteNumberText powerChargeMaxNumberText;

    public BattleStatusHudUI(
        Color danger,
        Color accent,
        Color success)
    {
        this.danger = danger;
        this.accent = accent;
        this.success = success;
    }

    public void BuildEnemyName(RectTransform parent)
    {
        enemyNameText = RuntimeUiFactory.CreateText(
            "EnemyName",
            parent,
            "Enemy",
            28,
            new Vector2(0.56f, 0.84f),
            new Vector2(0.79f, 0.92f),
            TextAlignmentOptions.Right,
            Color.white);
        enemyProgressCurrentNumberText = new SpriteNumberText(
            parent,
            "EnemyProgressCurrentNumberText",
            NumberResourceRoot,
            22f,
            new Vector2(0.8f, 0.84f),
            new Vector2(0.88f, 0.92f));
        enemyProgressSeparatorText = RuntimeUiFactory.CreateText(
            "EnemyProgressSeparatorText",
            parent,
            "/",
            22,
            new Vector2(0.88f, 0.84f),
            new Vector2(0.9f, 0.92f),
            TextAlignmentOptions.Center,
            Color.white);
        enemyProgressMaxNumberText = new SpriteNumberText(
            parent,
            "EnemyProgressMaxNumberText",
            NumberResourceRoot,
            22f,
            new Vector2(0.9f, 0.84f),
            new Vector2(0.96f, 0.92f));
        enemyProgressSuffixText = RuntimeUiFactory.CreateText(
            "EnemyProgressSuffixText",
            parent,
            "s",
            20,
            new Vector2(0.9f, 0.84f),
            new Vector2(0.96f, 0.92f),
            TextAlignmentOptions.Left,
            Color.white);
    }

    public void BuildEnemyHealth(RectTransform parent)
    {
        enemyHealthFill = BattleHudUiFactory.CreateHealthBar(
            parent,
            "EnemyHealthBar",
            danger,
            new Vector2(0.7f, 0.64f),
            new Vector2(0.96f, 0.68f));

        enemyHealthCurrentNumberText = new SpriteNumberText(
            parent,
            "EnemyHealthCurrentNumberText",
            NumberResourceRoot,
            20f,
            new Vector2(0.705f, 0.635f),
            new Vector2(0.82f, 0.685f));
        RuntimeUiFactory.CreateText(
            "EnemyHealthSeparatorText",
            parent,
            "/",
            20,
            new Vector2(0.82f, 0.635f),
            new Vector2(0.84f, 0.685f),
            TextAlignmentOptions.Center,
            Color.white);
        enemyHealthMaxNumberText = new SpriteNumberText(
            parent,
            "EnemyHealthMaxNumberText",
            NumberResourceRoot,
            20f,
            new Vector2(0.84f, 0.635f),
            new Vector2(0.955f, 0.685f));
    }

    public void BuildPlayerStatus(RectTransform parent)
    {
        combatStatusText = RuntimeUiFactory.CreateText(
            "CombatStatus",
            parent,
            "Preparing...",
            22,
            new Vector2(0.03f, 0.26f),
            new Vector2(0.37f, 0.32f),
            TextAlignmentOptions.Left,
            Color.white);

        playerHealthFill = BattleHudUiFactory.CreateHealthBar(
            parent,
            "PlayerHealthBar",
            success,
            new Vector2(0.03f, 0.2f),
            new Vector2(0.32f, 0.235f));
        playerHealthCurrentNumberText = new SpriteNumberText(
            parent,
            "PlayerHealthCurrentNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.035f, 0.195f),
            new Vector2(0.15f, 0.24f));
        RuntimeUiFactory.CreateText(
            "PlayerHealthSeparatorText",
            parent,
            "/",
            18,
            new Vector2(0.15f, 0.195f),
            new Vector2(0.17f, 0.24f),
            TextAlignmentOptions.Center,
            Color.white);
        playerHealthMaxNumberText = new SpriteNumberText(
            parent,
            "PlayerHealthMaxNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.17f, 0.195f),
            new Vector2(0.315f, 0.24f));

        powerChargeFill = BattleHudUiFactory.CreateHealthBar(
            parent,
            "PowerChargeBar",
            accent,
            new Vector2(0.03f, 0.145f),
            new Vector2(0.32f, 0.18f));
        powerChargeFillImage =
            powerChargeFill.GetComponent<Image>();
        powerChargeLabelText = RuntimeUiFactory.CreateText(
            "PowerChargeLabelText",
            parent,
            "Power",
            15,
            new Vector2(0.03f, 0.14f),
            new Vector2(0.13f, 0.185f),
            TextAlignmentOptions.Left,
            Color.white);
        powerChargeCurrentNumberText = new SpriteNumberText(
            parent,
            "PowerChargeCurrentNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.13f, 0.14f),
            new Vector2(0.205f, 0.185f));
        RuntimeUiFactory.CreateText(
            "PowerChargeSeparatorText",
            parent,
            "/",
            18,
            new Vector2(0.205f, 0.14f),
            new Vector2(0.225f, 0.185f),
            TextAlignmentOptions.Center,
            Color.white);
        powerChargeMaxNumberText = new SpriteNumberText(
            parent,
            "PowerChargeMaxNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.225f, 0.14f),
            new Vector2(0.32f, 0.185f));
    }

    public void Bind(RectTransform parent)
    {
        enemyNameText = RuntimeUiBinder.FindText(parent, "EnemyName");
        enemyProgressSeparatorText =
            RuntimeUiBinder.FindText(parent, "EnemyProgressSeparatorText");
        enemyProgressSuffixText =
            RuntimeUiBinder.FindText(parent, "EnemyProgressSuffixText");
        combatStatusText =
            RuntimeUiBinder.FindText(parent, "CombatStatus");
        powerChargeLabelText =
            RuntimeUiBinder.FindText(parent, "PowerChargeLabelText");

        enemyHealthFill = FindFill(parent, "EnemyHealthBar");
        playerHealthFill = FindFill(parent, "PlayerHealthBar");
        powerChargeFill = FindFill(parent, "PowerChargeBar");
        powerChargeFillImage =
            powerChargeFill == null
                ? null
                : powerChargeFill.GetComponent<Image>();

        enemyProgressCurrentNumberText = BindNumber(
            parent,
            "EnemyProgressCurrentNumberText",
            22f);
        enemyProgressMaxNumberText = BindNumber(
            parent,
            "EnemyProgressMaxNumberText",
            22f);
        enemyHealthCurrentNumberText = BindNumber(
            parent,
            "EnemyHealthCurrentNumberText",
            20f);
        enemyHealthMaxNumberText = BindNumber(
            parent,
            "EnemyHealthMaxNumberText",
            20f);
        playerHealthCurrentNumberText = BindNumber(
            parent,
            "PlayerHealthCurrentNumberText",
            18f);
        playerHealthMaxNumberText = BindNumber(
            parent,
            "PlayerHealthMaxNumberText",
            18f);
        powerChargeCurrentNumberText = BindNumber(
            parent,
            "PowerChargeCurrentNumberText",
            18f);
        powerChargeMaxNumberText = BindNumber(
            parent,
            "PowerChargeMaxNumberText",
            18f);
    }

    public void Refresh(BattleManager battleManager, PlayerData data)
    {
        if (battleManager == null || data == null)
            return;

        RefreshEnemyName(battleManager, data);

        enemyHealthCurrentNumberText.SetText(
            CompactNumberFormatter.Format(battleManager.EnemyHealth));
        enemyHealthMaxNumberText.SetText(
            CompactNumberFormatter.Format(battleManager.EnemyMaxHealth));

        playerHealthCurrentNumberText.SetText(
            CompactNumberFormatter.Format(battleManager.PlayerHealth));
        playerHealthMaxNumberText.SetText(
            CompactNumberFormatter.Format(battleManager.PlayerMaxHealth));

        powerChargeLabelText.text =
            LocalizationManager.Text("Power", "전력");
        powerChargeCurrentNumberText.SetText(
            CompactNumberFormatter.Format(
                Mathf.RoundToInt(battleManager.PowerCharge)));
        powerChargeMaxNumberText.SetText(
            CompactNumberFormatter.Format(
                Mathf.RoundToInt(battleManager.PowerChargeMax)));

        BattleHudUiFactory.SetBar(
            enemyHealthFill,
            battleManager.EnemyHealth,
            battleManager.EnemyMaxHealth);
        BattleHudUiFactory.SetBar(
            playerHealthFill,
            battleManager.PlayerHealth,
            battleManager.PlayerMaxHealth);
        BattleHudUiFactory.SetBar(
            powerChargeFill,
            battleManager.PowerCharge,
            battleManager.PowerChargeMax);

        bool fullPower =
            battleManager.PowerCharge >= battleManager.PowerChargeMax;
        bool enoughPower =
            battleManager.PowerCharge >=
            BattleManager.CompanionSkillPowerCost;
        combatStatusText.text = GetCombatStatusText(
            battleManager,
            fullPower,
            enoughPower);
        combatStatusText.color = fullPower ? accent : Color.white;

        if (powerChargeFillImage != null)
            powerChargeFillImage.color = fullPower
                ? success
                : enoughPower
                    ? accent
                    : Color.white;
    }

    private static string GetCombatStatusText(
        BattleManager battleManager,
        bool fullPower,
        bool enoughPower)
    {
        if (!battleManager.IsRunning)
            return LocalizationManager.Translate("Paused");

        if (battleManager.IsRecovering)
            return LocalizationManager.Text(
                "Recovering...",
                "\uD68C\uBCF5 \uC911...");

        if (fullPower)
        {
            return LocalizationManager.Text(
                "FULL POWER - skills boosted",
                "\uC804\uB825 \uCD5C\uB300 - \uC2A4\uD0AC \uAC00\uC18D");
        }

        return enoughPower
            ? LocalizationManager.Text(
                "Skill power ready",
                "\uC2A4\uD0AC \uC804\uB825 \uC900\uBE44")
            : LocalizationManager.Text(
                "Charge power",
                "\uC804\uB825 \uCDA9\uC804");
    }

    private void RefreshEnemyName(BattleManager battleManager, PlayerData data)
    {
        enemyNameText.text = battleManager.EnemyName;
        bool dangerTime =
            battleManager.IsBoss &&
            battleManager.BossTimeRemaining <= 5f;
        enemyNameText.color =
            dangerTime
                ? danger
                : Color.white;
        enemyProgressSeparatorText.gameObject.SetActive(!battleManager.IsBoss);
        enemyProgressMaxNumberText.SetActive(!battleManager.IsBoss);
        enemyProgressSuffixText.gameObject.SetActive(battleManager.IsBoss);
        enemyProgressSuffixText.color = dangerTime ? danger : Color.white;

        if (battleManager.IsBoss)
        {
            enemyProgressCurrentNumberText.SetText(
                battleManager.BossTimeRemaining.ToString(
                    "0.0",
                    CultureInfo.InvariantCulture));
            return;
        }

        enemyProgressCurrentNumberText.SetText(
            CompactNumberFormatter.Format(data.stageEnemyIndex + 1));
        enemyProgressMaxNumberText.SetText(
            CompactNumberFormatter.Format(
                GameBalance.EnemiesPerStage - 1));
    }

    private static RectTransform FindFill(
        Transform parent,
        string barName)
    {
        RectTransform bar = RuntimeUiBinder.FindRect(parent, barName);
        return RuntimeUiBinder.FindChildRect(bar, "Fill");
    }

    private static SpriteNumberText BindNumber(
        Transform parent,
        string name,
        float characterHeight)
    {
        return new SpriteNumberText(
            RuntimeUiBinder.FindRect(parent, name),
            NumberResourceRoot,
            characterHeight);
    }
}
