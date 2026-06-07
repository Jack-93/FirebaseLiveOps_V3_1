using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public sealed class GoogleSignInConfig
{
    public string webClientId;
}

public sealed class GoogleCredentialTokenProvider : IAccountTokenProvider
{
    public AccountLinkProvider Provider => AccountLinkProvider.Google;

    public Task<AccountLinkTokens> RequestTokensAsync()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return RequestAndroidTokensAsync();
#else
        return Task.FromException<AccountLinkTokens>(
            new NotSupportedException(
                "Google login is available on Android builds."));
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static Task<AccountLinkTokens> RequestAndroidTokensAsync()
    {
        string webClientId = LoadWebClientId();
        if (string.IsNullOrWhiteSpace(webClientId))
        {
            throw new InvalidOperationException(
                "Google web client ID is missing.");
        }

        TaskCompletionSource<AccountLinkTokens> completion =
            new TaskCompletionSource<AccountLinkTokens>();

        AndroidJavaObject activity = GetUnityActivity();
        AndroidJavaObject credentialManager =
            new AndroidJavaClass(
                "androidx.credentials.CredentialManager")
            .CallStatic<AndroidJavaObject>("create", activity);

        AndroidJavaObject googleOption =
            new AndroidJavaObject(
                "com.google.android.libraries.identity.googleid.GetGoogleIdOption$Builder")
            .Call<AndroidJavaObject>(
                "setServerClientId",
                webClientId)
            .Call<AndroidJavaObject>(
                "setFilterByAuthorizedAccounts",
                false)
            .Call<AndroidJavaObject>(
                "setAutoSelectEnabled",
                false)
            .Call<AndroidJavaObject>("build");

        AndroidJavaObject request =
            new AndroidJavaObject(
                "androidx.credentials.GetCredentialRequest$Builder")
            .Call<AndroidJavaObject>("addCredentialOption", googleOption)
            .Call<AndroidJavaObject>("build");

        AndroidJavaObject cancellationSignal =
            new AndroidJavaObject("android.os.CancellationSignal");
        AndroidJavaObject executor =
            activity.Call<AndroidJavaObject>("getMainExecutor");
        GoogleCredentialCallback callback =
            new GoogleCredentialCallback(completion);

        credentialManager.Call(
            "getCredentialAsync",
            activity,
            request,
            cancellationSignal,
            executor,
            callback);

        return completion.Task;
    }

    private static string LoadWebClientId()
    {
        TextAsset configAsset =
            Resources.Load<TextAsset>("GoogleSignInConfig");
        if (configAsset == null)
            return "";

        GoogleSignInConfig config =
            JsonUtility.FromJson<GoogleSignInConfig>(
                configAsset.text);
        return config?.webClientId ?? "";
    }

    private static AndroidJavaObject GetUnityActivity()
    {
        AndroidJavaClass unityPlayer =
            new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayer.GetStatic<AndroidJavaObject>(
            "currentActivity");
    }

    private sealed class GoogleCredentialCallback : AndroidJavaProxy
    {
        private readonly TaskCompletionSource<AccountLinkTokens>
            completion;

        public GoogleCredentialCallback(
            TaskCompletionSource<AccountLinkTokens> completion)
            : base("androidx.credentials.CredentialManagerCallback")
        {
            this.completion = completion;
        }

        public void onResult(AndroidJavaObject response)
        {
            try
            {
                AndroidJavaObject credential =
                    response.Call<AndroidJavaObject>("getCredential");
                string type = credential.Call<string>("getType");
                string googleType =
                    new AndroidJavaClass(
                        "com.google.android.libraries.identity.googleid.GoogleIdTokenCredential")
                    .GetStatic<string>(
                        "TYPE_GOOGLE_ID_TOKEN_CREDENTIAL");

                if (type != googleType)
                {
                    completion.TrySetException(
                        new InvalidOperationException(
                            "Google credential type was not returned."));
                    return;
                }

                AndroidJavaObject googleCredential =
                    new AndroidJavaClass(
                        "com.google.android.libraries.identity.googleid.GoogleIdTokenCredential")
                    .CallStatic<AndroidJavaObject>(
                        "createFrom",
                        credential.Call<AndroidJavaObject>("getData"));

                completion.TrySetResult(
                    new AccountLinkTokens
                    {
                        IdToken =
                            googleCredential.Call<string>("getIdToken")
                    });
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
        }

        public void onError(AndroidJavaObject exception)
        {
            string message = exception == null
                ? "Google login was cancelled."
                : exception.Call<string>("getMessage");
            completion.TrySetException(
                new InvalidOperationException(message));
        }
    }
#endif
}
