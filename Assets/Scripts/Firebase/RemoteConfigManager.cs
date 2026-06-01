using System;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using UnityEngine;

public class RemoteConfigManager : MonoBehaviour
{
    async void Start()
    {
        await FetchRemoteConfig();
    }

    public async System.Threading.Tasks.Task FetchRemoteConfig()
    {
        var defaults = new Dictionary<string, object>
        {
            { "nikke_gacha_event", false },
            { "current_banner", "default_banner" },
            { "starter_pack_discount", 0 }
        };

        await FirebaseRemoteConfig.DefaultInstance
            .SetDefaultsAsync(defaults);

        await FirebaseRemoteConfig.DefaultInstance
            .FetchAsync(TimeSpan.Zero);

        await FirebaseRemoteConfig.DefaultInstance
            .ActivateAsync();

        bool eventEnabled =
            FirebaseRemoteConfig.DefaultInstance
            .GetValue("nikke_gacha_event")
            .BooleanValue;

        string bannerName =
            FirebaseRemoteConfig.DefaultInstance
            .GetValue("current_banner")
            .StringValue;

        long discount =
            FirebaseRemoteConfig.DefaultInstance
            .GetValue("starter_pack_discount")
            .LongValue;

        Debug.Log($"Event Enabled: {eventEnabled}");
        Debug.Log($"Current Banner: {bannerName}");
        Debug.Log($"Starter Pack Discount: {discount}%");

        if(eventEnabled)
        {
            Debug.Log("Show Limited Banner");
        }
        else
        {
            Debug.Log("Hide Limited Banner");
        }
    }
}
