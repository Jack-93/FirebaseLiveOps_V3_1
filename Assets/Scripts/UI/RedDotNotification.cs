using UnityEngine;

public class RedDotNotification : MonoBehaviour
{
    public GameObject redDotObject;

    private bool hasFreeReward = true;

    void Start()
    {
        UpdateRedDot();
    }

    public void ClaimReward()
    {
        hasFreeReward = false;

        UpdateRedDot();

        Debug.Log("Reward Claimed");
    }

    void UpdateRedDot()
    {
        redDotObject.SetActive(hasFreeReward);
    }
}
