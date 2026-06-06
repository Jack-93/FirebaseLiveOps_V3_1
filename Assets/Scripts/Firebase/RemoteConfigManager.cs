using System;
using Firebase.RemoteConfig;
using UnityEngine;

public class RemoteConfigManager : MonoBehaviour
{
    private async void Start()
    {
        try
        {
            await FirebaseManager.WaitUntilReadyAsync();
            LogCurrentValues();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static void LogCurrentValues()
    {
        FirebaseRemoteConfig remoteConfig =
            FirebaseRemoteConfig.DefaultInstance;

        bool eventEnabled =
            remoteConfig.GetValue("nikke_gacha_event").BooleanValue;
        string bannerName =
            remoteConfig.GetValue("current_banner").StringValue;
        long discount =
            remoteConfig.GetValue("starter_pack_discount").LongValue;

        Debug.Log($"[RemoteConfig] Event Enabled: {eventEnabled}");
        Debug.Log($"[RemoteConfig] Current Banner: {bannerName}");
        Debug.Log($"[RemoteConfig] Starter Pack Discount: {discount}%");
    }
}
