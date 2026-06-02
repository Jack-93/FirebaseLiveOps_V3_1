using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultSlotUIManager : MonoBehaviour
{
    public Image backgroundImage;

    public Image characterIcon;

    public TMP_Text characterNameText;

    public TMP_Text rarityText;

    public void Setup(
    CharacterData character)
    {
        characterNameText.text =
            character.characterName;

        rarityText.text =
            character.rarity;

        characterIcon.sprite =
            character.icon;

        Color rarityColor =
            GetRarityColor(
                character.rarity);

        rarityText.color =
            rarityColor;

        backgroundImage.color =
            rarityColor;
    }

    private Color GetRarityColor(
    string rarity)
    {
        switch (rarity)
        {
            case "SSR":
                return new Color(
                    1f,
                    0.84f,
                    0f);

            case "SR":
                return new Color(
                    0.7f,
                    0.4f,
                    1f);

            default:
                return Color.gray;
        }
    }
}
