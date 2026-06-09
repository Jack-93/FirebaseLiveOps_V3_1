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

    public async Task<bool> ClaimRewardAsync()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || !CanClaimReward())
        {
            Debug.Log("[DailyReward] Already Claimed or data is not ready.");
            return false;
        }

        data.loginDay = GetNextRewardDay();
        GiveReward(data.loginDay);
        data.lastRewardDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (FirestoreManager.Instance != null)
            await FirestoreManager.Instance.SavePlayerDataAsync(data);

        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        AnalyticsManager.Instance?.LogDailyReward();

        Debug.Log($"[DailyReward] Day {data.loginDay} Claimed");
        return true;
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
}
