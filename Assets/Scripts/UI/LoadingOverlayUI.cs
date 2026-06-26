using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LoadingOverlayUI
{
    private readonly GameObject overlay;
    private readonly TMP_Text loadingText;
    private readonly Button retryButton;

    private static readonly Color Background =
        new Color32(17, 24, 39, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);

    public LoadingOverlayUI(RectTransform root, Action retry)
    {
        RectTransform overlayRect = RuntimeUiFactory.CreatePanel(
            "LoadingOverlay",
            root,
            Background,
            Vector2.zero,
            Vector2.one);
        overlay = overlayRect.gameObject;

        RuntimeUiFactory.CreateText(
            "GameTitle",
            overlayRect,
            "IDLE RPG PROTOTYPE",
            62,
            new Vector2(0.08f, 0.58f),
            new Vector2(0.92f, 0.72f),
            TextAlignmentOptions.Center,
            Accent);

        loadingText = RuntimeUiFactory.CreateText(
            "LoadingText",
            overlayRect,
            "Loading...",
            34,
            new Vector2(0.08f, 0.4f),
            new Vector2(0.92f, 0.57f),
            TextAlignmentOptions.Center,
            Color.white);

        retryButton = RuntimeUiFactory.CreateButton(
            "RetryButton",
            overlayRect,
            "RETRY",
            new Vector2(0.3f, 0.31f),
            new Vector2(0.7f, 0.39f),
            Accent,
            () => retry?.Invoke());
        retryButton.gameObject.SetActive(false);
    }

    public void SetLoading(bool visible, string message)
    {
        overlay.SetActive(visible);
        loadingText.text = message;
        retryButton.gameObject.SetActive(false);
    }

    public void ShowError(string message)
    {
        overlay.SetActive(true);
        loadingText.text = "Connection failed\n\n" + message;
        retryButton.gameObject.SetActive(true);
    }
}
