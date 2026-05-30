using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public PlayerData playerData;
    //public PlayerData PlayerData2;

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
        // PlayerData2 = new PlayerData();

        
    }
}