using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TopBarUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private RectTransform top;
    private TMP_Text stageLabelText;
    private TMP_Text goldLabelText;
    private TMP_Text powerLabelText;
    private SpriteNumberText stageNumberText;
    private SpriteNumberText goldNumberText;
    private SpriteNumberText powerNumberText;

    public GameObject GameObject => top == null ? null : top.gameObject;

    public TopBarUI(
        RectTransform root,
        Action previousStage,
        Action nextStage,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab("TopBar", root, out top))
        {
            Bind(previousStage, nextStage);
            return;
        }

        BuildGenerated(root, previousStage, nextStage);
    }

    public void BuildGenerated(
        RectTransform root,
        Action previousStage,
        Action nextStage)
    {
        top = RuntimeUiFactory.CreatePanel(
            "TopBar",
            root,
            new Color32(24, 35, 58, 215),
            new Vector2(0f, 0.9f),
            Vector2.one);

        stageLabelText = RuntimeUiFactory.CreateText(
            "StageLabelText",
            top,
            "Stage",
            30,
            new Vector2(0.075f, 0.12f),
            new Vector2(0.205f, 0.88f),
            TextAlignmentOptions.Left,
            Color.white);
        stageNumberText = new SpriteNumberText(
            top,
            "StageNumberText",
            NumberResourceRoot,
            34f,
            new Vector2(0.21f, 0.12f),
            new Vector2(0.315f, 0.88f));

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

        goldLabelText = RuntimeUiFactory.CreateText(
            "GoldLabelText",
            top,
            "Gold",
            30,
            new Vector2(0.455f, 0.12f),
            new Vector2(0.545f, 0.88f),
            TextAlignmentOptions.Left,
            new Color32(255, 201, 77, 255));
        goldNumberText = new SpriteNumberText(
            top,
            "GoldNumberText",
            NumberResourceRoot,
            34f,
            new Vector2(0.545f, 0.12f),
            new Vector2(0.69f, 0.88f));

        RuntimeUiFactory.CreateSpriteImage(
            "GoldIcon",
            top,
            PrototypeUiArt.GoldIcon,
            new Vector2(0.395f, 0.18f),
            new Vector2(0.455f, 0.82f));

        powerLabelText = RuntimeUiFactory.CreateText(
            "PowerLabelText",
            top,
            "Power",
            30,
            new Vector2(0.7f, 0.12f),
            new Vector2(0.81f, 0.88f),
            TextAlignmentOptions.Left,
            Color.white);
        powerNumberText = new SpriteNumberText(
            top,
            "PowerNumberText",
            NumberResourceRoot,
            32f,
            new Vector2(0.81f, 0.12f),
            new Vector2(0.96f, 0.88f));
    }

    public void Refresh(PlayerData data)
    {
        if (data == null)
            return;

        if (goldLabelText != null)
            goldLabelText.text = LocalizationManager.Text(
                "Gold",
                "\uACE8\uB4DC");
        goldNumberText?.SetText(CompactNumberFormatter.Format(data.gold));

        if (stageLabelText != null)
            stageLabelText.text = LocalizationManager.Text(
                "Stage",
                "\uC2A4\uD14C\uC774\uC9C0");
        stageNumberText?.SetText(
            CompactNumberFormatter.Format(data.currentStage));

        if (powerLabelText != null)
            powerLabelText.text = LocalizationManager.Text(
                "Power",
                "\uC804\uD22C\uB825");
        powerNumberText?.SetText(
            CompactNumberFormatter.Format(GameBalance.GetCombatPower(data)));
    }

    private void Bind(
        Action previousStage,
        Action nextStage)
    {
        stageLabelText =
            RuntimeUiBinder.FindText(top, "StageLabelText");
        stageNumberText = RuntimeUiBinder.BindNumber(
            top,
            "StageNumberText",
            NumberResourceRoot,
            34f);
        goldLabelText =
            RuntimeUiBinder.FindText(top, "GoldLabelText");
        goldNumberText = RuntimeUiBinder.BindNumber(
            top,
            "GoldNumberText",
            NumberResourceRoot,
            34f);
        powerLabelText =
            RuntimeUiBinder.FindText(top, "PowerLabelText");
        powerNumberText = RuntimeUiBinder.BindNumber(
            top,
            "PowerNumberText",
            NumberResourceRoot,
            32f);

        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(top, "PreviousStageButton"),
            () => previousStage?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(top, "NextStageButton"),
            () => nextStage?.Invoke());
    }
}
