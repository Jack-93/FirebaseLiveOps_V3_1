public static class EquipmentPanelFormatter
{
    public static string NoEquipment =>
        LocalizationManager.Translate("No equipment.");

    public static string FormatLoadout(PlayerData data)
    {
        string weapon = string.IsNullOrEmpty(data.equippedWeapon)
            ? LocalizationManager.Translate("None")
            : data.equippedWeapon;
        string armor = string.IsNullOrEmpty(data.equippedArmor)
            ? LocalizationManager.Translate("None")
            : data.equippedArmor;
        bool hasWeapon = !string.IsNullOrEmpty(data.equippedWeapon);
        bool hasArmor = !string.IsNullOrEmpty(data.equippedArmor);

        return
            $"{LocalizationManager.Translate("WEAPON")}\n" +
            $"{weapon}  Lv.{data.weaponUpgradeLevel}\n" +
            $"{LocalizationManager.Translate("Attack")} " +
            $"+{EquipmentManager.GetWeaponAttack(data)}\n" +
            $"{LocalizationManager.Translate("Next cost")} " +
            $"{FormatUpgradeCost(hasWeapon, data.weaponUpgradeLevel)}\n\n" +
            $"{LocalizationManager.Translate("ARMOR")}\n" +
            $"{armor}  Lv.{data.armorUpgradeLevel}\n" +
            $"{LocalizationManager.Translate("Health")} " +
            $"+{EquipmentManager.GetArmorHealth(data)}\n" +
            $"{LocalizationManager.Translate("Next cost")} " +
            $"{FormatUpgradeCost(hasArmor, data.armorUpgradeLevel)}";
    }

    public static string FormatPowerSummary(PlayerData data)
    {
        return
            $"{LocalizationManager.Translate("Power")} " +
            $"{GameBalance.GetCombatPower(data):N0}   " +
            $"{LocalizationManager.Translate("Gold")} " +
            $"{data.gold:N0}";
    }

    private static string FormatUpgradeCost(bool hasEquipment, int level)
    {
        return hasEquipment
            ? EquipmentManager.GetUpgradeCost(level).ToString("N0")
            : "-";
    }
}
