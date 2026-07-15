using System;

public sealed class GrowthActionController
{
    private readonly GrowthManager growthManager;
    private readonly Action<string> showToast;
    private readonly Action refreshGrowth;
    private readonly Action refreshBattle;
    private readonly Action refreshTopBar;

    public GrowthActionController(
        GrowthManager growthManager,
        Action<string> showToast,
        Action refreshGrowth,
        Action refreshBattle,
        Action refreshTopBar)
    {
        this.growthManager = growthManager;
        this.showToast = showToast;
        this.refreshGrowth = refreshGrowth;
        this.refreshBattle = refreshBattle;
        this.refreshTopBar = refreshTopBar;
    }

    public async void Upgrade(UpgradeType type)
    {
        if (growthManager == null)
            return;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        int cost = growthManager.GetCost(type);
        if (data.gold < cost)
        {
            showToast?.Invoke(
                $"Need {CompactNumberFormatter.Format(cost - data.gold)} more Gold.");
            return;
        }

        bool upgraded = await growthManager.TryUpgradeAsync(type);
        if (!upgraded)
        {
            showToast?.Invoke("Upgrade failed.");
            return;
        }

        refreshGrowth?.Invoke();
        refreshBattle?.Invoke();
        refreshTopBar?.Invoke();
        showToast?.Invoke(
            $"{GetUpgradeDisplayName(type)} Lv." +
            $"{CompactNumberFormatter.Format(growthManager.GetLevel(type))}");
    }

    private static string GetUpgradeDisplayName(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Attack:
                return "Attack";
            case UpgradeType.Health:
                return LocalizationManager.Text(
                    "Pole Durability",
                    "\uC804\uBD07\uB300 \uB0B4\uAD6C\uB3C4");
            case UpgradeType.AttackSpeed:
                return "Attack Speed";
            default:
                return type.ToString();
        }
    }
}
