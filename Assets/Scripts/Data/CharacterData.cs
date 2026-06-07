using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "CharacterData",
    menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public string rarity;

    public Sprite icon;
    public Sprite battleSprite;
    public RuntimeAnimatorController battleAnimator;
    public CompanionElement element;
    public CompanionRole role;
    public List<string> synergyTags = new List<string>();

    [TextArea]
    public string description;

    public string skillName = "Power Strike";
    public float skillCooldown = 8f;
    public float skillDamageMultiplier = 2f;
}
