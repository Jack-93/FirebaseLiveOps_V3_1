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

public class EquipmentManager : MonoBehaviour
{
    private const string EquipmentDatabaseResourcePath =
        "EquipmentDatabase";

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

        if (FindInstanceByDefinition(data, item.id) != null)
        {
            int coins = GetDismantleCoinReward(item.tier);
            data.flightEquipmentCoins += coins;
            SafeEvent.Invoke(
                OnEquipmentDropped,
                item.DisplayName + " dismantled: " + coins +
                " Flight Equipment Coins",
                "Equipment",
                nameof(OnEquipmentDropped));
            SafeEvent.Invoke(
                OnEquipmentChanged,
                "Equipment",
                nameof(OnEquipmentChanged));
            return;
        }

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
        List<EquipmentInstance> owned =
            new List<EquipmentInstance>();
        if (data?.equipmentInstances == null)
            return owned;

        EquipmentDatabase database = GetDatabase();
        if (database?.equipment == null)
            return owned;

        foreach (EquipmentInstance instance in data.equipmentInstances)
        {
            EquipmentDefinition item = instance == null
                ? null
                : database.Find(instance.definitionId);
            if (item == null || item.slot != slot)
            {
                continue;
            }

            owned.Add(instance);
        }

        owned.Sort((left, right) =>
        {
            EquipmentDefinition leftDefinition =
                database.Find(left.definitionId);
            EquipmentDefinition rightDefinition =
                database.Find(right.definitionId);
            int tierComparison = leftDefinition.tier.CompareTo(
                rightDefinition.tier);
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
        if (item == null || GetRolledOptionCount(item.tier) <= 0)
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
            newOptions = RollOptions(item)
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
            data.weaponUpgradeLevel *
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
              data.armorUpgradeLevel *
              GameBalanceConfig.EquipmentArmorHealthPerLevel;
    }

    public static float GetArmorPoleDamageReductionPercent(PlayerData data)
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
            data.armorUpgradeLevel *
            GameBalanceConfig.EquipmentArmorDamageReductionPerLevel;
        return Mathf.Clamp(
            percent,
            0f,
            GameBalanceConfig.EquipmentArmorMaxDamageReductionPercent);
    }

    public static float GetPoleDamageReductionPercent(PlayerData data)
    {
        return Mathf.Clamp(
            GetArmorPoleDamageReductionPercent(data) +
            GetOptionPercent(
                data,
                EquipmentOptionType.PoleDamageReductionPercent),
            0f,
            GameBalanceConfig.EquipmentArmorMaxDamageReductionPercent);
    }

