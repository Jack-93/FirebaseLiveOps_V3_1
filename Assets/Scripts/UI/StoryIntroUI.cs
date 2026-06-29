using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StoryIntroUI
{
    private GameObject overlayObject;
    private TMP_Text counterText;
    private TMP_Text titleText;
    private TMP_Text bodyText;
    private TMP_Text artText;
    private Button previousButton;
    private TMP_Text previousButtonText;
    private TMP_Text buttonText;
    private Image artImage;

    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color DisabledText =
        new Color32(160, 170, 190, 255);

    public GameObject GameObject => overlayObject;

    public StoryIntroUI(
        RectTransform root,
        Action nextAction,
        Action previousAction,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "StoryIntroOverlay",
                root,
                out RectTransform overlay))
        {
            Bind(overlay, nextAction, previousAction);
            return;
        }

        BuildGenerated(root, nextAction, previousAction);
    }

    public void BuildGenerated(
        RectTransform root,
        Action nextAction,
        Action previousAction)
    {
        RectTransform overlay = RuntimeUiFactory.CreatePanel(
            "StoryIntroOverlay",
            root,
            new Color32(8, 12, 22, 250),
            Vector2.zero,
            Vector2.one);
        overlayObject = overlay.gameObject;

        Button tapArea = overlayObject.AddComponent<Button>();
        tapArea.targetGraphic = overlay.GetComponent<Image>();
        tapArea.onClick.AddListener(() => nextAction?.Invoke());

        RectTransform artPanel = RuntimeUiFactory.CreatePanel(
            "StoryIntroArtPlaceholder",
            overlay,
            new Color32(135, 199, 255, 255),
            new Vector2(0.06f, 0.36f),
            new Vector2(0.94f, 0.9f));
        artImage = artPanel.GetComponent<Image>();

        RuntimeUiFactory.CreatePanel(
            "StoryIntroPixelFrame",
            artPanel,
            new Color32(255, 255, 255, 38),
            new Vector2(0.03f, 0.04f),
            new Vector2(0.97f, 0.96f));

        artText = RuntimeUiFactory.CreateText(
            "StoryIntroArtText",
            artPanel,
            "",
            34,
            new Vector2(0.08f, 0.34f),
            new Vector2(0.92f, 0.66f),
            TextAlignmentOptions.Center,
            Color.white);

        RectTransform dialoguePanel = RuntimeUiFactory.CreatePanel(
            "StoryIntroDialoguePanel",
            overlay,
            new Color32(28, 38, 60, 255),
            new Vector2(0.06f, 0.08f),
            new Vector2(0.94f, 0.33f));

        counterText = RuntimeUiFactory.CreateText(
            "StoryIntroCounter",
            dialoguePanel,
            "1/7",
            24,
            new Vector2(0.05f, 0.74f),
            new Vector2(0.24f, 0.94f),
            TextAlignmentOptions.Left,
            new Color32(255, 201, 77, 255));

        titleText = RuntimeUiFactory.CreateText(
            "StoryIntroTitle",
            dialoguePanel,
            "\uC804\uBD07\uB300 \uBC29\uC5B4 \uC791\uC804",
            36,
            new Vector2(0.25f, 0.7f),
            new Vector2(0.95f, 0.94f),
            TextAlignmentOptions.Right,
            new Color32(82, 188, 255, 255));

        bodyText = RuntimeUiFactory.CreateText(
            "StoryIntroBody",
            dialoguePanel,
            "\uB300\uC0AC\uB294 \uCD94\uD6C4 \uD655\uC815",
            31,
            new Vector2(0.05f, 0.26f),
            new Vector2(0.95f, 0.68f),
            TextAlignmentOptions.Left,
            Color.white);

        previousButton = RuntimeUiFactory.CreateButton(
            "StoryIntroPreviousButton",
            dialoguePanel,
            "\uC774\uC804",
            new Vector2(0.05f, 0.05f),
            new Vector2(0.31f, 0.23f),
            PanelLight,
            () => previousAction?.Invoke());
        previousButtonText =
            previousButton.GetComponentInChildren<TMP_Text>();
        previousButtonText.fontSizeMax = 21;

        Button nextButton = RuntimeUiFactory.CreateButton(
            "StoryIntroNextButton",
            dialoguePanel,
            "\uB2E4\uC74C",
            new Vector2(0.64f, 0.05f),
            new Vector2(0.95f, 0.23f),
            new Color32(255, 201, 77, 255),
            () => nextAction?.Invoke());
        buttonText = nextButton.GetComponentInChildren<TMP_Text>();

        RuntimeUiFactory.CreateText(
            "StoryIntroTapHint",
            dialoguePanel,
            "\uD654\uBA74\uC744 \uB204\uB974\uBA74 \uB2E4\uC74C \uCEF7\uC73C\uB85C \uC774\uB3D9",
            20,
            new Vector2(0.32f, 0.05f),
            new Vector2(0.63f, 0.23f),
            TextAlignmentOptions.Center,
            new Color32(190, 203, 225, 255));

        overlayObject.SetActive(false);
    }

    public void Refresh(TutorialManager tutorialManager)
    {
        if (tutorialManager == null)
        {
            overlayObject?.SetActive(false);
            return;
        }

        bool shouldShow = tutorialManager.ShouldShowStoryIntro;
        overlayObject?.SetActive(shouldShow);
        if (!shouldShow)
            return;

        StoryIntroCut cut = tutorialManager.CurrentStoryCut;
        List<StoryIntroCut> cuts = tutorialManager.StoryCuts;
        if (cut == null || cuts.Count == 0)
        {
            overlayObject?.SetActive(false);
            return;
        }

        if (counterText != null)
            counterText.text = $"{cut.cutIndex}/{cuts.Count}";
        if (titleText != null)
            titleText.text = cut.title;
        if (bodyText != null)
        {
            bodyText.text = string.IsNullOrWhiteSpace(cut.body)
                ? "\uB300\uC0AC\uB294 \uCD94\uD6C4 \uD655\uC815"
                : cut.body;
        }

        Sprite cutArt = LoadOptionalSprite(cut.artResourcePath);
        bool hasCutArt = cutArt != null;
        if (artText != null)
        {
            artText.text = hasCutArt
                ? string.Empty
                : string.IsNullOrWhiteSpace(cut.artDirection)
                    ? "(\uC544\uD2B8 \uD544\uC694)"
                    : cut.artDirection;
        }

        if (artImage != null)
        {
            artImage.sprite = cutArt;
            artImage.type = Image.Type.Simple;
            artImage.preserveAspect = true;
            artImage.color = hasCutArt
                ? Color.white
                : TryParseColor(cut.placeholderColorHex, PanelLight);
        }

        bool canGoPrevious = tutorialManager.CurrentStoryCutIndex > 0;
        if (previousButton != null)
            previousButton.interactable = canGoPrevious;
        if (previousButtonText != null)
        {
            previousButtonText.color = canGoPrevious
                ? Color.white
                : DisabledText;
        }

        if (buttonText != null)
        {
            buttonText.text = cut.cutIndex >= cuts.Count
                ? "\uC791\uC804 \uC2DC\uC791"
                : "\uB2E4\uC74C";
        }
    }

    private void Bind(
        RectTransform overlay,
        Action nextAction,
        Action previousAction)
    {
        overlayObject = overlay.gameObject;
        counterText = RuntimeUiBinder.FindText(overlay, "StoryIntroCounter");
        titleText = RuntimeUiBinder.FindText(overlay, "StoryIntroTitle");
        bodyText = RuntimeUiBinder.FindText(overlay, "StoryIntroBody");
        artText = RuntimeUiBinder.FindText(overlay, "StoryIntroArtText");
        artImage = RuntimeUiBinder.FindImage(
            overlay,
            "StoryIntroArtPlaceholder");

        Button tapArea = overlay.GetComponent<Button>();
        if (tapArea == null)
            tapArea = overlay.gameObject.AddComponent<Button>();
        tapArea.targetGraphic = overlay.GetComponent<Image>();
        RuntimeUiBinder.ReplaceButtonAction(
            tapArea,
            () => nextAction?.Invoke());

        previousButton = RuntimeUiBinder.FindButton(
            overlay,
            "StoryIntroPreviousButton");
        previousButtonText =
            previousButton == null
                ? null
                : previousButton.GetComponentInChildren<TMP_Text>();
        RuntimeUiBinder.ReplaceButtonAction(
            previousButton,
            () => previousAction?.Invoke());

        Button nextButton = RuntimeUiBinder.FindButton(
            overlay,
            "StoryIntroNextButton");
        buttonText =
            nextButton == null
                ? null
                : nextButton.GetComponentInChildren<TMP_Text>();
        RuntimeUiBinder.ReplaceButtonAction(
            nextButton,
            () => nextAction?.Invoke());

        overlayObject.SetActive(false);
    }

    private static Color TryParseColor(string htmlColor, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
            return fallback;

        return ColorUtility.TryParseHtmlString(
            htmlColor,
            out Color parsed)
            ? parsed
            : fallback;
    }

    private static Sprite LoadOptionalSprite(string resourcePath)
    {
        return string.IsNullOrWhiteSpace(resourcePath)
            ? null
            : Resources.Load<Sprite>(resourcePath);
    }
}
