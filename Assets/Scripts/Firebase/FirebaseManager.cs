using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.RemoteConfig;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    public static bool IsFirebaseReady { get; private set; }

    private static Task initializationTask;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        initializationTask = InitializeFirebaseAsync();
    }

    public static async Task WaitUntilReadyAsync()
    {
        while (initializationTask == null)
            await Task.Yield();

        await initializationTask;
    }

    private async Task InitializeFirebaseAsync()
    {
        DependencyStatus status =
            await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"[Firebase] Error: {status}");
            throw new InvalidOperationException(
                $"Firebase dependencies are unavailable: {status}");
        }

        IsFirebaseReady = true;
        Debug.Log("[Firebase] Ready");

        await InitializeRemoteConfigAsync();
    }

    private static async Task InitializeRemoteConfigAsync()
    {
        FirebaseRemoteConfig remoteConfig =
            FirebaseRemoteConfig.DefaultInstance;

        await remoteConfig.SetDefaultsAsync(
            new Dictionary<string, object>
            {
                { "ssr_rate", 3L },
                { "sr_rate", 17L },
                { "nikke_gacha_event", false },
                { "current_banner", "default_banner" },
                { "starter_pack_discount", 0L }
            });

        try
        {
            await remoteConfig.FetchAsync(TimeSpan.Zero);
            await remoteConfig.ActivateAsync();
            Debug.Log("[RemoteConfig] Loaded");
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[RemoteConfig] Fetch failed. Defaults will be used: " +
                exception.Message);
        }

        GachaConfig.SSRRate =
            Mathf.Clamp((int)remoteConfig.GetValue("ssr_rate").LongValue, 0, 100);
        GachaConfig.SRRate =
            Mathf.Clamp((int)remoteConfig.GetValue("sr_rate").LongValue, 0, 100);

        if (GachaConfig.SSRRate + GachaConfig.SRRate > 100)
            GachaConfig.SRRate = 100 - GachaConfig.SSRRate;

        Debug.Log(
            $"[RemoteConfig] SSR={GachaConfig.SSRRate}% " +
            $"SR={GachaConfig.SRRate}%");
    }
}
