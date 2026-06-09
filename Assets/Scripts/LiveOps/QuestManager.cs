using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    private enum SequentialQuestType
    {
        DefeatMonsters,
        RecruitCompanion,
        UpgradeHero,
        ClearStage,
        UpgradeEquipment
    }

    private static readonly string[] QuestNames =
    {
        "Defeat monsters",
        "Recruit companions",
        "Upgrade hero",
        "Clear a stage",
        "Upgrade equipment"
    };

    private static readonly string[] KoreanQuestNames =
    {
        "몬스터 처치",
        "동료 모집",
        "영웅 강화",
        "스테이지 클리어",
        "장비 강화"
    };

    private static readonly int[] QuestTargets = { 5, 1, 1, 1, 1 };
    private const int QuestRewardGold =
        GameBalanceConfig.QuestRewardGold;

    public static QuestManager Instance;

    private BattleManager battleManager;
    private GrowthManager growthManager;
    private EquipmentManager equipmentManager;

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

    public void Initialize(
        BattleManager battle,
        GrowthManager growth,
        EquipmentManager equipment)
    {
        if (battleManager != null)
        {
            battleManager.OnEnemyDefeated -= HandleEnemyDefeated;
            battleManager.OnStageCleared -= HandleStageCleared;
        }

        if (growthManager != null)
            growthManager.OnUpgraded -= HandleHeroUpgraded;

        if (equipmentManager != null)
            equipmentManager.OnEquipmentUpgraded -= HandleEquipmentUpgraded;

        battleManager = battle;
        growthManager = growth;
        equipmentManager = equipment;

        battleManager.OnEnemyDefeated += HandleEnemyDefeated;
        battleManager.OnStageCleared += HandleStageCleared;
        growthManager.OnUpgraded += HandleHeroUpgraded;
        equipmentManager.OnEquipmentUpgraded += HandleEquipmentUpgraded;
        NormalizeProgress();
    }

    public async Task<bool> ClaimCurrentQuestAsync()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return false;

        int target = GetTarget(data);
        if (data.sequentialQuestProgress < target)
            return false;

        data.gold += QuestRewardGold;
        data.sequentialQuestProgress = 0;
        data.sequentialQuestIndex++;
        if (data.sequentialQuestIndex >= QuestNames.Length)
        {
            data.sequentialQuestIndex = 0;
            data.sequentialQuestCycles++;
        }

        await SaveAsync(data);
        return true;
    }

    public void ReportGacha(int count)
    {
        AddProgress(SequentialQuestType.RecruitCompanion, count);
    }

    public async Task<int> ClaimAvailableAchievementsAsync()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return 0;

        int claimed = 0;
        if (data.highestStage >= 5 &&
            ClaimOnce(data, "stage_5"))
        {
            InventoryManager.Instance.AddItem(
                "Gem",
                GameBalanceConfig.AchievementStageFiveGemReward,
                false);
            claimed++;
        }

        if (data.totalMonstersDefeated >= 50 &&
            ClaimOnce(data, "kills_50"))
        {
            InventoryManager.Instance.AddItem(
                "GachaTicket",
                GameBalanceConfig.AchievementKillFiftyTicketReward,
                false);
            claimed++;
        }

        if (claimed > 0)
            await SaveAsync(data);

        return claimed;
    }

    public string GetStatusText()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return LocalizationManager.Text(
                "Quest data unavailable.",
                "퀘스트 정보를 불러올 수 없습니다.");

        NormalizeProgress();
        int index = data.sequentialQuestIndex;
        int target = QuestTargets[index];
        bool complete = data.sequentialQuestProgress >= target;
        string questName = LocalizationManager.Text(
            QuestNames[index],
            KoreanQuestNames[index]);
        string current =
            $"{LocalizationManager.Text("Quest", "퀘스트")} " +
            $"{index + 1}/5: {questName}\n" +
            $"{data.sequentialQuestProgress}/{target}  " +
            $"{LocalizationManager.Text("Reward", "보상")}: " +
            $"{QuestRewardGold} " +
            $"{LocalizationManager.Text("Gold", "골드")}\n" +
            (complete
                ? LocalizationManager.Text(
                    "Ready to claim. Next quest starts immediately.",
                    "수령 가능. 다음 퀘스트가 바로 시작됩니다.")
                : LocalizationManager.Text(
                    "Keep playing to complete this objective.",
                    "계속 플레이해서 목표를 완료하세요."));
        string stage = data.claimedAchievementIds.Contains("stage_5")
            ? LocalizationManager.Text(
                "Stage 5: Claimed",
                "스테이지 5: 수령 완료")
            : $"{LocalizationManager.Text("Stage", "스테이지")} 5: " +
              $"{Math.Min(data.highestStage, 5)}/5";
        string kills = data.claimedAchievementIds.Contains("kills_50")
            ? LocalizationManager.Text(
                "50 kills: Claimed",
                "50마리 처치: 수령 완료")
            : $"{LocalizationManager.Text("Total kills", "총 처치")}:" +
              $" {Math.Min(data.totalMonstersDefeated, 50)}/50";

        return $"{current}\n{stage} | {kills}";
    }

    private void HandleEnemyDefeated(int reward)
    {
        AddProgress(SequentialQuestType.DefeatMonsters, 1);
    }

    private void HandleStageCleared(int stage)
    {
        AddProgress(SequentialQuestType.ClearStage, 1);
    }

    private void HandleHeroUpgraded(UpgradeType type)
    {
        AddProgress(SequentialQuestType.UpgradeHero, 1);
    }

    private void HandleEquipmentUpgraded(EquipmentSlot slot)
    {
        AddProgress(SequentialQuestType.UpgradeEquipment, 1);
    }

    private static void AddProgress(
        SequentialQuestType questType,
        int amount)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            data.sequentialQuestIndex != (int)questType ||
            amount <= 0)
        {
            return;
        }

        int target = GetTarget(data);
        data.sequentialQuestProgress = Mathf.Min(
            target,
            data.sequentialQuestProgress + amount);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
    }

    private static int GetTarget(PlayerData data)
    {
        int index = Mathf.Clamp(
            data.sequentialQuestIndex,
            0,
            QuestTargets.Length - 1);
        return QuestTargets[index];
    }

    private static void NormalizeProgress()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        data.sequentialQuestIndex = Mathf.Clamp(
            data.sequentialQuestIndex,
            0,
            QuestTargets.Length - 1);
        data.sequentialQuestProgress = Mathf.Clamp(
            data.sequentialQuestProgress,
            0,
            GetTarget(data));
    }

    private static bool ClaimOnce(PlayerData data, string id)
    {
        if (data.claimedAchievementIds.Contains(id))
            return false;

        data.claimedAchievementIds.Add(id);
        return true;
    }

    private static async Task SaveAsync(PlayerData data)
    {
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        if (FirestoreManager.Instance != null)
            await FirestoreManager.Instance.SavePlayerDataAsync(data);
    }

    private void OnDestroy()
    {
        if (battleManager != null)
        {
            battleManager.OnEnemyDefeated -= HandleEnemyDefeated;
            battleManager.OnStageCleared -= HandleStageCleared;
        }

        if (growthManager != null)
            growthManager.OnUpgraded -= HandleHeroUpgraded;

        if (equipmentManager != null)
            equipmentManager.OnEquipmentUpgraded -= HandleEquipmentUpgraded;
    }
}
