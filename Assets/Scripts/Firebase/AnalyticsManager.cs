using UnityEngine;
using Firebase.Analytics;

/* Analytics로 전달됐는지 확인하기 위한 Manager 클래스*/
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;
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

    public void LogLogin()
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
        Debug.Log("[Analytics] Logged Login Event");
    }

    public void LogDailyReward()
    {
        FirebaseAnalytics.LogEvent("daily_reward_collected");
        Debug.Log("[Analytics] Logged Daily Reward Collected Event");
    }

    public void LogGachaRoll(string characterName, string rarity)
    {
        FirebaseAnalytics.LogEvent("gacha_roll", "rarity", rarity);
        Debug.Log($"[Analytics] Logged Gacha Roll Event with Rarity: {rarity}");
    }

    public void LogSSR(string characterName)
    {
        FirebaseAnalytics.LogEvent(
            "gacha_ssr",
            "character",
            characterName);

        Debug.Log(
            $"[Analytics] SSR!!! {characterName}");
    }

}
