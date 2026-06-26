using System;
using TMPro;
using UnityEngine;

public sealed class EventPanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text eventText;
    private readonly TMP_Text progressText;
    private readonly RectTransform killFill;
    private readonly RectTransform gachaFill;
    private readonly RectTransform pointFill;

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
    private static readonly Color Danger =
        new Color32(238, 91, 103, 255);
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public EventPanelUI(
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

        progressText = RuntimeUiFactory.CreateText(
            "EventProgressText",
            card,
            "0 / 0",
            21,
            new Vector2(0.07f, 0.41f),
            new Vector2(0.93f, 0.48f),
            TextAlignmentOptions.Left,
            MutedText);
        killFill = CreateHealthBar(
            card,
            "EventKillProgressBar",
            Danger,
            new Vector2(0.07f, 0.36f),
            new Vector2(0.93f, 0.4f));
        gachaFill = CreateHealthBar(
            card,
            "EventGachaProgressBar",
            Accent,
            new Vector2(0.07f, 0.3f),
            new Vector2(0.93f, 0.34f));
        pointFill = CreateHealthBar(
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
            eventText.text = LocalizationManager.Text(
                "Event data unavailable.",
                "Event data unavailable.");
            SetEmptyProgress();
            return;
        }

        eventText.text = eventManager.GetStatusText();
        RefreshProgress(data);
    }

    private void RefreshProgress(PlayerData data)
    {
        if (data == null)
        {
            SetEmptyProgress();
            return;
        }

        SetBar(killFill, data.eventKillCount, EventMissionManager.KillTarget);
        SetBar(gachaFill, data.eventGachaCount, EventMissionManager.GachaTarget);
        SetBar(
            pointFill,
            data.eventMissionPoints,
            EventMissionManager.RewardPointTarget);

        progressText.text =
            $"{LocalizationManager.Text("Kills", "Kills")} " +
            $"{Mathf.Min(data.eventKillCount, EventMissionManager.KillTarget)}/" +
            $"{EventMissionManager.KillTarget}   " +
            $"{LocalizationManager.Text("Gacha", "Gacha")} " +
            $"{Mathf.Min(data.eventGachaCount, EventMissionManager.GachaTarget)}/" +
            $"{EventMissionManager.GachaTarget}   " +
            $"{LocalizationManager.Text("Points", "Points")} " +
            $"{Mathf.Min(data.eventMissionPoints, EventMissionManager.RewardPointTarget)}/" +
            $"{EventMissionManager.RewardPointTarget}";
    }

    private void SetEmptyProgress()
    {
        SetBar(killFill, 0, 1);
        SetBar(gachaFill, 0, 1);
        SetBar(pointFill, 0, 1);
        progressText.text = "0 / 0";
    }

    private static RectTransform CreateHealthBar(
        RectTransform parent,
        string name,
        Color fillColor,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform background = RuntimeUiFactory.CreatePanel(
            name,
            parent,
            new Color32(12, 18, 30, 255),
            anchorMin,
            anchorMax);

        return RuntimeUiFactory.CreatePanel(
            "Fill",
            background,
            fillColor,
            Vector2.zero,
            Vector2.one);
    }

    private static void SetBar(
        RectTransform fill,
        int current,
        int maximum)
    {
        if (fill == null)
            return;

        float ratio = maximum <= 0
            ? 0f
            : Mathf.Clamp01(current / (float)maximum);
        fill.anchorMax = new Vector2(ratio, 1f);
    }
}
