using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public const int TutorialGachaTicketCount = 10;

    public static TutorialManager Instance;

    public event Action OnTutorialChanged;

    public bool IsComplete =>
        PlayerDataManager.Instance?.playerData?.tutorialCompleted == true;

    public int CurrentStep =>
        PlayerDataManager.Instance?.playerData?.tutorialStep ?? 0;

    public bool ShouldShowStoryIntro
    {
        get
        {
            PlayerData data = PlayerDataManager.Instance?.playerData;
            return data != null &&
                !data.storyIntroCompleted &&
                !data.tutorialCompleted;
        }
    }

    public int CurrentStoryCutIndex =>
        PlayerDataManager.Instance?.playerData?.storyIntroCutIndex ?? 0;

    public bool ShouldShowTutorialTicketGift
    {
        get
        {
            PlayerData data = PlayerDataManager.Instance?.playerData;
            return data != null &&
                data.storyIntroCompleted &&
                !data.tutorialCompleted &&
                data.tutorialStep == 0 &&
                !data.tutorialGachaTicketsGranted &&
                !data.tutorialGachaClaimed;
        }
    }

    public bool IsWaitingForTutorialGacha
    {
        get
        {
            PlayerData data = PlayerDataManager.Instance?.playerData;
            return data != null &&
                data.storyIntroCompleted &&
                !data.tutorialCompleted &&
                data.tutorialStep == 0 &&
                data.tutorialGachaTicketsGranted &&
                !data.tutorialGachaClaimed;
        }
    }

    public List<StoryIntroCut> StoryCuts =>
        StoryIntroDatabase.GetCuts();

    public StoryIntroCut CurrentStoryCut
    {
        get
        {
            List<StoryIntroCut> cuts = StoryCuts;
            if (cuts.Count == 0)
                return null;

            int index = Mathf.Clamp(
                CurrentStoryCutIndex,
                0,
                cuts.Count - 1);
            return cuts[index];
        }
    }

    public string CurrentMessage =>
        GetCleanTutorialMessage(CurrentStep);

    private string GetCleanTutorialMessage(int step)
    {
        switch (step)
        {
            case 0:
                if (ShouldShowTutorialTicketGift)
                {
                    return LocalizationManager.Text(
                        "Gift: 10 recruitment tickets. Tap Next to receive them.",
                        "\uC120\uBB3C: \uBAA8\uC9D1 \uD2F0\uCF13 10\uC7A5. \uB2E4\uC74C\uC744 \uB20C\uB7EC \uBC1B\uC73C\uC138\uC694.");
                }

                return LocalizationManager.Text(
                    "Use the tickets to recruit 10 companions.",
                    "\uBC1B\uC740 \uD2F0\uCF13\uC73C\uB85C 10\uD68C \uBAA8\uC9D1\uC744 \uB20C\uB7EC \uB3D9\uB8CC\uB97C \uBAA8\uC9D1\uD558\uC138\uC694.");
            case 1:
                return LocalizationManager.Text(
                    "Charge power once.",
                    "\uC804\uB825\uC744 \uD55C \uBC88 \uCDA9\uC804\uD558\uC138\uC694.");
            case 2:
                return LocalizationManager.Text(
                    "Open Growth and upgrade Attack once.",
                    "\uC131\uC7A5\uC5D0\uC11C \uACF5\uACA9\uB825\uC744 \uD55C \uBC88 \uAC15\uD654\uD558\uC138\uC694.");
            case 3:
                return LocalizationManager.Text(
                    "Return to Battle and defeat one enemy.",
                    "\uC804\uD22C\uB85C \uB3CC\uC544\uAC00 \uACE0\uC591\uC774\uB97C \uD55C \uB9C8\uB9AC \uCC98\uCE58\uD558\uC138\uC694.");
            default:
                return LocalizationManager.Text(
                    "Tutorial complete. Keep advancing.",
                    "\uD29C\uD1A0\uB9AC\uC5BC \uC644\uB8CC. \uACC4\uC18D \uC804\uC9C4\uD558\uC138\uC694.");
        }
    }

    private BattleManager battleManager;
    private GrowthManager growthManager;
    private bool isBound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            return;

        Instance = this;
    }

    public void Initialize(
        BattleManager battle,
        GrowthManager growth)
    {
        if (Instance == null)
            Instance = this;

        battleManager = battle;
        growthManager = growth;

        if (!isBound)
        {
            growthManager.OnUpgraded += HandleUpgraded;
            battleManager.OnPowerChargePerformed += HandlePowerCharge;
            battleManager.OnEnemyDefeated += HandleEnemyDefeated;
            isBound = true;
        }

        battleManager.SetRunning(
            !ShouldShowStoryIntro &&
            (IsComplete || CurrentStep >= 1));
        NotifyTutorialChanged();
    }

    public void AdvanceStoryIntro()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            data.storyIntroCompleted ||
            data.tutorialCompleted)
        {
            return;
        }

        List<StoryIntroCut> cuts = StoryCuts;
        if (cuts.Count == 0 ||
            data.storyIntroCutIndex >= cuts.Count - 1)
        {
            CompleteStoryIntro(data);
            return;
        }

        data.storyIntroCutIndex++;
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        NotifyTutorialChanged();
        _ = SaveAsync();
    }

    public void PreviousStoryIntro()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            data.storyIntroCompleted ||
            data.tutorialCompleted)
        {
            return;
        }

        int previousIndex = Math.Max(0, data.storyIntroCutIndex - 1);
        if (previousIndex == data.storyIntroCutIndex)
            return;

        data.storyIntroCutIndex = previousIndex;
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        NotifyTutorialChanged();
        _ = SaveAsync();
    }

    public void BeginTutorial()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || data.tutorialCompleted || data.tutorialStep != 0)
            return;

        data.storyIntroCompleted = true;
        data.storyIntroCutIndex =
            Math.Max(0, StoryCuts.Count - 1);
        if (data.tutorialGachaClaimed)
        {
            data.tutorialStep = Math.Max(1, data.tutorialStep);
            battleManager?.SetRunning(true);
        }
        else
        {
            data.tutorialStep = 0;
            battleManager?.SetRunning(false);
        }

        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        NotifyTutorialChanged();
        _ = SaveAsync();
    }

    private void CompleteStoryIntro(PlayerData data)
    {
        data.storyIntroCompleted = true;
        data.storyIntroCutIndex =
            Math.Max(0, StoryCuts.Count - 1);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        NotifyTutorialChanged();
        _ = SaveAsync();
    }

    private void HandleUpgraded(UpgradeType type)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            data.tutorialCompleted ||
            data.tutorialStep != 2 ||
            type != UpgradeType.Attack)
        {
            return;
        }

        data.tutorialStep = 3;
        battleManager.SetRunning(true);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        NotifyTutorialChanged();
        _ = SaveAsync();
    }

    private void HandlePowerCharge()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            data.tutorialCompleted ||
            data.tutorialStep != 1)
        {
            return;
        }

        data.tutorialStep = 2;
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        NotifyTutorialChanged();
        _ = SaveAsync();
    }

    private void HandleEnemyDefeated(int reward)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            data.tutorialCompleted ||
            data.tutorialStep != 3)
        {
            return;
        }

        data.tutorialStep = 3;
        data.tutorialCompleted = true;
        PlayerDataManager.Instance.NotifyPlayerDataChanged();
        NotifyTutorialChanged();
        _ = SaveAsync();
    }

    public bool ClaimTutorialGachaTickets()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (!ShouldShowTutorialTicketGift || data == null)
            return false;

        data.EnsureInitialized();
        data.inventory.items["GachaTicket"] =
            GachaEconomy.GetItemCount(data, "GachaTicket") +
            TutorialGachaTicketCount;
        data.tutorialGachaTicketsGranted = true;
        data.pendingTutorialGachaResults.Clear();
        data.pendingTutorialGachaOwnedBefore.Clear();

        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        NotifyTutorialChanged();
        _ = SaveAsync();
        return true;
    }

    public bool TryCompleteTutorialGacha(int pullCount)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            pullCount != TutorialGachaTicketCount ||
            !IsWaitingForTutorialGacha)
        {
            return false;
        }

        data.EnsureInitialized();
        data.tutorialGachaClaimed = true;
        data.tutorialStep = 1;
        data.pendingTutorialGachaResults.Clear();
        data.pendingTutorialGachaOwnedBefore.Clear();

        CompanionManager.Instance?.TryEquipBestOwned(out _);
        CompanionManager.Instance?.Initialize();
        battleManager?.SetRunning(true);
        battleManager?.RefreshPlayerStats();

        NotifyTutorialChanged();
        return true;
    }

    private async Task SaveAsync()
    {
        try
        {
            PlayerData data = PlayerDataManager.Instance?.playerData;
            if (PlayerDataSaveScheduler.Instance != null)
            {
                await PlayerDataSaveScheduler.Instance.SaveNowAsync(data);
            }
            else if (FirestoreManager.Instance != null)
            {
                await FirestoreManager.Instance.SavePlayerDataAsync(data);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (!isBound)
            return;

        if (growthManager != null)
            growthManager.OnUpgraded -= HandleUpgraded;

        if (battleManager != null)
        {
            battleManager.OnPowerChargePerformed -= HandlePowerCharge;
            battleManager.OnEnemyDefeated -= HandleEnemyDefeated;
        }
    }

    private void NotifyTutorialChanged()
    {
        SafeEvent.Invoke(
            OnTutorialChanged,
            "Tutorial",
            nameof(OnTutorialChanged));
    }
}
