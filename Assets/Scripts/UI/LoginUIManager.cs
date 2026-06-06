using System;
using System.Threading.Tasks;
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
    private bool isInitializingPlayer;

    private async void Start()
    {
        try
        {
            await FirebaseManager.WaitUntilReadyAsync();
            auth = FirebaseAuth.DefaultInstance;

            if (auth.CurrentUser == null)
            {
                SetLoginStatus("Not Logged In", null);
                return;
            }

            user = auth.CurrentUser;
            SetLoginStatus("Auto Login Success", user);
            Debug.Log("[Auth] Auto Login");

            CheckNewUser();
            await InitializePlayerAfterLoginAsync();
        }
        catch (Exception exception)
        {
            SetLoginStatus("Initialization Failed", null);
            Debug.LogException(exception);
        }
    }

    public async void GuestLogin()
    {
        if (isInitializingPlayer)
            return;

        SetLoginStatus("Logging In...", null);

        try
        {
            await FirebaseManager.WaitUntilReadyAsync();
            auth = auth ?? FirebaseAuth.DefaultInstance;

            AuthResult result = await auth.SignInAnonymouslyAsync();
            user = result.User;

            SetLoginStatus("Guest Login Success", user);
            Debug.Log($"[Auth] Login Success: {user.UserId}");

            CheckNewUser();
            await InitializePlayerAfterLoginAsync();
        }
        catch (Exception exception)
        {
            SetLoginStatus("Login Failed", null);
            Debug.LogError($"[Auth] {exception.Message}");
        }
    }

    public void Logout()
    {
        if (auth == null)
            return;

        auth.SignOut();
        user = null;

        PlayerDataManager.Instance?.SetPlayerData(new PlayerData());
        SetLoginStatus("Logged Out", null);

        Debug.Log("[Auth] Logout");
    }

    private async Task InitializePlayerAfterLoginAsync()
    {
        if (isInitializingPlayer || user == null)
            return;

        isInitializingPlayer = true;

        try
        {
            PlayerData data =
                await FirestoreManager.Instance.LoadPlayerDataAsync();

            if (data == null)
                return;

            data.uid = user.UserId;
            data.lastLoginDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            PlayerDataManager.Instance.SetPlayerData(data);

            await FirestoreManager.Instance.LoadGlobalMailsAsync();
            await FirestoreManager.Instance.SavePlayerDataAsync(data);

            PlayerDataManager.Instance.NotifyPlayerDataChanged();
            AnalyticsManager.Instance?.LogLogin();

            Debug.Log("[Login] Player Initialized");
        }
        finally
        {
            isInitializingPlayer = false;
        }
    }

    private void CheckNewUser()
    {
        if (user == null)
            return;

        bool isNewUser =
            user.Metadata.CreationTimestamp ==
            user.Metadata.LastSignInTimestamp;

        Debug.Log(isNewUser
            ? "[Auth] New User"
            : "[Auth] Returning User");

        if (statusText != null)
            statusText.text += isNewUser
                ? "\nNew User"
                : "\nReturning User";
    }

    private void SetLoginStatus(string status, FirebaseUser currentUser)
    {
        if (statusText != null)
            statusText.text = status;

        if (uidText != null)
        {
            uidText.text = currentUser == null
                ? "UID: -"
                : $"UID: {currentUser.UserId}";
        }
    }
}
