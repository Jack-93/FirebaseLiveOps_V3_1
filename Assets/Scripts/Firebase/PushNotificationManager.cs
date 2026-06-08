using System;
using System.Threading.Tasks;
using Firebase.Messaging;
using UnityEngine;

public class PushNotificationManager : MonoBehaviour
{
    public static PushNotificationManager Instance;

    public bool IsInitialized => initialized;

    private bool initialized;

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

    public async Task InitializeAsync()
    {
        if (initialized)
            return;

        initialized = true;
        FirebaseMessaging.TokenReceived += HandleTokenReceived;
        FirebaseMessaging.MessageReceived += HandleMessageReceived;
        FirebaseMessaging.TokenRegistrationOnInitEnabled = true;

        try
        {
            await FirebaseMessaging.RequestPermissionAsync();
            string token = await FirebaseMessaging.GetTokenAsync();
            SaveToken(token);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[Messaging] Token initialization deferred: " +
                exception.Message);
        }
    }

    public string GetTokenStatus()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        string token = data?.fcmToken;
        string tokenPreview = string.IsNullOrWhiteSpace(token)
            ? "None"
            : token.Substring(0, Math.Min(12, token.Length)) + "...";

        return
            $"FCM: {(initialized ? "Initialized" : "Pending")}  " +
            $"Token: {tokenPreview}";
    }

    private void OnDestroy()
    {
        FirebaseMessaging.TokenReceived -= HandleTokenReceived;
        FirebaseMessaging.MessageReceived -= HandleMessageReceived;
    }

    private static void HandleTokenReceived(
        object sender,
        TokenReceivedEventArgs args)
    {
        SaveToken(args.Token);
    }

    private static void HandleMessageReceived(
        object sender,
        MessageReceivedEventArgs args)
    {
        Debug.Log(
            $"[Messaging] Message received: " +
            args.Message?.MessageId);
    }

    private static void SaveToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        data.fcmToken = token;
        data.fcmTokenUpdatedUnixTime =
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerDataManager.Instance.NotifyPlayerDataChanged();

        if (FirestoreManager.Instance != null)
            _ = FirestoreManager.Instance.SavePlayerDataAsync(data);

        Debug.Log("[Messaging] FCM token saved.");
    }
}
