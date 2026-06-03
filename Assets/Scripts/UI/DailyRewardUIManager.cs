using TMPro;
using UnityEngine;

public class DailyRewardUIManager : MonoBehaviour
{
    public TMP_Text dayText;

    public void RefreshUI()
    {
        PlayerData player =
            PlayerDataManager.Instance.playerData;

        dayText.text =
            $"Day {player.loginDay + 1}";
    }

    public void ClaimReward()
    {
        DailyRewardManager.Instance
            .ClaimReward();

        RefreshUI();
    }
}