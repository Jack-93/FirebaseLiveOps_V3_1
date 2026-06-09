using System;
using System.Threading.Tasks;
using UnityEngine;

public enum ShopProduct
{
    GoldPouch,
    TicketBundle,
    GrowthChest
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

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

    public async Task<bool> TryPurchaseAsync(ShopProduct product)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return false;

        int gemCost = GetGemCost(product);
        int currentGems = GachaEconomy.GetItemCount(data, "Gem");
        if (currentGems < gemCost)
            return false;

        int previousGold = data.gold;
        int previousTickets =
            GachaEconomy.GetItemCount(data, "GachaTicket");

        SetItemCount(data, "Gem", currentGems - gemCost);
        GrantProduct(data, product);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();

        try
        {
            if (FirestoreManager.Instance != null)
                await FirestoreManager.Instance.SavePlayerDataAsync(data);

            return true;
        }
        catch
        {
            data.gold = previousGold;
            SetItemCount(data, "Gem", currentGems);
            SetItemCount(data, "GachaTicket", previousTickets);
            PlayerDataManager.Instance.NotifyPlayerDataChanged();
            throw;
        }
    }

    public static int GetGemCost(ShopProduct product)
    {
        switch (product)
        {
            case ShopProduct.GoldPouch:
                return GameBalanceConfig.ShopGoldPouchGemCost;
            case ShopProduct.TicketBundle:
                return GameBalanceConfig.ShopTicketBundleGemCost;
            case ShopProduct.GrowthChest:
                return GameBalanceConfig.ShopGrowthChestGemCost;
            default:
                throw new ArgumentOutOfRangeException(nameof(product));
        }
    }

    public static string GetDescription(ShopProduct product)
    {
        switch (product)
        {
            case ShopProduct.GoldPouch:
                return $"{GameBalanceConfig.ShopGoldPouchGold:N0} Gold";
            case ShopProduct.TicketBundle:
                return
                    $"{GameBalanceConfig.ShopTicketBundleTickets} " +
                    "Gacha Tickets";
            case ShopProduct.GrowthChest:
                return $"{GameBalanceConfig.ShopGrowthChestGold:N0} Gold";
            default:
                throw new ArgumentOutOfRangeException(nameof(product));
        }
    }

    private static void GrantProduct(PlayerData data, ShopProduct product)
    {
        switch (product)
        {
            case ShopProduct.GoldPouch:
                data.gold += GameBalanceConfig.ShopGoldPouchGold;
                break;
            case ShopProduct.TicketBundle:
                SetItemCount(
                    data,
                    "GachaTicket",
                    GachaEconomy.GetItemCount(data, "GachaTicket") +
                    GameBalanceConfig.ShopTicketBundleTickets);
                break;
            case ShopProduct.GrowthChest:
                data.gold += GameBalanceConfig.ShopGrowthChestGold;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(product));
        }
    }

    private static void SetItemCount(
        PlayerData data,
        string itemName,
        int amount)
    {
        if (amount > 0)
            data.inventory.items[itemName] = amount;
        else
            data.inventory.items.Remove(itemName);
    }
}
