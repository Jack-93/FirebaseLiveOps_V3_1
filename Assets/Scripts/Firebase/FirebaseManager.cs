using Firebase;
using Firebase.RemoteConfig;
using System.Threading.Tasks;
using System;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static bool IsFirebaseReady;

    async void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status == DependencyStatus.Available)
        {
            IsFirebaseReady = true;

            Debug.Log("[Firebase] Ready");

            await InitializeRemoteConfig();
        }
        else
        {
            Debug.LogError($"[Firebase] Error: {status}");
        }
    }

    private async Task InitializeRemoteConfig()
    {
        await FirebaseRemoteConfig.DefaultInstance
            .FetchAsync(TimeSpan.Zero);

        await FirebaseRemoteConfig.DefaultInstance
            .ActivateAsync();

        Debug.Log(
            "[RemoteConfig] Loaded");

        GachaConfig.SSRRate =
            (int)FirebaseRemoteConfig
            .DefaultInstance.GetValue("ssr_rate").LongValue;

        GachaConfig.SRRate =
            (int)FirebaseRemoteConfig
            .DefaultInstance.GetValue("sr_rate").LongValue;

        Debug.Log(
            $"[RemoteConfig] SSR = {GachaConfig.SSRRate}%");

    }


}
