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

    public string skillName = "Power Strike";
    public float skillCooldown = 8f;
    public float skillDamageMultiplier = 2f;
}
