/* 
 * mail data class
 * 제목 : 보상
 * 내용 : 아이템 이름, 수량
 * 수령 여부 T/F
 * 
 */
[System.Serializable]
public class MailData
{
    public string mailId;

    public string title;

    public string itemName;

    public int amount;

    public bool isClaimed;
}