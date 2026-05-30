
using Firebase.Analytics;
using UnityEngine;

public class FunnelAnalyticsManager : MonoBehaviour
{
    public void EnterLobby()
    {
        FirebaseAnalytics.LogEvent("enter_lobby");
    }

    public void StartTutorial()
    {
        FirebaseAnalytics.LogEvent("tutorial_start");
    }

    public void CompleteTutorial()
    {
        FirebaseAnalytics.LogEvent("tutorial_complete");
    }

    public void EnterBattle()
    {
        FirebaseAnalytics.LogEvent("battle_enter");
    }

    public void ClearBattle()
    {
        FirebaseAnalytics.LogEvent("battle_clear");
    }

    public void EnterShop()
    {
        FirebaseAnalytics.LogEvent("shop_enter");
    }

    public void TryFirstPurchase()
    {
        FirebaseAnalytics.LogEvent("first_purchase_try");
    }
}
