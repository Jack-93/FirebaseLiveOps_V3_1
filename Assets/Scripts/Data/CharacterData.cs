using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterData",
    menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public string rarity;

    public Sprite icon;

    [TextArea]
    public string description;
}