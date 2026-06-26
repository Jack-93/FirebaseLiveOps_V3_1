using System;

public sealed class EquipmentActionController
{
    private readonly BattleManager battleManager;
    private readonly Action<string> showToast;
    private readonly Action refreshEquipment;

    public EquipmentActionController(
        BattleManager battleManager,
        Action<string> showToast,
        Action refreshEquipment)
    {
        this.battleManager = battleManager;
        this.showToast = showToast;
        this.refreshEquipment = refreshEquipment;
    }

    public async void Upgrade(EquipmentSlot slot)
    {
        if (EquipmentManager.Instance == null)
            return;

        bool upgraded =
            await EquipmentManager.Instance.TryUpgradeAsync(slot);
        if (!upgraded)
        {
            showToast?.Invoke("Equipment missing or not enough Gold.");
            return;
        }

        battleManager?.RefreshPlayerStats();
        refreshEquipment?.Invoke();
        showToast?.Invoke($"{slot} upgraded.");
    }

    public void HandleDropped(string itemName)
    {
        battleManager?.RefreshPlayerStats();
        showToast?.Invoke($"Equipment found: {itemName}");
        refreshEquipment?.Invoke();
    }
}
