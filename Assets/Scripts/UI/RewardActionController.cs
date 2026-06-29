using System;
using UnityEngine;

public sealed class RewardActionController
{
    private readonly Action<string> showToast;
    private readonly Action refreshTopBar;
    private readonly Action refreshMore;
    private readonly Action refreshQuests;
    private readonly Action refreshEvent;

    public RewardActionController(
        Action<string> showToast,
        Action refreshTopBar,
        Action refreshMore,
        Action refreshQuests,
        Action refreshEvent)
    {
        this.showToast = showToast;
        this.refreshTopBar = refreshTopBar;
        this.refreshMore = refreshMore;
        this.refreshQuests = refreshQuests;
        this.refreshEvent = refreshEvent;
    }

    public async void ClaimDailyReward()
    {
        try
        {
            if (DailyRewardManager.Instance == null)
                return;

            bool claimed =
                await DailyRewardManager.Instance.ClaimRewardAsync();
            showToast?.Invoke(claimed
                ? "Daily reward collected."
                : "Daily reward is not available.");
            refreshMore?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            showToast?.Invoke("Reward claim failed.");
        }
    }

    public async void ClaimAllMail()
    {
        try
        {
            if (MailboxManager.Instance == null)
                return;

            int claimed =
                await MailboxManager.Instance.ClaimAllMailsAsync();
            showToast?.Invoke(claimed > 0
                ? $"{claimed} mail reward(s) collected."
                : "No mail rewards available.");
            refreshMore?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            showToast?.Invoke("Mail claim failed.");
        }
    }

    public async void ClaimCurrentQuest()
    {
        try
        {
            bool claimed = QuestManager.Instance != null &&
                await QuestManager.Instance.ClaimCurrentQuestAsync();
            showToast?.Invoke(claimed
                ? "Quest reward collected. Next quest started."
                : "Current quest is not complete.");
            refreshQuests?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            showToast?.Invoke("Quest reward claim failed.");
        }
    }

    public async void ClaimAchievements()
    {
        try
        {
            int claimed = QuestManager.Instance == null
                ? 0
                : await QuestManager.Instance
                    .ClaimAvailableAchievementsAsync();
            showToast?.Invoke(claimed > 0
                ? $"{claimed} achievement reward(s) collected."
                : "No achievement rewards available.");
            refreshQuests?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            showToast?.Invoke("Achievement reward claim failed.");
        }
    }

    public async void ClaimEventReward()
    {
        try
        {
            bool claimed = EventMissionManager.Instance != null &&
                await EventMissionManager.Instance.ClaimRewardAsync();
            showToast?.Invoke(claimed
                ? "Event reward collected."
                : "Event missions are not complete.");
            refreshTopBar?.Invoke();
            refreshMore?.Invoke();
            refreshEvent?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            showToast?.Invoke("Event reward claim failed.");
        }
    }
}
