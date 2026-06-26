using System;
using TMPro;
using UnityEngine;

public sealed class QuestPanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text questText;
    private readonly TMP_Text progressText;
    private readonly RectTransform progressFill;

    public GameObject GameObject => panel.gameObject;

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
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public QuestPanelUI(
        RectTransform root,
        Action showMore,
        Action claimQuest,
        Action claimAchievements)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "QuestPanel",
            root,
            OverlayBackground,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateButton(
            "QuestBackButton",
            panel,
            "BACK",
            new Vector2(0.04f, 0.9f),
            new Vector2(0.22f, 0.97f),
            PanelLight,
            () => showMore?.Invoke());

        RuntimeUiFactory.CreateText(
            "QuestTitle",
            panel,
            "MAIN QUEST",
            46,
            new Vector2(0.24f, 0.9f),
            new Vector2(0.96f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RuntimeUiFactory.CreateText(
            "QuestSubtitle",
            panel,
            "Complete one objective to unlock the next.",
            24,
            new Vector2(0.24f, 0.86f),
            new Vector2(0.96f, 0.9f),
            TextAlignmentOptions.Center,
            MutedText);

        RectTransform card = RuntimeUiFactory.CreatePanel(
            "QuestCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.31f),
            new Vector2(0.94f, 0.84f));

        RuntimeUiFactory.CreateText(
            "QuestCardTitle",
            card,
            "CURRENT OBJECTIVE",
            27,
            new Vector2(0.07f, 0.84f),
            new Vector2(0.93f, 0.95f),
            TextAlignmentOptions.Left,
            Gold);

        questText = RuntimeUiFactory.CreateText(
            "QuestText",
            card,
            "Quest data unavailable.",
            32,
            new Vector2(0.07f, 0.42f),
            new Vector2(0.93f, 0.82f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        progressFill = RuntimeProgressBar.Create(
            card,
            "QuestProgressBar",
            Success,
            new Vector2(0.07f, 0.32f),
            new Vector2(0.93f, 0.39f));
        progressText = RuntimeUiFactory.CreateText(
            "QuestProgressText",
            card,
            "0 / 0",
            22,
            new Vector2(0.07f, 0.31f),
            new Vector2(0.93f, 0.4f),
            TextAlignmentOptions.Center,
            Color.white);

        RuntimeUiFactory.CreateButton(
            "ClaimQuestButton",
            card,
            "CLAIM QUEST",
            new Vector2(0.07f, 0.08f),
            new Vector2(0.47f, 0.27f),
            Success,
            () => claimQuest?.Invoke());

        RuntimeUiFactory.CreateButton(
            "ClaimAchievementButton",
            card,
            "CLAIM ACHIEVEMENTS",
            new Vector2(0.53f, 0.08f),
            new Vector2(0.93f, 0.27f),
            Gold,
            () => claimAchievements?.Invoke());
    }

    public void Refresh(QuestManager questManager, PlayerData data)
    {
        if (questManager == null)
        {
            questText.text = QuestPanelFormatter.DataUnavailable;
            SetProgress(QuestPanelFormatter.BuildProgress(null));
            return;
        }

        questText.text = questManager.GetStatusText();
        RefreshProgress(data);
    }

    private void RefreshProgress(PlayerData data)
    {
        if (data == null || QuestManager.QuestCount <= 0)
        {
            SetProgress(QuestPanelFormatter.BuildProgress(null));
            return;
        }

        SetProgress(QuestPanelFormatter.BuildProgress(data));
    }

    private void SetProgress(QuestProgressView progress)
    {
        RuntimeProgressBar.Set(
            progressFill,
            progress.Progress,
            progress.Target);
        progressText.text = progress.Text;
    }
}
