using System;
using TMPro;
using UnityEngine;

public sealed class QuestPanelUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private RectTransform panel;
    private TMP_Text questText;
    private TMP_Text questProgressLabelText;
    private TMP_Text objectiveProgressLabelText;
    private SpriteNumberText questIndexNumberText;
    private SpriteNumberText questCountNumberText;
    private SpriteNumberText progressNumberText;
    private SpriteNumberText targetNumberText;
    private RectTransform progressFill;

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
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public QuestPanelUI(
        RectTransform root,
        Action showMore,
        Action claimQuest,
        Action claimAchievements,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "QuestPanel",
                root,
                out panel))
        {
            Bind(showMore, claimQuest, claimAchievements);
            return;
        }

        BuildGenerated(root, showMore, claimQuest, claimAchievements);
    }

    public void BuildGenerated(
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
        questProgressLabelText = RuntimeUiFactory.CreateText(
            "QuestProgressLabelText",
            card,
            "Quest",
            18,
            new Vector2(0.07f, 0.31f),
            new Vector2(0.19f, 0.4f),
            TextAlignmentOptions.Left,
            Color.white);
        questIndexNumberText = new SpriteNumberText(
            card,
            "QuestIndexNumberText",
            NumberResourceRoot,
            20f,
            new Vector2(0.19f, 0.31f),
            new Vector2(0.27f, 0.4f));
        RuntimeUiFactory.CreateText(
            "QuestProgressSlashText",
            card,
            "/",
            20,
            new Vector2(0.27f, 0.31f),
            new Vector2(0.3f, 0.4f),
            TextAlignmentOptions.Center,
            Color.white);
        questCountNumberText = new SpriteNumberText(
            card,
            "QuestCountNumberText",
            NumberResourceRoot,
            20f,
            new Vector2(0.3f, 0.31f),
            new Vector2(0.38f, 0.4f));
        objectiveProgressLabelText = RuntimeUiFactory.CreateText(
            "ObjectiveProgressLabelText",
            card,
            "Progress",
            18,
            new Vector2(0.42f, 0.31f),
            new Vector2(0.56f, 0.4f),
            TextAlignmentOptions.Left,
            Color.white);
        progressNumberText = new SpriteNumberText(
            card,
            "ObjectiveProgressNumberText",
            NumberResourceRoot,
            20f,
            new Vector2(0.56f, 0.31f),
            new Vector2(0.66f, 0.4f));
        RuntimeUiFactory.CreateText(
            "ObjectiveProgressSlashText",
            card,
            "/",
            20,
            new Vector2(0.66f, 0.31f),
            new Vector2(0.69f, 0.4f),
            TextAlignmentOptions.Center,
            Color.white);
        targetNumberText = new SpriteNumberText(
            card,
            "ObjectiveTargetNumberText",
            NumberResourceRoot,
            20f,
            new Vector2(0.69f, 0.31f),
            new Vector2(0.82f, 0.4f));

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
            SetText(questText, QuestPanelFormatter.DataUnavailable);
            SetProgress(QuestPanelFormatter.BuildProgress(null));
            return;
        }

        SetText(questText, questManager.GetStatusText());
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
        SetText(
            questProgressLabelText,
            LocalizationManager.Translate("Quest"));
        SetText(
            objectiveProgressLabelText,
            LocalizationManager.Translate("Progress"));
        questIndexNumberText?.SetText(
            CompactNumberFormatter.Format(progress.QuestIndex + 1));
        questCountNumberText?.SetText(
            CompactNumberFormatter.Format(
                Mathf.Max(1, QuestManager.QuestCount)));
        progressNumberText?.SetText(
            CompactNumberFormatter.Format(progress.Progress));
        targetNumberText?.SetText(
            CompactNumberFormatter.Format(progress.Target));
    }

    private void Bind(
        Action showMore,
        Action claimQuest,
        Action claimAchievements)
    {
        questText = RuntimeUiBinder.FindText(panel, "QuestText");
        progressFill =
            RuntimeUiBinder.FindProgressFill(panel, "QuestProgressBar");
        questProgressLabelText =
            RuntimeUiBinder.FindText(panel, "QuestProgressLabelText");
        objectiveProgressLabelText =
            RuntimeUiBinder.FindText(panel, "ObjectiveProgressLabelText");
        questIndexNumberText = RuntimeUiBinder.BindNumber(
            panel,
            "QuestIndexNumberText",
            NumberResourceRoot,
            20f);
        questCountNumberText = RuntimeUiBinder.BindNumber(
            panel,
            "QuestCountNumberText",
            NumberResourceRoot,
            20f);
        progressNumberText = RuntimeUiBinder.BindNumber(
            panel,
            "ObjectiveProgressNumberText",
            NumberResourceRoot,
            20f);
        targetNumberText = RuntimeUiBinder.BindNumber(
            panel,
            "ObjectiveTargetNumberText",
            NumberResourceRoot,
            20f);

        Replace("QuestBackButton", showMore);
        Replace("ClaimQuestButton", claimQuest);
        Replace("ClaimAchievementButton", claimAchievements);
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
