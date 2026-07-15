using UnityEngine;
using UnityEngine.UI;

public sealed class BattlePoleView : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] deathFrames;
    [SerializeField] private float frameSeconds = 0.18f;
    [SerializeField] private float hitShakeSeconds = 0.22f;
    [SerializeField] private float hitShakePixels = 8f;
    [SerializeField] private float hitScalePunch = 0.045f;

    private BattleManager battleManager;
    private RectTransform targetRect;
    private Sprite[] activeFrames;
    private int activeFrameIndex;
    private float frameTimer;
    private float hitTimer;
    private Vector2 baseAnchoredPosition;
    private Vector3 baseScale = Vector3.one;
    private bool loop;
    private bool destroyedState;

    private const string IdleResourcePath =
        "Battle/PoleAnimations/Idle/PoleBattle_Idle_";
    private const string DeathResourcePath =
        "Battle/PoleAnimations/Death/PoleBattle_Death_";

    private void Awake()
    {
        EnsureTargetImage();
        CaptureBaseTransform();
        LoadDefaultFramesIfNeeded();
        PlayIdle();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void Update()
    {
        TickHit(Time.deltaTime);

        if (activeFrames == null || activeFrames.Length == 0)
            return;

        frameTimer += Time.deltaTime;
        if (frameTimer < frameSeconds)
            return;

        frameTimer = 0f;
        activeFrameIndex++;
        if (activeFrameIndex >= activeFrames.Length)
        {
            if (!loop)
            {
                activeFrameIndex = activeFrames.Length - 1;
                ApplyFrame();
                return;
            }

            activeFrameIndex = 0;
        }

        ApplyFrame();
    }

    public void Bind(BattleManager manager)
    {
        if (battleManager == manager)
            return;

        Unbind();
        battleManager = manager;
        if (battleManager == null)
            return;

        battleManager.OnPoleDestroyed += HandlePoleDestroyed;
        battleManager.OnPoleDamaged += HandlePoleDamaged;
        battleManager.OnBattleStateChanged += HandleBattleStateChanged;

        if (battleManager.IsPoleDestroyed)
            PlayDeath();
        else
            PlayIdle();
    }

    public void Unbind()
    {
        if (battleManager == null)
            return;

        battleManager.OnPoleDestroyed -= HandlePoleDestroyed;
        battleManager.OnPoleDamaged -= HandlePoleDamaged;
        battleManager.OnBattleStateChanged -= HandleBattleStateChanged;
        battleManager = null;
    }

    public void PlayIdle()
    {
        destroyedState = false;
        ResetHitTransform();
        StartAnimation(idleFrames, true);
    }

    public void PlayDeath()
    {
        destroyedState = true;
        ResetHitTransform();
        StartAnimation(deathFrames, false);
    }

    public void PlayHit()
    {
        if (destroyedState)
            return;

        if (targetRect == null)
            CaptureBaseTransform();
        hitTimer = Mathf.Max(hitTimer, hitShakeSeconds);
    }

    private void HandlePoleDestroyed()
    {
        PlayDeath();
    }

    private void HandlePoleDamaged(int damage)
    {
        if (damage > 0)
            PlayHit();
    }

    private void HandleBattleStateChanged()
    {
        if (battleManager == null)
            return;

        if (destroyedState && !battleManager.IsPoleDestroyed)
            PlayIdle();
    }

    private void StartAnimation(Sprite[] frames, bool shouldLoop)
    {
        if (frames == null || frames.Length == 0)
            return;

        activeFrames = frames;
        activeFrameIndex = 0;
        frameTimer = 0f;
        loop = shouldLoop;
        ApplyFrame();
    }

    private void ApplyFrame()
    {
        EnsureTargetImage();
        if (targetImage == null ||
            activeFrames == null ||
            activeFrames.Length == 0)
        {
            return;
        }

        targetImage.sprite = activeFrames[activeFrameIndex];
        targetImage.color = GetCurrentColor();
        targetImage.preserveAspect = true;
        targetImage.raycastTarget = false;
    }

    private void EnsureTargetImage()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    private void CaptureBaseTransform()
    {
        EnsureTargetImage();
        if (targetImage == null)
            return;

        targetRect = targetImage.rectTransform;
        baseAnchoredPosition = targetRect.anchoredPosition;
        baseScale = targetRect.localScale;
    }

    private void TickHit(float deltaTime)
    {
        if (hitTimer <= 0f)
            return;

        hitTimer = Mathf.Max(0f, hitTimer - deltaTime);
        if (targetRect == null)
            CaptureBaseTransform();
        if (targetRect == null)
            return;

        float ratio = hitShakeSeconds <= 0f
            ? 0f
            : hitTimer / hitShakeSeconds;
        if (ratio <= 0f)
        {
            ResetHitTransform();
            return;
        }

        float shake = Mathf.Sin(Time.unscaledTime * 95f) *
            hitShakePixels * ratio;
        float punch = 1f + hitScalePunch * ratio;
        targetRect.anchoredPosition =
            baseAnchoredPosition + new Vector2(shake, 0f);
        targetRect.localScale = baseScale * punch;

        if (targetImage != null)
            targetImage.color = GetCurrentColor();
    }

    private void ResetHitTransform()
    {
        hitTimer = 0f;
        if (targetRect == null)
            CaptureBaseTransform();
        if (targetRect == null)
            return;

        targetRect.anchoredPosition = baseAnchoredPosition;
        targetRect.localScale = baseScale;
        if (targetImage != null)
            targetImage.color = Color.white;
    }

    private Color GetCurrentColor()
    {
        if (hitTimer <= 0f || hitShakeSeconds <= 0f)
            return Color.white;

        float ratio = Mathf.Clamp01(hitTimer / hitShakeSeconds);
        return Color.Lerp(
            Color.white,
            new Color(1f, 0.78f, 0.42f, 1f),
            ratio);
    }

    private void LoadDefaultFramesIfNeeded()
    {
        if (idleFrames == null || idleFrames.Length == 0)
            idleFrames = LoadFrames(IdleResourcePath);
        if (deathFrames == null || deathFrames.Length == 0)
            deathFrames = LoadFrames(DeathResourcePath);
    }

    private static Sprite[] LoadFrames(string resourcePrefix)
    {
        Sprite[] frames = new Sprite[8];
        int count = 0;
        for (int index = 0; index < frames.Length; index++)
        {
            Sprite sprite = Resources.Load<Sprite>(
                resourcePrefix + (index + 1).ToString("00"));
            if (sprite == null)
                continue;

            frames[count] = sprite;
            count++;
        }

        if (count == frames.Length)
            return frames;

        Sprite[] compact = new Sprite[count];
        for (int index = 0; index < count; index++)
            compact[index] = frames[index];
        return compact;
    }
}
