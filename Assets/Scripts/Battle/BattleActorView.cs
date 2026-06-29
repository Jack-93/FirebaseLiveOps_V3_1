using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BattleAnimationCue
{
    Idle,
    Attack,
    Hit,
    Death,
    Skill
}

public class BattleActorView : MonoBehaviour
{
    public bool HasSprite => currentBaseSprite != null || currentSprite != null;

    private const float SpriteFrameSeconds = 0.085f;

    private Image image;
    private Animator animator;
    private TMP_Text placeholder;
    private Color fallbackColor = Color.white;
    private Sprite currentSprite;
    private Sprite currentBaseSprite;
    private RuntimeAnimatorController currentController;
    private Dictionary<BattleAnimationCue, Sprite[]> spriteAnimations;
    private Sprite[] activeSpriteFrames;
    private int activeSpriteFrameIndex;
    private float spriteFrameTimer;
    private bool activeSpriteLoop;

    public void Initialize(TMP_Text placeholderText, Color fallback)
    {
        image = GetComponent<Image>();
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = gameObject.AddComponent<Animator>();

        placeholder = placeholderText;
        fallbackColor = fallback;
    }

    public void SetVisual(
        Sprite sprite,
        RuntimeAnimatorController controller)
    {
        if (image == null)
            image = GetComponent<Image>();

        if (currentBaseSprite != sprite)
        {
            currentBaseSprite = sprite;
            if (currentController == null)
                ApplySprite(sprite);
        }

        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator != null && currentController != controller)
        {
            currentController = controller;
            animator.runtimeAnimatorController = controller;
            animator.enabled = controller != null;
            if (controller != null)
            {
                StopSpriteAnimation(false);
                Play(BattleAnimationCue.Idle);
            }
            else if (spriteAnimations != null)
            {
                TryStartSpriteAnimation(BattleAnimationCue.Idle, true);
            }
            else
            {
                ApplySprite(currentBaseSprite);
            }
        }
    }

    public void SetSpriteAnimations(
        Dictionary<BattleAnimationCue, Sprite[]> animations)
    {
        if (ReferenceEquals(spriteAnimations, animations))
            return;

        spriteAnimations = animations;
        StopSpriteAnimation(true);

        if (currentController != null)
            return;

        if (spriteAnimations == null ||
            !TryStartSpriteAnimation(BattleAnimationCue.Idle, true))
        {
            ApplySprite(currentBaseSprite);
        }
    }

    public void Play(BattleAnimationCue cue)
    {
        if (animator == null ||
            !animator.enabled ||
            animator.runtimeAnimatorController == null)
        {
            TryStartSpriteAnimation(cue, cue == BattleAnimationCue.Idle);
            return;
        }

        string trigger = cue.ToString();
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger &&
                parameter.name == trigger)
            {
                animator.SetTrigger(trigger);
                return;
            }
        }
    }

    private void Update()
    {
        if (currentController != null ||
            activeSpriteFrames == null ||
            activeSpriteFrames.Length == 0)
        {
            return;
        }

        spriteFrameTimer += Time.deltaTime;
        if (spriteFrameTimer < SpriteFrameSeconds)
            return;

        spriteFrameTimer = 0f;
        activeSpriteFrameIndex++;
        if (activeSpriteFrameIndex >= activeSpriteFrames.Length)
        {
            if (activeSpriteLoop)
            {
                activeSpriteFrameIndex = 0;
            }
            else
            {
                TryStartSpriteAnimation(BattleAnimationCue.Idle, true);
                return;
            }
        }

        ApplySprite(activeSpriteFrames[activeSpriteFrameIndex]);
    }

    private bool TryStartSpriteAnimation(BattleAnimationCue cue, bool loop)
    {
        if (spriteAnimations == null)
            return false;

        Sprite[] frames = ResolveFrames(cue);
        if (frames == null || frames.Length == 0)
            return false;

        activeSpriteFrames = frames;
        activeSpriteFrameIndex = 0;
        activeSpriteLoop = loop;
        spriteFrameTimer = 0f;
        ApplySprite(frames[0]);
        return true;
    }

    private Sprite[] ResolveFrames(BattleAnimationCue cue)
    {
        if (TryGetFrames(cue, out Sprite[] frames))
            return frames;

        if (cue == BattleAnimationCue.Skill &&
            TryGetFrames(BattleAnimationCue.Attack, out frames))
        {
            return frames;
        }

        if (cue == BattleAnimationCue.Death &&
            TryGetFrames(BattleAnimationCue.Hit, out frames))
        {
            return frames;
        }

        if (cue != BattleAnimationCue.Idle &&
            TryGetFrames(BattleAnimationCue.Idle, out frames))
        {
            return frames;
        }

        return null;
    }

    private bool TryGetFrames(
        BattleAnimationCue cue,
        out Sprite[] frames)
    {
        frames = null;
        return spriteAnimations != null &&
            spriteAnimations.TryGetValue(cue, out frames) &&
            frames != null &&
            frames.Length > 0;
    }

    private void StopSpriteAnimation(bool resetSprite)
    {
        activeSpriteFrames = null;
        activeSpriteFrameIndex = 0;
        spriteFrameTimer = 0f;
        activeSpriteLoop = false;

        if (resetSprite)
            ApplySprite(currentBaseSprite);
    }

    private void ApplySprite(Sprite sprite)
    {
        if (image == null)
            image = GetComponent<Image>();
        if (image == null)
            return;

        currentSprite = sprite;
        image.sprite = sprite;
        image.preserveAspect = true;
        image.color = sprite == null ? fallbackColor : Color.white;
        if (placeholder != null)
            placeholder.gameObject.SetActive(sprite == null);
    }
}