    public static float GetAttackPercent(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionPercent(data, EquipmentOptionType.AttackPercent));
    }

    public static float GetPoleDurabilityPercent(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionPercent(
                data,
                EquipmentOptionType.PoleDurabilityPercent));
    }

    public static float GetPoleRepairPercent(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionPercent(data, EquipmentOptionType.PoleRepairPercent));
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

    public static float GetPoleRecoverySpeedPercent(PlayerData data)
    {
        return Mathf.Max(
            0f,
            GetOptionPercent(
                data,
                EquipmentOptionType.PoleRecoverySpeedPercent));
    }

    public static string GetEquipmentOptionSummary(string equipmentId)
    {
        EquipmentDefinition item = GetDatabase()?.Find(equipmentId);
        if (item?.options == null || item.options.Count == 0)
            return "";

        return GetEquipmentOptionSummary(item, 0);
    }

    public static string GetEquipmentOptionSummary(
        PlayerData data,
        EquipmentSlot slot)
    {
        if (data == null)
            return "";

        string equipmentId = slot == EquipmentSlot.Weapon
            ? data.equippedWeapon
            : data.equippedArmor;
        int upgradeLevel = slot == EquipmentSlot.Weapon
            ? data.weaponUpgradeLevel
            : data.armorUpgradeLevel;
        EquipmentDefinition item =
            GetDatabase()?.Find(equipmentId, slot);
        string baseOptions = GetEquipmentOptionSummary(item, upgradeLevel);
        EquipmentInstance instance = GetEquippedInstance(data, slot);
        string rolledOptions = FormatRolledOptions(instance);
        if (string.IsNullOrWhiteSpace(rolledOptions))
            return baseOptions;

        string rolledText = rolledOptions.TrimStart(' ', '+');
        return string.IsNullOrWhiteSpace(baseOptions)
            ? rolledText
            : baseOptions + ", " + rolledText;
    }

    private static string GetEquipmentOptionSummary(
        EquipmentDefinition item,
        int upgradeLevel)
    {
        if (item?.options == null || item.options.Count == 0)
            return "";

        string summary = "";
        for (int index = 0; index < item.options.Count; index++)
        {
            EquipmentOption option = item.options[index];
            float value = GetOptionValue(
                option,
                Mathf.Max(0, upgradeLevel));
            if (option == null || Mathf.Approximately(value, 0f))
                continue;

            if (summary.Length > 0)
                summary += ", ";
            summary += GetOptionLabel(option.type) + " +" +
                FormatOptionValue(option.type, value);
        }

        return summary;
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

        float total = 0f;
        total += GetOptionValue(
            GetDatabase()?.Find(data.equippedWeapon, EquipmentSlot.Weapon),
            type,
            data.weaponUpgradeLevel);
        total += GetOptionValue(
            GetDatabase()?.Find(data.equippedArmor, EquipmentSlot.Armor),
            type,
            data.armorUpgradeLevel);
        total += GetRolledOptionValue(
            GetEquippedInstance(data, EquipmentSlot.Weapon),
            type);
        total += GetRolledOptionValue(
            GetEquippedInstance(data, EquipmentSlot.Armor),
            type);
        return total;
    }

    private static float GetOptionValue(
        EquipmentDefinition item,
        EquipmentOptionType type,
        int upgradeLevel)
    {
        if (item?.options == null)
            return 0f;

        float total = 0f;
        foreach (EquipmentOption option in item.options)
        {
            if (option == null || option.type != type)
                continue;

            total += GetOptionValue(option, upgradeLevel);
        }

        return total;
    }

    private static float GetOptionValue(
        EquipmentOption option,
        int upgradeLevel)
    {
        if (option == null)
            return 0f;

        return option.value +
            option.valuePerUpgradeLevel *
            Mathf.Max(0, upgradeLevel);
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
            case EquipmentOptionType.PoleDurabilityPercent:
                return LocalizationManager.Text("Pole DUR", "전봇대내구");
            case EquipmentOptionType.PoleDamageReductionPercent:
                return LocalizationManager.Text("Damage Reduction", "피해감소");
            case EquipmentOptionType.PoleRepairPercent:
                return LocalizationManager.Text("Repair", "수리");
            case EquipmentOptionType.PowerChargePerTapFlat:
                return LocalizationManager.Text("Charge", "충전");
            case EquipmentOptionType.PoleRecoverySpeedPercent:
                return LocalizationManager.Text("Recovery", "복구");
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

    public static int GetRerollCoinCost(int tier)
    {
        switch (Mathf.Max(0, tier))
        {
            case 0:
                return 0;
            case 1:
                return GameBalanceConfig.EquipmentRerollCoinTier1;
            case 2:
                return GameBalanceConfig.EquipmentRerollCoinTier2;
            default:
                return GameBalanceConfig.EquipmentRerollCoinTier3;
        }
    }

    private static int GetStarForce(PlayerData data, EquipmentSlot slot)
    {
        return slot == EquipmentSlot.Weapon
            ? data.weaponUpgradeLevel
            : data.armorUpgradeLevel;
    }

    private static void SetStarForce(
        PlayerData data,
        EquipmentSlot slot,
        int value)
    {
        if (slot == EquipmentSlot.Weapon)
            data.weaponUpgradeLevel = value;
        else
            data.armorUpgradeLevel = value;
    }

    private static int GetStarForceDowngradeFails(
        PlayerData data,
        EquipmentSlot slot)
    {
        return slot == EquipmentSlot.Weapon
            ? data.weaponStarForceDowngradeFails
            : data.armorStarForceDowngradeFails;
    }

    private static void SetStarForceDowngradeFails(
        PlayerData data,
        EquipmentSlot slot,
        int value)
    {
        if (slot == EquipmentSlot.Weapon)
            data.weaponStarForceDowngradeFails = value;
        else
            data.armorStarForceDowngradeFails = value;
    }

    private static void EnsureEquipmentInstances(PlayerData data)
    {
        if (data == null)
            return;

        data.EnsureInitialized();
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

            if (FindInstanceByDefinition(data, item.id) == null)
            {
                data.equipmentInstances.Add(CreateInstance(item, false));
            }

            if (count > 1)
            {
                data.flightEquipmentCoins +=
                    (count - 1) * GetDismantleCoinReward(item.tier);
            }

            SetInventoryEquipmentCount(data, item.id, 1);
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
        List<EquipmentRolledOption> options =
            new List<EquipmentRolledOption>();
        int count = GetRolledOptionCount(item.tier);
        List<EquipmentOptionType> pool = item.slot == EquipmentSlot.Weapon
            ? new List<EquipmentOptionType>
            {
                EquipmentOptionType.AttackPercent,
                EquipmentOptionType.SkillDamagePercent,
                EquipmentOptionType.BossDamagePercent
            }
            : new List<EquipmentOptionType>
            {
                EquipmentOptionType.PoleDurabilityPercent,
                EquipmentOptionType.PoleDamageReductionPercent,
                EquipmentOptionType.PoleRepairPercent,
                EquipmentOptionType.PoleRecoverySpeedPercent
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

    private static int GetRolledOptionCount(int tier)
    {
        if (tier <= 0)
            return 0;

        return tier >= 3 ? 2 : 1;
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
