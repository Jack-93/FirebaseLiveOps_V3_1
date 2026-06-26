using System;
using UnityEngine;

public sealed class ShopActionController
{
    private readonly Action<string> showToast;
    private readonly Action refreshTopBar;
    private readonly Action refreshMore;
    private readonly Action refreshShop;

    public ShopActionController(
        Action<string> showToast,
        Action refreshTopBar,
        Action refreshMore,
        Action refreshShop)
    {
        this.showToast = showToast;
        this.refreshTopBar = refreshTopBar;
        this.refreshMore = refreshMore;
        this.refreshShop = refreshShop;
    }

    public async void BuyRealMoneyProduct(RealMoneyProduct product)
    {
        MonetizationManager monetization = MonetizationManager.Instance;
        if (monetization == null)
            return;

        string message = await monetization.PurchaseAsync(product);
        showToast?.Invoke(message);
        RefreshShopState();
    }

    public async void BuyShopProduct(ShopProduct product)
    {
        try
        {
            bool purchased = ShopManager.Instance != null &&
                await ShopManager.Instance.TryPurchaseAsync(product);
            showToast?.Invoke(purchased
                ? $"Purchased: {ShopManager.GetDescription(product)}."
                : "Not enough Gems.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            showToast?.Invoke("Purchase failed. Gems were restored.");
        }

        RefreshShopState();
    }

    public async void WatchRewardedAd()
    {
        MonetizationManager monetization = MonetizationManager.Instance;
        if (monetization == null)
            return;

        string message = await monetization.ShowRewardedAdAsync(
            RewardedAdPlacement.ShopFreeGems);
        showToast?.Invoke(message);
        RefreshShopState();
    }

    private void RefreshShopState()
    {
        refreshTopBar?.Invoke();
        refreshMore?.Invoke();
        refreshShop?.Invoke();
    }
}
