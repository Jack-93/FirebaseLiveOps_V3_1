using System;

public static class GachaEconomy
{
    public const int SingleGemCost =
        GameBalanceConfig.GachaSingleGemCost;
    public const int TenGemCost =
        GameBalanceConfig.GachaTenGemCost;

    public static bool TrySpend(
        PlayerData data,
        int pullCount,
        out GachaPayment payment)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (pullCount != 1 && pullCount != 10)
            throw new ArgumentOutOfRangeException(nameof(pullCount));

        data.EnsureInitialized();

        int ticketCost = pullCount;
        int tickets = GetItemCount(data, "GachaTicket");

        if (tickets >= ticketCost)
        {
            RemoveItem(data, "GachaTicket", ticketCost);
            payment = new GachaPayment(true, ticketCost);
            return true;
        }

        int gemCost =
            pullCount == 1 ? SingleGemCost : TenGemCost;
        int gems = GetItemCount(data, "Gem");

        if (gems >= gemCost)
        {
            RemoveItem(data, "Gem", gemCost);
            payment = new GachaPayment(false, gemCost);
            return true;
        }

        payment = default;
        return false;
    }

    public static void Refund(PlayerData data, GachaPayment payment)
    {
        if (data == null || payment.Amount <= 0)
            return;

        AddItem(
            data,
            payment.UsedTickets ? "GachaTicket" : "Gem",
            payment.Amount);
    }

    public static int GetItemCount(PlayerData data, string itemName)
    {
        if (data?.inventory?.items == null)
            return 0;

        return data.inventory.items.TryGetValue(
            itemName,
            out int amount)
            ? amount
            : 0;
    }

    private static void RemoveItem(
        PlayerData data,
        string itemName,
        int amount)
    {
        int remaining = GetItemCount(data, itemName) - amount;
        if (remaining > 0)
            data.inventory.items[itemName] = remaining;
        else
            data.inventory.items.Remove(itemName);
    }

    private static void AddItem(
        PlayerData data,
        string itemName,
        int amount)
    {
        data.inventory.items[itemName] =
            GetItemCount(data, itemName) + amount;
    }
}

public readonly struct GachaPayment
{
    public bool UsedTickets { get; }
    public int Amount { get; }

    public GachaPayment(bool usedTickets, int amount)
    {
        UsedTickets = usedTickets;
        Amount = amount;
    }
}
