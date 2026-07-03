using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BattleProjectileVisual
{
    public Sprite sprite;
    public Color tint = Color.white;
    public float duration;
    public float size;

    public bool HasSprite => sprite != null;

    public Color ResolveTint(Color fallback)
    {
        return tint.a <= 0.01f ? fallback : tint;
    }

    public float ResolveDuration(float fallback)
    {
        return duration > 0f ? duration : fallback;
    }

    public float ResolveSize(float fallback)
    {
        return size > 0f ? size : fallback;
    }
}

[Serializable]
public sealed class BattleSpriteAnimation
{
    public BattleAnimationCue cue = BattleAnimationCue.Idle;
    public Sprite[] frames;
}

[Serializable]
public sealed class BattleActorVisualSet
{
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;
    public List<BattleSpriteAnimation> spriteAnimations =
        new List<BattleSpriteAnimation>();
    public BattleProjectileVisual basicProjectile =
        new BattleProjectileVisual();
    public BattleProjectileVisual skillProjectile =
        new BattleProjectileVisual();

    public bool HasActorVisual =>
        sprite != null ||
        animatorController != null ||
        HasSpriteAnimations();

    public bool HasAnyVisual =>
        HasActorVisual ||
        (basicProjectile != null && basicProjectile.HasSprite) ||
        (skillProjectile != null && skillProjectile.HasSprite);

    public Dictionary<BattleAnimationCue, Sprite[]> CreateAnimationLookup()
    {
        if (spriteAnimations == null || spriteAnimations.Count == 0)
            return null;

        Dictionary<BattleAnimationCue, Sprite[]> lookup =
            new Dictionary<BattleAnimationCue, Sprite[]>();
        foreach (BattleSpriteAnimation animation in spriteAnimations)
        {
            if (animation?.frames == null || animation.frames.Length == 0)
                continue;

            lookup[animation.cue] = animation.frames;
        }

        return lookup.Count == 0 ? null : lookup;
    }

    public BattleProjectileVisual GetProjectile(bool skill)
    {
        if (skill && skillProjectile != null && skillProjectile.HasSprite)
            return skillProjectile;

        if (basicProjectile != null && basicProjectile.HasSprite)
            return basicProjectile;

        return skill ? skillProjectile : basicProjectile;
    }

    public static BattleActorVisualSet FromLegacy(
        Sprite sprite,
        RuntimeAnimatorController animatorController,
        Sprite basicProjectileSprite = null,
        Sprite skillProjectileSprite = null,
        Color? basicProjectileTint = null,
        Color? skillProjectileTint = null)
    {
        return new BattleActorVisualSet
        {
            sprite = sprite,
            animatorController = animatorController,
            basicProjectile = new BattleProjectileVisual
            {
                sprite = basicProjectileSprite,
                tint = basicProjectileTint ?? Color.white
            },
            skillProjectile = new BattleProjectileVisual
            {
                sprite = skillProjectileSprite,
                tint = skillProjectileTint ?? Color.white
            }
        };
    }

    public static BattleActorVisualSet FromPrototype(
        Sprite sprite,
        Dictionary<BattleAnimationCue, Sprite[]> animations)
    {
        BattleActorVisualSet set = FromLegacy(sprite, null);
        if (animations == null || animations.Count == 0)
            return set;

        foreach (KeyValuePair<BattleAnimationCue, Sprite[]> pair in animations)
        {
            if (pair.Value == null || pair.Value.Length == 0)
                continue;

            set.spriteAnimations.Add(new BattleSpriteAnimation
            {
                cue = pair.Key,
                frames = pair.Value
            });
        }

        return set;
    }

    public static bool IsConfigured(BattleActorVisualSet set)
    {
        return set != null && set.HasAnyVisual;
    }

    private bool HasSpriteAnimations()
    {
        if (spriteAnimations == null)
            return false;

        foreach (BattleSpriteAnimation animation in spriteAnimations)
        {
            if (animation?.frames != null && animation.frames.Length > 0)
                return true;
        }

        return false;
    }
}
