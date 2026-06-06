using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public class FirestoreManager : MonoBehaviour
{
    public static FirestoreManager Instance;

    private readonly SemaphoreSlim saveLock = new SemaphoreSlim(1, 1);
    private FirebaseFirestore db;

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

    public async Task<PlayerData> LoadPlayerDataAsync()
    {
        FirebaseUser user = await GetCurrentUserAsync();
        if (user == null)
            return null;

        DocumentReference docRef =
            db.Collection("users").Document(user.UserId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            PlayerData newData = new PlayerData
            {
                uid = user.UserId
            };

            await SavePlayerDataAsync(newData);
            Debug.Log("[Firestore] New User Data Created");
            return newData;
        }

        PlayerData data =
            PlayerDataConverter.FromDictionary(snapshot.ToDictionary());
        data.uid = user.UserId;

        Debug.Log("[Firestore] Data Loaded");
        return data;
    }

    public async Task SavePlayerDataAsync(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogError("[Firestore] Cannot save null PlayerData.");
            return;
        }

        FirebaseUser user = await GetCurrentUserAsync();
        if (user == null)
            return;

        await saveLock.WaitAsync();

        try
        {
            data.uid = user.UserId;

            DocumentReference docRef =
                db.Collection("users").Document(user.UserId);
            Dictionary<string, object> values =
                PlayerDataConverter.ToDictionary(data);

            await docRef.SetAsync(values);
            Debug.Log("[Firestore] Save Success");
        }
        finally
        {
            saveLock.Release();
        }
    }

    public async void SavePlayerData(PlayerData data)
    {
        try
        {
            await SavePlayerDataAsync(data);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async Task UpdateNicknameAsync(string nickname)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        data.nickname = nickname;
        await SavePlayerDataAsync(data);
    }

    public async void UpdateNickname(string nickname)
    {
        try
        {
            await UpdateNicknameAsync(nickname);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async Task AddGoldAsync(int amount)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        data.gold += amount;
        await SavePlayerDataAsync(data);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();

        Debug.Log($"[Firestore] Gold Updated: {data.gold}");
    }

    public async void AddGold(int amount)
    {
        try
        {
            await AddGoldAsync(amount);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async Task<int> LoadGlobalMailsAsync()
    {
        await EnsureDatabaseAsync();

        PlayerData playerData = PlayerDataManager.Instance?.playerData;
        if (playerData == null)
        {
            Debug.LogError("[Mailbox] PlayerData is not ready.");
            return 0;
        }

        playerData.EnsureInitialized();

        QuerySnapshot snapshot =
            await db.Collection("global_mails").GetSnapshotAsync();
        int addedCount = 0;

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            Dictionary<string, object> values = document.ToDictionary();

            if (!GetBoolean(values, "isActive"))
                continue;

            if (HasMail(playerData, document.Id) ||
                playerData.claimedMailIds.Contains(document.Id))
            {
                continue;
            }

            if (!values.TryGetValue("title", out object title) ||
                !values.TryGetValue("itemName", out object itemName) ||
                !values.TryGetValue("amount", out object amount))
            {
                Debug.LogWarning(
                    $"[Mailbox] Global mail {document.Id} is missing fields.");
                continue;
            }

            playerData.mailbox.Add(new MailData
            {
                mailId = document.Id,
                title = title.ToString(),
                itemName = itemName.ToString(),
                amount = Convert.ToInt32(amount),
                isClaimed = false
            });

            addedCount++;
        }

        Debug.Log($"[Mailbox] Global mails loaded: {addedCount}");
        return addedCount;
    }

    public async void LoadGlobalMails()
    {
        try
        {
            int addedCount = await LoadGlobalMailsAsync();

            if (addedCount > 0)
            {
                await SavePlayerDataAsync(
                    PlayerDataManager.Instance.playerData);
                PlayerDataManager.Instance.NotifyPlayerDataChanged();
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private async Task<FirebaseUser> GetCurrentUserAsync()
    {
        await EnsureDatabaseAsync();

        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
            Debug.LogError("[Firestore] No authenticated user.");

        return user;
    }

    private async Task EnsureDatabaseAsync()
    {
        await FirebaseManager.WaitUntilReadyAsync();

        if (db == null)
            db = FirebaseFirestore.DefaultInstance;
    }

    private static bool HasMail(PlayerData data, string mailId)
    {
        foreach (MailData mail in data.mailbox)
        {
            if (mail != null && mail.mailId == mailId)
                return true;
        }

        return false;
    }

    private static bool GetBoolean(
        Dictionary<string, object> values,
        string key)
    {
        return values.TryGetValue(key, out object value) &&
            value != null &&
            Convert.ToBoolean(value);
    }
}
