using System;
using System.Threading.Tasks;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public event Action OnTutorialChanged;

    public bool IsComplete =>
        PlayerDataManager.Instance?.playerData?.tutorialCompleted == true;

    public int CurrentStep =>
        PlayerDataManager.Instance?.playerData?.tutorialStep ?? 0;

    public string CurrentMessage
    {
        get
        {
            switch (CurrentStep)
            {
                case 0:
                    return LocalizationManager.Text(
                        "Welcome. Begin the adventure.",
                        "환영합니다. 모험을 시작하세요.");
                case 1:
                    return LocalizationManager.Text(
                        "Open Growth and upgrade Attack once.",
                        "성장에서 공격력을 한 번 강화하세요.");
                case 2:
                    return LocalizationManager.Text(
                        "Return to Battle and defeat one enemy.",
                        "전투로 돌아가 적 한 마리를 처치하세요.");
                default:
                    return LocalizationManager.Text(
                        "Tutorial complete. Keep advancing.",
                        "튜토리얼 완료. 계속 전진하세요.");
            }
        }
    }

    private BattleManager battleManager;
    private GrowthManager growthManager;
    private bool isBound;

    public void Initialize(
        BattleManager battle,
        GrowthManager growth)
    {
        battleManager = battle;
        growthManager = growth;

        if (!isBound)
        {
            growthManager.OnUpgraded += HandleUpgraded;
            battleManager.OnEnemyDefeated += HandleEnemyDefeated;
            isBound = true;
        }

        battleManager.SetRunning(IsComplete || CurrentStep >= 2);
        OnTutorialChanged?.Invoke();
    }

    public void BeginTutorial()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || data.tutorialCompleted || data.tutorialStep != 0)
            return;

        data.tutorialStep = 1;
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        OnTutorialChanged?.Invoke();
        _ = SaveAsync();
    }

    private void HandleUpgraded(UpgradeType type)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            data.tutorialCompleted ||
            data.tutorialStep != 1 ||
            type != UpgradeType.Attack)
        {
            return;
        }

        data.tutorialStep = 2;
        battleManager.SetRunning(true);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        OnTutorialChanged?.Invoke();
        _ = SaveAsync();
    }

    private void HandleEnemyDefeated(int reward)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            data.tutorialCompleted ||
            data.tutorialStep != 2)
        {
            return;
        }

        data.tutorialStep = 3;
        data.tutorialCompleted = true;
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        OnTutorialChanged?.Invoke();
        _ = SaveAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            if (FirestoreManager.Instance != null)
            {
                await FirestoreManager.Instance.SavePlayerDataAsync(
                    PlayerDataManager.Instance.playerData);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void OnDestroy()
    {
        if (!isBound)
            return;

        if (growthManager != null)
            growthManager.OnUpgraded -= HandleUpgraded;

        if (battleManager != null)
            battleManager.OnEnemyDefeated -= HandleEnemyDefeated;
    }
}
