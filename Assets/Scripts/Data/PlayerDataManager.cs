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

    public void NotifyPlayerDataChanged(bool requestSave = false)
    {
        if (requestSave)
            PlayerDataSaveScheduler.Instance?.RequestSave(playerData);

        InvokePlayerDataChanged();
    }

    private void InvokePlayerDataChanged()
    {
        SafeEvent.Invoke(
            OnPlayerDataChanged,
            "PlayerData",
            nameof(OnPlayerDataChanged));
    }
}
