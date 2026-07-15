using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum EquipmentOptionType
{
    AttackPercent,
    SkillDamagePercent,
    BossDamagePercent,
    PoleDurabilityPercent,
    PoleDamageReductionPercent,
    PoleRepairPercent,
    PowerChargePerTapFlat,
    PoleRecoverySpeedPercent
}

[Serializable]
public sealed class EquipmentOption
{
    public EquipmentOptionType type;
    public float value;
    public float valuePerUpgradeLevel;
}

[Serializable]
public sealed class EquipmentRolledOption
{
    public EquipmentOptionType type;
    public float value;
}

[Serializable]
public sealed class EquipmentInstance
{
    public string instanceId;
    public string definitionId;
    public List<EquipmentRolledOption> rolledOptions =
        new List<EquipmentRolledOption>();
}

[Serializable]
public sealed class EquipmentDefinition
{
    public string id;
    public string displayName;
    public EquipmentSlot slot;
    [Min(0)] public int tier;
    public Sprite icon;
    public List<EquipmentOption> options =
        new List<EquipmentOption>();

    public string DisplayName =>
        string.IsNullOrWhiteSpace(displayName) ? id : displayName;

    public bool Matches(string value)
    {
        return string.Equals(id, value, StringComparison.Ordinal);
    }
}

[CreateAssetMenu(
    fileName = "EquipmentDatabase",
    menuName = "Game/Equipment Database")]
public sealed class EquipmentDatabase : ScriptableObject
{
    public List<EquipmentDefinition> equipment =
        new List<EquipmentDefinition>();

    public EquipmentDefinition Find(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || equipment == null)
            return null;

        for (int i = 0; i < equipment.Count; i++)
        {
            EquipmentDefinition item = equipment[i];
            if (item != null && item.Matches(value))
                return item;
        }

        return null;
    }

    public EquipmentDefinition Find(string value, EquipmentSlot slot)
    {
        EquipmentDefinition item = Find(value);
        return item != null && item.slot == slot ? item : null;
    }

    public EquipmentDefinition GetStarter(EquipmentSlot slot)
    {
        if (equipment == null)
            return null;

        EquipmentDefinition best = null;
        for (int i = 0; i < equipment.Count; i++)
        {
            EquipmentDefinition item = equipment[i];
            if (item == null || item.slot != slot)
                continue;

            if (best == null || item.tier < best.tier)
                best = item;
        }

        return best;
    }

    public EquipmentDefinition GetByTier(EquipmentSlot slot, int tier)
    {
        if (equipment == null)
            return null;

        EquipmentDefinition best = null;
        for (int i = 0; i < equipment.Count; i++)
        {
            EquipmentDefinition item = equipment[i];
            if (item == null || item.slot != slot)
                continue;

            if (item.tier == tier)
                return item;

            if (item.tier <= tier &&
                (best == null || item.tier > best.tier))
                best = item;
        }

        return best ?? GetStarter(slot);
    }
}
