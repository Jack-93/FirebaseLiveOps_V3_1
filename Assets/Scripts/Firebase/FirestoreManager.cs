using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using System.Collections.Generic;
using System;

public class FirestoreManager : MonoBehaviour
{
    public static FirestoreManager Instance;

    private FirebaseFirestore db;

    private void Awake()
    {
        Instance = this;

        db = FirebaseFirestore.DefaultInstance;

        DontDestroyOnLoad(gameObject);
    }

    public async void SavePlayerData(PlayerData data)
    {
        FirebaseUser user =
            FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No User");
            return;
        }
        // User Id 데이터 저장
        DocumentReference docRef =
            db.Collection("users")
            .Document(user.UserId); 

        Dictionary<string, object> playerData =
            new Dictionary<string, object>()
        {
            { "nickname", data.nickname },
            { "level", data.level },
            { "gold", data.gold },
            { "tutorialCompleted", data.tutorialCompleted },
            { "inventory", data.inventory.items },
            { "lastLoginDate", data.lastLoginDate }
        };

        await docRef.SetAsync(playerData);

        Debug.Log("[Firestore] Save Success");
    }

    // 유저 닉네임 저장
    public async void UpdateNickname(string nickname)
    {
        FirebaseUser user =
            FirebaseAuth.DefaultInstance.CurrentUser;

        DocumentReference docRef =
            db.Collection("users")
            .Document(user.UserId);

        await docRef.UpdateAsync("nickname", nickname);

        Debug.Log("[Firestore] Nickname Updated");
    }

    // 유저 골드 저장
    public async void AddGold(int amount)
    {
        FirebaseUser user =
            FirebaseAuth.DefaultInstance.CurrentUser;

        PlayerDataManager.Instance.playerData.gold += amount;

        int newGold =
            PlayerDataManager.Instance.playerData.gold;

        DocumentReference docRef =
            db.Collection("users")
            .Document(user.UserId);

        await docRef.UpdateAsync("gold", newGold);

        Debug.Log($"[Firestore] Gold Updated: {newGold}");
    }

    public async void LoadPlayerData()
    {
        FirebaseUser user =
            FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No User");
            return;
        }

        DocumentReference docRef =
            db.Collection("users")
            .Document(user.UserId);

        DocumentSnapshot snapshot =
            await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            Debug.Log("[Firestore] Data Loaded");

            Debug.Log(snapshot.ToDictionary()["nickname"]);
        }
        else
        {
            Debug.Log("[Firestore] New User Data");

            PlayerData newPlayer =
                new PlayerData();

            SavePlayerData(newPlayer);
        }
    }

    internal void CheckDailyReward()
    {
        throw new NotImplementedException();
    }
}