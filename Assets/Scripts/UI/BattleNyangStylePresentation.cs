using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// First-pass battle presentation layer for the bright, rounded "Nyang" direction.
/// It keeps the existing combat logic and art assets intact while adding a clear
/// auto-flow / crisis-control language, stronger color grouping, and readable
/// moment-to-moment feedback.
/// </summary>
[DisallowMultipleComponent]
public sealed class BattleNyangStylePresentation : MonoBehaviour
{
    private static readonly Color Sky = new Color32(103, 205, 239, 255);
    private static readonly Color Mint = new Color32(123, 225, 179, 255);
    private static readonly Color Sun = new Color32(255, 199, 92, 255);
    private static readonly Color Coral = new Color32(255, 112, 117, 255);
    private static readonly Color Paper = new Color32(255, 249, 229, 255);

    private RectTransform panel;
    private RectTransform battlefieldLayer;
    private RectTransform presentationRoot;
    private RectTransform crisisBadge;
    private RectTransform comboBadge;
    private Image arenaTint;
    private Image crisisBadgeImage;
    private Image crisisAccentImage;
    private Image comboBadgeImage;
    private TMP_Text modeTitleText;
    private TMP_Text modeHintText;
    private TMP_Text stageText;
    private TMP_Text crisisText;
    private TMP_Text comboText;
    private CanvasGroup crisisGroup;
    private CanvasGroup comboGroup;

    private float crisisTimer;
    private float comboTimer;
    private float pulseClock;
    private bool bound;
    private bool isBoss;

    private void Awake()
    {
        if (bound)
            return;

        RectTransform root = transform as RectTransform;
        if (root == null)
            return;

        Bind(
            root,
            RuntimeUiBinder.FindRect(root, "BattlefieldLayer") ?? root,
            RuntimeUiBinder.FindRect(root, "GamePlayLine"));
    }

    public void Bind(
        RectTransform panel,
        RectTransform battlefieldLayer,
        RectTransform gameplayArea)
    {
        if (bound || panel == null)
            return;

        this.panel = panel;
        this.battlefieldLayer = battlefieldLayer;
        bound = true;

        CreateArenaTint();
        CreatePresentationRoot();
        CreateModeCard();
        CreateCrisisBadge();
        CreateComboBadge();
        SetAutoAdvance(true);
        SetStage(1, false);
        HideCrisisImmediately();
        HideComboImmediately();
    }

    public void SetStage(int stage, bool boss)
    {
        if (!bound)
            return;

        isBoss = boss;
        if (stageText != null)
        {
            stageText.text = boss
                ? "BOSS WAVE"
                : $"STAGE {Mathf.Max(1, stage):00}";
            stageText.color = boss ? Coral : Sun;
        }

        if (arenaTint != null && crisisTimer <= 0f)
        {
            arenaTint.color = boss
                ? new Color(0.98f, 0.42f, 0.42f, 0.075f)
                : new Color(1f, 0.78f, 0.38f, 0.055f);
        }
    }

    public void SetAutoAdvance(bool enabled)
    {
        if (modeTitleText != null)
            modeTitleText.text = enabled ? "AUTO FLOW" : "MANUAL FLOW";
        if (modeHintText != null)
            modeHintText.text = enabled
                ? "보스 · 위기 때 직접 조작"
                : "직접 조작 중";
    }

    public void OnAutoAttack(int damage)
    {
        ShowCombo("AUTO STRIKE", Sky, damage);
    }

    public void OnCompanionAttack(string companionName, int damage)
    {
        string label = string.IsNullOrWhiteSpace(companionName)
            ? "PARTY LINK"
            : $"{companionName} LINK";
        ShowCombo(label, Sun, damage);
    }

    public void OnPowerCharged(float current, float max)
    {
        ShowCombo(
            $"POWER {Mathf.RoundToInt(current):00}/{Mathf.RoundToInt(max):00}",
            Mint,
            0);
    }

    public void OnThreatStarted(string message)
    {
        ShowCrisis(string.IsNullOrWhiteSpace(message) ? "위기 감지" : message);
    }

    public void OnThreatResolved(bool hit)
    {
        if (hit)
            ShowCrisis("피격 · 회복 준비", Coral);
        else
            ShowCrisis("회피 성공", Mint);
    }

    public void OnBossWarning(string message)
    {
        ShowCrisis(
            string.IsNullOrWhiteSpace(message)
                ? "BOSS WARNING"
                : message,
            Coral);
    }

    public void OnBossCast(string message)
    {
        ShowCrisis(
            string.IsNullOrWhiteSpace(message)
                ? "직접 조작"
                : message,
            Sun);
    }

    public void OnBossImpact(bool hit)
    {
        ShowCrisis(hit ? "패턴 적중" : "패턴 회피", hit ? Coral : Mint);
    }

    public void OnEnemyDefeated(bool boss, int reward)
    {
        ShowCombo(
            boss ? "BOSS BREAK" : "WAVE CLEAR",
            boss ? Sun : Mint,
            reward);
        HideCrisisAfter(0.12f);
    }

