using TMPro;
using UnityEngine;

public class ResultSlotUI : MonoBehaviour
{
    public TMP_Text resultText;

    public void Setup(string rarity, string characterName)
    {
        resultText.text =
            $"[{rarity}] {characterName}";
    }
}