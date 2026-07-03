using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class BattleSlotGuide : MonoBehaviour
{
    [SerializeField] private string label;
    [SerializeField] private Color color = Color.cyan;

    public void Configure(string guideLabel, Color guideColor)
    {
        label = guideLabel;
        color = guideColor;
    }

    private void OnDrawGizmos()
    {
        RectTransform rect = transform as RectTransform;
        if (rect == null)
            return;

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        Color drawColor = color;
        drawColor.a = 0.8f;
        Gizmos.color = drawColor;
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);

#if UNITY_EDITOR
        Vector3 center = (corners[0] + corners[2]) * 0.5f;
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = drawColor;
        Handles.Label(center, label, style);
#endif
    }
}
