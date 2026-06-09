using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum RealMoneyProduct
{
    StarterPack,
    GemPackSmall,
    GemPackLarge
}

public enum RewardedAdPlacement
{
    ShopFreeGems
}

public sealed class StorePurchaseResult
{
    public bool Completed;
    public bool ReceiptValidated;
    public string ProductId;
    public string TransactionId;
    public string Receipt;
    public string Message;
}

public sealed class RewardedAdResult
{
    public bool Completed;
    public string RewardId;
    public string Message;
}

public interface IStorePurchaseProvider
{
    bool IsReady { get; }
    Task InitializeAsync();
    Task<StorePurchaseResult> PurchaseAsync(string productId);
    string GetLocalizedPrice(string productId);
}

public interface IRewardedAdProvider
{
    bool IsReady { get; }
    Task InitializeAsync();
    Task<RewardedAdResult> ShowAsync(string placementId);
}

public class MonetizationManager : MonoBehaviour
{
    public const int DailyRewardedAdLimit =
        GameBalanceConfig.RewardedAdDailyLimit;
    public const int RewardedAdGemAmount =
        GameBalanceConfig.RewardedAdGemAmount;
    public const int RewardedAdCooldownSeconds =
        GameBalanceConfig.RewardedAdCooldownSeconds;

    public static MonetizationManager Instance;

    public event Action OnMonetizationChanged;
    public bool IsBusy { get; private set; }
    public bool StoreReady => storeProvider?.IsReady == true;
    public bool AdProviderReady => adProvider?.IsReady == true;
    public bool RewardedAdReady =>
        AdProviderReady && CanWatchRewardedAd(out _);

    private IStorePurchaseProvider storeProvider;
    private IRewardedAdProvider adProvider;
    private bool initialized;

    private static readonly Dictionary<RealMoneyProduct, string>
        ProductIds = new Dictionary<RealMoneyProduct, string>
        {
            {
                RealMoneyProduct.StarterPack,
                "com.dourgame.gameliveops.starter_pack"
            },
            {
                RealMoneyProduct.GemPackSmall,
                "com.dourgame.gameliveops.gems_1200"
            },
            {
                RealMoneyProduct.GemPackLarge,
                "com.dourgame.gameliveops.gems_6500"
            }
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

#if UNITY_EDITOR
        storeProvider = new EditorStorePurchaseProvider();
        adProvider = new EditorRewardedAdProvider();
#endif
    }

    public void RegisterStoreProvider(IStorePurchaseProvider provider)
    {
        storeProvider = provider;
        initialized = false;
    }

    public void RegisterRewardedAdProvider(IRewardedAdProvider provider)
    {
        adProvider = provider;
        initialized = false;
    }

    public async Task InitializeAsync()
    {
        if (initialized)
            return;

        try
        {
            if (storeProvider != null)
                await storeProvider.InitializeAsync();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[Monetization] Store initialization failed: " +
                exception.Message);
        }

