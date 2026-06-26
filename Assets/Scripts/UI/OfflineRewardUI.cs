using TMPro;
using UnityEngine;

public sealed class OfflineRewardUI
{
    private readonly GameObject overlay;
    private readonly TMP_Text rewardText;

    public OfflineRewardUI(RectTransform root)
    {
        RectTransform overlayRect = RuntimeUiFactory.CreatePanel(
            "OfflineOverlay",
            root,
            new Color(0f, 0f, 0f, 0.75f),
            Vector2.zero,
            Vector2.one);
        overlay = overlayRect.gameObject;

        RectTransform card = RuntimeUiFactory.CreatePanel(
            "OfflineCard",
            overlayRect,
            new Color32(37, 49, 73, 245),
            new Vector2(0.1f, 0.32f),
            new Vector2(0.9f, 0.68f));

        rewardText = RuntimeUiFactory.CreateText(
            "OfflineText",
            card,
            "Welcome back!",
            42,
            new Vector2(0.08f, 0.3f),
            new Vector2(0.92f, 0.9f),
            TextAlignmentOptions.Center,
            new Color32(255, 201, 77, 255));

        RuntimeUiFactory.CreateButton(
            "OfflineConfirm",
            card,
            "COLLECT",
            new Vector2(0.22f, 0.08f),
            new Vector2(0.78f, 0.27f),
            new Color32(76, 205, 145, 255),
            Hide);

        overlay.SetActive(false);
    }

    public void Show(long seconds, int gold)
    {
        long minutes = System.Math.Max(1, seconds / 60);
        rewardText.text =
            $"Welcome back!\n\nAway: {minutes} min\nGold earned: {gold:N0}";
        overlay.SetActive(true);
    }

    private void Hide()
    {
        overlay.SetActive(false);
    }
}
