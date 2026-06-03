

[System.Serializable]
public class PlayerData
{
    public string nickname;
    public int level;
    public int gold;
    public bool tutorialCompleted;

    public InventoryData inventory;

    public int pityCount;

    public string uid;

    // Daily Reward 관련 데이터
    public string lastLoginDate;
    public string lastRewardDate; // Firestore에 저장할 때는 DateTime을 문자열로 변환, 저장
    public int loginDay;

    // 서버, 클라이언트 둘 다 사용 (디폴트값 설정소)
    public PlayerData()
    {
        nickname = "NewPlayer";
        level = 1;
        gold = 1000;
        tutorialCompleted = false;

        inventory = new InventoryData();

        lastLoginDate = "";

        pityCount = 0;
    }
}