using TMPro;
using UnityEngine;

public sealed class BattleStatusHudUI
{
    private readonly Color danger;
    private readonly Color accent;
    private readonly Color success;

    private RectTransform enemyHealthFill;
    private RectTransform playerHealthFill;
    private RectTransform powerChargeFill;
    private TMP_Text enemyNameText;
    private TMP_Text enemyHealthText;
    private TMP_Text playerHealthText;
    private TMP_Text powerChargeText;
    private TMP_Text combatStatusText;

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
            new Vector2(0.66f, 0.84f),
            new Vector2(0.96f, 0.92f),
            TextAlignmentOptions.Center,
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

        enemyHealthText = RuntimeUiFactory.CreateText(
            "EnemyHealthText",
            parent,
            "0 / 0",
            20,
            new Vector2(0.7f, 0.635f),
            new Vector2(0.96f, 0.685f),
            TextAlignmentOptions.Center,
            Color.white);
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
        playerHealthText = RuntimeUiFactory.CreateText(
            "PlayerHealthText",
            parent,
            "0 / 0",
            18,
            new Vector2(0.03f, 0.195f),
            new Vector2(0.32f, 0.24f),
            TextAlignmentOptions.Center,
            Color.white);

        powerChargeFill = BattleHudUiFactory.CreateHealthBar(
            parent,
            "PowerChargeBar",
            accent,
            new Vector2(0.03f, 0.145f),
            new Vector2(0.32f, 0.18f));
        powerChargeText = RuntimeUiFactory.CreateText(
            "PowerChargeText",
            parent,
            "Power 0 / 100",
            18,
            new Vector2(0.03f, 0.14f),
            new Vector2(0.32f, 0.185f),
            TextAlignmentOptions.Center,
            Color.white);
    }

    public void Refresh(BattleManager battleManager, PlayerData data)
    {
        if (battleManager == null || data == null)
            return;

        RefreshEnemyName(battleManager, data);

        enemyHealthText.text =
            $"{battleManager.EnemyHealth:N0} / " +
            $"{battleManager.EnemyMaxHealth:N0}";
        playerHealthText.text =
            $"{battleManager.PlayerHealth:N0} / " +
            $"{battleManager.PlayerMaxHealth:N0}";
        powerChargeText.text =
            $"{LocalizationManager.Translate("Power Charge")} " +
            $"{battleManager.PowerCharge:0} / " +
            $"{battleManager.PowerChargeMax:0}";

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

        combatStatusText.text = !battleManager.IsRunning
            ? LocalizationManager.Translate("Paused")
            : $"{LocalizationManager.Translate("Companion DMG")} " +
              $"{battleManager.LastPlayerDamage:N0}  " +
              $"{LocalizationManager.Translate("Charge")} " +
              $"{battleManager.PowerCharge:0}%";
    }

    private void RefreshEnemyName(BattleManager battleManager, PlayerData data)
    {
        enemyNameText.text = battleManager.IsBoss
            ? $"{battleManager.EnemyName}  " +
              $"{battleManager.BossTimeRemaining:0.0}s"
            : $"{battleManager.EnemyName}  " +
              $"{data.stageEnemyIndex + 1}/" +
              $"{GameBalance.EnemiesPerStage - 1}";
        enemyNameText.color =
            battleManager.IsBoss &&
            battleManager.BossTimeRemaining <= 5f
                ? danger
                : Color.white;
    }
}
