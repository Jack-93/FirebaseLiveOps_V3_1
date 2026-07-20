using UnityEngine;

public enum EnemyAttackType
{
    Melee,
    Ranged,
    Dash,
    Boss
}

public readonly struct EnemyCombatProfile
{
    public EnemyAttackType AttackType { get; }
    public float AttackRange { get; }
    public float ApproachSpeed { get; }
    public float AttackInterval { get; }
    public float ProjectileDuration { get; }
    public float DamageMultiplier { get; }

    public bool UsesProjectile => AttackType == EnemyAttackType.Ranged;
    public bool RequiresApproach =>
        AttackRange > 0f && ApproachSpeed > 0f;

    public EnemyCombatProfile(
        EnemyAttackType attackType,
        float attackRange,
        float approachSpeed,
        float attackInterval,
        float projectileDuration,
        float damageMultiplier)
    {
        AttackType = attackType;
        AttackRange = Mathf.Max(0f, attackRange);
        ApproachSpeed = Mathf.Max(0f, approachSpeed);
        AttackInterval = Mathf.Max(0.2f, attackInterval);
        ProjectileDuration = Mathf.Max(0.1f, projectileDuration);
        DamageMultiplier = Mathf.Max(0.1f, damageMultiplier);
    }
}

public static class EnemyCombatProfileResolver
{
    private static readonly EnemyCombatProfile Melee =
        new EnemyCombatProfile(
            EnemyAttackType.Melee,
            110f,
            500f,
            2f,
            0.25f,
            1f);

    private static readonly EnemyCombatProfile Ranged =
        new EnemyCombatProfile(
            EnemyAttackType.Ranged,
            225f,
            420f,
            2.5f,
            0.62f,
            0.9f);

    private static readonly EnemyCombatProfile Dash =
        new EnemyCombatProfile(
            EnemyAttackType.Dash,
            96f,
            900f,
            2.6f,
            0.2f,
            1.1f);

    private static readonly EnemyCombatProfile Boss =
        new EnemyCombatProfile(
            EnemyAttackType.Boss,
            135f,
            650f,
            1.6f,
            0.25f,
            1f);

    public static EnemyCombatProfile Resolve(int stage, bool isBoss)
    {
        if (isBoss)
            return Boss;

        switch (Random.Range(0, 3))
        {
            case 0:
                return Melee;
            case 1:
                return Ranged;
            default:
                return Dash;
        }
    }
}