    public void OnHeroDefeated()
    {
        ShowCrisis("재정비 중", Coral);
    }

    public void OnHeroRecovered()
    {
        ShowCombo("READY", Mint, 0);
        HideCrisisAfter(0.1f);
    }

    public void UpdatePresentation(float deltaTime)
    {
        if (!bound)
            return;

        float scaledDelta = Mathf.Max(0f, deltaTime);
        pulseClock += scaledDelta;
        crisisTimer = Mathf.Max(0f, crisisTimer - scaledDelta);
        comboTimer = Mathf.Max(0f, comboTimer - scaledDelta);

        UpdateCrisisVisuals();
        UpdateComboVisuals();

        if (arenaTint != null && crisisTimer <= 0f)
        {
            float breathing = 1f + Mathf.Sin(pulseClock * 1.6f) * 0.035f;
            arenaTint.rectTransform.localScale =
                new Vector3(breathing, breathing, 1f);
        }
    }

    private void CreateArenaTint()
    {
        if (battlefieldLayer == null)
            return;

        RectTransform tintRect = RuntimeUiFactory.CreatePanel(
            "NyangArenaTint",
            battlefieldLayer,
            new Color(1f, 0.78f, 0.38f, 0.055f),
            Vector2.zero,
            Vector2.one);
        SetNonInteractive(tintRect);
        arenaTint = tintRect.GetComponent<Image>();
    }

    private void CreatePresentationRoot()
    {
        GameObject rootObject = new GameObject(
            "NyangStylePresentation",
            typeof(RectTransform),
            typeof(CanvasGroup));
        rootObject.transform.SetParent(panel, false);
        presentationRoot = rootObject.GetComponent<RectTransform>();
        presentationRoot.anchorMin = Vector2.zero;
        presentationRoot.anchorMax = Vector2.one;
        presentationRoot.offsetMin = Vector2.zero;
        presentationRoot.offsetMax = Vector2.zero;

        CanvasGroup group = rootObject.GetComponent<CanvasGroup>();
        group.ignoreParentGroups = true;
    }

    private void CreateModeCard()
    {
        RectTransform card = RuntimeUiFactory.CreatePanel(
            "NyangModeCard",
            presentationRoot,
            new Color(0.17f, 0.12f, 0.31f, 0.9f),
            new Vector2(0.055f, 0.855f),
            new Vector2(0.945f, 0.925f));
        SetNonInteractive(card);

        modeTitleText = RuntimeUiFactory.CreateText(
            "NyangModeTitle",
            card,
            "AUTO FLOW",
            23f,
            new Vector2(0.045f, 0.2f),
            new Vector2(0.36f, 0.82f),
            TextAlignmentOptions.Left,
            Paper);
        modeTitleText.fontStyle = FontStyles.Bold;

        modeHintText = RuntimeUiFactory.CreateText(
            "NyangModeHint",
            card,
            "보스 · 위기 때 직접 조작",
            18f,
            new Vector2(0.37f, 0.2f),
            new Vector2(0.77f, 0.82f),
            TextAlignmentOptions.Center,
            new Color32(203, 191, 230, 255));

        stageText = RuntimeUiFactory.CreateText(
            "NyangStageLabel",
            card,
            "STAGE 01",
            19f,
            new Vector2(0.78f, 0.2f),
            new Vector2(0.96f, 0.82f),
            TextAlignmentOptions.Right,
            Sun);
        stageText.fontStyle = FontStyles.Bold;
    }

    private void CreateCrisisBadge()
    {
        crisisBadge = RuntimeUiFactory.CreatePanel(
            "NyangCrisisCard",
            presentationRoot,
            new Color(Coral.r, Coral.g, Coral.b, 0.94f),
            new Vector2(0.21f, 0.765f),
            new Vector2(0.79f, 0.835f));
        SetNonInteractive(crisisBadge);

        crisisGroup = crisisBadge.gameObject.AddComponent<CanvasGroup>();

        crisisAccentImage = RuntimeUiFactory.CreatePanel(
            "NyangCrisisAccent",
            crisisBadge,
            new Color(1f, 1f, 1f, 0.55f),
            new Vector2(0.015f, 0.18f),
            new Vector2(0.03f, 0.82f)).GetComponent<Image>();
        SetNonInteractive(crisisAccentImage.rectTransform);

        crisisText = RuntimeUiFactory.CreateText(
            "NyangCrisisText",
            crisisBadge,
            "위기 감지",
            26f,
            new Vector2(0.06f, 0.08f),
            new Vector2(0.94f, 0.92f),
            TextAlignmentOptions.Center,
            Paper);
        crisisText.fontStyle = FontStyles.Bold;
        crisisBadgeImage = crisisBadge.GetComponent<Image>();
    }

