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
            "No equipment.",
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

        weaponUpgradeFill = CreateHealthBar(
            card,
            "WeaponUpgradeProgressBar",
            Accent,
            new Vector2(0.07f, 0.31f),
            new Vector2(0.93f, 0.35f));
        armorUpgradeFill = CreateHealthBar(
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
            equipmentText.text = LocalizationManager.Text(
                "No equipment.",
                "No equipment.");
            equipmentPowerText.text = string.Empty;
            SetBar(weaponUpgradeFill, 0, 20);
            SetBar(armorUpgradeFill, 0, 20);
            return;
        }

        string weapon = string.IsNullOrEmpty(data.equippedWeapon)
            ? LocalizationManager.Text("None", "없음")
            : data.equippedWeapon;
        string armor = string.IsNullOrEmpty(data.equippedArmor)
            ? LocalizationManager.Text("None", "없음")
            : data.equippedArmor;
        bool hasWeapon = !string.IsNullOrEmpty(data.equippedWeapon);
        bool hasArmor = !string.IsNullOrEmpty(data.equippedArmor);

        equipmentText.text =
            $"{LocalizationManager.Text("WEAPON", "무기")}\n" +
            $"{weapon}  Lv.{data.weaponUpgradeLevel}\n" +
            $"{LocalizationManager.Text("Attack", "공격력")} " +
            $"+{EquipmentManager.GetWeaponAttack(data)}\n" +
            $"{LocalizationManager.Text("Next cost", "다음 비용")} " +
            $"{(hasWeapon ? EquipmentManager.GetUpgradeCost(data.weaponUpgradeLevel).ToString("N0") : "-")}\n\n" +
            $"{LocalizationManager.Text("ARMOR", "방어구")}\n" +
            $"{armor}  Lv.{data.armorUpgradeLevel}\n" +
            $"{LocalizationManager.Text("Health", "체력")} " +
            $"+{EquipmentManager.GetArmorHealth(data)}\n" +
            $"{LocalizationManager.Text("Next cost", "다음 비용")} " +
            $"{(hasArmor ? EquipmentManager.GetUpgradeCost(data.armorUpgradeLevel).ToString("N0") : "-")}";

        equipmentPowerText.text =
            $"{LocalizationManager.Text("Power", "전투력")} " +
            $"{GameBalance.GetCombatPower(data):N0}   " +
            $"{LocalizationManager.Text("Gold", "골드")} " +
            $"{data.gold:N0}";

        SetBar(
            weaponUpgradeFill,
            Mathf.Clamp(data.weaponUpgradeLevel, 0, 20),
            20);
        SetBar(
            armorUpgradeFill,
            Mathf.Clamp(data.armorUpgradeLevel, 0, 20),
            20);
    }

    private static RectTransform CreateHealthBar(
        RectTransform parent,
        string name,
        Color fillColor,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform background = RuntimeUiFactory.CreatePanel(
            name,
            parent,
            new Color32(12, 18, 30, 255),
            anchorMin,
            anchorMax);

        return RuntimeUiFactory.CreatePanel(
            "Fill",
            background,
            fillColor,
            Vector2.zero,
            Vector2.one);
    }

    private static void SetBar(
        RectTransform fill,
        int current,
        int maximum)
    {
        if (fill == null)
            return;

        float ratio = maximum <= 0
            ? 0f
            : Mathf.Clamp01(current / (float)maximum);
        fill.anchorMax = new Vector2(ratio, 1f);
    }
}
