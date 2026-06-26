using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StoryIntroUI
{
    private readonly GameObject overlayObject;
    private readonly TMP_Text counterText;
    private readonly TMP_Text titleText;
    private readonly TMP_Text bodyText;
    private readonly TMP_Text artText;
    private readonly TMP_Text buttonText;
    private readonly Image artImage;

    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);

    public StoryIntroUI(
        RectTransform root,
        Action nextAction,
        Action skipAction)
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
            "1 / 7",
            24,
            new Vector2(0.05f, 0.74f),
            new Vector2(0.24f, 0.94f),
            TextAlignmentOptions.Left,
            new Color32(255, 201, 77, 255));

        titleText = RuntimeUiFactory.CreateText(
            "StoryIntroTitle",
            dialoguePanel,
            "전봇대 위의 세상",
            36,
            new Vector2(0.25f, 0.7f),
            new Vector2(0.95f, 0.94f),
            TextAlignmentOptions.Right,
            new Color32(82, 188, 255, 255));

        bodyText = RuntimeUiFactory.CreateText(
            "StoryIntroBody",
            dialoguePanel,
            "대사는 추후 확정",
            31,
            new Vector2(0.05f, 0.26f),
            new Vector2(0.95f, 0.68f),
            TextAlignmentOptions.Left,
            Color.white);

        Button skipButton = RuntimeUiFactory.CreateButton(
            "StoryIntroSkipButton",
            dialoguePanel,
            "SKIP",
            new Vector2(0.05f, 0.05f),
            new Vector2(0.31f, 0.23f),
            PanelLight,
            () => skipAction?.Invoke());
        skipButton.GetComponentInChildren<TMP_Text>().fontSizeMax = 21;

        Button nextButton = RuntimeUiFactory.CreateButton(
            "StoryIntroNextButton",
            dialoguePanel,
            "NEXT",
            new Vector2(0.64f, 0.05f),
            new Vector2(0.95f, 0.23f),
            new Color32(255, 201, 77, 255),
            () => nextAction?.Invoke());
        buttonText = nextButton.GetComponentInChildren<TMP_Text>();

        RuntimeUiFactory.CreateText(
            "StoryIntroTapHint",
            dialoguePanel,
            "화면을 눌러 다음 컷으로 이동",
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
            overlayObject.SetActive(false);
            return;
        }

        bool shouldShow = tutorialManager.ShouldShowStoryIntro;
        overlayObject.SetActive(shouldShow);
        if (!shouldShow)
            return;

        StoryIntroCut cut = tutorialManager.CurrentStoryCut;
        IReadOnlyList<StoryIntroCut> cuts = tutorialManager.StoryCuts;
        if (cut == null || cuts.Count == 0)
        {
            overlayObject.SetActive(false);
            return;
        }

        counterText.text = $"{cut.cutIndex} / {cuts.Count}";
        titleText.text = cut.title;
        bodyText.text = string.IsNullOrWhiteSpace(cut.body)
            ? "대사는 추후 확정"
            : cut.body;

        Sprite cutArt = LoadOptionalSprite(cut.artResourcePath);
        bool hasCutArt = cutArt != null;
        artText.text = hasCutArt
            ? string.Empty
            : string.IsNullOrWhiteSpace(cut.artDirection)
                ? "(아트 필요)"
                : cut.artDirection;

        artImage.sprite = cutArt;
        artImage.type = Image.Type.Simple;
        artImage.preserveAspect = true;
        artImage.color = hasCutArt
            ? Color.white
            : TryParseColor(cut.placeholderColorHex, PanelLight);

        buttonText.text = cut.cutIndex >= cuts.Count
            ? "작전 시작"
            : "다음";
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
