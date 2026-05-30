using Firebase.Auth;
using TMPro;
using UnityEngine;
public class LoginUIManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text statusText;
    public TMP_Text uidText;

    private FirebaseAuth auth;
    private FirebaseUser user;

    async void Start()
    {
        // Firebase 초기화 대기
        while (!FirebaseManager.IsFirebaseReady)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        auth = FirebaseAuth.DefaultInstance;

        // 자동 로그인 체크
        if (auth.CurrentUser != null)
        {
            user = auth.CurrentUser;

            statusText.text = "Auto Login Success";
            uidText.text = $"UID: {user.UserId}";

            Debug.Log("[Auth] Auto Login");

            FirestoreManager.Instance.LoadPlayerData();
            DailyRewardManager.Instance.CheckDailyReward();

            CheckNewUser();
        }
        else
        {
            statusText.text = "Not Logged In";
            uidText.text = "UID: -";
        }


    }

    public async void GuestLogin()
    {
        statusText.text = "Logging In...";

        try
        {
            var result =
                await auth.SignInAnonymouslyAsync();

            user = result.User;

            statusText.text = "Guest Login Success";
            uidText.text = $"UID: {user.UserId}";

            // 로그인 후 초기 유저 데이터 로드
            Debug.Log($"[Auth] Login Success: {user.UserId}");

            FirestoreManager.Instance.LoadPlayerData();
            DailyRewardManager.Instance.CheckDailyReward();

            CheckNewUser();
        }
        catch (System.Exception e)
        {
            statusText.text = "Login Failed";

            Debug.LogError($"[Auth] {e.Message}");
        }
    }

    private void CheckNewUser()
    {
        if (user.Metadata.CreationTimestamp ==
            user.Metadata.LastSignInTimestamp)
        {
            Debug.Log("[Auth] New User");

            statusText.text += "\\nNew User";
        }
        else
        {
            Debug.Log("[Auth] Returning User");

            statusText.text += "\\nReturning User";
        }
    }

    public void Logout()
    {
        auth.SignOut();

        user = null;

        statusText.text = "Logged Out";
        uidText.text = "UID: -";

        Debug.Log("[Auth] Logout");
    }
}