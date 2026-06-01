using Firebase.RemoteConfig;
using UnityEngine;

public class GachaBannerController : MonoBehaviour
{
    public GameObject limitedBanner;
    public GameObject defaultBanner;

    void Start()
    {
        bool eventEnabled =
            FirebaseRemoteConfig.DefaultInstance
            .GetValue("nikke_gacha_event")
            .BooleanValue;

        if(eventEnabled)
        {
            limitedBanner.SetActive(true);
            defaultBanner.SetActive(false);

            Debug.Log("Limited Banner Active");
        }
        else
        {
            limitedBanner.SetActive(false);
            defaultBanner.SetActive(true);

            Debug.Log("Default Banner Active");
        }
    }
}
