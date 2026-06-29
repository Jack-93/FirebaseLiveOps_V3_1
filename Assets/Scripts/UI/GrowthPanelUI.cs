using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class GrowthPanelUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";
    private const string GrowthPanelPrefabResourcePath =
        "Prefabs/UI/GrowthPanel";

    private RectTransform panel;
    private GrowthUpgradeRowUI attackGrowthRow;
    private GrowthUpgradeRowUI healthGrowthRow;
    private GrowthUpgradeRowUI speedGrowthRow;

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
        Action upgradeAttackSpeed,
        bool usePrefab = true)
    {
        if (usePrefab &&
            TryBuildFromPrefab(
                root,
                upgradeAttack,
                upgradeHealth,
                upgradeAttackSpeed))
        {
            return;
        }

        BuildGenerated(
            root,
            upgradeAttack,
            upgradeHealth,
            upgradeAttackSpeed);
    }

    public void BuildGenerated(
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

        attackGrowthRow = CreateUpgradeRow(
            panel,
            "Attack",
            "Increase damage",
            new Vector2(0.06f, 0.59f),
            () => upgradeAttack?.Invoke());

        healthGrowthRow = CreateUpgradeRow(
            panel,
            "Health",
            "Increase maximum HP",
            new Vector2(0.06f, 0.35f),
            () => upgradeHealth?.Invoke());

        speedGrowthRow = CreateUpgradeRow(
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

        attackGrowthRow.Refresh(
            LocalizationManager.Text("Attack", "\uACF5\uACA9"),
            growthManager.GetLevel(UpgradeType.Attack),
            LocalizationManager.Text("ATK", "\uACF5\uACA9"),
            CompactNumberFormatter.Format(GameBalance.GetPlayerAttack(data)),
            CompactNumberFormatter.Format(
                growthManager.GetCost(UpgradeType.Attack)));
        healthGrowthRow.Refresh(
            LocalizationManager.Text("Health", "\uCCB4\uB825"),
            growthManager.GetLevel(UpgradeType.Health),
            "HP",
            CompactNumberFormatter.Format(GetPlayerMaxHealth(battleManager)),
            CompactNumberFormatter.Format(
                growthManager.GetCost(UpgradeType.Health)));
        speedGrowthRow.Refresh(
            LocalizationManager.Text(
                "Attack Speed",
                "\uACF5\uACA9 \uC18D\uB3C4"),
            growthManager.GetLevel(UpgradeType.AttackSpeed),
            LocalizationManager.Text("Interval", "\uAC04\uACA9"),
            GameBalance.GetPlayerAttackInterval(data)
                .ToString("0.00", CultureInfo.InvariantCulture),
            CompactNumberFormatter.Format(
                growthManager.GetCost(UpgradeType.AttackSpeed)));
    }

    private bool TryBuildFromPrefab(
        RectTransform root,
        Action upgradeAttack,
        Action upgradeHealth,
        Action upgradeAttackSpeed)
    {
        GameObject prefab =
            Resources.Load<GameObject>(GrowthPanelPrefabResourcePath);
        if (prefab == null)
            return false;

        GameObject instance = UnityEngine.Object.Instantiate(
            prefab,
            root,
            false);
        instance.name = "GrowthPanel";
        panel = instance.GetComponent<RectTransform>();
        if (panel == null)
            return false;

        BindPrefab(upgradeAttack, upgradeHealth, upgradeAttackSpeed);
        return true;
    }

    private void BindPrefab(
        Action upgradeAttack,
        Action upgradeHealth,
        Action upgradeAttackSpeed)
    {
        attackGrowthRow = BindUpgradeRow(
            panel,
            "Attack",
            () => upgradeAttack?.Invoke());
        healthGrowthRow = BindUpgradeRow(
            panel,
            "Health",
            () => upgradeHealth?.Invoke());
        speedGrowthRow = BindUpgradeRow(
            panel,
            "Attack Speed",
            () => upgradeAttackSpeed?.Invoke());
    }

    private static int GetPlayerMaxHealth(BattleManager battleManager)
    {
        return battleManager == null
            ? 0
            : battleManager.PlayerMaxHealth;
    }

    private static GrowthUpgradeRowUI CreateUpgradeRow(
        RectTransform parent,
        string title,
        string description,
        Vector2 anchorMin,
        UnityAction action)
    {
        RectTransform row = RuntimeUiFactory.CreatePanel(
            title + "Row",
            parent,
            Panel,
            anchorMin,
            new Vector2(0.94f, anchorMin.y + 0.2f));

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

        TMP_Text titleText = RuntimeUiFactory.CreateText(
            title + "Title",
            row,
            title,
            27,
            new Vector2(0.17f, 0.62f),
            new Vector2(0.43f, 0.92f),
            TextAlignmentOptions.Left,
            Gold);
        RuntimeUiFactory.CreateText(
            title + "LevelLabel",
            row,
            "Lv.",
            21,
            new Vector2(0.43f, 0.62f),
            new Vector2(0.51f, 0.92f),
            TextAlignmentOptions.Right,
            Color.white);
        SpriteNumberText levelNumberText = new SpriteNumberText(
            row,
            title + "LevelNumberText",
            NumberResourceRoot,
            21f,
            new Vector2(0.51f, 0.62f),
            new Vector2(0.66f, 0.92f));

        TMP_Text valueLabelText = RuntimeUiFactory.CreateText(
            title + "ValueLabel",
            row,
            "Value",
            20,
            new Vector2(0.17f, 0.34f),
            new Vector2(0.32f, 0.58f),
            TextAlignmentOptions.Left,
            Color.white);
        SpriteNumberText valueNumberText = new SpriteNumberText(
            row,
            title + "ValueNumberText",
            NumberResourceRoot,
            21f,
            new Vector2(0.32f, 0.34f),
            new Vector2(0.66f, 0.58f));

        RuntimeUiFactory.CreateText(
            title + "CostLabel",
            row,
            LocalizationManager.Text("Cost", "\uBE44\uC6A9"),
            20,
            new Vector2(0.17f, 0.08f),
            new Vector2(0.32f, 0.31f),
            TextAlignmentOptions.Left,
            Color.white);
        SpriteNumberText costNumberText = new SpriteNumberText(
            row,
            title + "CostNumberText",
            NumberResourceRoot,
            21f,
            new Vector2(0.32f, 0.08f),
            new Vector2(0.66f, 0.31f));

        RuntimeUiFactory.CreateButton(
            title + "Button",
            row,
            "UPGRADE",
            new Vector2(0.68f, 0.16f),
            new Vector2(0.95f, 0.84f),
            Accent,
            action);

        return new GrowthUpgradeRowUI(
            titleText,
            valueLabelText,
            levelNumberText,
            valueNumberText,
            costNumberText);
    }

    private static GrowthUpgradeRowUI BindUpgradeRow(
        RectTransform parent,
        string title,
        UnityAction action)
    {
        RectTransform row =
            RuntimeUiBinder.FindRect(parent, title + "Row");
        Button button =
            RuntimeUiBinder.FindButton(row, title + "Button");
        RuntimeUiBinder.ReplaceButtonAction(button, action);

        return new GrowthUpgradeRowUI(
            RuntimeUiBinder.FindText(row, title + "Title"),
            RuntimeUiBinder.FindText(row, title + "ValueLabel"),
            new SpriteNumberText(
                RuntimeUiBinder.FindRect(row, title + "LevelNumberText"),
                NumberResourceRoot,
                21f),
            new SpriteNumberText(
                RuntimeUiBinder.FindRect(row, title + "ValueNumberText"),
                NumberResourceRoot,
                21f),
            new SpriteNumberText(
                RuntimeUiBinder.FindRect(row, title + "CostNumberText"),
                NumberResourceRoot,
                21f));
    }

    private sealed class GrowthUpgradeRowUI
    {
        private readonly TMP_Text titleText;
        private readonly TMP_Text valueLabelText;
        private readonly SpriteNumberText levelNumberText;
        private readonly SpriteNumberText valueNumberText;
        private readonly SpriteNumberText costNumberText;

        public GrowthUpgradeRowUI(
            TMP_Text titleText,
            TMP_Text valueLabelText,
            SpriteNumberText levelNumberText,
            SpriteNumberText valueNumberText,
            SpriteNumberText costNumberText)
        {
            this.titleText = titleText;
            this.valueLabelText = valueLabelText;
            this.levelNumberText = levelNumberText;
            this.valueNumberText = valueNumberText;
            this.costNumberText = costNumberText;
        }

        public void Refresh(
            string title,
            int level,
            string valueLabel,
            string value,
            string cost)
        {
            if (titleText != null)
                titleText.text = title;
            if (valueLabelText != null)
                valueLabelText.text = valueLabel;
            levelNumberText?.SetText(CompactNumberFormatter.Format(level));
            valueNumberText?.SetText(value);
            costNumberText?.SetText(cost);
        }
    }
}
