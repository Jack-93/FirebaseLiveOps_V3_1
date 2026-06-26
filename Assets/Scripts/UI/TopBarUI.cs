using System;
using TMPro;
using UnityEngine;

public sealed class TopBarUI
{
    private readonly TMP_Text stageText;
    private readonly TMP_Text goldText;
    private readonly TMP_Text powerText;

    public TopBarUI(
        RectTransform root,
        Action previousStage,
        Action nextStage)
    {
        RectTransform top = RuntimeUiFactory.CreatePanel(
            "TopBar",
            root,
            new Color32(24, 35, 58, 215),
            new Vector2(0f, 0.9f),
            Vector2.one);

        stageText = RuntimeUiFactory.CreateText(
            "StageText",
            top,
            "Stage 1",
            38,
            new Vector2(0.075f, 0.12f),
            new Vector2(0.315f, 0.88f),
            TextAlignmentOptions.Center,
            Color.white);

        RuntimeUiFactory.CreateButton(
            "PreviousStageButton",
            top,
            "<",
            new Vector2(0.012f, 0.22f),
            new Vector2(0.065f, 0.78f),
            new Color32(52, 68, 96, 255),
            () => previousStage?.Invoke());

        RuntimeUiFactory.CreateButton(
            "NextStageButton",
            top,
            ">",
            new Vector2(0.325f, 0.22f),
            new Vector2(0.378f, 0.78f),
            new Color32(52, 68, 96, 255),
            () => nextStage?.Invoke());

        goldText = RuntimeUiFactory.CreateText(
            "GoldText",
            top,
            "Gold 0",
            38,
            new Vector2(0.45f, 0.12f),
            new Vector2(0.68f, 0.88f),
            TextAlignmentOptions.Center,
            new Color32(255, 201, 77, 255));

        RuntimeUiFactory.CreateSpriteImage(
            "GoldIcon",
            top,
            PrototypeUiArt.GoldIcon,
            new Vector2(0.395f, 0.18f),
            new Vector2(0.455f, 0.82f));

        powerText = RuntimeUiFactory.CreateText(
            "PowerText",
            top,
            "Power 0",
            36,
            new Vector2(0.7f, 0.12f),
            new Vector2(0.96f, 0.88f),
            TextAlignmentOptions.Right,
            Color.white);
    }

    public void Refresh(PlayerData data)
    {
        if (data == null)
            return;

        goldText.text =
            $"{LocalizationManager.Text("Gold", "골드")}  {data.gold:N0}";
        stageText.text =
            $"{LocalizationManager.Text("Stage", "스테이지")} {data.currentStage}";
        powerText.text =
            $"{LocalizationManager.Text("Power", "전투력")} " +
            $"{GameBalance.GetCombatPower(data):N0}";
    }
}
