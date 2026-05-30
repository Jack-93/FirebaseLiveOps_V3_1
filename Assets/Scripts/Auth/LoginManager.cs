
using Firebase.Auth;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance;

    private FirebaseAuth auth;
    private FirebaseUser user;

    public FirebaseUser CurrentUser => user;

    async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        while (!FirebaseManager.IsFirebaseReady)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        auth = FirebaseAuth.DefaultInstance;

        await GuestLogin();
    }

    public async System.Threading.Tasks.Task GuestLogin()
    {
        try
        {
            var result = await auth.SignInAnonymouslyAsync();

            user = result.User;

            Debug.Log("[Auth] Guest Login Success");
            Debug.Log($"[Auth] UID: {user.UserId}");

            CheckNewUser();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Auth] Login Failed: {e.Message}");
        }
    }

    private void CheckNewUser()
    {
        if (user.Metadata.CreationTimestamp ==
            user.Metadata.LastSignInTimestamp)
        {
            Debug.Log("[Auth] New User");
        }
        else
        {
            Debug.Log("[Auth] Returning User");
        }
    }

    public void Logout()
    {
        auth.SignOut();

        Debug.Log("[Auth] Logout");
    }
}
