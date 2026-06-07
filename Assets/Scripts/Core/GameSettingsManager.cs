using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    private const string SoundKey = "settings.sound";
    private const string VibrationKey = "settings.vibration";
    private const string NotificationsKey = "settings.notifications";
    private const string FrameRateKey = "settings.frameRate";
    private const string LanguageKey = "settings.language";

    public static GameSettingsManager Instance;

    public bool SoundEnabled =>
        PlayerPrefs.GetInt(SoundKey, 1) == 1;

    public bool VibrationEnabled =>
        PlayerPrefs.GetInt(VibrationKey, 1) == 1;

    public bool NotificationsEnabled =>
        PlayerPrefs.GetInt(NotificationsKey, 1) == 1;

    public static bool IsKoreanLanguage =>
        PlayerPrefs.GetInt(LanguageKey, 0) == 1;

    public int TargetFrameRate =>
        PlayerPrefs.GetInt(FrameRateKey, 60) == 30 ? 30 : 60;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplySavedSettings();
    }

    public void ToggleSound()
    {
        PlayerPrefs.SetInt(SoundKey, SoundEnabled ? 0 : 1);
        SaveAndApply();
    }

    public void ToggleVibration()
    {
        PlayerPrefs.SetInt(VibrationKey, VibrationEnabled ? 0 : 1);
        SaveAndApply();
    }

    public void ToggleNotifications()
    {
        PlayerPrefs.SetInt(
            NotificationsKey,
            NotificationsEnabled ? 0 : 1);
        SaveAndApply();
    }

    public void ToggleFrameRate()
    {
        PlayerPrefs.SetInt(
            FrameRateKey,
            TargetFrameRate == 60 ? 30 : 60);
        SaveAndApply();
    }

    public void ToggleLanguage()
    {
        PlayerPrefs.SetInt(LanguageKey, IsKoreanLanguage ? 0 : 1);
        SaveAndApply();
    }

    public static void ApplySavedSettings()
    {
        bool soundEnabled =
            PlayerPrefs.GetInt(SoundKey, 1) == 1;
        int frameRate =
            PlayerPrefs.GetInt(FrameRateKey, 60) == 30 ? 30 : 60;

        AudioListener.volume = soundEnabled ? 1f : 0f;
        Application.targetFrameRate = frameRate;
    }

    private static void SaveAndApply()
    {
        PlayerPrefs.Save();
        ApplySavedSettings();
    }
}
