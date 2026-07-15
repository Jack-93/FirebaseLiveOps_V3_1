using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

[CreateAssetMenu(
    fileName = "CharacterData",
    menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public string rarity;

    public Sprite icon;
    public Sprite battleSprite;
    public Sprite basicProjectileSprite;
    public Sprite skillProjectileSprite;
    public Color basicProjectileTint = Color.white;
    public Color skillProjectileTint = Color.white;
    public RuntimeAnimatorController battleAnimator;
    public BattleActorVisualSet battleVisual =
        new BattleActorVisualSet();
    public CompanionElement element;
    public CompanionRole role;
    public List<string> synergyTags = new List<string>();

    [TextArea]
    public string description;

    public string skillName = "Power Strike";
    public float skillCooldown = 4f;
    public CompanionSkillEffect skillEffect =
        CompanionSkillEffect.DamageEnemy;
    public float basicAttackMultiplier = 1f;
    public float skillDamageMultiplier = 3f;
    public int skillHitCount = 1;
    public float skillDamageBuffPercent;
    public float skillDamageBuffDuration = 10f;
    [FormerlySerializedAs("skillHealPercent")]
    [FormerlySerializedAs("lineRepairPercent")]
    public float poleRepairPercent = 0.35f;
    [FormerlySerializedAs("healthPercentBonus")]
    [FormerlySerializedAs("lineDefensePercentBonus")]
    public float poleDurabilityPercentBonus;

    public BattleActorVisualSet ResolveBattleVisual()
    {
        if (BattleActorVisualSet.IsConfigured(battleVisual))
            return battleVisual;

        return BattleActorVisualSet.FromLegacy(
            battleSprite ?? icon,
            battleAnimator,
            basicProjectileSprite,
            skillProjectileSprite,
            basicProjectileTint,
            skillProjectileTint);
    }

    public Sprite ResolveBattleSprite()
    {
        return ResolveBattleVisual()?.sprite ?? battleSprite ?? icon;
    }

    public Sprite ResolveGachaSprite()
    {
        Sprite idleSprite = ResolveAnimationSprite(BattleAnimationCue.Idle);
        return idleSprite ?? ResolvePortraitSprite();
    }

    public Sprite ResolvePortraitSprite()
    {
        return icon ?? ResolveBattleSprite();
    }

    private Sprite ResolveAnimationSprite(BattleAnimationCue cue)
    {
        BattleActorVisualSet visual = ResolveBattleVisual();
        if (visual?.spriteAnimations == null)
            return null;

        foreach (BattleSpriteAnimation animation in visual.spriteAnimations)
        {
            if (animation == null ||
                animation.cue != cue ||
                animation.frames == null ||
                animation.frames.Length == 0)
            {
                continue;
            }

            return animation.frames[0];
        }

        return null;
    }
}
