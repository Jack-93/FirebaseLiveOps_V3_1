using UnityEngine;
using UnityEngine.UI;

public sealed class BossPatternPresentation
{
    private const string EffectRoot =
        "Battle/Enemies/BossCatCerberus/Effects/";

    private readonly RectTransform effectLayer;
    private readonly RectTransform enemyVisual;
    private readonly RectTransform enemyRoot;
    private readonly BattleActorView enemyActorView;
    private readonly RectTransform[] warnings = new RectTransform[3];
    private readonly Image[] warningImages = new Image[3];
    private readonly RectTransform[] projectiles = new RectTransform[3];
    private readonly Image[] projectileImages = new Image[3];
    private readonly ProjectileState[] projectileStates =
        new ProjectileState[3];
    private readonly Vector3[] worldCorners = new Vector3[4];

    private readonly Sprite thunderBolt;
    private readonly Sprite thunderWarning;
    private readonly Sprite fireBreath;
    private readonly Sprite fireWarning;
    private readonly Sprite spiritBolt;
    private readonly Sprite spiritWarning;
    private float warningPulse;

    public BossPatternPresentation(
        RectTransform effectLayer,
        RectTransform enemyVisual,
        RectTransform enemyRoot,
        BattleActorView enemyActorView)
    {
        this.effectLayer = effectLayer;
        this.enemyVisual = enemyVisual;
        this.enemyRoot = enemyRoot;
        this.enemyActorView = enemyActorView;

        thunderBolt = Resources.Load<Sprite>(EffectRoot + "ThunderBolt");
        thunderWarning = Resources.Load<Sprite>(
            EffectRoot + "ThunderTargetWarning");
        fireBreath = Resources.Load<Sprite>(EffectRoot + "FireBreath");
        fireWarning = Resources.Load<Sprite>(EffectRoot + "FireLaneWarning");
        spiritBolt = Resources.Load<Sprite>(EffectRoot + "SpiritBolt");
        spiritWarning = Resources.Load<Sprite>(
            EffectRoot + "SpiritTripleWarning");

        for (int index = 0; index < 3; index++)
        {
            warnings[index] = RuntimeUiBinder.FindRect(
                effectLayer,
                "BossPatternWarning" + (index + 1));
            warningImages[index] = GetImage(warnings[index]);
            projectiles[index] = RuntimeUiBinder.FindRect(
                effectLayer,
                "BossPatternProjectile" + (index + 1));
            projectileImages[index] = GetImage(projectiles[index]);
        }

        HideAll();
    }

    public void ShowWarning(BossPatternRuntime runtime)
    {
        HideWarnings();
        if (runtime?.Pattern == null || effectLayer == null)
            return;

        warningPulse = 0f;
        switch (runtime.Pattern.patternType)
        {
            case BossPatternType.TargetedThunder:
                if (runtime.TargetPositions.Length > 0)
                {
                    ShowWarningAt(
                        0,
                        thunderWarning,
                        runtime.TargetPositions[0],
                        new Vector2(154f, 154f));
                }
                break;
            case BossPatternType.TripleFireBreath:
                int warningIndex = 0;
                for (int lane = 0; lane < 3; lane++)
                {
                    if (lane == runtime.SafeLaneIndex)
                        continue;

                    ShowWarningAt(
                        warningIndex++,
                        fireWarning,
                        new Vector2(0.5f, (lane + 0.5f) / 3f),
                        new Vector2(
                            effectLayer.rect.width * 0.92f,
                            effectLayer.rect.height * 0.27f));
                }
                break;
            case BossPatternType.SpiritVolley:
                if (runtime.TargetPositions.Length > 0)
                {
                    Vector2 center = Vector2.zero;
                    foreach (Vector2 target in runtime.TargetPositions)
                        center += target;
                    center /= runtime.TargetPositions.Length;
                    float warningSize = Mathf.Min(
                        effectLayer.rect.width * 0.5f,
                        effectLayer.rect.height * 0.8f);
                    ShowWarningAt(
                        0,
                        spiritWarning,
                        center,
                        new Vector2(warningSize, warningSize));
                }
                break;
        }
    }

    public void ShowCast(BossPatternRuntime runtime)
    {
        HideWarnings();
        HideProjectiles();
        if (runtime?.Pattern == null || effectLayer == null)
            return;

        float travel = Mathf.Max(0.05f, runtime.Pattern.impactDelay);
        Vector2 from = GetEnemyImpactAnchor();
        switch (runtime.Pattern.patternType)
        {
            case BossPatternType.TargetedThunder:
                enemyActorView?.Play(BattleAnimationCue.SkillLeft);
                if (runtime.TargetPositions.Length > 0)
                {
                    StartProjectile(
                        0,
                        thunderBolt,
                        from,
                        runtime.TargetPositions[0],
                        travel,
                        new Vector2(150f, 150f),
                        false);
                }
                break;
            case BossPatternType.TripleFireBreath:
                enemyActorView?.Play(BattleAnimationCue.SkillCenter);
                int projectileIndex = 0;
                for (int lane = 0; lane < 3; lane++)
                {
                    if (lane == runtime.SafeLaneIndex)
                        continue;

                    StartProjectile(
                        projectileIndex++,
                        fireBreath,
                        new Vector2(0.52f, (lane + 0.5f) / 3f),
                        new Vector2(0.52f, (lane + 0.5f) / 3f),
                        travel,
                        new Vector2(
                            effectLayer.rect.width * 0.88f,
                            effectLayer.rect.height * 0.28f),
                        true);
                }
                break;
            case BossPatternType.SpiritVolley:
                enemyActorView?.Play(BattleAnimationCue.SkillRight);
                for (int index = 0;
                     index < runtime.TargetPositions.Length && index < 3;
                     index++)
                {
                    StartProjectile(
                        index,
                        spiritBolt,
                        from + new Vector2(0f, (index - 1) * 0.035f),
                        runtime.TargetPositions[index],
                        travel,
                        new Vector2(132f, 132f),
                        false);
                }
                break;
        }
    }

