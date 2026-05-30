using UnityEngine;

public class DailyLoginReward : MonoBehaviour
{
    public int currentDay = 1;

    public void ClaimDailyReward()
    {
        Debug.Log($"Claim Daily Reward Day: {currentDay}");

        currentDay++;
    }
}
