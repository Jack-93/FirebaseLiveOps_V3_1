using System;

public sealed class SettingsActionController
{
    private readonly Action refreshSettings;
    private readonly Action refreshAll;
    private readonly Action<string> showToast;

    public SettingsActionController(
        Action refreshSettings,
        Action refreshAll,
        Action<string> showToast)
    {
        this.refreshSettings = refreshSettings;
        this.refreshAll = refreshAll;
        this.showToast = showToast;
    }

    public void ToggleSound()
    {
        GameSettingsManager.Instance?.ToggleSound();
        refreshSettings?.Invoke();
    }

    public void ToggleVibration()
    {
        GameSettingsManager.Instance?.ToggleVibration();
        refreshSettings?.Invoke();
    }

    public void ToggleNotifications()
    {
        GameSettingsManager.Instance?.ToggleNotifications();
        refreshSettings?.Invoke();
    }

    public void ToggleFrameRate()
    {
        GameSettingsManager.Instance?.ToggleFrameRate();
        refreshSettings?.Invoke();
    }

    public void ToggleLanguage()
    {
        GameSettingsManager.Instance?.ToggleLanguage();
        refreshAll?.Invoke();
        string message = GameSettingsManager.IsKoreanLanguage
            ? "\uC5B8\uC5B4\uAC00 \uD55C\uAD6D\uC5B4\uB85C \uBCC0\uACBD\uB418\uC5C8\uC2B5\uB2C8\uB2E4."
            : "Language changed to English.";
        showToast?.Invoke(message);
    }
}
