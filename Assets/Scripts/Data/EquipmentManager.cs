using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class EquipmentStarForceResult
{
    public EquipmentSlot slot;
    public int previousStar;
    public int currentStar;
    public int goldCost;
    public float successPercent;
    public bool chanceTime;
    public bool success;
    public bool downgraded;
}

public sealed class EquipmentCubePreview
{
    public EquipmentSlot slot;
    public string instanceId;
    public string equipmentName;
    public int coinCost;
    public List<EquipmentRolledOption> currentOptions;
    public List<EquipmentRolledOption> newOptions;
}

public sealed class EquipmentDismantleResult
{
    public string equipmentName;
    public int coinReward;
}

public class EquipmentManager : MonoBehaviour
{
    private const string EquipmentDatabaseResourcePath =
        "EquipmentDatabase";

    private static readonly Dictionary<string, string> LegacyEquipmentIdMap =
        new Dictionary<string, string>
        {
            { "Wooden Blade", "equip101" },
            { "Iron Blade", "equip102" },
            { "Moon Blade", "equip103" },
            { "Nova Blade", "equip104" },
            { "Cloth Vest", "equip201" },
            { "Iron Guard", "equip202" },
            { "Moon Guard", "equip203" },
            { "Nova Guard", "equip204" }
        };

    public static EquipmentManager Instance;

    [SerializeField] private EquipmentDatabase equipmentDatabase;

    public event Action<string> OnEquipmentDropped;
    public event Action OnEquipmentChanged;
    public event Action<EquipmentSlot> OnEquipmentUpgraded;

    private static EquipmentDatabase cachedDatabase;
    private EquipmentCubePreview pendingCubePreview;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (equipmentDatabase == null)
            equipmentDatabase = GetDatabase();
        cachedDatabase = equipmentDatabase;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeStarterEquipment()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        data.EnsureInitialized();
        EnsureEquipmentInstances(data);

        if (string.IsNullOrEmpty(data.equippedWeapon))
        {
            EquipmentDefinition weapon =
                GetDatabase()?.GetStarter(EquipmentSlot.Weapon);
            if (weapon != null)
            {
                EquipmentInstance instance = CreateInstance(weapon);
                data.equipmentInstances.Add(instance);
                EquipInstance(data, instance, weapon);
                SetInventoryEquipmentCount(data, weapon.id, 1);
            }
        }

