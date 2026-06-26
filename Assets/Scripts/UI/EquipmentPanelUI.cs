using System;
using TMPro;
using UnityEngine;

public sealed class EquipmentPanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text equipmentText;
    private readonly TMP_Text equipmentPowerText;
    private readonly RectTransform weaponUpgradeFill;
    private readonly RectTransform armorUpgradeFill;

    public GameObject GameObject => panel.gameObject;

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

        equipmentText = RuntimeUiFactory.CreateText(
            "EquipmentText",
            card,
            EquipmentPanelFormatter.NoEquipment,
            31,
            new Vector2(0.07f, 0.45f),
            new Vector2(0.93f, 0.82f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        equipmentPowerText = RuntimeUiFactory.CreateText(
            "EquipmentPowerText",
            card,
            "0",
            23,
            new Vector2(0.07f, 0.37f),
            new Vector2(0.93f, 0.43f),
            TextAlignmentOptions.Left,
            MutedText);

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
            equipmentText.text = EquipmentPanelFormatter.NoEquipment;
            equipmentPowerText.text = string.Empty;
            RuntimeProgressBar.Set(weaponUpgradeFill, 0, 20);
            RuntimeProgressBar.Set(armorUpgradeFill, 0, 20);
            return;
        }

        equipmentText.text = EquipmentPanelFormatter.FormatLoadout(data);
        equipmentPowerText.text =
            EquipmentPanelFormatter.FormatPowerSummary(data);

        RuntimeProgressBar.Set(
            weaponUpgradeFill,
            data.weaponUpgradeLevel,
            20);
        RuntimeProgressBar.Set(
            armorUpgradeFill,
            data.armorUpgradeLevel,
            20);
    }
}
