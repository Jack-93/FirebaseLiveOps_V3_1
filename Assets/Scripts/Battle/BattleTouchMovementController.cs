using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class BattleTouchMovementController : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    ICanvasRaycastFilter
{
    [SerializeField] private RectTransform controlledActor;
    [SerializeField] private RectTransform movementBoundsTarget;
    [SerializeField, Min(0f)] private float movementPadding = 8f;
    [SerializeField, Min(0f)] private float movementSpeed = 850f;

    private RectTransform inputArea;
    private RectTransform actorParent;
    private readonly Vector3[] inputWorldCorners = new Vector3[4];
    private readonly Vector3[] targetWorldCorners = new Vector3[4];
    private Selectable[] selectableBuffer = new Selectable[32];
    private Vector2 targetAnchoredPosition;
    private bool hasDestination;

    private void Awake()
    {
        inputArea = transform as RectTransform;
        actorParent = controlledActor == null
            ? null
            : controlledActor.parent as RectTransform;
        if (movementBoundsTarget == null && controlledActor != null)
        {
            Transform playerVisual = controlledActor.Find("PlayerVisual");
            movementBoundsTarget = playerVisual as RectTransform
                ?? controlledActor;
        }
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

    public bool IsRaycastLocationValid(
        Vector2 screenPoint,
        Camera eventCamera)
    {
        Transform uiRoot = transform.root;
        if (selectableBuffer.Length < Selectable.allSelectableCount)
        {
            selectableBuffer =
                new Selectable[Selectable.allSelectableCount];
        }

        int selectableCount =
            Selectable.AllSelectablesNoAlloc(selectableBuffer);
        for (int index = 0; index < selectableCount; index++)
        {
            Selectable selectable = selectableBuffer[index];
            if (selectable == null ||
                !selectable.IsActive() ||
                !selectable.IsInteractable())
            {
                continue;
            }

            Transform selectableTransform = selectable.transform;
            if (selectableTransform == transform ||
                selectableTransform.IsChildOf(transform) ||
                !selectableTransform.IsChildOf(uiRoot))
            {
                continue;
            }

            if (selectableTransform is RectTransform selectableRect &&
                RectTransformUtility.RectangleContainsScreenPoint(
                    selectableRect,
                    screenPoint,
                    eventCamera))
            {
                return false;
            }
        }

        return true;
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

        Vector2 requestedPosition =
            localPoint - GetAnchorReferencePoint(controlledActor, actorParent);
        targetAnchoredPosition = ClampToInputBounds(requestedPosition);
        hasDestination = true;
    }

    private Vector2 ClampToInputBounds(Vector2 requestedPosition)
    {
        RectTransform boundsTarget = movementBoundsTarget == null
            ? controlledActor
            : movementBoundsTarget;
        if (boundsTarget == null)
            return requestedPosition;

        GetLocalBounds(
            inputArea,
            actorParent,
            inputWorldCorners,
            out Vector2 inputMin,
            out Vector2 inputMax);
        inputMin += Vector2.one * movementPadding;
        inputMax -= Vector2.one * movementPadding;

        GetLocalBounds(
            boundsTarget,
            actorParent,
            targetWorldCorners,
            out Vector2 targetMin,
            out Vector2 targetMax);

        Vector2 requestedDelta =
            requestedPosition - controlledActor.anchoredPosition;
        float minDeltaX = inputMin.x - targetMin.x;
        float maxDeltaX = inputMax.x - targetMax.x;
        float minDeltaY = inputMin.y - targetMin.y;
        float maxDeltaY = inputMax.y - targetMax.y;
        requestedDelta.x = ClampDelta(
            requestedDelta.x,
            minDeltaX,
            maxDeltaX);
        requestedDelta.y = ClampDelta(
            requestedDelta.y,
            minDeltaY,
            maxDeltaY);
        return controlledActor.anchoredPosition + requestedDelta;
    }

    private static void GetLocalBounds(
        RectTransform rect,
        RectTransform reference,
        Vector3[] corners,
        out Vector2 minimum,
        out Vector2 maximum)
    {
        rect.GetWorldCorners(corners);
        Vector3 first = reference.InverseTransformPoint(corners[0]);
        minimum = first;
        maximum = first;
        for (int index = 1; index < corners.Length; index++)
        {
            Vector3 point = reference.InverseTransformPoint(corners[index]);
            minimum = Vector2.Min(minimum, point);
            maximum = Vector2.Max(maximum, point);
        }
    }

    private static float ClampDelta(float value, float minimum, float maximum)
    {
        return minimum <= maximum
            ? Mathf.Clamp(value, minimum, maximum)
            : (minimum + maximum) * 0.5f;
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
