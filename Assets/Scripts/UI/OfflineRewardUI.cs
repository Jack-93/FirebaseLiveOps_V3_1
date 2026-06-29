using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class OfflineRewardUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private GameObject overlay;
    private TMP_Text rewardText;
    private TMP_Text awayLabelText;
    private TMP_Text awaySuffixText;
    private TMP_Text goldLabelText;
    private SpriteNumberText awayNumberText;
    private SpriteNumberText goldNumberText;
    private Button confirmButton;

    public GameObject GameObject => overlay;

    public OfflineRewardUI(RectTransform root, bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "OfflineOverlay",
                root,
                out RectTransform overlayRect))
        {
            Bind(overlayRect);
            return;
        }

        BuildGenerated(root);
    }

    public void BuildGenerated(RectTransform root)
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
            new Vector2(0.08f, 0.68f),
            new Vector2(0.92f, 0.9f),
            TextAlignmentOptions.Center,
            new Color32(255, 201, 77, 255));
        awayLabelText = RuntimeUiFactory.CreateText(
            "OfflineAwayLabel",
            card,
            "Away",
            26,
            new Vector2(0.15f, 0.52f),
            new Vector2(0.36f, 0.64f),
            TextAlignmentOptions.Right,
            Color.white);
        awayNumberText = new SpriteNumberText(
            card,
            "OfflineAwayNumberText",
            NumberResourceRoot,
            28f,
            new Vector2(0.38f, 0.51f),
            new Vector2(0.58f, 0.65f));
        awaySuffixText = RuntimeUiFactory.CreateText(
            "OfflineAwaySuffix",
            card,
            "min",
            24,
            new Vector2(0.58f, 0.52f),
            new Vector2(0.72f, 0.64f),
            TextAlignmentOptions.Left,
            Color.white);
        goldLabelText = RuntimeUiFactory.CreateText(
            "OfflineGoldLabel",
            card,
            "Gold",
            26,
            new Vector2(0.15f, 0.36f),
            new Vector2(0.36f, 0.48f),
            TextAlignmentOptions.Right,
            Color.white);
        goldNumberText = new SpriteNumberText(
            card,
            "OfflineGoldNumberText",
            NumberResourceRoot,
            30f,
            new Vector2(0.38f, 0.35f),
            new Vector2(0.75f, 0.49f));

        confirmButton = RuntimeUiFactory.CreateButton(
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
        if (rewardText != null)
        {
            rewardText.text = LocalizationManager.Text(
                "Welcome back!",
                "\uB2E4\uC2DC \uC624\uC2E0 \uAC78 \uD658\uC601\uD574\uC694!");
        }
        if (awayLabelText != null)
        {
            awayLabelText.text = LocalizationManager.Text(
                "Away",
                "\uC790\uB9AC\uBE44\uC6C0");
        }
        if (awaySuffixText != null)
            awaySuffixText.text = LocalizationManager.Text("min", "\uBD84");
        if (goldLabelText != null)
            goldLabelText.text = LocalizationManager.Translate("Gold");
        awayNumberText?.SetText(CompactNumberFormatter.Format(minutes));
        goldNumberText?.SetText(CompactNumberFormatter.Format(gold));
        overlay?.SetActive(true);
    }

    private void Hide()
    {
        overlay?.SetActive(false);
    }

    private void Bind(RectTransform overlayRect)
    {
        overlay = overlayRect.gameObject;
        rewardText = RuntimeUiBinder.FindText(overlayRect, "OfflineText");
        awayLabelText =
            RuntimeUiBinder.FindText(overlayRect, "OfflineAwayLabel");
        awaySuffixText =
            RuntimeUiBinder.FindText(overlayRect, "OfflineAwaySuffix");
        goldLabelText =
            RuntimeUiBinder.FindText(overlayRect, "OfflineGoldLabel");
        awayNumberText = RuntimeUiBinder.BindNumber(
            overlayRect,
            "OfflineAwayNumberText",
            NumberResourceRoot,
            28f);
        goldNumberText = RuntimeUiBinder.BindNumber(
            overlayRect,
            "OfflineGoldNumberText",
            NumberResourceRoot,
            30f);
        confirmButton =
            RuntimeUiBinder.FindButton(overlayRect, "OfflineConfirm");
        RuntimeUiBinder.ReplaceButtonAction(confirmButton, Hide);
        overlay.SetActive(false);
    }
}
