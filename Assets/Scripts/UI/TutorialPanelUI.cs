using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TutorialPanelUI
{
    private GameObject panelObject;
    private TMP_Text objectiveTitleText;
    private TMP_Text tutorialText;
    private RectTransform tutorialTextRect;
    private Button tutorialButton;
    private TMP_Text tutorialButtonText;
    private bool preserveTutorialTextLayout;

    public GameObject GameObject => panelObject;

    public TutorialPanelUI(
        RectTransform root,
        Action action,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "TutorialPanel",
                root,
                out RectTransform panel))
        {
            Bind(panel, action);
            return;
        }

        BuildGenerated(root, action);
    }

    public void BuildGenerated(RectTransform root, Action action)
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
        tutorialTextRect = tutorialText.GetComponent<RectTransform>();
        preserveTutorialTextLayout = false;

        tutorialButton = RuntimeUiFactory.CreateButton(
            "TutorialAction",
            panel,
            "START",
            new Vector2(0.75f, 0.18f),
            new Vector2(0.97f, 0.82f),
            new Color32(255, 201, 77, 255),
            () => action?.Invoke());
        tutorialButtonText =
            tutorialButton.GetComponentInChildren<TMP_Text>();
    }

    public void Refresh(
        TutorialManager tutorialManager,
        bool shouldHideForStoryIntro)
    {
        if (tutorialManager == null)
            return;

        if (shouldHideForStoryIntro)
        {
            panelObject?.SetActive(false);
            return;
        }

        panelObject?.SetActive(!tutorialManager.IsComplete);
        if (tutorialManager.IsComplete)
            return;

        if (objectiveTitleText != null)
        {
            objectiveTitleText.text =
                tutorialManager.CurrentStep == 0
                    ? LocalizationManager.Text(
                        "WELCOME",
                        "\uC791\uC804 \uC2DC\uC791")
                    : LocalizationManager.Text(
                        "NEXT OBJECTIVE",
                        "\uB2E4\uC74C \uBAA9\uD45C");
        }

        if (tutorialText != null)
            tutorialText.text = tutorialManager.CurrentMessage;

        bool showActionButton =
            ShouldShowActionButton(tutorialManager.CurrentStep);
        if (tutorialButton != null)
            tutorialButton.gameObject.SetActive(showActionButton);
        if (tutorialTextRect != null && !preserveTutorialTextLayout)
        {
            tutorialTextRect.anchorMax = showActionButton
                ? new Vector2(0.72f, 0.58f)
                : new Vector2(0.96f, 0.58f);
        }

        if (showActionButton && tutorialButtonText != null)
            tutorialButtonText.text = GetButtonLabel(tutorialManager);
    }

    private static bool ShouldShowActionButton(int step)
    {
        return step != 1;
    }

    private void Bind(RectTransform panel, Action action)
    {
        panelObject = panel.gameObject;
        objectiveTitleText =
            RuntimeUiBinder.FindText(panel, "ObjectiveTitle");
        tutorialText = RuntimeUiBinder.FindText(panel, "TutorialText");
        tutorialTextRect =
            RuntimeUiBinder.FindRect(panel, "TutorialText");
        preserveTutorialTextLayout = true;
        tutorialButton =
            RuntimeUiBinder.FindButton(panel, "TutorialAction");
        tutorialButtonText =
            tutorialButton == null
                ? null
                : tutorialButton.GetComponentInChildren<TMP_Text>();
        RuntimeUiBinder.ReplaceButtonAction(
            tutorialButton,
            () => action?.Invoke());
    }

    private static string GetButtonLabel(TutorialManager tutorialManager)
    {
        return tutorialManager.CurrentStep switch
        {
            0 when tutorialManager.ShouldShowTutorialTicketGift =>
                LocalizationManager.Text(
                    "NEXT",
                    "\uB2E4\uC74C"),
            0 => LocalizationManager.Text(
                "RECRUIT",
                "\uBAA8\uC9D1\uC73C\uB85C"),
            1 => LocalizationManager.Text(
                "BATTLE",
                "\uC804\uD22C\uB85C"),
            2 => LocalizationManager.Text(
                "GROWTH",
                "\uC131\uC7A5\uC73C\uB85C"),
            _ => LocalizationManager.Text(
                "BATTLE",
                "\uC804\uD22C\uB85C")
        };
    }
}