        try
        {
            if (adProvider != null)
                await adProvider.InitializeAsync();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[Monetization] Ads initialization failed: " +
                exception.Message);
        }

        initialized = true;
        OnMonetizationChanged?.Invoke();
    }

    public async Task<string> PurchaseAsync(RealMoneyProduct product)
    {
        if (IsBusy)
            return "Another purchase is already running.";
        if (storeProvider?.IsReady != true)
        {
            return
                "Store SDK is not connected yet. " +
                "The purchase reward layer is ready.";
        }

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return "Player data is not ready.";
        data.EnsureInitialized();

        string productId = GetProductId(product);
        if (product == RealMoneyProduct.StarterPack &&
            data.ownedPurchaseProducts.Contains(productId))
        {
            return "Starter Pack is already owned.";
        }

        IsBusy = true;
        OnMonetizationChanged?.Invoke();

        try
        {
            StorePurchaseResult result =
                await storeProvider.PurchaseAsync(productId);
            if (result == null || !result.Completed)
                return result?.Message ?? "Purchase was not completed.";
            if (result.ProductId != productId)
                return "Store returned a different product.";
            if (!result.ReceiptValidated)
                return "Purchase receipt could not be verified.";
            if (string.IsNullOrWhiteSpace(result.TransactionId))
                return "Store returned an invalid transaction.";
            if (data.processedPurchaseIds.Contains(result.TransactionId))
                return "This purchase was already granted.";

            GrantPurchase(data, product);
            AddLimited(
                data.processedPurchaseIds,
                result.TransactionId,
                100);
            if (product == RealMoneyProduct.StarterPack)
                data.ownedPurchaseProducts.Add(productId);

            PlayerDataManager.Instance.NotifyPlayerDataChanged();
            await SaveAsync(data);
            return $"{GetProductName(product)} purchased.";
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            return "Purchase failed. Please try again.";
        }
        finally
        {
            IsBusy = false;
            OnMonetizationChanged?.Invoke();
        }
    }

    public async Task<string> ShowRewardedAdAsync(
        RewardedAdPlacement placement)
    {
        if (IsBusy)
            return "Another reward request is already running.";
        if (adProvider?.IsReady != true)
        {
            return
                "Rewarded ad SDK is not connected yet. " +
                "The reward layer is ready.";
        }
        if (!CanWatchRewardedAd(out string reason))
            return reason;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return "Player data is not ready.";

        IsBusy = true;
        OnMonetizationChanged?.Invoke();

        try
        {
            RewardedAdResult result =
                await adProvider.ShowAsync(GetPlacementId(placement));
            if (result == null || !result.Completed)
                return result?.Message ?? "Ad was not completed.";
            if (string.IsNullOrWhiteSpace(result.RewardId))
                return "Ad network returned an invalid reward.";
            if (data.processedAdRewardIds.Contains(result.RewardId))
                return "This ad reward was already granted.";

            ResetAdDayIfNeeded(data);
            AddItem(data, "Gem", RewardedAdGemAmount);
            data.rewardedAdsWatchedToday++;
            data.lastRewardedAdUnixTime =
                DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            AddLimited(data.processedAdRewardIds, result.RewardId, 100);

            PlayerDataManager.Instance.NotifyPlayerDataChanged();
            await SaveAsync(data);
            return $"Ad reward: {RewardedAdGemAmount} Gems.";
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            return "Rewarded ad failed. Please try again.";
        }
        finally
        {
            IsBusy = false;
            OnMonetizationChanged?.Invoke();
        }
    }

    public bool CanWatchRewardedAd(out string reason)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
        {
            reason = "Player data is not ready.";
            return false;
        }

        ResetAdDayIfNeeded(data);
        if (data.rewardedAdsWatchedToday >= DailyRewardedAdLimit)
        {
            reason = "Daily rewarded ad limit reached.";
            return false;
        }

        long elapsed =
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() -
            data.lastRewardedAdUnixTime;
        if (elapsed < RewardedAdCooldownSeconds)
        {
            reason =
                $"Next ad in {RewardedAdCooldownSeconds - elapsed}s.";
            return false;
        }

        reason = "";
        return true;
    }

    public string GetStoreStatus()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data != null)
            ResetAdDayIfNeeded(data);
        int watched = data?.rewardedAdsWatchedToday ?? 0;
        string store = StoreReady ? "Store ready" : "Store SDK pending";
        string ads = adProvider?.IsReady == true
            ? $"Rewarded ads {watched}/{DailyRewardedAdLimit}"
            : "Ad SDK pending";
        return $"{store}  |  {ads}";
    }

    public string GetPriceLabel(RealMoneyProduct product)
    {
        string price =
            storeProvider?.GetLocalizedPrice(GetProductId(product));
        return string.IsNullOrWhiteSpace(price)
            ? GetFallbackPrice(product)
            : price;
    }

    public static string GetProductId(RealMoneyProduct product)
    {
        return ProductIds[product];
    }

    public static string GetProductName(RealMoneyProduct product)
    {
        switch (product)
        {
            case RealMoneyProduct.StarterPack:
                return "Starter Pack";
            case RealMoneyProduct.GemPackSmall:
                return "1,200 Gems";
            case RealMoneyProduct.GemPackLarge:
                return "6,500 Gems";
            default:
                throw new ArgumentOutOfRangeException(nameof(product));
        }
    }

    private static string GetFallbackPrice(RealMoneyProduct product)
    {
        switch (product)
        {
            case RealMoneyProduct.StarterPack:
                return "$1.99";
            case RealMoneyProduct.GemPackSmall:
                return "$4.99";
            case RealMoneyProduct.GemPackLarge:
                return "$19.99";
            default:
                return "-";
        }
    }

    private static string GetPlacementId(RewardedAdPlacement placement)
    {
        switch (placement)
        {
            case RewardedAdPlacement.ShopFreeGems:
                return "shop_free_gems";
            default:
                throw new ArgumentOutOfRangeException(nameof(placement));
        }
    }

    private static void GrantPurchase(
        PlayerData data,
        RealMoneyProduct product)
    {
        switch (product)
        {
            case RealMoneyProduct.StarterPack:
                data.gold += GameBalanceConfig.StarterPackGold;
                AddItem(data, "Gem", GameBalanceConfig.StarterPackGems);
                AddItem(
                    data,
                    "GachaTicket",
                    GameBalanceConfig.StarterPackTickets);
                break;
            case RealMoneyProduct.GemPackSmall:
                AddItem(data, "Gem", GameBalanceConfig.SmallGemPackGems);
                break;
            case RealMoneyProduct.GemPackLarge:
                AddItem(data, "Gem", GameBalanceConfig.LargeGemPackGems);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(product));
        }
    }

    private static void AddItem(
        PlayerData data,
        string itemName,
        int amount)
    {
        int current = GachaEconomy.GetItemCount(data, itemName);
        data.inventory.items[itemName] = current + amount;
    }

    private static void ResetAdDayIfNeeded(PlayerData data)
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        if (data.rewardedAdDate == today)
            return;

        data.rewardedAdDate = today;
        data.rewardedAdsWatchedToday = 0;
    }

    private static void AddLimited(
        List<string> values,
        string value,
        int maximum)
    {
        values.Add(value);
        if (values.Count > maximum)
            values.RemoveRange(0, values.Count - maximum);
    }

    private static async Task SaveAsync(PlayerData data)
    {
        if (FirestoreManager.Instance != null)
            await FirestoreManager.Instance.SavePlayerDataAsync(data);
    }
}

