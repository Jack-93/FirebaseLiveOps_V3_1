using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class BattleTouchMovementController : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler
{
    [SerializeField] private RectTransform controlledActor;
    [SerializeField, Min(0f)] private float movementSpeed = 850f;

    private RectTransform inputArea;
    private RectTransform actorParent;
    private Vector2 targetAnchoredPosition;
    private bool hasDestination;

    private void Awake()
    {
        inputArea = transform as RectTransform;
        actorParent = controlledActor == null
            ? null
            : controlledActor.parent as RectTransform;
        if (controlledActor != null)
            targetAnchoredPosition = controlledActor.anchoredPosition;
    }

    private void Update()
    {
        if (!hasDestination || controlledActor == null)
            return;

        controlledActor.anchoredPosition = Vector2.MoveTowards(
            controlledActor.anchoredPosition,
            targetAnchoredPosition,
            movementSpeed *
            BattleTempo.ScaleDeltaTime(Time.unscaledDeltaTime));

        if (Vector2.SqrMagnitude(
                controlledActor.anchoredPosition - targetAnchoredPosition) <
            0.01f)
        {
            controlledActor.anchoredPosition = targetAnchoredPosition;
            hasDestination = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetDestination(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetDestination(eventData);
    }

    private void SetDestination(PointerEventData eventData)
    {
        if (inputArea == null || actorParent == null || controlledActor == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                actorParent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        Vector3[] corners = new Vector3[4];
        inputArea.GetWorldCorners(corners);
        Vector2 min = actorParent.InverseTransformPoint(corners[0]);
        Vector2 max = actorParent.InverseTransformPoint(corners[2]);
        localPoint.x = Mathf.Clamp(localPoint.x, min.x, max.x);
        localPoint.y = Mathf.Clamp(localPoint.y, min.y, max.y);

        targetAnchoredPosition =
            localPoint - GetAnchorReferencePoint(controlledActor, actorParent);
        hasDestination = true;
    }

    private static Vector2 GetAnchorReferencePoint(
        RectTransform rect,
        RectTransform parent)
    {
        Vector2 anchorCenter = (rect.anchorMin + rect.anchorMax) * 0.5f;
        Rect parentRect = parent.rect;
        return new Vector2(
            Mathf.Lerp(parentRect.xMin, parentRect.xMax, anchorCenter.x),
            Mathf.Lerp(parentRect.yMin, parentRect.yMax, anchorCenter.y));
    }
}
