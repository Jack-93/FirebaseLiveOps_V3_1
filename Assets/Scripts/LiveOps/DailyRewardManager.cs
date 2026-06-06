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
        switch (day)
        {
            case 1:
                InventoryManager.Instance.AddItem("Gem", 100, false);
                break;
            case 2:
                InventoryManager.Instance.AddItem("Gem", 200, false);
                break;
            case 3:
                InventoryManager.Instance.AddItem("Gem", 300, false);
                break;
            case 4:
                InventoryManager.Instance.AddItem("GachaTicket", 1, false);
                break;
            case 5:
                InventoryManager.Instance.AddItem("Gem", 500, false);
                break;
            case 6:
                InventoryManager.Instance.AddItem("GachaTicket", 3, false);
                break;
            case 7:
                InventoryManager.Instance.AddItem("SSRTicket", 1, false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(day));
        }

        Debug.Log($"[DailyReward] Day {day} Reward Given");
    }
}
