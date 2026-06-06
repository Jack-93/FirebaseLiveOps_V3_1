using System;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public PlayerData playerData;
    public event Action OnPlayerDataChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerData = new PlayerData();
    }

    public void SetPlayerData(PlayerData data)
    {
        playerData = data ?? new PlayerData();
        playerData.EnsureInitialized();

        NotifyPlayerDataChanged();
        Debug.Log("[PlayerData] Data Set");
    }

    public void NotifyPlayerDataChanged()
    {
        OnPlayerDataChanged?.Invoke();
    }
}
