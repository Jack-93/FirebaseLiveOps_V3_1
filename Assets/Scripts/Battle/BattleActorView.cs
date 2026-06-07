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
    public bool HasSprite => currentSprite != null;

    private Image image;
    private Animator animator;
    private TMP_Text placeholder;
    private Color fallbackColor = Color.white;
    private Sprite currentSprite;
    private RuntimeAnimatorController currentController;

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

        if (currentSprite != sprite)
        {
            currentSprite = sprite;
            image.sprite = sprite;
            image.preserveAspect = true;
        }

        image.color = sprite == null ? fallbackColor : Color.white;
        if (placeholder != null)
            placeholder.gameObject.SetActive(sprite == null);

        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator != null && currentController != controller)
        {
            currentController = controller;
            animator.runtimeAnimatorController = controller;
            animator.enabled = controller != null;
            if (controller != null)
                Play(BattleAnimationCue.Idle);
        }
    }

    public void Play(BattleAnimationCue cue)
    {
        if (animator == null ||
            !animator.enabled ||
            animator.runtimeAnimatorController == null)
        {
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
}
