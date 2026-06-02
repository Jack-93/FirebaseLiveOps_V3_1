using Firebase.Analytics;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class GachaUIManager : MonoBehaviour
{
    public TMP_Text gachaResultText;

    public Transform content;
    public GameObject resultSlotPrefab;

    public void SingleRoll()
    {
        ClearSlots();

        CharacterData result = GachaManager.Instance
            .RollCharacterWithPity();

        // string rewardName = result.characterName;

        if (result.rarity == "SSR")
        {
            AnalyticsManager.Instance
                .LogSSR(result);
        }

        //저장
        InventoryManager.Instance
            .AddItem(result.characterName, 1);
        AnalyticsManager.Instance
            .LogGachaRoll(result);

        ////결과UI
        //gachaResultText.text = $"[{result.rarity}]: {result.characterName}";

        //Debug.Log(
        //   $"[Gacha Result] {result.rarity} {result.characterName}");

        if (result.rarity == "SSR")
        {
            AnalyticsManager.Instance
                .LogSSR(
                    result);
        }

        CreateSlot(result);

        // SendAnalytics(result);
    }

    public void TenRoll()
    {
        ClearSlots();

        List<CharacterData> results =
            GachaManager.Instance
            .RollTen();

        //StringBuilder sb =
        //    new StringBuilder();

        //sb.AppendLine("===== 10 Roll =====");

        foreach (CharacterData result in results)
        {
            // 저장
            InventoryManager.Instance
                .AddItem(result.characterName, 1);
            AnalyticsManager.Instance
                .LogGachaRoll(
                    result);


            if (result.rarity == "SSR")
            {
                AnalyticsManager.Instance
                    .LogSSR(
                        result);
            }
        }

        foreach (CharacterData result in results)
        {
            CreateSlot(result);
        }

        //StringBuilder sb =
        //    new StringBuilder();

        //sb.AppendLine("===== 10 Roll =====");
        //sb.AppendLine(
        //    $"[{result.rarity}] {result.characterName}");
        //gachaResultText.text = sb.ToString();
    }

    private void CreateSlot(CharacterData result)
    {
        GameObject slot = Instantiate(
            resultSlotPrefab,
            content,
            false);

        slot.GetComponent<ResultSlotUIManager>().Setup(
            result);
    }

    private void ClearSlots()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }
    /*
    private void SendAnalytics(CharacterData result)
    {
        
        Debug.Log(
            $"[Analytics] Gacha: " +
            $"{result.characterName} / " +
            $"{result.rarity}");
        

        FirebaseAnalytics.LogEvent(
            $"[Analytics] gacha_roll: ",
            result.characterName,
            result.rarity);
    }
    */
}
