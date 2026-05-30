
using Firebase.Analytics;
using UnityEngine;

public enum ABGroup
{
    A,
    B
}

public class ABTestManager : MonoBehaviour
{
    public ABGroup currentGroup;

    [SerializeField]
    private GameObject specialEffect;

    void Start()
    {
        currentGroup =
            Random.Range(0, 2) == 0
            ? ABGroup.A
            : ABGroup.B;

        FirebaseAnalytics.LogEvent(
            "ab_group_assigned",
            new Parameter("group", currentGroup.ToString())
        );

        ApplyGroup();
    }

    void ApplyGroup()
    {
        if (currentGroup == ABGroup.B)
        {
            specialEffect.SetActive(true);
            Debug.Log("[ABTest] Group B");
        }
        else
        {
            specialEffect.SetActive(false);
            Debug.Log("[ABTest] Group A");
        }
    }
}
