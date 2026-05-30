using UnityEngine;
using TMPro;

public class GachaUIManager : MonoBehaviour
{
    public TMP_Text gachaResultText;

    public void RollGacha()
    {
        CharacterData result = GachaManager.Instance
            .RollCharacter();

        string rewardName = result.characterName;

        //저장
        InventoryManager.Instance.AddItem(rewardName, 1);

        //결과UI
        gachaResultText.text = $"[{result.rarity}]: {rewardName}";

        Debug.Log(
           $"[Gacha Result] {result.rarity} {rewardName}");

        SendAnalytics(result);
    }
    private void SendAnalytics(CharacterData result)
    {
        Debug.Log(
            $"[Analytics] Gacha: " +
            $"{result.characterName} / " +
            $"{result.rarity}");
    }
}
