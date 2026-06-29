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

    public Task<bool> TryPurchaseAsync(ShopProduct product)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return Task.FromResult(false);

        if (!IsValidProduct(product))
            return Task.FromResult(false);

        data.EnsureInitialized();
        int gemCost = GetGemCost(product);
        int currentGems = GachaEconomy.GetItemCount(data, "Gem");
        if (currentGems < gemCost)
            return Task.FromResult(false);

        SetItemCount(data, "Gem", currentGems - gemCost);
        GrantProduct(data, product);
        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        return Task.FromResult(true);
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
                return CompactNumberFormatter.Format(
                    GameBalanceConfig.ShopGoldPouchGold) + " Gold";
            case ShopProduct.TicketBundle:
                return
                    $"{CompactNumberFormatter.Format(GameBalanceConfig.ShopTicketBundleTickets)} " +
                    "Gacha Tickets";
            case ShopProduct.GrowthChest:
                return CompactNumberFormatter.Format(
                    GameBalanceConfig.ShopGrowthChestGold) + " Gold";
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

    private static bool IsValidProduct(ShopProduct product)
    {
        return product == ShopProduct.GoldPouch ||
            product == ShopProduct.TicketBundle ||
            product == ShopProduct.GrowthChest;
    }
}
