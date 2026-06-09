using System;
using System.Threading.Tasks;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    public event Action<string> OnEquipmentDropped;
    public event Action OnEquipmentChanged;
    public event Action<EquipmentSlot> OnEquipmentUpgraded;

    private static readonly string[] Weapons =
    {
        "Wooden Blade",
        "Iron Blade",
        "Moon Blade",
        "Nova Blade"
    };

    private static readonly string[] Armors =
    {
        "Cloth Vest",
        "Iron Guard",
        "Moon Guard",
        "Nova Guard"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeStarterEquipment()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        if (string.IsNullOrEmpty(data.equippedWeapon))
        {
            data.equippedWeapon = Weapons[0];
            AddInventoryItem(data, Weapons[0]);
        }

        if (string.IsNullOrEmpty(data.equippedArmor))
        {
            data.equippedArmor = Armors[0];
            AddInventoryItem(data, Armors[0]);
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

        int tier = Mathf.Clamp((stage - 1) / 10, 0, Weapons.Length - 1);
        bool weaponDrop = UnityEngine.Random.value < 0.5f;
        string itemName = weaponDrop ? Weapons[tier] : Armors[tier];

        InventoryManager.Instance?.AddItem(itemName, 1, false);
        AutoEquip(data, itemName, weaponDrop, tier);
        OnEquipmentDropped?.Invoke(itemName);
        OnEquipmentChanged?.Invoke();
    }

    public async Task<bool> TryUpgradeAsync(EquipmentSlot slot)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return false;

        string equipped = slot == EquipmentSlot.Weapon
            ? data.equippedWeapon
            : data.equippedArmor;
        if (string.IsNullOrEmpty(equipped))
            return false;

        int level = slot == EquipmentSlot.Weapon
            ? data.weaponUpgradeLevel
            : data.armorUpgradeLevel;
        int cost = GetUpgradeCost(level);
        if (data.gold < cost)
            return false;

        data.gold -= cost;
        if (slot == EquipmentSlot.Weapon)
            data.weaponUpgradeLevel++;
        else
            data.armorUpgradeLevel++;

        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        OnEquipmentChanged?.Invoke();
        OnEquipmentUpgraded?.Invoke(slot);

        if (FirestoreManager.Instance != null)
            await FirestoreManager.Instance.SavePlayerDataAsync(data);

        return true;
    }

    public static int GetWeaponAttack(PlayerData data)
    {
        int tier = GetTier(data.equippedWeapon, Weapons);
        return tier < 0
            ? 0
            : GameBalanceConfig.EquipmentWeaponBaseAttack +
              tier * GameBalanceConfig.EquipmentWeaponAttackPerTier +
              data.weaponUpgradeLevel *
              GameBalanceConfig.EquipmentWeaponAttackPerLevel;
    }

    public static int GetArmorHealth(PlayerData data)
    {
        int tier = GetTier(data.equippedArmor, Armors);
        return tier < 0
            ? 0
            : GameBalanceConfig.EquipmentArmorBaseHealth +
              tier * GameBalanceConfig.EquipmentArmorHealthPerTier +
              data.armorUpgradeLevel *
              GameBalanceConfig.EquipmentArmorHealthPerLevel;
    }

    public static int GetUpgradeCost(int currentLevel)
    {
        return GameBalanceConfig.EquipmentUpgradeBaseCost +
            currentLevel *
            currentLevel *
            GameBalanceConfig.EquipmentUpgradeQuadraticCost;
    }

    private static void AutoEquip(
        PlayerData data,
        string itemName,
        bool weapon,
        int newTier)
    {
        if (weapon)
        {
            int currentTier = GetTier(data.equippedWeapon, Weapons);
            if (newTier > currentTier)
            {
                data.equippedWeapon = itemName;
                data.weaponUpgradeLevel = 0;
            }
        }
        else
        {
            int currentTier = GetTier(data.equippedArmor, Armors);
            if (newTier > currentTier)
            {
                data.equippedArmor = itemName;
                data.armorUpgradeLevel = 0;
            }
        }
    }

    private static int GetTier(string itemName, string[] items)
    {
        return Array.IndexOf(items, itemName);
    }

    private static void AddInventoryItem(PlayerData data, string itemName)
    {
        if (!data.inventory.items.ContainsKey(itemName))
            data.inventory.items[itemName] = 1;
    }
}
