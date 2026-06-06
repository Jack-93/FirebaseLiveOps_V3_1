using System.Collections.Generic;

/*
 * 플레이어 데이터 클래스
 * 닉네임, 레벨, 골드, 튜토리얼 완료 여부, 인벤토리, 일일 보상 관련 데이터, 우편함 등
 *  Player Data -> Firestore -> JSON
 */
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

    public List<MailData> mailbox;
    public List<string> claimedMailIds;

    // Daily Reward 관련 데이터
    public string lastLoginDate;
    public string lastRewardDate; // Firestore에 저장할 때는 DateTime을 문자열로 변환, 저장
    public int loginDay;

    public int currentStage;
    public int highestStage;
    public int stageEnemyIndex;
    public int attackLevel;
    public int healthLevel;
    public int attackSpeedLevel;
    public int tutorialStep;
    public int totalMonstersDefeated;
    public long lastOnlineUnixTime;

    // 서버, 클라이언트 둘 다 사용 (디폴트값 설정소)
    public PlayerData()
    {
        nickname = "NewPlayer";
        level = 1;
        gold = 1000;
        tutorialCompleted = false;

        inventory = new InventoryData();

        lastLoginDate = "";
        lastRewardDate = "";
        loginDay = 0;

        pityCount = 0;

        mailbox = new List<MailData>();
        claimedMailIds = new List<string>();

        currentStage = 1;
        highestStage = 1;
        stageEnemyIndex = 0;
        attackLevel = 1;
        healthLevel = 1;
        attackSpeedLevel = 1;
        tutorialStep = 0;
        totalMonstersDefeated = 0;
        lastOnlineUnixTime = 0;
    }

    public void EnsureInitialized()
    {
        if (inventory == null)
            inventory = new InventoryData();

        if (inventory.items == null)
            inventory.items = new Dictionary<string, int>();

        if (mailbox == null)
            mailbox = new List<MailData>();

        if (claimedMailIds == null)
            claimedMailIds = new List<string>();

        if (nickname == null)
            nickname = "NewPlayer";

        if (lastLoginDate == null)
            lastLoginDate = "";

        if (lastRewardDate == null)
            lastRewardDate = "";

        currentStage = System.Math.Max(1, currentStage);
        highestStage = System.Math.Max(currentStage, highestStage);
        stageEnemyIndex = System.Math.Max(
            0,
            System.Math.Min(4, stageEnemyIndex));
        attackLevel = System.Math.Max(1, attackLevel);
        healthLevel = System.Math.Max(1, healthLevel);
        attackSpeedLevel = System.Math.Max(1, attackSpeedLevel);
        tutorialStep = System.Math.Max(0, tutorialStep);
        totalMonstersDefeated =
            System.Math.Max(0, totalMonstersDefeated);
        lastOnlineUnixTime =
            System.Math.Max(0, lastOnlineUnixTime);
    }
}
