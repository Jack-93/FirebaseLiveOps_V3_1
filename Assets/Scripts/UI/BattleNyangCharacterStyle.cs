using UnityEngine;
using UnityEngine.UI;

public enum BattleNyangCharacterRole
{
    Hero,
    Companion,
    Striker,
    Guardian,
    Support,
    Melee,
    Ranged,
    Dash,
    Boss
}

/// <summary>
/// Applies the first-pass bright, rounded character treatment to the
/// existing battle actors. The sprite and animation contract stays in
/// BattleActorView so final character art can be swapped without gameplay
/// changes.
/// </summary>
[DisallowMultipleComponent]
public sealed class BattleNyangCharacterStyle : MonoBehaviour
{
    private static readonly Color HeroTint =
        new Color32(186, 235, 255, 255);
    private static readonly Color CompanionTint =
        new Color32(201, 255, 230, 255);
    private static readonly Color StrikerTint =
        new Color32(255, 230, 173, 255);
    private static readonly Color GuardianTint =
        new Color32(211, 230, 255, 255);
    private static readonly Color SupportTint =
        new Color32(213, 255, 243, 255);
    private static readonly Color MeleeTint =
        new Color32(255, 205, 194, 255);
    private static readonly Color RangedTint =
        new Color32(225, 213, 255, 255);
    private static readonly Color DashTint =
        new Color32(255, 243, 184, 255);
    private static readonly Color BossTint =
        new Color32(255, 191, 208, 255);

    private static readonly Color HeroAccent =
        new Color32(77, 190, 235, 255);
    private static readonly Color CompanionAccent =
        new Color32(91, 211, 161, 255);
    private static readonly Color StrikerAccent =
        new Color32(255, 170, 72, 255);
    private static readonly Color GuardianAccent =
        new Color32(116, 156, 235, 255);
    private static readonly Color SupportAccent =
        new Color32(93, 204, 178, 255);
    private static readonly Color MeleeAccent =
        new Color32(234, 102, 106, 255);
    private static readonly Color RangedAccent =
        new Color32(144, 112, 230, 255);
    private static readonly Color DashAccent =
        new Color32(238, 178, 53, 255);
    private static readonly Color BossAccent =
        new Color32(190, 74, 123, 255);

    private RectTransform actorRoot;
    private RectTransform visual;
    private Image visualImage;
    private Image groundShadowImage;
    private Outline outline;
    private Shadow graphicShadow;
    private BattleNyangCharacterRole role;
    private Color roleTint;
    private Color roleAccent;
    private bool bound;

    public static BattleNyangCharacterStyle Attach(
        RectTransform actorRoot,
        RectTransform visual,
        BattleNyangCharacterRole role)
    {
        if (actorRoot == null)
            return null;

        BattleNyangCharacterStyle style =
            actorRoot.GetComponent<BattleNyangCharacterStyle>();
        if (style == null)
            style = actorRoot.gameObject.AddComponent<
                BattleNyangCharacterStyle>();

        style.Bind(actorRoot, visual, role);
        return style;
    }

    public void Bind(
        RectTransform actorRoot,
        RectTransform visual,
        BattleNyangCharacterRole role)
    {
        if (bound && this.visual == visual && this.role == role)
            return;

        this.actorRoot = actorRoot;
        this.visual = visual;
        bound = actorRoot != null && visual != null;
        if (!bound)
            return;

        visualImage = visual.GetComponent<Image>();
        ConfigureGraphicEffects();
        groundShadowImage = FindOrCreateGroundShadow();
        SetRole(role);
        ApplyFrame();
    }

    public void SetRole(BattleNyangCharacterRole nextRole)
    {
        role = nextRole;
        roleTint = GetTint(nextRole);
        roleAccent = GetAccent(nextRole);

        if (outline != null)
        {
            outline.effectColor = new Color(
                roleAccent.r,
                roleAccent.g,
                roleAccent.b,
                IsBossRole(nextRole) ? 0.95f : 0.82f);
            outline.effectDistance = IsBossRole(nextRole)
                ? new Vector2(3f, -3f)
                : new Vector2(2f, -2f);
        }

        if (graphicShadow != null)
        {
            graphicShadow.effectColor = new Color(
                roleAccent.r * 0.22f,
                roleAccent.g * 0.22f,
                roleAccent.b * 0.22f,
                IsBossRole(nextRole) ? 0.42f : 0.3f);
        }

        if (groundShadowImage != null)
        {
            groundShadowImage.color = new Color(
                roleAccent.r,
                roleAccent.g,
                roleAccent.b,
                IsBossRole(nextRole) ? 0.28f : 0.2f);
        }
    }

