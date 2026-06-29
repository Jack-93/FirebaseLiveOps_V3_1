using System;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerDataSaveScheduler : MonoBehaviour
{
    public static PlayerDataSaveScheduler Instance;

    private const float RemoteDebounceSeconds = 5f;
    private const float BusyRetrySeconds = 1f;

    private PlayerData latestData;
    private bool hasPendingRemoteSave;
    private bool isSaving;
    private float remoteSaveTimer;
    private int saveVersion;

    public bool HasPendingRemoteSave => hasPendingRemoteSave || isSaving;

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

    private void Update()
    {
        if (!hasPendingRemoteSave || isSaving)
            return;

        remoteSaveTimer -= Time.unscaledDeltaTime;
        if (remoteSaveTimer <= 0f)
            _ = SavePendingRemoteAsync();
    }

    public void RequestSave(PlayerData data)
    {
        if (!CanSave(data))
            return;

        latestData = data;
        StampOnlineTime(data);
        PlayerDataLocalCache.Save(data);

        saveVersion++;
        hasPendingRemoteSave = true;
        remoteSaveTimer = RemoteDebounceSeconds;
    }

    public async Task SaveNowAsync(PlayerData data = null)
    {
        PlayerData target =
            data ??
            latestData ??
            PlayerDataManager.Instance?.playerData;

        if (!CanSave(target))
            return;

        latestData = target;
        StampOnlineTime(target);
        PlayerDataLocalCache.Save(target);

        saveVersion++;
        hasPendingRemoteSave = true;
        remoteSaveTimer = 0f;
        await SaveRemoteSafelyAsync(target, saveVersion);
    }

    public async Task FlushPendingSaveAsync()
    {
        PlayerData target =
            latestData ??
            PlayerDataManager.Instance?.playerData;

        if (!CanSave(target))
            return;

        latestData = target;
        StampOnlineTime(target);
        PlayerDataLocalCache.Save(target);
        saveVersion++;
        hasPendingRemoteSave = true;
        remoteSaveTimer = 0f;

        await SaveRemoteSafelyAsync(target, saveVersion);
    }

    private async Task SavePendingRemoteAsync()
    {
        PlayerData target = latestData;
        int targetVersion = saveVersion;
        remoteSaveTimer = 0f;
        await SaveRemoteSafelyAsync(target, targetVersion);
    }

    private async Task SaveRemoteSafelyAsync(
        PlayerData data,
        int targetVersion)
    {
        if (!CanSave(data))
            return;

        if (FirestoreManager.Instance == null)
        {
            hasPendingRemoteSave = true;
            remoteSaveTimer = RemoteDebounceSeconds;
            return;
        }

        if (isSaving)
        {
            hasPendingRemoteSave = true;
            remoteSaveTimer = BusyRetrySeconds;
            return;
        }

        isSaving = true;
        try
        {
            await FirestoreManager.Instance.SavePlayerDataAsync(data);
            if (targetVersion == saveVersion)
            {
                hasPendingRemoteSave = false;
                remoteSaveTimer = 0f;
            }
            else
            {
                hasPendingRemoteSave = true;
                remoteSaveTimer = BusyRetrySeconds;
            }
        }
        catch (Exception exception)
        {
            hasPendingRemoteSave = true;
            remoteSaveTimer = RemoteDebounceSeconds;
            Debug.LogWarning(
                "[PlayerDataSaveScheduler] Remote save deferred: " +
                exception.Message);
        }
        finally
        {
            isSaving = false;
        }
    }

    private static bool CanSave(PlayerData data)
    {
        return data != null &&
            !string.IsNullOrWhiteSpace(data.uid);
    }

    private static void StampOnlineTime(PlayerData data)
    {
        data.lastOnlineUnixTime =
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
