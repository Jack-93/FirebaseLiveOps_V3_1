using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public enum AccountLinkProvider
{
    Google,
    Apple
}

public sealed class AccountLinkTokens
{
    public string IdToken;
    public string AccessToken;
    public string RawNonce;
}

public interface IAccountTokenProvider
{
    AccountLinkProvider Provider { get; }
    Task<AccountLinkTokens> RequestTokensAsync();
}

public sealed class AccountLinkResult
{
    public bool Success;
    public string Message;

    public static AccountLinkResult Completed(string message)
    {
        return new AccountLinkResult
        {
            Success = true,
            Message = message
        };
    }

    public static AccountLinkResult Failed(string message)
    {
        return new AccountLinkResult
        {
            Success = false,
            Message = message
        };
    }
}

public class AccountLinkManager : MonoBehaviour
{
    public static AccountLinkManager Instance;

    public event Action OnAccountChanged;
    public bool IsBusy { get; private set; }

    private readonly Dictionary<AccountLinkProvider, IAccountTokenProvider>
        tokenProviders =
            new Dictionary<AccountLinkProvider, IAccountTokenProvider>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RegisterTokenProvider(new GoogleCredentialTokenProvider());
    }

    public void RegisterTokenProvider(IAccountTokenProvider provider)
    {
        if (provider != null)
            tokenProviders[provider.Provider] = provider;
    }

    public bool IsLinked(AccountLinkProvider provider)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
            return false;

        string providerId = GetProviderId(provider);
        foreach (IUserInfo info in user.ProviderData)
        {
            if (info != null && info.ProviderId == providerId)
                return true;
        }

        return false;
    }

    public string GetAccountSummary()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
            return "No account is signed in.";

        List<string> linked = new List<string>();
        if (IsLinked(AccountLinkProvider.Google))
            linked.Add("Google");

        string type = user.IsAnonymous ? "Guest" : "Linked";
        string providers = linked.Count == 0
            ? "None"
            : string.Join(", ", linked);
        string uid = user.UserId ?? "-";
        string shortUid = uid.Length > 20
            ? uid.Substring(0, 20) + "..."
            : uid;

        return
            $"Account: {type}\n" +
            $"Linked: {providers}\n" +
            $"UID: {shortUid}";
    }

    public async Task<AccountLinkResult> LinkAsync(
        AccountLinkProvider provider)
    {
        if (IsBusy)
            return AccountLinkResult.Failed(
                "Another account request is already running.");

        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
            return AccountLinkResult.Failed("No signed-in user.");

        if (IsLinked(provider))
        {
            return AccountLinkResult.Completed(
                $"{provider} is already linked.");
        }

        IsBusy = true;
        OnAccountChanged?.Invoke();

        try
        {
            if (tokenProviders.TryGetValue(
                    provider,
                    out IAccountTokenProvider tokenProvider))
            {
                AccountLinkTokens tokens =
                    await tokenProvider.RequestTokensAsync();
                return await LinkTokensAsync(provider, tokens);
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (provider == AccountLinkProvider.Apple)
                return await LinkAppleWithWebFlowAsync(user);
#endif

            return AccountLinkResult.Failed(
                GetMissingProviderMessage(provider));
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            return AccountLinkResult.Failed(
                GetFriendlyError(exception));
        }
        finally
        {
            IsBusy = false;
            OnAccountChanged?.Invoke();
        }
    }

    public Task<AccountLinkResult> LinkGoogleTokensAsync(
        string idToken,
        string accessToken)
    {
        return LinkTokensAsync(
            AccountLinkProvider.Google,
            new AccountLinkTokens
            {
                IdToken = idToken,
                AccessToken = accessToken
            });
    }

    public Task<AccountLinkResult> LinkAppleTokensAsync(
        string idToken,
        string rawNonce)
    {
        return LinkTokensAsync(
            AccountLinkProvider.Apple,
            new AccountLinkTokens
            {
                IdToken = idToken,
                RawNonce = rawNonce
            });
    }

    private async Task<AccountLinkResult> LinkTokensAsync(
        AccountLinkProvider provider,
        AccountLinkTokens tokens)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
            return AccountLinkResult.Failed("No signed-in user.");
        if (tokens == null || string.IsNullOrWhiteSpace(tokens.IdToken))
            return AccountLinkResult.Failed(
                $"{provider} did not return a valid ID token.");

        Credential credential;
        switch (provider)
        {
            case AccountLinkProvider.Google:
                credential = GoogleAuthProvider.GetCredential(
                    tokens.IdToken,
                    tokens.AccessToken);
                break;
            case AccountLinkProvider.Apple:
                credential = OAuthProvider.GetCredential(
                    "apple.com",
                    tokens.IdToken,
                    tokens.RawNonce,
                    null);
                break;
            default:
                return AccountLinkResult.Failed(
                    "Unsupported account provider.");
        }

        await user.LinkWithCredentialAsync(credential);
        OnAccountChanged?.Invoke();
        return AccountLinkResult.Completed(
            $"{provider} account linked. Your game data is preserved.");
    }

    private static async Task<AccountLinkResult>
        LinkAppleWithWebFlowAsync(FirebaseUser user)
    {
        FederatedOAuthProviderData providerData =
            new FederatedOAuthProviderData("apple.com")
            {
                Scopes = new List<string> { "email", "name" }
            };
        FederatedOAuthProvider provider =
            new FederatedOAuthProvider(providerData);

        try
        {
            await user.LinkWithProviderAsync(provider);
        }
        finally
        {
            provider.Dispose();
            providerData.Dispose();
        }

        return AccountLinkResult.Completed(
            "Apple account linked. Your game data is preserved.");
    }

    private static string GetProviderId(AccountLinkProvider provider)
    {
        return provider == AccountLinkProvider.Google
            ? GoogleAuthProvider.ProviderId
            : "apple.com";
    }

    private static string GetMissingProviderMessage(
        AccountLinkProvider provider)
    {
        if (provider == AccountLinkProvider.Google)
        {
            return
                "Google Sign-In SDK is not installed yet. " +
                "The Firebase linking layer is ready.";
        }

        return
            "Apple Sign-In token provider is not installed for this " +
            "platform yet. The Firebase linking layer is ready.";
    }

    private static string GetFriendlyError(Exception exception)
    {
        FirebaseException firebaseException =
            FindFirebaseException(exception);
        if (firebaseException == null)
            return "Account linking failed. Please try again.";

        AuthError error = (AuthError)firebaseException.ErrorCode;
        switch (error)
        {
            case AuthError.CredentialAlreadyInUse:
            case AuthError.AccountExistsWithDifferentCredentials:
            case AuthError.EmailAlreadyInUse:
                return
                    "This account is already used by another game account. " +
                    "Your current guest data was not changed.";
            case AuthError.ProviderAlreadyLinked:
                return "This provider is already linked.";
            case AuthError.NetworkRequestFailed:
                return "Network error. Please try again.";
            case AuthError.OperationNotAllowed:
                return
                    "This login provider is not enabled in Firebase Console.";
            case AuthError.WebContextCancelled:
                return "Account linking was cancelled.";
            default:
                return $"Account linking failed: {error}";
        }
    }

    private static FirebaseException FindFirebaseException(
        Exception exception)
    {
        if (exception is FirebaseException firebaseException)
            return firebaseException;

        if (exception is AggregateException aggregate)
        {
            foreach (Exception inner in aggregate.Flatten().InnerExceptions)
            {
                FirebaseException found = FindFirebaseException(inner);
                if (found != null)
                    return found;
            }
        }

        return exception?.InnerException == null
            ? null
            : FindFirebaseException(exception.InnerException);
    }
}