    /// <summary>
    /// Reapplies the grade after BattleHudUI's hit and defeat color pass.
    /// </summary>
    public void ApplyFrame()
    {
        if (!bound || visualImage == null)
            return;

        if (visualImage.sprite == null)
        {
            if (groundShadowImage != null)
                groundShadowImage.color = Color.clear;
            return;
        }

        Color current = visualImage.color;
        Color graded = Color.Lerp(current, roleTint, 0.12f);
        graded.a = current.a;
        visualImage.color = graded;

        if (groundShadowImage != null)
        {
            float alpha = Mathf.Clamp01(current.a) *
                (IsBossRole(role) ? 0.28f : 0.2f);
            groundShadowImage.color = new Color(
                roleAccent.r,
                roleAccent.g,
                roleAccent.b,
                alpha);
        }
    }

    public static BattleNyangCharacterRole FromEnemy(
        EnemyAttackType attackType,
        bool boss)
    {
        if (boss || attackType == EnemyAttackType.Boss)
            return BattleNyangCharacterRole.Boss;

        switch (attackType)
        {
            case EnemyAttackType.Ranged:
                return BattleNyangCharacterRole.Ranged;
            case EnemyAttackType.Dash:
                return BattleNyangCharacterRole.Dash;
            default:
                return BattleNyangCharacterRole.Melee;
        }
    }

    public static BattleNyangCharacterRole FromCompanion(
        CompanionRole companionRole)
    {
        switch (companionRole)
        {
            case CompanionRole.Striker:
                return BattleNyangCharacterRole.Striker;
            case CompanionRole.Guardian:
                return BattleNyangCharacterRole.Guardian;
            case CompanionRole.Support:
                return BattleNyangCharacterRole.Support;
            default:
                return BattleNyangCharacterRole.Companion;
        }
    }

    private void ConfigureGraphicEffects()
    {
        if (visualImage == null)
            return;

        outline = visual.GetComponent<Outline>();
        if (outline == null)
            outline = visual.gameObject.AddComponent<Outline>();
        outline.useGraphicAlpha = true;

        graphicShadow = visual.GetComponent<Shadow>();
        if (graphicShadow == null)
            graphicShadow = visual.gameObject.AddComponent<Shadow>();
        graphicShadow.useGraphicAlpha = true;
    }

    private Image FindOrCreateGroundShadow()
    {
        Transform existing = actorRoot.Find("NyangGroundShadow");
        if (existing != null)
            return existing.GetComponent<Image>();

        GameObject shadowObject = new GameObject(
            "NyangGroundShadow",
            typeof(RectTransform),
            typeof(Image));
        shadowObject.transform.SetParent(actorRoot, false);

        RectTransform shadowRect =
            shadowObject.GetComponent<RectTransform>();
        shadowRect.anchorMin = new Vector2(0.12f, 0.015f);
        shadowRect.anchorMax = new Vector2(0.88f, 0.22f);
        shadowRect.offsetMin = Vector2.zero;
        shadowRect.offsetMax = Vector2.zero;
        shadowRect.SetSiblingIndex(0);

        Image image = shadowObject.GetComponent<Image>();
        image.sprite = PrototypeUiArt.ActorShadow;
        image.preserveAspect = false;
        image.raycastTarget = false;
        image.color = Color.clear;
        return image;
    }

    private static Color GetTint(BattleNyangCharacterRole role)
    {
        switch (role)
        {
            case BattleNyangCharacterRole.Hero:
                return HeroTint;
            case BattleNyangCharacterRole.Companion:
                return CompanionTint;
            case BattleNyangCharacterRole.Striker:
                return StrikerTint;
            case BattleNyangCharacterRole.Guardian:
                return GuardianTint;
            case BattleNyangCharacterRole.Support:
                return SupportTint;
            case BattleNyangCharacterRole.Ranged:
                return RangedTint;
            case BattleNyangCharacterRole.Dash:
                return DashTint;
            case BattleNyangCharacterRole.Boss:
                return BossTint;
            default:
                return MeleeTint;
        }
    }

    private static Color GetAccent(BattleNyangCharacterRole role)
    {
        switch (role)
        {
            case BattleNyangCharacterRole.Hero:
                return HeroAccent;
            case BattleNyangCharacterRole.Companion:
                return CompanionAccent;
            case BattleNyangCharacterRole.Striker:
                return StrikerAccent;
            case BattleNyangCharacterRole.Guardian:
                return GuardianAccent;
            case BattleNyangCharacterRole.Support:
                return SupportAccent;
            case BattleNyangCharacterRole.Ranged:
                return RangedAccent;
            case BattleNyangCharacterRole.Dash:
                return DashAccent;
            case BattleNyangCharacterRole.Boss:
                return BossAccent;
            default:
                return MeleeAccent;
        }
    }

    private static bool IsBossRole(BattleNyangCharacterRole role)
    {
        return role == BattleNyangCharacterRole.Boss;
    }
}
