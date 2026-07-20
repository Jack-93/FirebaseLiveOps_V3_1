using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TitleScreenUI
{
    private GameObject overlay;
    private TMP_Text statusText;
    private Button googleButton;
    private Button guestButton;

    private static readonly Color Background =
        new Color32(17, 24, 39, 255);
    private static readonly Color Panel =
        new Color32(37, 49, 73, 245);
    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public GameObject GameObject => overlay;

    public TitleScreenUI(
        RectTransform root,
        Action startGoogle,
        Action startGuest,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "TitleOverlay",
                root,
                out RectTransform overlayRect))
        {
            Bind(overlayRect, startGoogle, startGuest);
            return;
        }

        BuildGenerated(root, startGoogle, startGuest);
    }

    public void BuildGenerated(
        RectTransform root,
        Action startGoogle,
        Action startGuest)
    {
        RectTransform overlayRect = RuntimeUiFactory.CreatePanel(
            "TitleOverlay",
            root,
            Background,
            Vector2.zero,
            Vector2.one);
        overlay = overlayRect.gameObject;

        RectTransform artPanel = RuntimeUiFactory.CreatePanel(
            "TitleArtPanel",
            overlayRect,
            Panel,
            new Vector2(0.08f, 0.43f),
            new Vector2(0.92f, 0.64f));
        Image artImage = RuntimeUiFactory.CreateSpriteImage(
            "TitleStagePreview",
            artPanel,
            PrototypeBattleArt.GetStageBackground(1),
            Vector2.zero,
            Vector2.one);
        artImage.preserveAspect = false;
        RuntimeUiFactory.CreatePanel(
            "TitlePreviewShade",
            artPanel,
            new Color32(8, 12, 22, 90),
            Vector2.zero,
            Vector2.one);
        RuntimeUiFactory.CreateSpriteImage(
            "TitleSupportSparrow",
            artPanel,
            PrototypeBattleArt.GetSupportHeroSprite(),
            new Vector2(0.12f, 0.08f),
            new Vector2(0.42f, 0.44f));

        RuntimeUiFactory.CreateText(
            "ArtNeededLabel",
            artPanel,
            "PIXEL IDLE RPG",
            42,
            new Vector2(0.08f, 0.64f),
            new Vector2(0.92f, 0.82f),
            TextAlignmentOptions.Center,
            Accent);

        RuntimeUiFactory.CreateText(
            "ArtNeededDescription",
            artPanel,
            "Sparrows survive cat raids with their flight squad.",
            25,
            new Vector2(0.32f, 0.16f),
            new Vector2(0.9f, 0.36f),
            TextAlignmentOptions.Center,
            MutedText);

        RuntimeUiFactory.CreateText(
            "GameLogo",
            overlayRect,
            "IDLE RPG\nPROTOTYPE",
            64,
            new Vector2(0.08f, 0.67f),
            new Vector2(0.92f, 0.84f),
            TextAlignmentOptions.Center,
            Accent);

        googleButton = RuntimeUiFactory.CreateButton(
            "TitleGoogleButton",
            overlayRect,
            "START WITH GOOGLE",
            new Vector2(0.12f, 0.31f),
            new Vector2(0.88f, 0.38f),
            Accent,
            () => startGoogle?.Invoke());

        guestButton = RuntimeUiFactory.CreateButton(
            "TitleGuestButton",
            overlayRect,
            "PLAY AS GUEST",
            new Vector2(0.12f, 0.22f),
            new Vector2(0.88f, 0.29f),
            PanelLight,
            () => startGuest?.Invoke());

        statusText = RuntimeUiFactory.CreateText(
            "TitleStatus",
            overlayRect,
            "Checking login...",
            24,
            new Vector2(0.1f, 0.11f),
            new Vector2(0.9f, 0.2f),
            TextAlignmentOptions.Center,
            MutedText);

        RuntimeUiFactory.CreateText(
            "TitleNotice",
            overlayRect,
            "Android only. Guest progress can be linked to Google later.",
            22,
            new Vector2(0.1f, 0.05f),
            new Vector2(0.9f, 0.1f),
            TextAlignmentOptions.Center,
            MutedText);

        overlay.SetActive(false);
    }

    public void Show(string status)
    {
        overlay?.SetActive(true);
        SetBusy(false, status);
    }

    public void Hide()
    {
        overlay?.SetActive(false);
    }

    public void SetBusy(bool busy, string status)
    {
        if (statusText != null)
        {
            statusText.text = string.IsNullOrWhiteSpace(status)
                ? "Android build uses Google login or guest play."
                : status;
        }
        if (googleButton != null)
            googleButton.interactable = !busy;
        if (guestButton != null)
            guestButton.interactable = !busy;
    }

    private void Bind(
        RectTransform overlayRect,
        Action startGoogle,
        Action startGuest)
    {
        overlay = overlayRect.gameObject;
        statusText = RuntimeUiBinder.FindText(overlayRect, "TitleStatus");
        googleButton =
            RuntimeUiBinder.FindButton(overlayRect, "TitleGoogleButton");
        guestButton =
            RuntimeUiBinder.FindButton(overlayRect, "TitleGuestButton");
        RuntimeUiBinder.ReplaceButtonAction(
            googleButton,
            () => startGoogle?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            guestButton,
            () => startGuest?.Invoke());
        overlay.SetActive(false);
    }
}
