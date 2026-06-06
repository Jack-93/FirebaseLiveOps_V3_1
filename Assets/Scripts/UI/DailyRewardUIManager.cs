using System;
using TMPro;
using UnityEngine;

public class DailyRewardUIManager : MonoBehaviour
{
    public TMP_Text dayText;

    private void Start()
    {
        RefreshUI();

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataChanged += RefreshUI;
        }
    }

    public void RefreshUI()
    {
        if (dayText == null || DailyRewardManager.Instance == null)
            return;

        int day = DailyRewardManager.Instance.GetNextRewardDay();
        string state = DailyRewardManager.Instance.CanClaimReward()
            ? "Available"
            : "Claimed";

        dayText.text = $"Claim Day {day} ({state})";
    }

    public async void ClaimReward()
    {
        try
        {
            await DailyRewardManager.Instance.ClaimRewardAsync();
            RefreshUI();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataChanged -= RefreshUI;
        }
    }
}
