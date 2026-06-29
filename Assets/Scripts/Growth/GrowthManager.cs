using System;
using System.Threading.Tasks;
using UnityEngine;

public class GrowthManager : MonoBehaviour
{
    public event Action<UpgradeType> OnUpgraded;

    private BattleManager battleManager;

    public void Initialize(BattleManager battle)
    {
        battleManager = battle;
    }

    public int GetLevel(UpgradeType type)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return 1;

        switch (type)
        {
            case UpgradeType.Attack:
                return data.attackLevel;
            case UpgradeType.Health:
                return data.healthLevel;
            case UpgradeType.AttackSpeed:
                return data.attackSpeedLevel;
            default:
                return 1;
        }
    }

    public int GetCost(UpgradeType type)
    {
        return GameBalance.GetUpgradeCost(type, GetLevel(type));
    }

    public Task<bool> TryUpgradeAsync(UpgradeType type)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return Task.FromResult(false);

        if (!IsValidUpgradeType(type))
            return Task.FromResult(false);

        int cost = GetCost(type);
        if (data.gold < cost)
        {
            Debug.Log("[Growth] Not enough gold.");
            return Task.FromResult(false);
        }

        data.gold -= cost;

        switch (type)
        {
            case UpgradeType.Attack:
                data.attackLevel++;
                break;
            case UpgradeType.Health:
                data.healthLevel++;
                break;
            case UpgradeType.AttackSpeed:
                data.attackSpeedLevel++;
                break;
        }

        PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
        RefreshBattleStatsSafely();
        SafeEvent.Invoke(
            OnUpgraded,
            type,
            "Growth",
            nameof(OnUpgraded));

        return Task.FromResult(true);
    }

    public async void UpgradeAttack()
    {
        await RunUpgradeAsync(UpgradeType.Attack);
    }

    public async void UpgradeHealth()
    {
        await RunUpgradeAsync(UpgradeType.Health);
    }

    public async void UpgradeAttackSpeed()
    {
        await RunUpgradeAsync(UpgradeType.AttackSpeed);
    }

    private async Task RunUpgradeAsync(UpgradeType type)
    {
        try
        {
            await TryUpgradeAsync(type);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void RefreshBattleStatsSafely()
    {
        try
        {
            battleManager?.RefreshPlayerStats();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[Growth] Battle stat refresh failed: " +
                exception.Message);
            Debug.LogException(exception);
        }
    }

    private static bool IsValidUpgradeType(UpgradeType type)
    {
        return type == UpgradeType.Attack ||
            type == UpgradeType.Health ||
            type == UpgradeType.AttackSpeed;
    }

}
