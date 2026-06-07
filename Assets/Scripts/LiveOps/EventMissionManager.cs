using System;
using System.Threading.Tasks;
using UnityEngine;

public class EventMissionManager : MonoBehaviour
{
    public const int KillTarget = 4;
    public const int GachaTarget = 3;
    public const int RewardPointTarget = 30;

    public static EventMissionManager Instance;

    private BattleManager battleManager;

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

    public void Initialize(BattleManager battle)
    {
        if (battleManager != null)
            battleManager.OnEnemyDefeated -= HandleEnemyDefeated;

        battleManager = battle;
        battleManager.OnEnemyDefeated += HandleEnemyDefeated;
        ResetIfNeeded();
    }

    public void ReportGacha(int count)
    {
        ResetIfNeeded();
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || count <= 0)
            return;

        data.eventGachaCount = Mathf.Min(
            GachaTarget,
            data.eventGachaCount + count);
        UpdatePoints(data);
    }

    public async Task<bool> ClaimRewardAsync()
    {
        ResetIfNeeded();
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            data.eventRewardClaimed ||
            data.eventMissionPoints < RewardPointTarget)
        {
            return false;
        }

        data.eventRewardClaimed = true;
        InventoryManager.Instance.AddItem("Gem", 300, false);
        InventoryManager.Instance.AddItem("GachaTicket", 2, false);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();

        if (FirestoreManager.Instance != null)
            await FirestoreManager.Instance.SavePlayerDataAsync(data);

        return true;
    }

    public string GetStatusText()
    {
        ResetIfNeeded();
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return "Event data unavailable.";

        string reward = data.eventRewardClaimed
            ? "Reward claimed"
            : $"Points {data.eventMissionPoints}/{RewardPointTarget}";
        return
            "DAILY PLAY EVENT\n" +
            $"Defeat monsters  {data.eventKillCount}/{KillTarget}  " +
            "(10 pts)\n" +
            $"Recruit companions  {data.eventGachaCount}/{GachaTarget}  " +
            "(20 pts)\n\n" +
            $"{reward}\nReward: 300 Gems + 2 Gacha Tickets";
    }

    private void HandleEnemyDefeated(int reward)
    {
        ResetIfNeeded();
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        data.eventKillCount = Mathf.Min(
            KillTarget,
            data.eventKillCount + 1);
        UpdatePoints(data);
    }

    private static void UpdatePoints(PlayerData data)
    {
        int points = 0;
        if (data.eventKillCount >= KillTarget)
            points += 10;
        if (data.eventGachaCount >= GachaTarget)
            points += 20;

        data.eventMissionPoints = points;
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
    }

    private static void ResetIfNeeded()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        if (data.eventMissionDate == today)
            return;

        data.eventMissionDate = today;
        data.eventKillCount = 0;
        data.eventGachaCount = 0;
        data.eventMissionPoints = 0;
        data.eventRewardClaimed = false;
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
    }

    private void OnDestroy()
    {
        if (battleManager != null)
            battleManager.OnEnemyDefeated -= HandleEnemyDefeated;
    }
}
