
using Firebase.Analytics;
using UnityEngine;

public class SessionTracker : MonoBehaviour
{
    private float sessionStartTime;

    void Start()
    {
        sessionStartTime = Time.time;
    }

    void OnApplicationQuit()
    {
        float sessionLength = Time.time - sessionStartTime;

        FirebaseAnalytics.LogEvent(
            "session_end",
            new Parameter("session_seconds", sessionLength)
        );

        Debug.Log($"[Session] {sessionLength}");
    }
}
