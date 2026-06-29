using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class GachaResultUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private TMP_Text placeholderText;
    private TMP_Text pointText;
    private SpriteNumberText pointNumberText;
    private Button confirmButton;
    private readonly RectTransform[] cards = new RectTransform[10];
    private readonly Image[] portraits = new Image[10];
    private readonly Image[] frames = new Image[10];
    private readonly TMP_Text[] labels = new TMP_Text[10];
    private readonly TMP_Text[] badges = new TMP_Text[10];
    private readonly TMP_Text[] counts = new TMP_Text[10];
    private readonly SpriteNumberText[] countNumberTexts =
        new SpriteNumberText[10];

    public bool IsVisible { get; private set; }

    public GachaResultUI(
        RectTransform panel,
        UnityAction onConfirm,
        Color accent)
    {
        Build(panel, onConfirm, accent);
    }

    public GachaResultUI(
        RectTransform panel,
        UnityAction onConfirm,
        Color accent,
        bool bindExisting)
    {
        if (bindExisting)
            Bind(panel, onConfirm);
        else
            Build(panel, onConfirm, accent);
    }

    public void SetPoint(int point)
    {
        if (pointText == null || pointNumberText == null)
            return;

        pointText.text = LocalizationManager.Text(
            "Point",
            "\uD3EC\uC778\uD2B8");
        pointNumberText.SetText(CompactNumberFormatter.Format(point));
    }

    public void ShowResults(
        List<CharacterData> results,
        Dictionary<string, int> ownedBefore)
    {
        Dictionary<string, int> shownCounts =
            new Dictionary<string, int>();
        IsVisible = results != null && results.Count > 0;
        if (placeholderText != null)
            placeholderText.gameObject.SetActive(!IsVisible);
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(IsVisible);

        for (int index = 0; index < cards.Length; index++)
        {
            bool hasResult = results != null && index < results.Count;
            if (cards[index] != null)
                cards[index].gameObject.SetActive(hasResult);
            if (!hasResult)
                continue;

            CharacterData character = results[index];
            if (character == null)
                continue;

            if (!shownCounts.ContainsKey(character.characterName))
                shownCounts[character.characterName] = 0;
            shownCounts[character.characterName]++;

            bool alreadyOwned =
                ownedBefore != null &&
                ownedBefore.TryGetValue(
                    character.characterName,
                    out int owned) &&
                owned > 0;
            bool isNew = !alreadyOwned &&
                shownCounts[character.characterName] == 1;
            bool duplicateResult =
                alreadyOwned ||
                shownCounts[character.characterName] > 1;

            Sprite portrait = character.icon ?? character.battleSprite;
            if (portraits[index] != null)
            {
                portraits[index].sprite = portrait;
                portraits[index].color =
                    portrait == null
                        ? GetRarityFallbackColor(character.rarity)
                        : Color.white;
            }

            if (frames[index] != null)
                frames[index].color = GetGachaCardColor(character.rarity);

            if (labels[index] != null)
            {
                labels[index].text =
                    $"[{character.rarity}]\n{character.characterName}";
            }

            bool showBadge = isNew || character.rarity != "R";
            if (badges[index] != null)
            {
                badges[index].gameObject.SetActive(showBadge);
                badges[index].text = isNew
                    ? LocalizationManager.Text(
                        "NEW",
                        "\uC2E0\uADDC")
                    : character.rarity;
            }

            if (counts[index] != null)
                counts[index].text = "x";
            countNumberTexts[index]?.SetText(
                duplicateResult ? "5" : "1");
        }
    }

    public void Clear()
    {
        IsVisible = false;
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);
        if (placeholderText != null)
        {
            placeholderText.gameObject.SetActive(true);
            placeholderText.text = LocalizationManager.Text(
                "Recruit companions to see results.",
                "\uB3D9\uB8CC\uB97C \uBAA8\uC9D1\uD558\uBA74 " +
                "\uACB0\uACFC\uAC00 \uD45C\uC2DC\uB429\uB2C8\uB2E4.");
        }

        for (int index = 0; index < cards.Length; index++)
        {
            if (cards[index] != null)
                cards[index].gameObject.SetActive(false);
        }
    }

    private void Build(
        RectTransform panel,
        UnityAction onConfirm,
        Color accent)
    {
        RectTransform resultCard = RuntimeUiFactory.CreatePanel(
            "GachaResultCard",
            panel,
            new Color32(220, 242, 250, 220),
            new Vector2(0.04f, 0.22f),
            new Vector2(0.96f, 0.66f));

        placeholderText = RuntimeUiFactory.CreateText(
            "GachaResultText",
            resultCard,
            "Recruit companions to see results.",
            27,
            new Vector2(0.06f, 0.08f),
            new Vector2(0.94f, 0.92f),
            TextAlignmentOptions.Center,
            new Color32(45, 60, 82, 255));

        BuildGrid(resultCard);

        pointText = RuntimeUiFactory.CreateText(
            "RecruitPointText",
            resultCard,
            "Point",
            22,
            new Vector2(0.76f, 0.01f),
            new Vector2(0.88f, 0.08f),
            TextAlignmentOptions.Right,
            new Color32(36, 66, 95, 255));
        pointNumberText = new SpriteNumberText(
            resultCard,
            "RecruitPointNumberText",
            NumberResourceRoot,
            20f,
            new Vector2(0.88f, 0.01f),
            new Vector2(0.97f, 0.08f));

        confirmButton = RuntimeUiFactory.CreateButton(
            "GachaResultConfirmButton",
            panel,
            "CONFIRM",
            new Vector2(0.34f, 0.055f),
            new Vector2(0.66f, 0.145f),
            accent,
            onConfirm);
        confirmButton.gameObject.SetActive(false);
    }

    private void Bind(
        RectTransform panel,
        UnityAction onConfirm)
    {
        RectTransform resultCard =
            RuntimeUiBinder.FindRect(panel, "GachaResultCard");
        placeholderText =
            RuntimeUiBinder.FindText(resultCard, "GachaResultText");
        pointText =
            RuntimeUiBinder.FindText(resultCard, "RecruitPointText");
        pointNumberText = new SpriteNumberText(
            RuntimeUiBinder.FindRect(
                resultCard,
                "RecruitPointNumberText"),
            NumberResourceRoot,
            20f);

        confirmButton =
            RuntimeUiBinder.FindButton(panel, "GachaResultConfirmButton");
        RuntimeUiBinder.ReplaceButtonAction(confirmButton, onConfirm);
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);

        BindGrid(resultCard);
    }

    private void BuildGrid(RectTransform parent)
    {
        Vector2 start = new Vector2(0.06f, 0.51f);
        Vector2 cell = new Vector2(0.17f, 0.37f);
        Vector2 gap = new Vector2(0.015f, 0.08f);

        for (int index = 0; index < cards.Length; index++)
        {
            int column = index % 5;
            int row = index / 5;
            float left = start.x + column * (cell.x + gap.x);
            float top = start.y - row * (cell.y + gap.y);

            RectTransform card = RuntimeUiFactory.CreatePanel(
                $"GachaResultSlot{index + 1}",
                parent,
                new Color32(245, 252, 255, 225),
                new Vector2(left, top),
                new Vector2(left + cell.x, top + cell.y));
            cards[index] = card;

            Image frame = RuntimeUiFactory.CreateSpriteImage(
                "Frame",
                card,
                PrototypeUiArt.ButtonNormal,
                Vector2.zero,
                Vector2.one);
            frame.type = Image.Type.Sliced;
            frame.preserveAspect = false;
            frames[index] = frame;

            portraits[index] = RuntimeUiFactory.CreateSpriteImage(
                "Portrait",
                card,
                null,
                new Vector2(0.16f, 0.24f),
                new Vector2(0.84f, 0.82f));

            badges[index] = RuntimeUiFactory.CreateText(
                "NewBadge",
                card,
                "NEW",
                18,
                new Vector2(0.02f, 0.78f),
                new Vector2(0.53f, 0.98f),
                TextAlignmentOptions.Left,
                new Color32(255, 201, 77, 255));

            labels[index] = RuntimeUiFactory.CreateText(
                "Name",
                card,
                "",
                18,
                new Vector2(0.06f, 0.04f),
                new Vector2(0.94f, 0.24f),
                TextAlignmentOptions.Center,
                new Color32(39, 52, 72, 255));

            counts[index] = RuntimeUiFactory.CreateText(
                "Count",
                card,
                "x",
                22,
                new Vector2(0.58f, 0.18f),
                new Vector2(0.72f, 0.38f),
                TextAlignmentOptions.Right,
                new Color32(39, 52, 72, 255));
            countNumberTexts[index] = new SpriteNumberText(
                card,
                "CountNumberText",
                NumberResourceRoot,
                20f,
                new Vector2(0.72f, 0.18f),
                new Vector2(0.94f, 0.38f));

            card.gameObject.SetActive(false);
        }
    }

    private void BindGrid(RectTransform parent)
    {
        for (int index = 0; index < cards.Length; index++)
        {
            RectTransform card = RuntimeUiBinder.FindRect(
                parent,
                $"GachaResultSlot{index + 1}");
            cards[index] = card;
            if (card == null)
                continue;

            frames[index] =
                RuntimeUiBinder.FindImage(card, "Frame");
            portraits[index] =
                RuntimeUiBinder.FindImage(card, "Portrait");
            badges[index] =
                RuntimeUiBinder.FindText(card, "NewBadge");
            labels[index] =
                RuntimeUiBinder.FindText(card, "Name");
            counts[index] =
                RuntimeUiBinder.FindText(card, "Count");
            countNumberTexts[index] = new SpriteNumberText(
                RuntimeUiBinder.FindRect(card, "CountNumberText"),
                NumberResourceRoot,
                20f);
            card.gameObject.SetActive(false);
        }
    }

    private static Color GetGachaCardColor(string rarity)
    {
        switch (rarity)
        {
            case "SSR":
                return new Color32(255, 205, 106, 255);
            case "SR":
                return new Color32(213, 151, 255, 255);
            default:
                return new Color32(176, 214, 255, 255);
        }
    }

    private static Color GetRarityFallbackColor(string rarity)
    {
        switch (rarity)
        {
            case "SSR":
                return new Color32(184, 112, 255, 255);
            case "SR":
                return new Color32(77, 137, 235, 255);
            default:
                return new Color32(52, 68, 96, 255);
        }
    }
}
