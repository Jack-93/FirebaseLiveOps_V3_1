using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TutorialPanelUI
{
    private readonly GameObject panelObject;
    private readonly TMP_Text objectiveTitleText;
    private readonly TMP_Text tutorialText;
    private readonly TMP_Text tutorialButtonText;

    public TutorialPanelUI(RectTransform root, Action action)
    {
        RectTransform panel = RuntimeUiFactory.CreatePanel(
            "TutorialPanel",
            root,
            new Color32(26, 38, 61, 250),
            new Vector2(0.04f, 0.125f),
            new Vector2(0.96f, 0.235f));
        panelObject = panel.gameObject;

        objectiveTitleText = RuntimeUiFactory.CreateText(
            "ObjectiveTitle",
            panel,
            "NEXT OBJECTIVE",
            26,
            new Vector2(0.04f, 0.62f),
            new Vector2(0.72f, 0.88f),
            TextAlignmentOptions.Left,
            new Color32(255, 201, 77, 255));

        tutorialText = RuntimeUiFactory.CreateText(
            "TutorialText",
            panel,
            "Tutorial",
            29,
            new Vector2(0.04f, 0.16f),
            new Vector2(0.72f, 0.58f),
            TextAlignmentOptions.Left,
            new Color32(190, 203, 225, 255));

        Button button = RuntimeUiFactory.CreateButton(
            "TutorialAction",
            panel,
            "START",
            new Vector2(0.75f, 0.18f),
            new Vector2(0.97f, 0.82f),
            new Color32(255, 201, 77, 255),
            () => action?.Invoke());
        tutorialButtonText = button.GetComponentInChildren<TMP_Text>();
    }

    public void Refresh(
        TutorialManager tutorialManager,
        bool shouldHideForStoryIntro)
    {
        if (tutorialManager == null)
            return;

        if (shouldHideForStoryIntro)
        {
            panelObject.SetActive(false);
            return;
        }

        panelObject.SetActive(!tutorialManager.IsComplete);
        if (tutorialManager.IsComplete)
            return;

        objectiveTitleText.text =
            tutorialManager.CurrentStep == 0
                ? "WELCOME"
                : "NEXT OBJECTIVE";
        tutorialText.text = tutorialManager.CurrentMessage;
        tutorialButtonText.text =
            GetButtonLabel(tutorialManager.CurrentStep);
    }

    private static string GetButtonLabel(int step)
    {
        return step switch
        {
            0 => "START",
            1 => "CHARGE",
            2 => "GROWTH",
            _ => "BATTLE"
        };
    }
}
