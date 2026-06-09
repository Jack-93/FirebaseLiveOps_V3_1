using UnityEngine;
using System;
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public event Action OnInventoryChanged;

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

    public void AddItem(
        string itemName,
        int amount,
        bool savePlayerData = true)
    {
        if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
            return;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
        {
            Debug.LogWarning("[Inventory] PlayerData is not ready.");
            return;
        }

        data.EnsureInitialized();
        var inventory =
            data.inventory.items;

        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName] += amount;
        }
        else
        {
            inventory.Add(itemName, amount);
        }

        Debug.Log(
            $"[Inventory] {itemName} +{amount}"
        );

        if (savePlayerData && FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance
                .SavePlayerData(
                    data);
        }

        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(
        string itemName,
        int amount,
        bool savePlayerData = true)
    {
        if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
            return;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
        {
            Debug.LogWarning("[Inventory] PlayerData is not ready.");
            return;
        }

        data.EnsureInitialized();
        var inventory =
            data.inventory.items;

        if (!inventory.ContainsKey(itemName))
            return;

        inventory[itemName] -= amount;

        if (inventory[itemName] <= 0)
        {
            inventory.Remove(itemName);
        }

        Debug.Log(
            $"[Inventory] {itemName} - {amount}"
        );

        if (savePlayerData && FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance
                .SavePlayerData(
                    data);
        }

        OnInventoryChanged?.Invoke();
    }
}
