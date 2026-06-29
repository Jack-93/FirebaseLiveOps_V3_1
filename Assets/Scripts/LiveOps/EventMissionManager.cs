using System;
using System.Threading.Tasks;
using UnityEngine;

public class EventMissionManager : MonoBehaviour
{
    public const int KillTarget =
        GameBalanceConfig.EventKillTarget;
    public const int GachaTarget =
        GameBalanceConfig.EventGachaTarget;
    public const int RewardPointTarget =
        GameBalanceConfig.EventRewardPointTarget;

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

        int nextCount = Mathf.Min(
            GachaTarget,
            data.eventGachaCount + count);
        if (nextCount == data.eventGachaCount)
            return;

        data.eventGachaCount = nextCount;
        UpdatePoints(data);
    }

    public Task<bool> ClaimRewardAsync()
    {
        ResetIfNeeded();
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            data.eventRewardClaimed ||
            data.eventMissionPoints < RewardPointTarget)
        {
            return Task.FromResult(false);
        }

        InventoryManager inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            Debug.LogError("[Event] InventoryManager is missing.");
            return Task.FromResult(false);
        }

        data.EnsureInitialized();
        data.eventRewardClaimed = true;
        inventoryManager.AddItem(
            "Gem",
            GameBalanceConfig.EventRewardGems,
            false);
        inventoryManager.AddItem(
            "GachaTicket",
            GameBalanceConfig.EventRewardTickets,
            false);
        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        return Task.FromResult(true);
    }

    public string GetStatusText()
    {
        ResetIfNeeded();
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return LocalizationManager.Text(
                "Event data unavailable.",
                "이벤트 정보를 불러올 수 없습니다.");

        string reward = data.eventRewardClaimed
            ? LocalizationManager.Text(
                "Reward claimed",
                "보상 수령 완료")
            : $"{LocalizationManager.Text("Points", "포인트")} " +
              $"{CompactNumberFormatter.Format(data.eventMissionPoints)}/" +
              $"{CompactNumberFormatter.Format(RewardPointTarget)}";
        return
            $"{LocalizationManager.Text("DAILY PLAY EVENT", "일일 플레이 이벤트")}\n" +
            $"{LocalizationManager.Text("Defeat monsters", "몬스터 처치")}  " +
            $"{CompactNumberFormatter.Format(data.eventKillCount)}/" +
            $"{CompactNumberFormatter.Format(KillTarget)}  " +
            $"({CompactNumberFormatter.Format(GameBalanceConfig.EventKillPoints)} " +
            $"{LocalizationManager.Text("pts", "점")})\n" +
            $"{LocalizationManager.Text("Recruit companions", "동료 모집")}  " +
            $"{CompactNumberFormatter.Format(data.eventGachaCount)}/" +
            $"{CompactNumberFormatter.Format(GachaTarget)}  " +
            $"({CompactNumberFormatter.Format(GameBalanceConfig.EventGachaPoints)} " +
            $"{LocalizationManager.Text("pts", "점")})\n\n" +
            $"{reward}\n" +
            $"{LocalizationManager.Text("Reward", "보상")}: " +
            $"{CompactNumberFormatter.Format(GameBalanceConfig.EventRewardGems)} " +
            $"{LocalizationManager.Text("Gems", "젬")} + " +
            $"{CompactNumberFormatter.Format(GameBalanceConfig.EventRewardTickets)} " +
            $"{LocalizationManager.Text("Gacha Tickets", "뽑기 티켓")}";
    }

    private void HandleEnemyDefeated(int reward)
    {
        ResetIfNeeded();
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        int nextCount = Mathf.Min(
            KillTarget,
            data.eventKillCount + 1);
        if (nextCount == data.eventKillCount)
            return;

        data.eventKillCount = nextCount;
        UpdatePoints(data);
    }

    private static void UpdatePoints(PlayerData data)
    {
        int points = 0;
        if (data.eventKillCount >= KillTarget)
            points += GameBalanceConfig.EventKillPoints;
        if (data.eventGachaCount >= GachaTarget)
            points += GameBalanceConfig.EventGachaPoints;

        data.eventMissionPoints = points;
        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
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
        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
    }

    private void OnDestroy()
    {
        if (battleManager != null)
            battleManager.OnEnemyDefeated -= HandleEnemyDefeated;
    }
}