    public void ShowImpact(BossPatternRuntime runtime)
    {
        HideWarnings();
    }

    public void Update(float deltaTime)
    {
        warningPulse += deltaTime;
        float warningAlpha = Mathf.Lerp(
            0.45f,
            0.95f,
            (Mathf.Sin(warningPulse * 12f) + 1f) * 0.5f);
        foreach (Image image in warningImages)
        {
            if (image == null || !image.gameObject.activeSelf)
                continue;

            Color color = image.color;
            color.a = warningAlpha;
            image.color = color;
        }

        for (int index = 0; index < projectileStates.Length; index++)
        {
            ProjectileState state = projectileStates[index];
            RectTransform projectile = projectiles[index];
            Image image = projectileImages[index];
            if (!state.active || projectile == null || image == null)
                continue;

            state.elapsed += deltaTime;
            float travelProgress = Mathf.Clamp01(
                state.elapsed / state.travelDuration);
            if (!state.stationary)
            {
                BattleHudUiFactory.SetAnchoredPoint(
                    projectile,
                    effectLayer,
                    Vector2.Lerp(
                        state.from,
                        state.to,
                        Mathf.SmoothStep(0f, 1f, travelProgress)));
            }

            float pulse = Mathf.Sin(travelProgress * Mathf.PI);
            float direction = state.stationary ? -1f : 1f;
            projectile.localScale = new Vector3(
                direction * Mathf.Lerp(0.88f, 1.08f, pulse),
                Mathf.Lerp(0.88f, 1.08f, pulse),
                1f);

            float fade = state.elapsed <= state.travelDuration
                ? 1f
                : 1f - Mathf.InverseLerp(
                    state.travelDuration,
                    state.lifeDuration,
                    state.elapsed);
            Color color = image.color;
            color.a = Mathf.Clamp01(fade);
            image.color = color;

            if (state.elapsed >= state.lifeDuration)
            {
                state.active = false;
                projectile.gameObject.SetActive(false);
            }

            projectileStates[index] = state;
        }
    }

    public void HideAll()
    {
        HideWarnings();
        HideProjectiles();
    }

    private void ShowWarningAt(
        int index,
        Sprite sprite,
        Vector2 point,
        Vector2 size)
    {
        if (index < 0 || index >= warnings.Length)
            return;

        RectTransform warning = warnings[index];
        Image image = warningImages[index];
        if (warning == null || image == null)
            return;

        image.sprite = sprite;
        image.preserveAspect = true;
        image.color = Color.white;
        warning.sizeDelta = size;
        warning.localScale = Vector3.one;
        BattleHudUiFactory.SetAnchoredPoint(warning, effectLayer, point);
        warning.SetAsLastSibling();
        warning.gameObject.SetActive(sprite != null);
    }

    private void StartProjectile(
        int index,
        Sprite sprite,
        Vector2 from,
        Vector2 to,
        float travelDuration,
        Vector2 size,
        bool stationary)
    {
        if (index < 0 || index >= projectiles.Length)
            return;

        RectTransform projectile = projectiles[index];
        Image image = projectileImages[index];
        if (projectile == null || image == null || sprite == null)
            return;

        image.sprite = sprite;
        image.preserveAspect = true;
        image.color = Color.white;
        projectile.sizeDelta = size;
        projectile.localScale = stationary
            ? new Vector3(-1f, 1f, 1f)
            : Vector3.one;
        BattleHudUiFactory.SetAnchoredPoint(projectile, effectLayer, from);
        projectile.SetAsLastSibling();
        projectile.gameObject.SetActive(true);
        projectileStates[index] = new ProjectileState
        {
            active = true,
            stationary = stationary,
            from = from,
            to = to,
            elapsed = 0f,
            travelDuration = travelDuration,
            lifeDuration = travelDuration + 0.18f
        };
    }

    private Vector2 GetEnemyImpactAnchor()
    {
        RectTransform target = enemyVisual != null ? enemyVisual : enemyRoot;
        if (target == null || effectLayer == null)
            return new Vector2(0.78f, 0.58f);

        Rect reference = effectLayer.rect;
        if (reference.width <= 0.01f || reference.height <= 0.01f)
            return new Vector2(0.78f, 0.58f);

        target.GetWorldCorners(worldCorners);
        Vector3 centerWorld =
            (worldCorners[0] + worldCorners[2]) * 0.5f;
        Vector3 local = effectLayer.InverseTransformPoint(centerWorld);
        return new Vector2(
            Mathf.InverseLerp(reference.xMin, reference.xMax, local.x),
            Mathf.InverseLerp(reference.yMin, reference.yMax, local.y));
    }

    private void HideWarnings()
    {
        foreach (RectTransform warning in warnings)
        {
            if (warning != null)
                warning.gameObject.SetActive(false);
        }
    }

    private void HideProjectiles()
    {
        for (int index = 0; index < projectiles.Length; index++)
        {
            projectileStates[index] = default;
            if (projectiles[index] != null)
                projectiles[index].gameObject.SetActive(false);
        }
    }

    private static Image GetImage(RectTransform target)
    {
        if (target == null)
            return null;

        Image image = target.GetComponent<Image>();
        if (image != null)
            image.raycastTarget = false;
        return image;
    }

    private struct ProjectileState
    {
        public bool active;
        public bool stationary;
        public Vector2 from;
        public Vector2 to;
        public float elapsed;
        public float travelDuration;
        public float lifeDuration;
    }
}
