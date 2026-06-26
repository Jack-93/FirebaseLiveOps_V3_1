public sealed class SessionActionController
{
    private readonly MainGameBootstrap bootstrap;

    public SessionActionController(MainGameBootstrap bootstrap)
    {
        this.bootstrap = bootstrap;
    }

    public void SaveNow()
    {
        bootstrap?.SaveNow();
    }

    public void Logout()
    {
        bootstrap?.Logout();
    }

    public void StartGoogleLogin()
    {
        bootstrap?.StartGoogleLogin();
    }

    public void StartGuestLogin()
    {
        bootstrap?.StartGuestLogin();
    }

    public void RetryInitialization()
    {
        bootstrap?.RetryInitialization();
    }
}
