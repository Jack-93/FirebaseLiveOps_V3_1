using System;
using TMPro;
using UnityEngine;

public sealed class SettingsPanelUI
{
    private RectTransform panel;
    private TMP_Text settingsText;
    private TMP_Text summaryText;

    public GameObject GameObject => panel == null ? null : panel.gameObject;

    private static readonly Color OverlayBackground =
        new Color32(12, 18, 30, 218);
    private static readonly Color Panel =
        new Color32(37, 49, 73, 245);
    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color Danger =
        new Color32(238, 91, 103, 255);
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public SettingsPanelUI(
        RectTransform root,
        Action showMore,
        Action toggleSound,
        Action toggleVibration,
        Action toggleNotifications,
        Action toggleFrameRate,
        Action toggleLanguage,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "SettingsPanel",
                root,
                out panel))
        {
            Bind(
                showMore,
                toggleSound,
                toggleVibration,
                toggleNotifications,
                toggleFrameRate,
                toggleLanguage);
            return;
        }

        BuildGenerated(
            root,
            showMore,
            toggleSound,
            toggleVibration,
            toggleNotifications,
            toggleFrameRate,
            toggleLanguage);
    }

    public void BuildGenerated(
        RectTransform root,
        Action showMore,
        Action toggleSound,
        Action toggleVibration,
        Action toggleNotifications,
        Action toggleFrameRate,
        Action toggleLanguage)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "SettingsPanel",
            root,
            OverlayBackground,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateButton(
            "SettingsBackButton",
            panel,
            "BACK",
            new Vector2(0.05f, 0.9f),
            new Vector2(0.25f, 0.98f),
            PanelLight,
            () => showMore?.Invoke());

        RuntimeUiFactory.CreateText(
            "SettingsTitle",
            panel,
            "SETTINGS",
            48,
            new Vector2(0.3f, 0.9f),
            new Vector2(0.7f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RuntimeUiFactory.CreateText(
            "SettingsSubtitle",
            panel,
            "Device, notification, language and frame-rate options.",
            24,
            new Vector2(0.15f, 0.85f),
            new Vector2(0.85f, 0.9f),
            TextAlignmentOptions.Center,
            MutedText);

        RectTransform card = RuntimeUiFactory.CreatePanel(
            "SettingsCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.27f),
            new Vector2(0.94f, 0.84f));

        RuntimeUiFactory.CreateText(
            "SettingsCardTitle",
            card,
            "PREFERENCES",
            28,
            new Vector2(0.08f, 0.86f),
            new Vector2(0.92f, 0.96f),
            TextAlignmentOptions.Left,
            Gold);

        settingsText = RuntimeUiFactory.CreateText(
            "SettingsText",
            card,
            "Settings unavailable.",
            32,
            new Vector2(0.08f, 0.66f),
            new Vector2(0.92f, 0.84f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        summaryText = RuntimeUiFactory.CreateText(
            "SettingsSummaryText",
            card,
            "",
            22,
            new Vector2(0.08f, 0.58f),
            new Vector2(0.92f, 0.65f),
            TextAlignmentOptions.Left,
            MutedText);

        RuntimeUiFactory.CreateButton(
            "ToggleSoundButton",
            card,
            "TOGGLE SOUND",
            new Vector2(0.08f, 0.47f),
            new Vector2(0.92f, 0.57f),
            Accent,
            () => toggleSound?.Invoke());

        RuntimeUiFactory.CreateButton(
            "ToggleVibrationButton",
            card,
            "TOGGLE VIBRATION",
            new Vector2(0.08f, 0.34f),
            new Vector2(0.92f, 0.44f),
            Success,
            () => toggleVibration?.Invoke());

        RuntimeUiFactory.CreateButton(
            "ToggleNotificationsButton",
            card,
            "TOGGLE NOTIFICATIONS",
            new Vector2(0.08f, 0.21f),
            new Vector2(0.92f, 0.31f),
            PanelLight,
            () => toggleNotifications?.Invoke());

        RuntimeUiFactory.CreateButton(
            "ToggleFrameRateButton",
            card,
            "SWITCH 30 / 60 FPS",
            new Vector2(0.08f, 0.09f),
            new Vector2(0.47f, 0.18f),
            Gold,
            () => toggleFrameRate?.Invoke());

        RuntimeUiFactory.CreateButton(
            "ToggleLanguageButton",
            card,
            "SWITCH LANGUAGE",
            new Vector2(0.53f, 0.09f),
            new Vector2(0.92f, 0.18f),
            Danger,
            () => toggleLanguage?.Invoke());
    }

    public void Refresh(GameSettingsManager settings)
    {
        if (settings == null)
        {
            SetText(
                settingsText,
                LocalizationManager.Translate("Settings unavailable."));
            SetText(summaryText, string.Empty);
            return;
        }

        string on = LocalizationManager.Translate("ON");
        string off = LocalizationManager.Translate("OFF");
        string language = GameSettingsManager.IsKoreanLanguage
            ? "\uD55C\uAD6D\uC5B4"
            : "English";

        SetText(
            settingsText,
            $"{LocalizationManager.Translate("Sound")}   " +
            $"{(settings.SoundEnabled ? on : off)}\n" +
            $"{LocalizationManager.Translate("Vibration")}   " +
            $"{(settings.VibrationEnabled ? on : off)}\n" +
            $"{LocalizationManager.Translate("Notifications")}   " +
            $"{(settings.NotificationsEnabled ? on : off)}\n" +
            $"{LocalizationManager.Translate("Frame Rate")}   " +
            $"{settings.TargetFrameRate} FPS\n" +
            $"{LocalizationManager.Translate("Language")}   " +
            $"{language}");

        SetText(
            summaryText,
            $"{settings.TargetFrameRate} FPS  |  " +
            $"{(settings.SoundEnabled ? on : off)}  |  " +
            $"{language}");
    }

    private void Bind(
        Action showMore,
        Action toggleSound,
        Action toggleVibration,
        Action toggleNotifications,
        Action toggleFrameRate,
        Action toggleLanguage)
    {
        settingsText = RuntimeUiBinder.FindText(panel, "SettingsText");
        summaryText =
            RuntimeUiBinder.FindText(panel, "SettingsSummaryText");

        Replace("SettingsBackButton", showMore);
        Replace("ToggleSoundButton", toggleSound);
        Replace("ToggleVibrationButton", toggleVibration);
        Replace("ToggleNotificationsButton", toggleNotifications);
        Replace("ToggleFrameRateButton", toggleFrameRate);
        Replace("ToggleLanguageButton", toggleLanguage);
    }

    private void Replace(string buttonName, Action action)
    {
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(panel, buttonName),
            () => action?.Invoke());
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}
