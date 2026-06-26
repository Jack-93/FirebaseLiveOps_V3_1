using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GrowthPanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text attackGrowthText;
    private readonly TMP_Text healthGrowthText;
    private readonly TMP_Text speedGrowthText;

    public GameObject GameObject => panel.gameObject;

    private static readonly Color OverlayBackground =
        new Color32(12, 18, 30, 218);
    private static readonly Color Panel =
        new Color32(37, 49, 73, 245);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public GrowthPanelUI(
        RectTransform root,
        Action upgradeAttack,
        Action upgradeHealth,
        Action upgradeAttackSpeed)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "GrowthPanel",
            root,
            OverlayBackground,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateText(
            "GrowthTitle",
            panel,
            "GROWTH",
            48,
            new Vector2(0.05f, 0.88f),
            new Vector2(0.95f, 0.97f),
            TextAlignmentOptions.Center,
            Accent);

        RuntimeUiFactory.CreateText(
            "GrowthSubtitle",
            panel,
            "Spend Gold to strengthen your hero.",
            27,
            new Vector2(0.06f, 0.82f),
            new Vector2(0.94f, 0.88f),
            TextAlignmentOptions.Center,
            MutedText);

        attackGrowthText = CreateUpgradeRow(
            panel,
            "Attack",
            "Increase damage",
            new Vector2(0.06f, 0.59f),
            () => upgradeAttack?.Invoke());

        healthGrowthText = CreateUpgradeRow(
            panel,
            "Health",
            "Increase maximum HP",
            new Vector2(0.06f, 0.35f),
            () => upgradeHealth?.Invoke());

        speedGrowthText = CreateUpgradeRow(
            panel,
            "Attack Speed",
            "Attack more frequently",
            new Vector2(0.06f, 0.11f),
            () => upgradeAttackSpeed?.Invoke());
    }

    public void Refresh(
        GrowthManager growthManager,
        BattleManager battleManager,
        PlayerData data)
    {
        if (growthManager == null || data == null)
            return;

        attackGrowthText.text =
            $"{LocalizationManager.Text("Attack", "공격력")} " +
            $"Lv.{growthManager.GetLevel(UpgradeType.Attack)}\n" +
            $"{LocalizationManager.Text("ATK", "공격")} " +
            $"{GameBalance.GetPlayerAttack(data):N0}  (+base 6)\n" +
            $"{LocalizationManager.Text("Cost", "비용")} " +
            $"{growthManager.GetCost(UpgradeType.Attack):N0}";
        healthGrowthText.text =
            $"{LocalizationManager.Text("Health", "체력")} " +
            $"Lv.{growthManager.GetLevel(UpgradeType.Health)}\n" +
            $"HP {GetPlayerMaxHealth(battleManager):N0}  (+base 30)\n" +
            $"{LocalizationManager.Text("Cost", "비용")} " +
            $"{growthManager.GetCost(UpgradeType.Health):N0}";
        speedGrowthText.text =
            $"{LocalizationManager.Text("Attack Speed", "공격 속도")} " +
            $"Lv.{growthManager.GetLevel(UpgradeType.AttackSpeed)}\n" +
            $"{LocalizationManager.Text("Interval", "간격")} " +
            $"{GameBalance.GetPlayerAttackInterval(data):0.00}s\n" +
            $"{LocalizationManager.Text("Cost", "비용")} " +
            $"{growthManager.GetCost(UpgradeType.AttackSpeed):N0}";
    }

    private static int GetPlayerMaxHealth(BattleManager battleManager)
    {
        return battleManager == null
            ? 0
            : battleManager.PlayerMaxHealth;
    }

    private static TMP_Text CreateUpgradeRow(
        RectTransform parent,
        string title,
        string description,
        Vector2 anchorMin,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform row = RuntimeUiFactory.CreatePanel(
            title + "Row",
            parent,
            Panel,
            anchorMin,
            new Vector2(0.94f, anchorMin.y + 0.2f));

        TMP_Text info = RuntimeUiFactory.CreateText(
            title + "Info",
            row,
            title,
            29,
            new Vector2(0.17f, 0.34f),
            new Vector2(0.66f, 0.9f),
            TextAlignmentOptions.Left,
            Gold);

        Image icon = RuntimeUiFactory.CreateSpriteImage(
            title + "Icon",
            row,
            PrototypeUiArt.GetButtonIcon("GrowthButton"),
            new Vector2(0.04f, 0.34f),
            new Vector2(0.15f, 0.82f));
        icon.color = Accent;

        RuntimeUiFactory.CreateText(
            title + "Description",
            row,
            description,
            22,
            new Vector2(0.17f, 0.08f),
            new Vector2(0.66f, 0.32f),
            TextAlignmentOptions.Left,
            new Color32(180, 194, 218, 255));

        RuntimeUiFactory.CreateButton(
            title + "Button",
            row,
            "UPGRADE",
            new Vector2(0.68f, 0.16f),
            new Vector2(0.95f, 0.84f),
            Accent,
            action);

        return info;
    }
}
