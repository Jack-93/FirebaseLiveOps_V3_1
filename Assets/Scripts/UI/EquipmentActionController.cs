using System;

public sealed class EquipmentActionController
{
    private readonly BattleManager battleManager;
    private readonly Action<string> showToast;
    private readonly Action refreshEquipment;
    private readonly Action<EquipmentCubePreview> showCubePreview;

    public EquipmentActionController(
        BattleManager battleManager,
        Action<string> showToast,
        Action refreshEquipment,
        Action<EquipmentCubePreview> showCubePreview)
    {
        this.battleManager = battleManager;
        this.showToast = showToast;
        this.refreshEquipment = refreshEquipment;
        this.showCubePreview = showCubePreview;
    }

    public bool Upgrade(
        EquipmentSlot slot,
        Action<EquipmentStarForceResult> showResult)
    {
        if (EquipmentManager.Instance == null)
            return false;

        if (!EquipmentManager.Instance.TryStarForce(slot, out var result))
        {
            showToast?.Invoke(
                "\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654\uB97C \uD560 \uC218 \uC5C6\uAC70\uB098 " +
                "\uACE8\uB4DC\uAC00 \uBD80\uC871\uD569\uB2C8\uB2E4.");
            return false;
        }

        battleManager?.RefreshPlayerStats();
        refreshEquipment?.Invoke();
        showResult?.Invoke(result);
        return true;
    }

    public void HandleDropped(string itemName)
    {
        battleManager?.RefreshPlayerStats();
        showToast?.Invoke($"Equipment found: {itemName}");
        refreshEquipment?.Invoke();
    }

    public void EquipNextOwned(EquipmentSlot slot)
    {
        if (EquipmentManager.Instance == null)
            return;

        if (!EquipmentManager.Instance.TryEquipNextOwned(
                slot,
                out EquipmentDefinition equipped))
        {
            showToast?.Invoke("No other owned equipment.");
            return;
        }

        battleManager?.RefreshPlayerStats();
        refreshEquipment?.Invoke();
        showToast?.Invoke($"Equipped: {equipped.DisplayName}");
    }

    public bool Equip(string instanceId)
    {
        if (EquipmentManager.Instance == null ||
            !EquipmentManager.Instance.TryEquip(instanceId))
        {
            showToast?.Invoke("\uC7A5\uBE44\uB97C \uCC29\uC6A9\uD560 \uC218 \uC5C6\uC2B5\uB2C8\uB2E4.");
            return false;
        }

        battleManager?.RefreshPlayerStats();
        refreshEquipment?.Invoke();
        showToast?.Invoke("\uC7A5\uBE44\uB97C \uCC29\uC6A9\uD588\uC2B5\uB2C8\uB2E4.");
        return true;
    }

    public bool Dismantle(string instanceId)
    {
        if (EquipmentManager.Instance == null ||
            !EquipmentManager.Instance.TryDismantle(instanceId, out var result))
        {
            showToast?.Invoke(
                "\uCC29\uC6A9 \uC911\uC778 \uC7A5\uBE44\uB294 \uD574\uCCB4\uD560 \uC218 \uC5C6\uC2B5\uB2C8\uB2E4.");
            return false;
        }

        refreshEquipment?.Invoke();
        showToast?.Invoke(
            result.equipmentName + " \uD574\uCCB4: \uBE44\uD589\uB2E8 \uC7A5\uBE44 \uCF54\uC778 x" +
            result.coinReward);
        return true;
    }

    public bool RerollOptions(EquipmentSlot slot)
    {
        if (EquipmentManager.Instance == null)
            return false;

        if (!EquipmentManager.Instance.TryCreateCubePreview(
                slot,
                out EquipmentCubePreview preview))
        {
            showToast?.Invoke(
                "\uC635\uC158 \uC7AC\uC124\uC815\uC744 \uD560 \uC218 \uC5C6\uAC70\uB098 " +
                "\uBE44\uD589\uB2E8 \uC7A5\uBE44 \uCF54\uC778\uC774 \uBD80\uC871\uD569\uB2C8\uB2E4.");
            return false;
        }

        refreshEquipment?.Invoke();
        showCubePreview?.Invoke(preview);
        return true;
    }

    public void ResolveCubePreview(bool applyNew)
    {
        if (EquipmentManager.Instance == null ||
            !EquipmentManager.Instance.TryResolveCubePreview(applyNew))
        {
            showToast?.Invoke(
                "\uC635\uC158 \uC7AC\uC124\uC815 \uACB0\uACFC\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4.");
            return;
        }

        battleManager?.RefreshPlayerStats();
        refreshEquipment?.Invoke();
        showToast?.Invoke(applyNew
            ? "\uC0C8 \uC635\uC158\uC744 \uC801\uC6A9\uD588\uC2B5\uB2C8\uB2E4."
            : "\uAE30\uC874 \uC635\uC158\uC744 \uC720\uC9C0\uD588\uC2B5\uB2C8\uB2E4.");
    }
}
