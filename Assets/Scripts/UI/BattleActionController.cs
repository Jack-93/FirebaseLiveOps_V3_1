using System;

public sealed class BattleActionController
{
    private readonly BattleManager battleManager;
    private readonly MainGameBootstrap bootstrap;
    private readonly Action<string> showToast;

    public BattleActionController(
        BattleManager battleManager,
        MainGameBootstrap bootstrap,
        Action<string> showToast)
    {
        this.battleManager = battleManager;
        this.bootstrap = bootstrap;
        this.showToast = showToast;
    }

    public void ChangeStage(int direction)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || battleManager == null)
            return;

        if (!battleManager.SelectStage(data.currentStage + direction))
        {
            showToast?.Invoke(direction < 0
                ? "This is the first stage."
                : "Clear the current highest stage first.");
            return;
        }

        SaveAsync();
    }

    public void ToggleAutoAdvance()
    {
        if (battleManager == null)
            return;

        battleManager.ToggleAutoAdvance();
        SaveAsync();

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return;

        showToast?.Invoke(data.autoAdvance
            ? "Auto stage advance enabled."
            : "Current stage repeat enabled.");
    }

    private void SaveAsync()
    {
        if (bootstrap != null)
            _ = bootstrap.SaveNowAsync();
    }
}