        if (string.IsNullOrEmpty(data.equippedArmor))
        {
            EquipmentDefinition armor =
                GetDatabase()?.GetStarter(EquipmentSlot.Armor);
            if (armor != null)
            {
                EquipmentInstance instance = CreateInstance(armor);
                data.equipmentInstances.Add(instance);
                EquipInstance(data, instance, armor);
                SetInventoryEquipmentCount(data, armor.id, 1);
            }
        }
    }

    public void TryGrantDrop(int stage, bool boss)
    {
        if (!boss &&
            UnityEngine.Random.value >
            GameBalanceConfig.NormalEquipmentDropChance)
            return;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        EnsureEquipmentInstances(data);

        EquipmentSlot slot = UnityEngine.Random.value < 0.5f
            ? EquipmentSlot.Weapon
            : EquipmentSlot.Armor;
        int tier = Mathf.Max(0, (stage - 1) / 10);
        EquipmentDefinition item =
            GetDatabase()?.GetByTier(slot, tier);
        if (item == null)
            return;

        EquipmentInstance instance = CreateInstance(item);
        data.equipmentInstances.Add(instance);
        SetInventoryEquipmentCount(data, item.id, 1);
        string rolledOptions = FormatRolledOptions(instance);
        SafeEvent.Invoke(
            OnEquipmentDropped,
            item.DisplayName +
            (string.IsNullOrWhiteSpace(rolledOptions)
                ? ""
                : " [" + rolledOptions + "]"),
            "Equipment",
            nameof(OnEquipmentDropped));
        SafeEvent.Invoke(
            OnEquipmentChanged,
            "Equipment",
            nameof(OnEquipmentChanged));
    }

    public bool TryEquip(string instanceId)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return false;

        EnsureEquipmentInstances(data);
        EquipmentInstance instance = FindInstance(data, instanceId);
        EquipmentDefinition item = instance == null
            ? null
            : GetDatabase()?.Find(instance.definitionId);
        if (item == null)
            return false;

        EquipInstance(data, instance, item);

        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        SafeEvent.Invoke(
            OnEquipmentChanged,
            "Equipment",
            nameof(OnEquipmentChanged));
        return true;
    }

    public bool TryDismantle(
        string instanceId,
        out EquipmentDismantleResult result)
    {
        result = null;
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return false;

        EnsureEquipmentInstances(data);
        EquipmentInstance instance = FindInstance(data, instanceId);
        EquipmentDefinition item = instance == null
            ? null
            : GetDatabase()?.Find(instance.definitionId);
        if (item == null || IsEquippedInstance(data, instance.instanceId))
            return false;

        if (!data.equipmentInstances.Remove(instance))
            return false;

        SetInventoryEquipmentCount(
            data,
            item.id,
            FindInstanceByDefinition(data, item.id) == null ? 0 : 1);
        int coinReward = GetDismantleCoinReward(item.tier);
        data.flightEquipmentCoins += coinReward;
        result = new EquipmentDismantleResult
        {
            equipmentName = item.DisplayName,
            coinReward = coinReward
        };

        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        SafeEvent.Invoke(
            OnEquipmentChanged,
            "Equipment",
            nameof(OnEquipmentChanged));
        return true;
    }

    public bool TryEquipNextOwned(
        EquipmentSlot slot,
        out EquipmentDefinition equipped)
    {
        equipped = null;
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return false;

        EnsureEquipmentInstances(data);
        List<EquipmentInstance> owned = GetOwnedEquipment(data, slot);
        if (owned.Count <= 1)
            return false;

        string currentId = slot == EquipmentSlot.Weapon
            ? data.equippedWeaponInstanceId
            : data.equippedArmorInstanceId;
        int currentIndex = owned.FindIndex(item => item.instanceId == currentId);
        int nextIndex = currentIndex < 0
            ? 0
            : (currentIndex + 1) % owned.Count;
        EquipmentInstance next = owned[nextIndex];
        if (!TryEquip(next.instanceId))
            return false;

        equipped = GetDatabase()?.Find(next.definitionId);
        return true;
    }

    public static List<EquipmentInstance> GetOwnedEquipment(
        PlayerData data,
        EquipmentSlot slot)
    {
        List<EquipmentInstance> owned = GetOwnedEquipment(data);
        owned.RemoveAll(instance => GetDatabase()?.Find(
            instance.definitionId)?.slot != slot);

        return owned;
    }

    public static List<EquipmentInstance> GetOwnedEquipment(
        PlayerData data)
    {
        List<EquipmentInstance> owned = new List<EquipmentInstance>();
        EnsureEquipmentInstances(data);
        if (data?.equipmentInstances == null)
            return owned;

        EquipmentDatabase database = GetDatabase();
        if (database?.equipment == null)
            return owned;

        foreach (EquipmentInstance instance in data.equipmentInstances)
        {
            if (instance != null && database.Find(instance.definitionId) != null)
                owned.Add(instance);
        }

        owned.Sort((left, right) =>
        {
            EquipmentDefinition leftDefinition =
                database.Find(left.definitionId);
            EquipmentDefinition rightDefinition =
                database.Find(right.definitionId);
            int slotComparison = leftDefinition.slot.CompareTo(
                rightDefinition.slot);
            if (slotComparison != 0)
                return slotComparison;

            int tierComparison = rightDefinition.tier.CompareTo(
                leftDefinition.tier);
            return tierComparison != 0
                ? tierComparison
                : string.CompareOrdinal(left.instanceId, right.instanceId);
        });
        return owned;
    }

    public Task<bool> TryUpgradeAsync(EquipmentSlot slot)
    {
        return Task.FromResult(TryStarForce(slot, out _));
    }

    public bool TryStarForce(
        EquipmentSlot slot,
        out EquipmentStarForceResult result)
    {
        result = null;
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return false;

        EnsureEquipmentInstances(data);
        string equipped = slot == EquipmentSlot.Weapon
            ? data.equippedWeapon
            : data.equippedArmor;
        if (string.IsNullOrEmpty(equipped))
            return false;

        int star = GetStarForce(data, slot);
        if (star >= GameBalanceConfig.EquipmentStarForceMaxLevel)
            return false;

        int cost = GetStarForceCost(star);
        if (data.gold < cost)
            return false;

        int downgradeFails = GetStarForceDowngradeFails(data, slot);
        bool canDowngrade = star >= 10;
        bool chanceTime = canDowngrade && downgradeFails >= 2;
        float successPercent = GetStarForceSuccessPercent(star);
        bool success = chanceTime ||
            UnityEngine.Random.value < successPercent / 100f;

        data.gold -= cost;
        int nextStar = star;
        if (success)
        {
            nextStar++;
            SetStarForceDowngradeFails(data, slot, 0);
        }
        else if (canDowngrade)
        {
            nextStar--;
            SetStarForceDowngradeFails(data, slot, downgradeFails + 1);
        }
        else
        {
            SetStarForceDowngradeFails(data, slot, 0);
        }

        SetStarForce(data, slot, nextStar);
        result = new EquipmentStarForceResult
        {
            slot = slot,
            previousStar = star,
            currentStar = nextStar,
            goldCost = cost,
            successPercent = successPercent,
            chanceTime = chanceTime,
            success = success,
            downgraded = !success && nextStar < star
        };

        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        SafeEvent.Invoke(
            OnEquipmentChanged,
            "Equipment",
            nameof(OnEquipmentChanged));
        if (success)
        {
            SafeEvent.Invoke(
                OnEquipmentUpgraded,
                slot,
                "Equipment",
                nameof(OnEquipmentUpgraded));
        }

        return true;
    }

    public bool TryCreateCubePreview(
        EquipmentSlot slot,
        out EquipmentCubePreview preview)
    {
        preview = null;
        if (pendingCubePreview != null)
            return false;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return false;

        EnsureEquipmentInstances(data);
        EquipmentInstance instance = GetEquippedInstance(data, slot);
        EquipmentDefinition item = instance == null
            ? null
            : GetDatabase()?.Find(instance.definitionId, slot);
        int optionLineCount = GetOptionLineCount(instance?.rolledOptions);
        if (item == null || optionLineCount <= 0)
            return false;

        int cost = GetRerollCoinCost(item.tier);
        if (data.flightEquipmentCoins < cost)
            return false;

        data.flightEquipmentCoins -= cost;
        preview = new EquipmentCubePreview
        {
            slot = slot,
            instanceId = instance.instanceId,
            equipmentName = item.DisplayName,
            coinCost = cost,
            currentOptions = CloneRolledOptions(instance.rolledOptions),
            newOptions = RollOptions(item, optionLineCount)
        };
        pendingCubePreview = preview;
        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        SafeEvent.Invoke(
            OnEquipmentChanged,
            "Equipment",
            nameof(OnEquipmentChanged));
        return true;
    }

    public bool TryResolveCubePreview(bool applyNew)
    {
        if (pendingCubePreview == null)
            return false;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        EquipmentInstance instance = FindInstance(
            data,
            pendingCubePreview.instanceId);
        if (instance == null)
        {
            pendingCubePreview = null;
            return false;
        }

        if (applyNew)
        {
            instance.rolledOptions = CloneRolledOptions(
                pendingCubePreview.newOptions);
        }

        pendingCubePreview = null;
        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        SafeEvent.Invoke(
            OnEquipmentChanged,
            "Equipment",
            nameof(OnEquipmentChanged));
        return true;
    }

    public static int GetWeaponAttack(PlayerData data)
    {
        if (data == null)
            return 0;

        EquipmentDefinition item =
            GetDatabase()?.Find(data.equippedWeapon, EquipmentSlot.Weapon);
        if (item == null)
            return 0;

        int flatAttack =
            GameBalanceConfig.EquipmentWeaponBaseAttack +
            item.tier * GameBalanceConfig.EquipmentWeaponAttackPerTier +
            GetEnhancementLevel(data, EquipmentSlot.Weapon) *
            GameBalanceConfig.EquipmentWeaponAttackPerLevel;
        return flatAttack;
    }

    public static int GetArmorHealth(PlayerData data)
    {
        if (data == null)
            return 0;

        EquipmentDefinition item =
            GetDatabase()?.Find(data.equippedArmor, EquipmentSlot.Armor);
        return item == null
            ? 0
            : GameBalanceConfig.EquipmentArmorBaseHealth +
              item.tier * GameBalanceConfig.EquipmentArmorHealthPerTier +
              GetEnhancementLevel(data, EquipmentSlot.Armor) *
              GameBalanceConfig.EquipmentArmorHealthPerLevel;
    }

    public static float GetArmorHeroDamageReductionPercent(PlayerData data)
    {
        if (data == null)
            return 0f;

        EquipmentDefinition item =
            GetDatabase()?.Find(data.equippedArmor, EquipmentSlot.Armor);
        if (item == null)
            return 0f;

        float percent =
            GameBalanceConfig.EquipmentArmorBaseDamageReductionPercent +
            item.tier *
            GameBalanceConfig.EquipmentArmorDamageReductionPerTier +
            GetEnhancementLevel(data, EquipmentSlot.Armor) *
            GameBalanceConfig.EquipmentArmorDamageReductionPerLevel;
        return Mathf.Clamp(
            percent,
            0f,
            GameBalanceConfig.EquipmentArmorMaxDamageReductionPercent);
    }

    public static float GetHeroDamageReductionPercent(PlayerData data)
    {
        return Mathf.Clamp(
            GetArmorHeroDamageReductionPercent(data) +
            GetOptionPercent(
                data,
                EquipmentOptionType.HeroDamageReductionPercent),
            0f,
            GameBalanceConfig.EquipmentArmorMaxDamageReductionPercent);
    }

    public static float GetAttackPercent(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionPercent(data, EquipmentOptionType.AttackPercent));
    }

    public static float GetHeroHealthPercent(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionPercent(
                data,
                EquipmentOptionType.HeroHealthPercent));
    }

    public static float GetHeroHealingPercent(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionPercent(data, EquipmentOptionType.HeroHealingPercent));
    }

    public static float GetSkillDamagePercent(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionPercent(data, EquipmentOptionType.SkillDamagePercent));
    }

    public static float GetBossDamagePercent(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionPercent(data, EquipmentOptionType.BossDamagePercent));
    }

    public static float GetPowerChargePerTapBonus(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionValue(
                data,
                EquipmentOptionType.PowerChargePerTapFlat));
    }

    public static float GetHeroRecoverySpeedPercent(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionPercent(
                data,
                EquipmentOptionType.HeroRecoverySpeedPercent));
    }

    public static string GetEquipmentOptionSummary(string equipmentId)
    {
        return "";
    }

    public static string GetEquipmentOptionSummary(
        PlayerData data,
        EquipmentSlot slot)
    {
        if (data == null)
            return "";

        EquipmentInstance instance = GetEquippedInstance(data, slot);
        return FormatRolledOptions(instance);
    }

    public static string GetEquipmentDisplayName(string equipmentId)
    {
        EquipmentDefinition item = GetDatabase()?.Find(equipmentId);
        return item == null ? equipmentId : item.DisplayName;
    }

    public static Sprite GetEquipmentIcon(string equipmentId)
    {
        return GetDatabase()?.Find(equipmentId)?.icon;
    }

    private static float GetOptionPercent(
        PlayerData data,
        EquipmentOptionType type)
    {
        return GetOptionValue(data, type);
    }

    private static float GetOptionValue(
        PlayerData data,
        EquipmentOptionType type)
    {
        if (data == null)
            return 0f;

        return GetRolledOptionValue(
            GetEquippedInstance(data, EquipmentSlot.Weapon),
            type) + GetRolledOptionValue(
            GetEquippedInstance(data, EquipmentSlot.Armor),
            type);
    }

    private static string GetOptionLabel(EquipmentOptionType type)
    {
        switch (type)
        {
            case EquipmentOptionType.AttackPercent:
                return LocalizationManager.Text("ATK", "공격");
            case EquipmentOptionType.SkillDamagePercent:
                return LocalizationManager.Text("Skill Damage", "스킬피해");
            case EquipmentOptionType.BossDamagePercent:
                return LocalizationManager.Text("Boss Damage", "보스피해");
            case EquipmentOptionType.HeroHealthPercent:
                return LocalizationManager.Text("Hero HP", "참새체력");
            case EquipmentOptionType.HeroDamageReductionPercent:
                return LocalizationManager.Text("Damage Reduction", "피해감소");
            case EquipmentOptionType.HeroHealingPercent:
                return LocalizationManager.Text("Healing", "회복량");
            case EquipmentOptionType.PowerChargePerTapFlat:
                return LocalizationManager.Text("Charge", "충전");
            case EquipmentOptionType.HeroRecoverySpeedPercent:
                return LocalizationManager.Text("Reentry", "재정비");
            default:
                return type.ToString();
        }
    }

    private static string FormatOptionValue(
        EquipmentOptionType type,
        float value)
    {
        string text = value.ToString("0.#");
        return type == EquipmentOptionType.PowerChargePerTapFlat
            ? text
            : text + "%";
    }

    public static int GetUpgradeCost(int currentLevel)
    {
        return GetStarForceCost(currentLevel);
    }

    public static int GetEnhancementLevel(
        PlayerData data,
        EquipmentSlot slot)
    {
        EquipmentInstance instance = GetEquippedInstance(data, slot);
        return Mathf.Clamp(
            instance?.enhancementLevel ?? 0,
            0,
            GameBalanceConfig.EquipmentStarForceMaxLevel);
    }

    public static int GetStarForceCost(int currentStar)
    {
        return GameBalanceConfig.EquipmentStarForceBaseCost +
            currentStar *
            currentStar *
            GameBalanceConfig.EquipmentStarForceQuadraticCost;
    }

    public static float GetStarForceSuccessPercent(int currentStar)
    {
        if (currentStar == 5 || currentStar == 10 || currentStar == 15)
            return 100f;

        if (currentStar <= 2)
            return 95f;
        if (currentStar <= 5)
            return 90f;
        if (currentStar <= 8)
            return 80f;
        if (currentStar <= 10)
            return 70f;
        if (currentStar <= 12)
            return 60f;
        if (currentStar <= 14)
            return 45f;
        if (currentStar <= 16)
            return 30f;
        if (currentStar == 17)
            return 25f;
        if (currentStar == 18)
            return 20f;
        return 15f;
    }

    public static int GetDismantleCoinReward(int tier)
    {
        switch (Mathf.Max(0, tier))
        {
            case 0:
                return GameBalanceConfig.EquipmentCoinTier0;
            case 1:
                return GameBalanceConfig.EquipmentCoinTier1;
            case 2:
                return GameBalanceConfig.EquipmentCoinTier2;
            default:
                return GameBalanceConfig.EquipmentCoinTier3;
        }
    }

    public static EquipmentDefinition GetEquipmentDefinition(
        string equipmentId)
    {
        return GetDatabase()?.Find(equipmentId);
    }

    public static int GetRerollCoinCost(int tier)
    {
        switch (Mathf.Max(0, tier))
        {
            case 0:
                return GameBalanceConfig.EquipmentRerollCoinTier0;
            case 1:
                return GameBalanceConfig.EquipmentRerollCoinTier1;
            case 2:
                return GameBalanceConfig.EquipmentRerollCoinTier2;
            default:
                return GameBalanceConfig.EquipmentRerollCoinTier3;
        }
    }

    public static int GetOptionResetCoinCost(
        PlayerData data,
        EquipmentSlot slot)
    {
        if (data == null)
            return -1;

        string equipmentId = slot == EquipmentSlot.Weapon
            ? data.equippedWeapon
            : data.equippedArmor;
        EquipmentDefinition item = GetDatabase()?.Find(
            equipmentId,
            slot);
        EquipmentInstance instance = GetEquippedInstance(data, slot);
        return item == null || GetOptionLineCount(instance?.rolledOptions) <= 0
            ? -1
            : GetRerollCoinCost(item.tier);
    }

    private static int GetStarForce(PlayerData data, EquipmentSlot slot)
    {
        return GetEnhancementLevel(data, slot);
    }

    private static void SetStarForce(
        PlayerData data,
        EquipmentSlot slot,
        int value)
    {
        EquipmentInstance instance = GetEquippedInstance(data, slot);
        if (instance != null)
        {
            instance.enhancementLevel = Mathf.Clamp(
                value,
                0,
                GameBalanceConfig.EquipmentStarForceMaxLevel);
        }
    }

    private static int GetStarForceDowngradeFails(
        PlayerData data,
        EquipmentSlot slot)
    {
        return Mathf.Max(
            0,
            GetEquippedInstance(data, slot)?.starForceDowngradeFails ?? 0);
    }

    private static void SetStarForceDowngradeFails(
        PlayerData data,
        EquipmentSlot slot,
        int value)
    {
        EquipmentInstance instance = GetEquippedInstance(data, slot);
        if (instance != null)
            instance.starForceDowngradeFails = Mathf.Max(0, value);
    }

    private static void EnsureEquipmentInstances(PlayerData data)
    {
        if (data == null)
            return;

        data.EnsureInitialized();
        MigrateLegacyEquipmentIds(data);
        EquipmentDatabase database = GetDatabase();
        if (database?.equipment == null)
            return;

        foreach (EquipmentDefinition item in database.equipment)
        {
            if (item == null ||
                !data.inventory.items.TryGetValue(item.id, out int count) ||
                count <= 0)
            {
                continue;
            }

            int instanceCount = CountInstancesByDefinition(data, item.id);
            for (int index = instanceCount; index < count; index++)
                data.equipmentInstances.Add(CreateInstance(item, false));

            // Equipment is now tracked only as individual instances.
            SetInventoryEquipmentCount(data, item.id, 0);
        }

        EnsureEquippedInstance(
            data,
            EquipmentSlot.Weapon,
            data.equippedWeapon,
            data.equippedWeaponInstanceId);
        EnsureEquippedInstance(
            data,
            EquipmentSlot.Armor,
            data.equippedArmor,
            data.equippedArmorInstanceId);
        MigrateLegacySlotEnhancements(data);
    }

    private static void MigrateLegacyEquipmentIds(PlayerData data)
    {
        if (data == null)
            return;

        data.equippedWeapon = MapLegacyEquipmentId(data.equippedWeapon);
        data.equippedArmor = MapLegacyEquipmentId(data.equippedArmor);

        if (data.equipmentInstances != null)
        {
            foreach (EquipmentInstance instance in data.equipmentInstances)
            {
                if (instance != null)
                {
                    instance.definitionId = MapLegacyEquipmentId(
                        instance.definitionId);
                }
            }
        }

        if (data.inventory?.items == null)
            return;

        foreach (KeyValuePair<string, string> entry in LegacyEquipmentIdMap)
        {
            if (!data.inventory.items.TryGetValue(
                    entry.Key,
                    out int legacyCount))
            {
                continue;
            }

            if (legacyCount > 0)
            {
                data.inventory.items.TryGetValue(
                    entry.Value,
                    out int currentCount);
                data.inventory.items[entry.Value] =
                    Mathf.Max(0, currentCount) + legacyCount;
            }

            data.inventory.items.Remove(entry.Key);
        }
    }

    private static string MapLegacyEquipmentId(string equipmentId)
    {
        return !string.IsNullOrWhiteSpace(equipmentId) &&
            LegacyEquipmentIdMap.TryGetValue(
                equipmentId,
                out string mappedId)
            ? mappedId
            : equipmentId;
    }

    private static void MigrateLegacySlotEnhancements(PlayerData data)
    {
        if (data == null || data.equipmentInstanceEnhancementsMigrated)
            return;

        MigrateLegacySlotEnhancement(
            GetEquippedInstance(data, EquipmentSlot.Weapon),
            data.weaponUpgradeLevel,
            data.weaponStarForceDowngradeFails);
        MigrateLegacySlotEnhancement(
            GetEquippedInstance(data, EquipmentSlot.Armor),
            data.armorUpgradeLevel,
            data.armorStarForceDowngradeFails);
        data.weaponUpgradeLevel = 0;
        data.armorUpgradeLevel = 0;
        data.weaponStarForceDowngradeFails = 0;
        data.armorStarForceDowngradeFails = 0;
        data.equipmentInstanceEnhancementsMigrated = true;
    }

    private static void MigrateLegacySlotEnhancement(
        EquipmentInstance instance,
        int enhancementLevel,
        int downgradeFails)
    {
        if (instance == null)
            return;

        instance.enhancementLevel = Mathf.Max(
            instance.enhancementLevel,
            Mathf.Clamp(
                enhancementLevel,
                0,
                GameBalanceConfig.EquipmentStarForceMaxLevel));
        instance.starForceDowngradeFails = Mathf.Max(
            instance.starForceDowngradeFails,
            Mathf.Max(0, downgradeFails));
    }

    private static void EnsureEquippedInstance(
        PlayerData data,
        EquipmentSlot slot,
        string definitionId,
        string instanceId)
    {
        EquipmentInstance instance = FindInstance(data, instanceId);
        if (instance == null && !string.IsNullOrWhiteSpace(definitionId))
            instance = FindInstanceByDefinition(data, definitionId);
        if (instance == null && !string.IsNullOrWhiteSpace(definitionId))
        {
            EquipmentDefinition legacyItem = GetDatabase()?.Find(
                definitionId,
                slot);
            if (legacyItem != null)
            {
                instance = CreateInstance(legacyItem, false);
                data.equipmentInstances.Add(instance);
                SetInventoryEquipmentCount(data, legacyItem.id, 1);
            }
        }
        if (instance == null)
            return;

        EquipmentDefinition item = GetDatabase()?.Find(
            instance.definitionId,
            slot);
        if (item != null)
            EquipInstance(data, instance, item);
    }

    private static EquipmentInstance CreateInstance(
        EquipmentDefinition item,
        bool rollOptions = true)
    {
        return new EquipmentInstance
        {
            instanceId = Guid.NewGuid().ToString("N"),
            definitionId = item.id,
            rolledOptions = rollOptions
                ? RollOptions(item)
                : new List<EquipmentRolledOption>()
        };
    }

    private static List<EquipmentRolledOption> RollOptions(
        EquipmentDefinition item)
    {
        int lineCount = UnityEngine.Random.Range(
            GameBalanceConfig.EquipmentRandomOptionMinLines,
            GameBalanceConfig.EquipmentRandomOptionMaxLines + 1);
        return RollOptions(item, lineCount);
    }

    private static List<EquipmentRolledOption> RollOptions(
        EquipmentDefinition item,
        int lineCount)
    {
        List<EquipmentRolledOption> options =
            new List<EquipmentRolledOption>();
        int count = Mathf.Clamp(lineCount, 0, 3);
        List<EquipmentOptionType> pool = item.slot == EquipmentSlot.Weapon
            ? new List<EquipmentOptionType>
            {
                EquipmentOptionType.AttackPercent,
                EquipmentOptionType.SkillDamagePercent,
                EquipmentOptionType.BossDamagePercent
            }
            : new List<EquipmentOptionType>
            {
                EquipmentOptionType.HeroHealthPercent,
                EquipmentOptionType.HeroDamageReductionPercent,
                EquipmentOptionType.HeroHealingPercent,
                EquipmentOptionType.HeroRecoverySpeedPercent
            };

        for (int index = 0; index < count && pool.Count > 0; index++)
        {
            int poolIndex = UnityEngine.Random.Range(0, pool.Count);
            EquipmentOptionType type = pool[poolIndex];
            pool.RemoveAt(poolIndex);
            options.Add(new EquipmentRolledOption
            {
                type = type,
                value = UnityEngine.Random.Range(
                    GameBalanceConfig.EquipmentOptionMinPercent,
                    GameBalanceConfig.EquipmentOptionMaxPercent + 1)
            });
        }

        return options;
    }

    private static int GetOptionLineCount(
        List<EquipmentRolledOption> options)
    {
        if (options == null)
            return 0;

        int count = 0;
        foreach (EquipmentRolledOption option in options)
        {
            if (option != null && option.value > 0f)
                count++;
        }

        return count;
    }

    private static EquipmentInstance GetEquippedInstance(
        PlayerData data,
        EquipmentSlot slot)
    {
        return FindInstance(
            data,
            slot == EquipmentSlot.Weapon
                ? data?.equippedWeaponInstanceId
                : data?.equippedArmorInstanceId);
    }

    private static bool IsEquippedInstance(
        PlayerData data,
        string instanceId)
    {
        return !string.IsNullOrWhiteSpace(instanceId) &&
            (data?.equippedWeaponInstanceId == instanceId ||
             data?.equippedArmorInstanceId == instanceId);
    }

    private static EquipmentInstance FindInstance(
        PlayerData data,
        string instanceId)
    {
        if (data?.equipmentInstances == null ||
            string.IsNullOrWhiteSpace(instanceId))
        {
            return null;
        }

        return data.equipmentInstances.Find(instance =>
            instance != null && instance.instanceId == instanceId);
    }

    private static EquipmentInstance FindInstanceByDefinition(
        PlayerData data,
        string definitionId)
    {
        if (data?.equipmentInstances == null ||
            string.IsNullOrWhiteSpace(definitionId))
        {
            return null;
        }

        return data.equipmentInstances.Find(instance =>
            instance != null && instance.definitionId == definitionId);
    }

    private static int CountInstancesByDefinition(
        PlayerData data,
        string definitionId)
    {
        if (data?.equipmentInstances == null ||
            string.IsNullOrWhiteSpace(definitionId))
        {
            return 0;
        }

        int count = 0;
        foreach (EquipmentInstance instance in data.equipmentInstances)
        {
            if (instance != null && instance.definitionId == definitionId)
                count++;
        }

        return count;
    }

    private static void EquipInstance(
        PlayerData data,
        EquipmentInstance instance,
        EquipmentDefinition item)
    {
        if (item.slot == EquipmentSlot.Weapon)
        {
            data.equippedWeaponInstanceId = instance.instanceId;
            data.equippedWeapon = item.id;
        }
        else
        {
            data.equippedArmorInstanceId = instance.instanceId;
            data.equippedArmor = item.id;
        }
    }

    private static void SetInventoryEquipmentCount(
        PlayerData data,
        string equipmentId,
        int count)
    {
        if (data?.inventory?.items == null ||
            string.IsNullOrWhiteSpace(equipmentId))
        {
            return;
        }

        if (count <= 0)
            data.inventory.items.Remove(equipmentId);
        else
            data.inventory.items[equipmentId] = count;
    }

    private static float GetRolledOptionValue(
        EquipmentInstance instance,
        EquipmentOptionType type)
    {
        if (instance?.rolledOptions == null)
            return 0f;

        float total = 0f;
        foreach (EquipmentRolledOption option in instance.rolledOptions)
        {
            if (option != null && option.type == type)
                total += option.value;
        }

        return total;
    }

    private static string FormatRolledOptions(EquipmentInstance instance)
    {
        return FormatRolledOptions(instance?.rolledOptions);
    }

    public static string FormatRolledOptions(
        List<EquipmentRolledOption> options)
    {
        if (options == null || options.Count == 0)
        {
            return "";
        }

        List<string> entries = new List<string>();
        foreach (EquipmentRolledOption option in options)
        {
            if (option == null || option.value <= 0f)
                continue;

            entries.Add(
                GetOptionLabel(option.type) + " +" +
                FormatOptionValue(option.type, option.value));
        }

        return string.Join(", ", entries);
    }

    private static List<EquipmentRolledOption> CloneRolledOptions(
        List<EquipmentRolledOption> options)
    {
        List<EquipmentRolledOption> clone =
            new List<EquipmentRolledOption>();
        if (options == null)
            return clone;

        foreach (EquipmentRolledOption option in options)
        {
            if (option == null)
                continue;

            clone.Add(new EquipmentRolledOption
            {
                type = option.type,
                value = option.value
            });
        }

        return clone;
    }

    private static EquipmentDatabase GetDatabase()
    {
        if (cachedDatabase != null)
            return cachedDatabase;

        if (Instance != null && Instance.equipmentDatabase != null)
        {
            cachedDatabase = Instance.equipmentDatabase;
            return cachedDatabase;
        }

        cachedDatabase =
            Resources.Load<EquipmentDatabase>(EquipmentDatabaseResourcePath);
        return cachedDatabase;
    }

    private static void AddInventoryItem(PlayerData data, string itemName)
    {
        if (data?.inventory?.items == null)
            return;

        if (!data.inventory.items.ContainsKey(itemName))
            data.inventory.items[itemName] = 1;
    }

    private static bool IsOwned(PlayerData data, string equipmentId)
    {
        return data?.inventory?.items != null &&
            !string.IsNullOrWhiteSpace(equipmentId) &&
            data.inventory.items.TryGetValue(equipmentId, out int count) &&
            count > 0;
    }

}
