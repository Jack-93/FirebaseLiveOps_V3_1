using System;
using System.Threading.Tasks;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance;

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

    public bool CanClaimReward()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return false;

        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        return data.lastRewardDate != today;
    }

    public int GetNextRewardDay()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return 1;

        return data.loginDay >= 7 ? 1 : data.loginDay + 1;
    }

    public Task<bool> ClaimRewardAsync()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || !CanClaimReward())
        {
            Debug.Log("[DailyReward] Already Claimed or data is not ready.");
            return Task.FromResult(false);
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[DailyReward] InventoryManager is missing.");
            return Task.FromResult(false);
        }

        data.EnsureInitialized();
        data.loginDay = GetNextRewardDay();
        GiveReward(data.loginDay);
        data.lastRewardDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        LogDailyRewardSafely();

        Debug.Log($"[DailyReward] Day {data.loginDay} Claimed");
        return Task.FromResult(true);
    }

    public async void ClaimReward()
    {
        try
        {
            await ClaimRewardAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static void GiveReward(int day)
    {
        int index = day - 1;
        if (index < 0 ||
            index >= GameBalanceConfig.DailyRewardItemNames.Length ||
            index >= GameBalanceConfig.DailyRewardAmounts.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(day));
        }

        InventoryManager.Instance.AddItem(
            GameBalanceConfig.DailyRewardItemNames[index],
            GameBalanceConfig.DailyRewardAmounts[index],
            false);

        Debug.Log($"[DailyReward] Day {day} Reward Given");
    }

    private static void LogDailyRewardSafely()
    {
        try
        {
            AnalyticsManager.Instance?.LogDailyReward();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[DailyReward] Analytics logging failed: " +
                exception.Message);
        }
    }
}
