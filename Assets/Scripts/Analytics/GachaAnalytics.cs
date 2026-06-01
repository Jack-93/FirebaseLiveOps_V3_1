using Firebase.Analytics;
using UnityEngine;

public class GachaAnalytics : MonoBehaviour
{
    public void OnLobbyEnter()
    {
        FirebaseAnalytics.LogEvent("lobby_enter");
    }

    public void OnGachaOpen()
    {
        FirebaseAnalytics.LogEvent("gacha_open");
    }

    public void OnGachaButtonClick()
    {
        FirebaseAnalytics.LogEvent("gacha_button_click");
    }

    public void OnGachaPull()
    {
        FirebaseAnalytics.LogEvent(
            "gacha_pull",
            new Parameter("banner_type", "limited")
        );
    }

    public void OnPackagePopup()
    {
        FirebaseAnalytics.LogEvent("package_popup");
    }

    public void OnPackageBuyTry()
    {
        FirebaseAnalytics.LogEvent("package_buy_try");
    }
}
