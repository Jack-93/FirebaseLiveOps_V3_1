using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class BattleMeleeMovementController : MonoBehaviour
{
    [SerializeField] private RectTransform attackTarget;
    [SerializeField, Min(0f)] private float approachSpeed = 500f;
    [SerializeField, Min(0f)] private float attackRange = 110f;

    private RectTransform actorRoot;
    private RectTransform actorParent;
    private Vector3 homeLocalPosition;
    private Action onImpact;
    private State state;

    private enum State
    {
        Idle,
        Approaching,
        WaitingForImpact
    }

    private void Awake()
    {
        actorRoot = transform as RectTransform;
        actorParent = actorRoot == null
            ? null
            : actorRoot.parent as RectTransform;
        if (actorRoot != null)
            homeLocalPosition = actorRoot.localPosition;
    }

    private void Update()
    {
        if (actorRoot == null || actorParent == null)
            return;

        if (state == State.Approaching)
            UpdateApproach();
    }

    public void SetImpactAction(Action callback)
    {
        onImpact = callback;
    }

    public void Configure(EnemyCombatProfile profile)
    {
        attackRange = profile.AttackRange;
        approachSpeed = profile.ApproachSpeed;
    }

    public void BeginAttack(Action impactAction = null)
    {
        if (impactAction != null)
            onImpact = impactAction;

        if (attackTarget == null || actorRoot == null || actorParent == null)
        {
            onImpact?.Invoke();
            return;
        }

        state = State.Approaching;
    }

    public void HoldPosition()
    {
        if (actorRoot == null)
            return;

        state = State.Idle;
    }

    public void CancelAttack()
    {
        HoldPosition();
    }

    public void ResetToStartPosition()
    {
        if (actorRoot == null)
            return;

        actorRoot.localPosition = homeLocalPosition;
        state = State.Idle;
    }

    private void UpdateApproach()
    {
        Vector3 targetLocalPosition = actorParent.InverseTransformPoint(
            attackTarget.position);
        Vector3 current = actorRoot.localPosition;
        Vector3 offset = targetLocalPosition - current;
        float distance = offset.magnitude;
        if (distance <= attackRange)
        {
            state = State.WaitingForImpact;
            onImpact?.Invoke();
            return;
        }

        float moveDistance = Mathf.Max(0f, distance - attackRange);
        float step = Mathf.Min(
            approachSpeed * BattleTempo.ScaleDeltaTime(Time.deltaTime),
            moveDistance);
        actorRoot.localPosition = current + offset.normalized * step;
    }

}