    private void CreateComboBadge()
    {
        comboBadge = RuntimeUiFactory.CreatePanel(
            "NyangComboCard",
            presentationRoot,
            new Color(0.17f, 0.12f, 0.31f, 0.9f),
            new Vector2(0.31f, 0.12f),
            new Vector2(0.69f, 0.19f));
        SetNonInteractive(comboBadge);

        comboGroup = comboBadge.gameObject.AddComponent<CanvasGroup>();

        comboText = RuntimeUiFactory.CreateText(
            "NyangComboText",
            comboBadge,
            "AUTO STRIKE",
            22f,
            new Vector2(0.06f, 0.1f),
            new Vector2(0.94f, 0.9f),
            TextAlignmentOptions.Center,
            Paper);
        comboText.fontStyle = FontStyles.Bold;
        comboBadgeImage = comboBadge.GetComponent<Image>();
    }

    private void ShowCombo(string label, Color accent, int damage)
    {
        if (!bound || comboText == null)
            return;

        comboText.text = damage > 0
            ? $"{label}  +{CompactNumberFormatter.Format(damage)}"
            : label;
        comboText.color = Paper;
        comboBadgeImage.color = new Color(
            accent.r * 0.32f,
            accent.g * 0.32f,
            accent.b * 0.32f,
            0.94f);
        comboTimer = 0.84f;
        comboBadge.localScale = Vector3.one * 0.86f;
        comboBadge.gameObject.SetActive(true);
    }

    private void ShowCrisis(string message, Color? accentOverride = null)
    {
        if (!bound || crisisText == null)
            return;

        Color accent = accentOverride ?? Coral;
        crisisText.text = message;
        crisisText.color = Paper;
        crisisBadgeImage.color = new Color(
            accent.r * 0.78f,
            accent.g * 0.78f,
            accent.b * 0.78f,
            0.97f);
        crisisAccentImage.color = new Color(
            Paper.r,
            Paper.g,
            Paper.b,
            0.7f);
        crisisTimer = 1.1f;
        crisisBadge.localScale = Vector3.one * 0.9f;
        crisisBadge.gameObject.SetActive(true);

        if (arenaTint != null)
        {
            arenaTint.color = new Color(
                accent.r,
                accent.g,
                accent.b,
                isBoss ? 0.12f : 0.08f);
        }
    }

    private void HideCrisisAfter(float seconds)
    {
        crisisTimer = Mathf.Min(crisisTimer, Mathf.Max(0f, seconds));
    }

    private void HideCrisisImmediately()
    {
        crisisTimer = 0f;
        if (crisisBadge != null)
            crisisBadge.gameObject.SetActive(false);
    }

    private void HideComboImmediately()
    {
        comboTimer = 0f;
        if (comboBadge != null)
            comboBadge.gameObject.SetActive(false);
    }

    private void UpdateCrisisVisuals()
    {
        if (crisisBadge == null || crisisGroup == null)
            return;

        if (crisisTimer <= 0f)
        {
            crisisGroup.alpha = 0f;
            crisisBadge.gameObject.SetActive(false);
            if (arenaTint != null)
            {
                arenaTint.color = isBoss
                    ? new Color(0.98f, 0.42f, 0.42f, 0.075f)
                    : new Color(1f, 0.78f, 0.38f, 0.055f);
            }
            return;
        }

        crisisBadge.gameObject.SetActive(true);
        float progress = Mathf.Clamp01(crisisTimer / 1.1f);
        crisisGroup.alpha = Mathf.Min(1f, progress + 0.25f);
        float pulse = 1f + Mathf.Sin(pulseClock * 8f) * 0.06f;
        crisisBadge.localScale = Vector3.one * pulse;
    }

    private void UpdateComboVisuals()
    {
        if (comboBadge == null || comboGroup == null)
            return;

        if (comboTimer <= 0f)
        {
            comboGroup.alpha = 0f;
            comboBadge.gameObject.SetActive(false);
            return;
        }

        comboBadge.gameObject.SetActive(true);
        float progress = Mathf.Clamp01(comboTimer / 0.84f);
        comboGroup.alpha = Mathf.Min(1f, progress * 1.5f);
        float entrance = SmoothStep(0f, 1f, 1f - progress);
        float pulse = 1f + Mathf.Sin(pulseClock * 6f) * 0.035f;
        float scale = Mathf.Lerp(0.86f, 1f, entrance) * pulse;
        comboBadge.localScale = Vector3.one * scale;
    }

    private static void SetNonInteractive(RectTransform rect)
    {
        if (rect == null)
            return;

        foreach (Image image in rect.GetComponentsInChildren<Image>(true))
            image.raycastTarget = false;
        foreach (TMP_Text text in rect.GetComponentsInChildren<TMP_Text>(true))
            text.raycastTarget = false;
    }

    private static float SmoothStep(float from, float to, float value)
    {
        float t = Mathf.Clamp01((value - from) / Mathf.Max(0.0001f, to - from));
        t = t * t * (3f - 2f * t);
        return Mathf.Lerp(from, to, t);
    }
}
