
using Firebase;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static bool IsFirebaseReady;

    async void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status == DependencyStatus.Available)
        {
            IsFirebaseReady = true;

            Debug.Log("[Firebase] Ready");
        }
        else
        {
            Debug.LogError($"[Firebase] Error: {status}");
        }
    }
}
