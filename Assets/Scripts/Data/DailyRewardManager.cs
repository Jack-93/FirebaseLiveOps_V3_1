using System;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CheckDailyReward()
    {
        PlayerData data =
            PlayerDataManager.Instance.playerData;

        string today =
            DateTime.UtcNow.ToString("yyyy-MM-dd");

        Debug.Log($"[DailyReward] Today: {today}");

        // 첫 로그인 or 날짜 다름
        if (data.lastLoginDate != today)
        {
            GiveDailyReward();

            data.lastLoginDate = today;

            FirestoreManager.Instance
                .SavePlayerData(data);

            Debug.Log(
                "[DailyReward] Reward Claimed"
            );
        }
        else
        {
            Debug.Log(
                "[DailyReward] Already Claimed"
            );
        }
    }

    private void GiveDailyReward()
    {
        InventoryManager.Instance
            .AddItem("Gem", 500);

        Debug.Log(
            "[DailyReward] Gem +500"
        );

        // InventoryUImanager.instance.RefreshInventoryUI();
    }
}