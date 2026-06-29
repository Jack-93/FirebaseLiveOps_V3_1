using System;
using TMPro;
using UnityEngine;

public sealed class EquipmentPanelUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private RectTransform panel;
    private TMP_Text noEquipmentText;
    private EquipmentLoadoutRowUI weaponRow;
    private EquipmentLoadoutRowUI armorRow;
    private TMP_Text powerLabelText;
    private TMP_Text goldLabelText;
    private SpriteNumberText powerNumberText;
    private SpriteNumberText goldNumberText;
    private RectTransform weaponUpgradeFill;
    private RectTransform armorUpgradeFill;

    public GameObject GameObject => panel == null ? null : panel.gameObject;

    private static readonly Color OverlayBackground =
        new Color32(12, 18, 30, 218);
    private static readonly Color Panel =
        new Color32(37, 49, 73, 245);
    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public EquipmentPanelUI(
        RectTransform root,
        Action showMore,
        Action upgradeWeapon,
        Action upgradeArmor,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "EquipmentPanel",
                root,
                out panel))
        {
            Bind(showMore, upgradeWeapon, upgradeArmor);
            return;
        }

        BuildGenerated(root, showMore, upgradeWeapon, upgradeArmor);
    }

    public void BuildGenerated(
        RectTransform root,
        Action showMore,
        Action upgradeWeapon,
        Action upgradeArmor)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "EquipmentPanel",
            root,
            OverlayBackground,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateButton(
            "EquipmentBackButton",
            panel,
            "BACK",
            new Vector2(0.04f, 0.9f),
            new Vector2(0.22f, 0.97f),
            PanelLight,
            () => showMore?.Invoke());

        RuntimeUiFactory.CreateText(
            "EquipmentTitle",
            panel,
            "EQUIPMENT",
            46,
            new Vector2(0.24f, 0.9f),
            new Vector2(0.96f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RuntimeUiFactory.CreateText(
            "EquipmentSubtitle",
            panel,
            "Upgrade equipped gear to raise combat power.",
            24,
            new Vector2(0.24f, 0.86f),
            new Vector2(0.96f, 0.9f),
            TextAlignmentOptions.Center,
            MutedText);

        RectTransform card = RuntimeUiFactory.CreatePanel(
            "EquipmentCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.28f),
            new Vector2(0.94f, 0.84f));

        RuntimeUiFactory.CreateText(
            "EquipmentCardTitle",
            card,
            "CURRENT LOADOUT",
            27,
            new Vector2(0.07f, 0.84f),
            new Vector2(0.93f, 0.95f),
            TextAlignmentOptions.Left,
            Gold);

        noEquipmentText = RuntimeUiFactory.CreateText(
            "EquipmentEmptyText",
            card,
            EquipmentPanelFormatter.NoEquipment,
            31,
            new Vector2(0.07f, 0.45f),
            new Vector2(0.93f, 0.82f),
            TextAlignmentOptions.TopLeft,
            Color.white);
        weaponRow = CreateLoadoutRow(
            card,
            "Weapon",
            "WEAPON",
            "Attack",
            Accent,
            new Vector2(0.07f, 0.63f),
            new Vector2(0.93f, 0.82f));
        armorRow = CreateLoadoutRow(
            card,
            "Armor",
            "ARMOR",
            "Health",
            Success,
            new Vector2(0.07f, 0.45f),
            new Vector2(0.93f, 0.62f));

        powerLabelText = RuntimeUiFactory.CreateText(
            "EquipmentPowerLabelText",
            card,
            "Power",
            23,
            new Vector2(0.07f, 0.37f),
            new Vector2(0.2f, 0.43f),
            TextAlignmentOptions.Left,
            MutedText);
        powerNumberText = new SpriteNumberText(
            card,
            "EquipmentPowerNumberText",
            NumberResourceRoot,
            22f,
            new Vector2(0.2f, 0.37f),
            new Vector2(0.43f, 0.43f));
        goldLabelText = RuntimeUiFactory.CreateText(
            "EquipmentGoldLabelText",
            card,
            "Gold",
            23,
            new Vector2(0.48f, 0.37f),
            new Vector2(0.6f, 0.43f),
            TextAlignmentOptions.Left,
            MutedText);
        goldNumberText = new SpriteNumberText(
            card,
            "EquipmentGoldNumberText",
            NumberResourceRoot,
            22f,
            new Vector2(0.6f, 0.37f),
            new Vector2(0.93f, 0.43f));

        weaponUpgradeFill = RuntimeProgressBar.Create(
            card,
            "WeaponUpgradeProgressBar",
            Accent,
            new Vector2(0.07f, 0.31f),
            new Vector2(0.93f, 0.35f));
        armorUpgradeFill = RuntimeProgressBar.Create(
            card,
            "ArmorUpgradeProgressBar",
            Success,
            new Vector2(0.07f, 0.25f),
            new Vector2(0.93f, 0.29f));

        RuntimeUiFactory.CreateButton(
            "UpgradeWeaponButton",
            card,
            "UPGRADE WEAPON",
            new Vector2(0.07f, 0.08f),
            new Vector2(0.47f, 0.21f),
            Accent,
            () => upgradeWeapon?.Invoke());

        RuntimeUiFactory.CreateButton(
            "UpgradeArmorButton",
            card,
            "UPGRADE ARMOR",
            new Vector2(0.53f, 0.08f),
            new Vector2(0.93f, 0.21f),
            Success,
            () => upgradeArmor?.Invoke());
    }

    public void Refresh(PlayerData data)
    {
        if (data == null)
        {
            SetTextActive(noEquipmentText, true);
            weaponRow?.SetActive(false);
            armorRow?.SetActive(false);
            SetSummaryVisible(false);
            RuntimeProgressBar.Set(weaponUpgradeFill, 0, 20);
            RuntimeProgressBar.Set(armorUpgradeFill, 0, 20);
            return;
        }

        SetTextActive(noEquipmentText, false);
        weaponRow?.SetActive(true);
        armorRow?.SetActive(true);
        SetSummaryVisible(true);
        RefreshLoadoutRows(data);
        SetText(powerLabelText, LocalizationManager.Translate("Power"));
        powerNumberText?.SetText(
            CompactNumberFormatter.Format(
                GameBalance.GetCombatPower(data)));
        SetText(goldLabelText, LocalizationManager.Translate("Gold"));
        goldNumberText?.SetText(
            CompactNumberFormatter.Format(data.gold));

        RuntimeProgressBar.Set(
            weaponUpgradeFill,
            data.weaponUpgradeLevel,
            20);
        RuntimeProgressBar.Set(
            armorUpgradeFill,
            data.armorUpgradeLevel,
            20);
    }

    private void SetSummaryVisible(bool visible)
    {
        SetTextActive(powerLabelText, visible);
        SetTextActive(goldLabelText, visible);
        powerNumberText?.SetActive(visible);
        goldNumberText?.SetActive(visible);
    }

    private void RefreshLoadoutRows(PlayerData data)
    {
        bool hasWeapon = !string.IsNullOrEmpty(data.equippedWeapon);
        bool hasArmor = !string.IsNullOrEmpty(data.equippedArmor);
        weaponRow?.Refresh(
            LocalizationManager.Translate("WEAPON"),
            hasWeapon
                ? data.equippedWeapon
                : LocalizationManager.Translate("None"),
            hasWeapon,
            data.weaponUpgradeLevel,
            LocalizationManager.Translate("Attack"),
            EquipmentManager.GetWeaponAttack(data),
            hasWeapon
                ? EquipmentManager.GetUpgradeCost(data.weaponUpgradeLevel)
                : -1);
        armorRow?.Refresh(
            LocalizationManager.Translate("ARMOR"),
            hasArmor
                ? data.equippedArmor
                : LocalizationManager.Translate("None"),
            hasArmor,
            data.armorUpgradeLevel,
            LocalizationManager.Translate("Health"),
            EquipmentManager.GetArmorHealth(data),
            hasArmor
                ? EquipmentManager.GetUpgradeCost(data.armorUpgradeLevel)
                : -1);
    }

    private void Bind(
        Action showMore,
        Action upgradeWeapon,
        Action upgradeArmor)
    {
        noEquipmentText =
            RuntimeUiBinder.FindText(panel, "EquipmentEmptyText");
        weaponRow = BindLoadoutRow("Weapon");
        armorRow = BindLoadoutRow("Armor");
        powerLabelText =
            RuntimeUiBinder.FindText(panel, "EquipmentPowerLabelText");
        goldLabelText =
            RuntimeUiBinder.FindText(panel, "EquipmentGoldLabelText");
        powerNumberText = RuntimeUiBinder.BindNumber(
            panel,
            "EquipmentPowerNumberText",
            NumberResourceRoot,
            22f);
        goldNumberText = RuntimeUiBinder.BindNumber(
            panel,
            "EquipmentGoldNumberText",
            NumberResourceRoot,
            22f);
        weaponUpgradeFill =
            RuntimeUiBinder.FindProgressFill(
                panel,
                "WeaponUpgradeProgressBar");
        armorUpgradeFill =
            RuntimeUiBinder.FindProgressFill(
                panel,
                "ArmorUpgradeProgressBar");
        Replace("EquipmentBackButton", showMore);
        Replace("UpgradeWeaponButton", upgradeWeapon);
        Replace("UpgradeArmorButton", upgradeArmor);
    }

    private EquipmentLoadoutRowUI BindLoadoutRow(string key)
    {
        RectTransform row =
            RuntimeUiBinder.FindRect(panel, key + "LoadoutRow");
        return new EquipmentLoadoutRowUI(
            row,
            RuntimeUiBinder.FindText(row, key + "SectionText"),
            RuntimeUiBinder.FindText(row, key + "NameText"),
            RuntimeUiBinder.FindText(row, key + "ValueLabelText"),
            RuntimeUiBinder.BindNumber(
                row,
                key + "LevelNumberText",
                NumberResourceRoot,
                19f),
            RuntimeUiBinder.BindNumber(
                row,
                key + "ValueNumberText",
                NumberResourceRoot,
                19f),
            RuntimeUiBinder.BindNumber(
                row,
                key + "CostNumberText",
                NumberResourceRoot,
                19f));
    }

    private void Replace(string buttonName, Action action)
    {
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(panel, buttonName),
            () => action?.Invoke());
    }

    private static EquipmentLoadoutRowUI CreateLoadoutRow(
        RectTransform parent,
        string key,
        string section,
        string valueLabel,
        Color accent,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform row = RuntimeUiFactory.CreatePanel(
            key + "LoadoutRow",
            parent,
            new Color32(24, 35, 58, 180),
            anchorMin,
            anchorMax);

        TMP_Text sectionText = RuntimeUiFactory.CreateText(
            key + "SectionText",
            row,
            section,
            20,
            new Vector2(0.02f, 0.56f),
            new Vector2(0.28f, 0.92f),
            TextAlignmentOptions.Left,
            accent);
        TMP_Text nameText = RuntimeUiFactory.CreateText(
            key + "NameText",
            row,
            "None",
            20,
            new Vector2(0.02f, 0.1f),
            new Vector2(0.36f, 0.52f),
            TextAlignmentOptions.Left,
            Color.white);
        RuntimeUiFactory.CreateText(
            key + "LevelLabel",
            row,
            "Lv.",
            18,
            new Vector2(0.38f, 0.56f),
            new Vector2(0.47f, 0.92f),
            TextAlignmentOptions.Right,
            MutedText);
        SpriteNumberText levelNumberText = new SpriteNumberText(
            row,
            key + "LevelNumberText",
            NumberResourceRoot,
            19f,
            new Vector2(0.47f, 0.56f),
            new Vector2(0.6f, 0.92f));
        TMP_Text valueLabelText = RuntimeUiFactory.CreateText(
            key + "ValueLabelText",
            row,
            valueLabel,
            18,
            new Vector2(0.38f, 0.1f),
            new Vector2(0.52f, 0.5f),
            TextAlignmentOptions.Left,
            MutedText);
        SpriteNumberText valueNumberText = new SpriteNumberText(
            row,
            key + "ValueNumberText",
            NumberResourceRoot,
            19f,
            new Vector2(0.52f, 0.1f),
            new Vector2(0.68f, 0.5f));
        RuntimeUiFactory.CreateText(
            key + "CostLabel",
            row,
            LocalizationManager.Text(
                "Next cost",
                "\uB2E4\uC74C \uBE44\uC6A9"),
            17,
            new Vector2(0.7f, 0.56f),
            new Vector2(0.98f, 0.92f),
            TextAlignmentOptions.Center,
            MutedText);
        SpriteNumberText costNumberText = new SpriteNumberText(
            row,
            key + "CostNumberText",
            NumberResourceRoot,
            19f,
            new Vector2(0.7f, 0.1f),
            new Vector2(0.98f, 0.5f));

        return new EquipmentLoadoutRowUI(
            row,
            sectionText,
            nameText,
            valueLabelText,
            levelNumberText,
            valueNumberText,
            costNumberText);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private static void SetTextActive(TMP_Text text, bool active)
    {
        if (text != null)
            text.gameObject.SetActive(active);
    }

    private sealed class EquipmentLoadoutRowUI
    {
        private readonly RectTransform root;
        private readonly TMP_Text sectionText;
        private readonly TMP_Text nameText;
        private readonly TMP_Text valueLabelText;
        private readonly SpriteNumberText levelNumberText;
        private readonly SpriteNumberText valueNumberText;
        private readonly SpriteNumberText costNumberText;

        public EquipmentLoadoutRowUI(
            RectTransform root,
            TMP_Text sectionText,
            TMP_Text nameText,
            TMP_Text valueLabelText,
            SpriteNumberText levelNumberText,
            SpriteNumberText valueNumberText,
            SpriteNumberText costNumberText)
        {
            this.root = root;
            this.sectionText = sectionText;
            this.nameText = nameText;
            this.valueLabelText = valueLabelText;
            this.levelNumberText = levelNumberText;
            this.valueNumberText = valueNumberText;
            this.costNumberText = costNumberText;
        }

        public void SetActive(bool active)
        {
            if (root != null)
                root.gameObject.SetActive(active);
        }

        public void Refresh(
            string section,
            string itemName,
            bool hasItem,
            int level,
            string valueLabel,
            int value,
            int nextCost)
        {
            SetText(sectionText, section);
            SetText(nameText, itemName);
            SetText(valueLabelText, valueLabel);
            levelNumberText?.SetText(hasItem
                ? CompactNumberFormatter.Format(level)
                : "-");
            valueNumberText?.SetText(
                CompactNumberFormatter.Format(value, "+"));
            costNumberText?.SetText(nextCost >= 0
                ? CompactNumberFormatter.Format(nextCost)
                : "-");
        }
    }
}
