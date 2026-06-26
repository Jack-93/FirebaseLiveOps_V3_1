using System;

public sealed class AccountActionController
{
    private readonly MainGameBootstrap bootstrap;
    private readonly Action refreshAccount;
    private readonly Action refreshMore;
    private readonly Action<string> showToast;

    public AccountActionController(
        MainGameBootstrap bootstrap,
        Action refreshAccount,
        Action refreshMore,
        Action<string> showToast)
    {
        this.bootstrap = bootstrap;
        this.refreshAccount = refreshAccount;
        this.refreshMore = refreshMore;
        this.showToast = showToast;
    }

    public void LinkGoogle()
    {
        Link(AccountLinkProvider.Google);
    }

    private async void Link(AccountLinkProvider provider)
    {
        if (bootstrap == null)
            return;

        refreshAccount?.Invoke();
        AccountLinkResult result =
            await bootstrap.LinkAccountAsync(provider);
        refreshAccount?.Invoke();
        refreshMore?.Invoke();
        showToast?.Invoke(result.Message);
    }
}