#if UNITY_EDITOR
internal sealed class EditorStorePurchaseProvider :
    IStorePurchaseProvider
{
    public bool IsReady { get; private set; }

    public Task InitializeAsync()
    {
        IsReady = true;
        return Task.CompletedTask;
    }

    public async Task<StorePurchaseResult> PurchaseAsync(string productId)
    {
        await Task.Delay(300);
        return new StorePurchaseResult
        {
            Completed = true,
            ReceiptValidated = true,
            ProductId = productId,
            TransactionId = "editor_" + Guid.NewGuid().ToString("N"),
            Receipt = "EDITOR_TEST_RECEIPT",
            Message = "Editor test purchase completed."
        };
    }

    public string GetLocalizedPrice(string productId)
    {
        return "TEST";
    }
}

internal sealed class EditorRewardedAdProvider : IRewardedAdProvider
{
    public bool IsReady { get; private set; }

    public Task InitializeAsync()
    {
        IsReady = true;
        return Task.CompletedTask;
    }

    public async Task<RewardedAdResult> ShowAsync(string placementId)
    {
        await Task.Delay(500);
        return new RewardedAdResult
        {
            Completed = true,
            RewardId = "editor_ad_" + Guid.NewGuid().ToString("N"),
            Message = "Editor test ad completed."
        };
    }
}
#endif
