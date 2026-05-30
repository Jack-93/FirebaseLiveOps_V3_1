[System.Serializable]
public class PlayerData
{
    public string nickname;
    public int level;
    public int gold;
    public bool tutorialCompleted;

    public InventoryData inventory;

    public string lastLoginDate;

    // 서버, 클라이언트 둘 다 사용 (디폴트값 설정소)
    public PlayerData() 
    {
        nickname = "NewPlayer";
        level = 1;
        gold = 1000;
        tutorialCompleted = false;

        inventory = new InventoryData();

        lastLoginDate = "";
    }
}