using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CollectionPanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text characterDetailText;
    private readonly Image characterDetailPortraitImage;
    private readonly CompanionSlotButtonsUI slotButtons;

    public GameObject GameObject => panel.gameObject;

    private static readonly Color OverlayBackground =
        new Color32(12, 18, 30, 218);
    private static readonly Color Panel =
        new Color32(37, 49, 73, 245);
    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public CollectionPanelUI(
        RectTransform root,
        CompanionManager companionManager,
        Action showMore,
        Action<CharacterData> selectCharacter,
        Action promoteSelectedCharacter,
        Action<int> toggleSelectedCharacterSlot)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "CollectionPanel",
            root,
            OverlayBackground,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateButton(
            "CollectionBackButton",
            panel,
            "BACK",
            new Vector2(0.04f, 0.9f),
            new Vector2(0.22f, 0.97f),
            PanelLight,
            () => showMore?.Invoke());

        RuntimeUiFactory.CreateText(
            "CollectionTitle",
            panel,
            "COMPANIONS",
            46,
            new Vector2(0.24f, 0.9f),
            new Vector2(0.96f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RuntimeUiFactory.CreateText(
            "CollectionSubtitle",
            panel,
            "Select a companion, then equip it to a party slot.",
            24,
            new Vector2(0.24f, 0.86f),
            new Vector2(0.96f, 0.9f),
            TextAlignmentOptions.Center,
            MutedText);

        BuildCharacterButtons(companionManager, selectCharacter);

        RectTransform detailCard = RuntimeUiFactory.CreatePanel(
            "CharacterDetailCard",
            panel,
            Panel,
            new Vector2(0.04f, 0.05f),
            new Vector2(0.96f, 0.34f));

        RuntimeUiFactory.CreateText(
            "CharacterDetailTitle",
            detailCard,
            "DETAIL / PARTY SLOTS",
            25,
            new Vector2(0.05f, 0.82f),
            new Vector2(0.67f, 0.95f),
            TextAlignmentOptions.Left,
            Gold);

        characterDetailText = RuntimeUiFactory.CreateText(
            "CharacterDetailText",
            detailCard,
            CollectionDetailFormatter.SelectionPrompt,
            25,
            new Vector2(0.05f, 0.29f),
            new Vector2(0.67f, 0.8f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        characterDetailPortraitImage =
            RuntimeUiFactory.CreateSpriteImage(
                "CharacterDetailPortrait",
                detailCard,
                null,
                new Vector2(0.7f, 0.24f),
                new Vector2(0.95f, 0.52f));

        RuntimeUiFactory.CreateButton(
            "PromoteButton",
            detailCard,
            "PROMOTE",
            new Vector2(0.7f, 0.55f),
            new Vector2(0.95f, 0.88f),
            Gold,
            () => promoteSelectedCharacter?.Invoke());

        slotButtons = new CompanionSlotButtonsUI(
            detailCard,
            toggleSelectedCharacterSlot,
            PanelLight,
            Accent,
            Gold,
            Success);
    }

    public void Refresh(
        CharacterData selectedCharacter,
        CompanionManager companionManager)
    {
        if (selectedCharacter == null || companionManager == null)
        {
            characterDetailText.text =
                CollectionDetailFormatter.SelectionPrompt;
            SetPortrait(null);
            slotButtons.Refresh(null, companionManager);
            return;
        }

        characterDetailText.text = CollectionDetailFormatter.Format(
            selectedCharacter,
            companionManager);
        SetPortrait(
            selectedCharacter.icon ??
            selectedCharacter.battleSprite);
        slotButtons.Refresh(
            selectedCharacter,
            companionManager);
    }

    private void BuildCharacterButtons(
        CompanionManager companionManager,
        Action<CharacterData> selectCharacter)
    {
        List<CharacterData> characters =
            companionManager?.GetAllCharacters() ??
            new List<CharacterData>();
        for (int index = 0; index < characters.Count; index++)
        {
            CharacterData character = characters[index];
            int column = index % 3;
            int row = index / 3;
            float xMin = 0.04f + column * 0.32f;
            float yMax = 0.83f - row * 0.115f;

            Button characterButton = RuntimeUiFactory.CreateButton(
                "Character_" + character.characterName,
                panel,
                $"[{character.rarity}]\n{character.characterName}",
                new Vector2(xMin, yMax - 0.09f),
                new Vector2(xMin + 0.28f, yMax),
                GetRarityColor(character.rarity),
                () => selectCharacter?.Invoke(character));

            Sprite portrait = character.icon ?? character.battleSprite;
            if (portrait == null)
                continue;

            RuntimeUiFactory.CreateSpriteImage(
                "Portrait",
                characterButton.transform,
                portrait,
                new Vector2(0.04f, 0.12f),
                new Vector2(0.32f, 0.88f));

            Transform label = characterButton.transform.Find("Label");
            if (label == null ||
                !label.TryGetComponent(out RectTransform labelRect))
            {
                continue;
            }

            labelRect.anchorMin = new Vector2(0.34f, 0.08f);
            labelRect.anchorMax = new Vector2(0.96f, 0.9f);
            TMP_Text labelText = label.GetComponent<TMP_Text>();
            if (labelText != null)
                labelText.alignment = TextAlignmentOptions.Left;
        }
    }

    private void SetPortrait(Sprite portrait)
    {
        characterDetailPortraitImage.sprite = portrait;
        characterDetailPortraitImage.color =
            portrait == null ? Color.clear : Color.white;
    }

    private static Color GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "SSR":
                return new Color32(184, 112, 255, 255);
            case "SR":
                return new Color32(77, 137, 235, 255);
            default:
                return PanelLight;
        }
    }
}
