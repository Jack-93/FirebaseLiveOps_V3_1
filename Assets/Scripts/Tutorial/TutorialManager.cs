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
                    return "Welcome. Begin the adventure.";
                case 1:
                    return "Open Growth and upgrade Attack once.";
                case 2:
                    return "Return to Battle and defeat one enemy.";
                default:
                    return "Tutorial complete. Keep advancing.";
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
