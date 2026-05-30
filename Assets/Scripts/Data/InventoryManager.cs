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

    public void AddItem(string itemName, int amount)
    {
        var inventory =
            PlayerDataManager.Instance
            .playerData
            .inventory
            .items;

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

        FirestoreManager.Instance
            .SavePlayerData(
                PlayerDataManager.Instance.playerData
            );

        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(string itemName, int amount)
    {
        var inventory =
            PlayerDataManager.Instance
            .playerData
            .inventory
            .items;

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

        FirestoreManager.Instance
            .SavePlayerData(
                PlayerDataManager.Instance
                .playerData
            );
    }
}