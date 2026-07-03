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
    private RectTransform artRect;

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
            Color.black,
            Vector2.zero,
            Vector2.one);
        artRect = artPanel;
        artImage = artPanel.GetComponent<Image>();
        artImage.preserveAspect = false;

        previousButton = CreateInvisibleTouchButton(
            "StoryIntroPreviousTouchArea",
            overlay,
            new Vector2(0f, 0f),
            new Vector2(0.22f, 1f),
            previousAction);

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
            artImage.preserveAspect = false;
            artImage.color = hasCutArt
                ? Color.white
                : TryParseColor(cut.placeholderColorHex, PanelLight);
        }

        bool canGoPrevious = tutorialManager.CurrentStoryCutIndex > 0;
        if (previousButton != null)
        {
            previousButton.gameObject.SetActive(canGoPrevious);
            previousButton.interactable = canGoPrevious;
        }
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
        artRect = artImage == null
            ? RuntimeUiBinder.FindRect(overlay, "StoryIntroArtPlaceholder")
            : artImage.GetComponent<RectTransform>();

        ApplyFullscreenArtLayout();
        HideStoryTextChrome(overlay);

        Button tapArea = overlay.GetComponent<Button>();
        if (tapArea == null)
            tapArea = overlay.gameObject.AddComponent<Button>();
        tapArea.targetGraphic = overlay.GetComponent<Image>();
        RuntimeUiBinder.ReplaceButtonAction(
            tapArea,
            () => nextAction?.Invoke());

        previousButton = CreateInvisibleTouchButton(
            "StoryIntroPreviousTouchArea",
            overlay,
            new Vector2(0f, 0f),
            new Vector2(0.22f, 1f),
            previousAction);
        previousButtonText = null;

        buttonText = null;

        overlayObject.SetActive(false);
    }

    private void ApplyFullscreenArtLayout()
    {
        if (artRect == null)
            return;

        artRect.anchorMin = Vector2.zero;
        artRect.anchorMax = Vector2.one;
        artRect.offsetMin = Vector2.zero;
        artRect.offsetMax = Vector2.zero;
        artRect.SetAsFirstSibling();

        if (artImage != null)
        {
            artImage.type = Image.Type.Simple;
            artImage.preserveAspect = false;
            artImage.raycastTarget = false;
        }
    }

    private static void HideStoryTextChrome(RectTransform overlay)
    {
        HideByName(overlay, "StoryIntroPixelFrame");
        HideByName(overlay, "StoryIntroArtText");
        HideByName(overlay, "StoryIntroDialoguePanel");
        HideByName(overlay, "StoryIntroCounter");
        HideByName(overlay, "StoryIntroTitle");
        HideByName(overlay, "StoryIntroBody");
        HideByName(overlay, "StoryIntroTapHint");
        HideByName(overlay, "StoryIntroPreviousButton");
        HideByName(overlay, "StoryIntroNextButton");
    }

    private static void HideByName(RectTransform root, string name)
    {
        RectTransform target = RuntimeUiBinder.FindRect(root, name);
        if (target != null)
            target.gameObject.SetActive(false);
    }

    private static Button CreateInvisibleTouchButton(
        string name,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Action action)
    {
        RectTransform existing = RuntimeUiBinder.FindRect(parent, name);
        RectTransform rect = existing != null
            ? existing
            : RuntimeUiFactory.CreatePanel(
                name,
                parent,
                new Color32(0, 0, 0, 0),
                anchorMin,
                anchorMax);

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        Image image = rect.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color32(0, 0, 0, 0);
            image.raycastTarget = true;
        }

        Button button = rect.GetComponent<Button>();
        if (button == null)
            button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        RuntimeUiBinder.ReplaceButtonAction(
            button,
            () => action?.Invoke());
        return button;
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
