using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public PlayerData playerData;
    //public PlayerData PlayerData2;

    private void Awake()
    {
        Instance = this;

        playerData = new PlayerData();
        // PlayerData2 = new PlayerData();
    }
}