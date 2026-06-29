using System;
using TMPro;
using UnityEngine;

public sealed class EventPanelUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private RectTransform panel;
    private TMP_Text eventText;
    private TMP_Text killLabelText;
    private TMP_Text gachaLabelText;
    private TMP_Text pointLabelText;
    private SpriteNumberText killCurrentNumberText;
    private SpriteNumberText killTargetNumberText;
    private SpriteNumberText gachaCurrentNumberText;
    private SpriteNumberText gachaTargetNumberText;
    private SpriteNumberText pointCurrentNumberText;
    private SpriteNumberText pointTargetNumberText;
    private RectTransform killFill;
    private RectTransform gachaFill;
    private RectTransform pointFill;

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

    public EventPanelUI(
        RectTransform root,
        Action showMore,
        Action claimReward,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "EventPanel",
                root,
                out panel))
        {
            Bind(showMore, claimReward);
            return;
        }

        BuildGenerated(root, showMore, claimReward);
    }

    public void BuildGenerated(
        RectTransform root,
        Action showMore,
        Action claimReward)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "EventPanel",
            root,
            OverlayBackground,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateButton(
            "EventBackButton",
            panel,
            "BACK",
            new Vector2(0.05f, 0.9f),
            new Vector2(0.25f, 0.98f),
            PanelLight,
            () => showMore?.Invoke());

        RuntimeUiFactory.CreateText(
            "EventTitle",
            panel,
            "EVENT",
            48,
            new Vector2(0.3f, 0.9f),
            new Vector2(0.7f, 0.98f),
            TextAlignmentOptions.Center,
            Success);

        RuntimeUiFactory.CreateText(
            "EventSubtitle",
            panel,
            "Limited-time mission and reward placeholder.",
            24,
            new Vector2(0.18f, 0.85f),
            new Vector2(0.82f, 0.9f),
            TextAlignmentOptions.Center,
            MutedText);

        RectTransform card = RuntimeUiFactory.CreatePanel(
            "EventCard",
            panel,
            Panel,
            new Vector2(0.06f, 0.31f),
            new Vector2(0.94f, 0.84f));

        RuntimeUiFactory.CreateText(
            "EventCardTitle",
            card,
            "ACTIVE EVENT",
            28,
            new Vector2(0.07f, 0.84f),
            new Vector2(0.93f, 0.96f),
            TextAlignmentOptions.Left,
            Gold);

        eventText = RuntimeUiFactory.CreateText(
            "EventText",
            card,
            "Event data unavailable.",
            31,
            new Vector2(0.07f, 0.49f),
            new Vector2(0.93f, 0.8f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        CreateProgressLine(
            card,
            "Kills",
            0.405f,
            out killLabelText,
            out killCurrentNumberText,
            out killTargetNumberText);
        CreateProgressLine(
            card,
            "Gacha",
            0.345f,
            out gachaLabelText,
            out gachaCurrentNumberText,
            out gachaTargetNumberText);
        CreateProgressLine(
            card,
            "Points",
            0.285f,
            out pointLabelText,
            out pointCurrentNumberText,
            out pointTargetNumberText);

        killFill = RuntimeProgressBar.Create(
            card,
            "EventKillProgressBar",
            Danger,
            new Vector2(0.07f, 0.36f),
            new Vector2(0.93f, 0.4f));
        gachaFill = RuntimeProgressBar.Create(
            card,
            "EventGachaProgressBar",
            Accent,
            new Vector2(0.07f, 0.3f),
            new Vector2(0.93f, 0.34f));
        pointFill = RuntimeProgressBar.Create(
            card,
            "EventPointProgressBar",
            Success,
            new Vector2(0.07f, 0.24f),
            new Vector2(0.93f, 0.28f));

        RuntimeUiFactory.CreateButton(
            "ClaimEventRewardButton",
            card,
            "CLAIM EVENT REWARD",
            new Vector2(0.07f, 0.05f),
            new Vector2(0.93f, 0.2f),
            Success,
            () => claimReward?.Invoke());
    }

    public void Refresh(EventMissionManager eventManager, PlayerData data)
    {
        if (eventManager == null)
        {
            SetText(eventText, EventPanelFormatter.DataUnavailable);
            SetEmptyProgress();
            return;
        }

        SetText(eventText, eventManager.GetStatusText());
        RefreshProgress(data);
    }

    private void RefreshProgress(PlayerData data)
    {
        if (data == null)
        {
            SetEmptyProgress();
            return;
        }

        RuntimeProgressBar.Set(
            killFill,
            data.eventKillCount,
            EventMissionManager.KillTarget);
        RuntimeProgressBar.Set(
            gachaFill,
            data.eventGachaCount,
            EventMissionManager.GachaTarget);
        RuntimeProgressBar.Set(
            pointFill,
            data.eventMissionPoints,
            EventMissionManager.RewardPointTarget);
        SetProgressLine(
            killLabelText,
            killCurrentNumberText,
            killTargetNumberText,
            "Kills",
            Math.Min(data.eventKillCount, EventMissionManager.KillTarget),
            EventMissionManager.KillTarget);
        SetProgressLine(
            gachaLabelText,
            gachaCurrentNumberText,
            gachaTargetNumberText,
            "Gacha",
            Math.Min(data.eventGachaCount, EventMissionManager.GachaTarget),
            EventMissionManager.GachaTarget);
        SetProgressLine(
            pointLabelText,
            pointCurrentNumberText,
            pointTargetNumberText,
            "Points",
            Math.Min(
                data.eventMissionPoints,
                EventMissionManager.RewardPointTarget),
            EventMissionManager.RewardPointTarget);
    }

    private void SetEmptyProgress()
    {
        RuntimeProgressBar.Set(killFill, 0, 1);
        RuntimeProgressBar.Set(gachaFill, 0, 1);
        RuntimeProgressBar.Set(pointFill, 0, 1);
        SetProgressLine(
            killLabelText,
            killCurrentNumberText,
            killTargetNumberText,
            "Kills",
            0,
            1);
        SetProgressLine(
            gachaLabelText,
            gachaCurrentNumberText,
            gachaTargetNumberText,
            "Gacha",
            0,
            1);
        SetProgressLine(
            pointLabelText,
            pointCurrentNumberText,
            pointTargetNumberText,
            "Points",
            0,
            1);
    }

    private void Bind(Action showMore, Action claimReward)
    {
        eventText = RuntimeUiBinder.FindText(panel, "EventText");
        killLabelText = RuntimeUiBinder.FindText(panel, "EventKillsLabel");
        gachaLabelText = RuntimeUiBinder.FindText(panel, "EventGachaLabel");
        pointLabelText = RuntimeUiBinder.FindText(panel, "EventPointsLabel");
        killCurrentNumberText = BindNumber("EventKillsCurrentNumberText");
        killTargetNumberText = BindNumber("EventKillsTargetNumberText");
        gachaCurrentNumberText = BindNumber("EventGachaCurrentNumberText");
        gachaTargetNumberText = BindNumber("EventGachaTargetNumberText");
        pointCurrentNumberText = BindNumber("EventPointsCurrentNumberText");
        pointTargetNumberText = BindNumber("EventPointsTargetNumberText");
        killFill =
            RuntimeUiBinder.FindProgressFill(panel, "EventKillProgressBar");
        gachaFill =
            RuntimeUiBinder.FindProgressFill(panel, "EventGachaProgressBar");
        pointFill =
            RuntimeUiBinder.FindProgressFill(panel, "EventPointProgressBar");
        Replace("EventBackButton", showMore);
        Replace("ClaimEventRewardButton", claimReward);
    }

    private SpriteNumberText BindNumber(string name)
    {
        return RuntimeUiBinder.BindNumber(
            panel,
            name,
            NumberResourceRoot,
            18f);
    }

    private void Replace(string buttonName, Action action)
    {
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(panel, buttonName),
            () => action?.Invoke());
    }

    private static void CreateProgressLine(
        RectTransform card,
        string label,
        float yMin,
        out TMP_Text labelText,
        out SpriteNumberText currentNumberText,
        out SpriteNumberText targetNumberText)
    {
        float yMax = yMin + 0.045f;
        labelText = RuntimeUiFactory.CreateText(
            "Event" + label + "Label",
            card,
            label,
            18,
            new Vector2(0.07f, yMin),
            new Vector2(0.22f, yMax),
            TextAlignmentOptions.Left,
            MutedText);
        currentNumberText = new SpriteNumberText(
            card,
            "Event" + label + "CurrentNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.22f, yMin),
            new Vector2(0.34f, yMax));
        RuntimeUiFactory.CreateText(
            "Event" + label + "Slash",
            card,
            "/",
            18,
            new Vector2(0.34f, yMin),
            new Vector2(0.37f, yMax),
            TextAlignmentOptions.Center,
            Color.white);
        targetNumberText = new SpriteNumberText(
            card,
            "Event" + label + "TargetNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.37f, yMin),
            new Vector2(0.5f, yMax));
    }

    private static void SetProgressLine(
        TMP_Text labelText,
        SpriteNumberText currentNumberText,
        SpriteNumberText targetNumberText,
        string label,
        int current,
        int target)
    {
        SetText(labelText, LocalizationManager.Translate(label));
        currentNumberText?.SetText(CompactNumberFormatter.Format(current));
        targetNumberText?.SetText(CompactNumberFormatter.Format(target));
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}
