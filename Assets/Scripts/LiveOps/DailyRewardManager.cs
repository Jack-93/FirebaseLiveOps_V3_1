using System;
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
        PlayerData data =
            PlayerDataManager.Instance.playerData;

        string today =
            DateTime.UtcNow.ToString("yyyy-MM-dd");

        return data.lastRewardDate != today;
    }

    public void ClaimReward()
    {
        PlayerData data =
            PlayerDataManager.Instance.playerData;

        if (!CanClaimReward())
        {
            Debug.Log(
                "[DailyReward] Already Claimed");
            return;
        }

        data.loginDay++;

        if (data.loginDay > 7)
        {
            data.loginDay = 1;
        }

        GiveReward(data.loginDay);

        data.lastRewardDate =
            DateTime.UtcNow.ToString("yyyy-MM-dd");

        FirestoreManager.Instance
            .SavePlayerData(data);

        Debug.Log(
            $"[DailyReward] Day {data.loginDay} Claimed");
    }

    private void GiveReward(int day)
    {
        switch (day)
        {
            case 1:
                InventoryManager.Instance
                    .AddItem("Gem", 100);
                break;

            case 2:
                InventoryManager.Instance
                    .AddItem("Gem", 200);
                break;

            case 3:
                InventoryManager.Instance
                    .AddItem("Gem", 300);
                break;

            case 4:
                InventoryManager.Instance
                    .AddItem("GachaTicket", 1);
                break;

            case 5:
                InventoryManager.Instance
                    .AddItem("Gem", 500);
                break;

            case 6:
                InventoryManager.Instance
                    .AddItem("GachaTicket", 3);
                break;

            case 7:
                InventoryManager.Instance
                    .AddItem("SSRTicket", 1);
                break;
        }

        Debug.Log(
            $"[DailyReward] Day {day} Reward Given");
    }
}