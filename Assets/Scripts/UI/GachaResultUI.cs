using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class GachaResultUI
{
    private readonly TMP_Text placeholderText;
    private readonly TMP_Text pointText;
    private readonly Button confirmButton;
    private readonly RectTransform[] cards = new RectTransform[10];
    private readonly Image[] portraits = new Image[10];
    private readonly Image[] frames = new Image[10];
    private readonly TMP_Text[] labels = new TMP_Text[10];
    private readonly TMP_Text[] badges = new TMP_Text[10];
    private readonly TMP_Text[] counts = new TMP_Text[10];

    public bool IsVisible { get; private set; }

    public GachaResultUI(
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
            "Point 0",
            22,
            new Vector2(0.76f, 0.01f),
            new Vector2(0.97f, 0.08f),
            TextAlignmentOptions.Right,
            new Color32(36, 66, 95, 255));

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

    public void SetPoint(int point)
    {
        if (pointText == null)
            return;

        pointText.text =
            $"{LocalizationManager.Text("Point", "포인트")} {point}";
    }

    public void ShowResults(
        List<CharacterData> results,
        Dictionary<string, int> ownedBefore)
    {
        Dictionary<string, int> shownCounts =
            new Dictionary<string, int>();
        IsVisible = results != null && results.Count > 0;
        placeholderText.gameObject.SetActive(!IsVisible);
        confirmButton.gameObject.SetActive(IsVisible);

        for (int index = 0; index < cards.Length; index++)
        {
            bool hasResult = results != null && index < results.Count;
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
            portraits[index].sprite = portrait;
            portraits[index].color =
                portrait == null
                    ? GetRarityFallbackColor(character.rarity)
                    : Color.white;
            frames[index].color = GetGachaCardColor(character.rarity);
            labels[index].text =
                $"[{character.rarity}]\n{character.characterName}";

            bool showBadge = isNew || character.rarity != "R";
            badges[index].gameObject.SetActive(showBadge);
            badges[index].text = isNew
                ? LocalizationManager.Text("NEW", "신규")
                : character.rarity;

            counts[index].text = duplicateResult ? "x5" : "x1";
        }
    }

    public void Clear()
    {
        IsVisible = false;
        confirmButton.gameObject.SetActive(false);
        placeholderText.gameObject.SetActive(true);
        placeholderText.text = LocalizationManager.Text(
            "Recruit companions to see results.",
            "동료를 모집하면 결과가 표시됩니다.");

        for (int index = 0; index < cards.Length; index++)
        {
            cards[index].gameObject.SetActive(false);
        }
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
                "x1",
                22,
                new Vector2(0.58f, 0.18f),
                new Vector2(0.94f, 0.38f),
                TextAlignmentOptions.Right,
                new Color32(39, 52, 72, 255));

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
