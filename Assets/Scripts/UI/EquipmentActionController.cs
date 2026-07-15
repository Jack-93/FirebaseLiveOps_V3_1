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

    public void Upgrade(EquipmentSlot slot)
    {
        if (EquipmentManager.Instance == null)
            return;

        if (!EquipmentManager.Instance.TryStarForce(slot, out var result))
        {
            showToast?.Invoke(
                "\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654\uB97C \uD560 \uC218 \uC5C6\uAC70\uB098 " +
                "\uACE8\uB4DC\uAC00 \uBD80\uC871\uD569\uB2C8\uB2E4.");
            return;
        }

        battleManager?.RefreshPlayerStats();
        refreshEquipment?.Invoke();
        if (result.success)
        {
            showToast?.Invoke(result.chanceTime
                ? $"\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654 \uD655\uC815 \uC131\uACF5! {result.currentStar}\uC131"
                : $"\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654 \uC131\uACF5! {result.currentStar}\uC131");
            return;
        }

        showToast?.Invoke(result.downgraded
            ? $"\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654 \uC2E4\uD328: {result.currentStar}\uC131"
            : "\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654 \uC2E4\uD328: \uB2E8\uACC4 \uC720\uC9C0");
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

    public void RerollOptions(EquipmentSlot slot)
    {
        if (EquipmentManager.Instance == null)
            return;

        if (!EquipmentManager.Instance.TryCreateCubePreview(
                slot,
                out EquipmentCubePreview preview))
        {
            showToast?.Invoke(
                "\uC635\uC158 \uC7AC\uC124\uC815\uC744 \uD560 \uC218 \uC5C6\uAC70\uB098 " +
                "\uBE44\uD589\uB2E8 \uC7A5\uBE44 \uCF54\uC778\uC774 \uBD80\uC871\uD569\uB2C8\uB2E4.");
            return;
        }

        refreshEquipment?.Invoke();
        showCubePreview?.Invoke(preview);
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
