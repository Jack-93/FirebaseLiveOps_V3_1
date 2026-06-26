using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CollectionPanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text characterDetailText;
    private readonly Image characterDetailPortraitImage;
    private readonly CompanionSlotButtonsUI slotButtons;
    private readonly List<Button> companionSlotButtons =
        new List<Button>();

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
            "Select a companion.",
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
            characterDetailText.text = LocalizationManager.Text(
                "Select a companion.",
                "동료를 선택하세요.");
            SetPortrait(null);
            slotButtons.Refresh(null, companionManager);
            return;
        }

        int owned = companionManager.GetOwnedCount(
            selectedCharacter.characterName);
        int stars =
            companionManager.GetStars(selectedCharacter.characterName);
        int bonus =
            CompanionManager.GetAttackBonusPercent(
                selectedCharacter.rarity,
                stars);
        int promotionCost =
            companionManager.GetPromotionCost(
                selectedCharacter.characterName);
        bool equippedSelected = IsCharacterEquipped(
            selectedCharacter,
            companionManager);
        bool canPromote =
            owned > 0 &&
            stars < 5 &&
            owned - 1 >= promotionCost &&
            promotionCost > 0;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(
            $"[{selectedCharacter.rarity}] " +
            $"{selectedCharacter.characterName}");
        builder.AppendLine(
            owned > 0
                ? $"{LocalizationManager.Text("Owned", "보유")} x{owned}  |  " +
                  (equippedSelected
                      ? LocalizationManager.Text("EQUIPPED", "장착 중")
                      : LocalizationManager.Text("READY", "준비됨"))
                : LocalizationManager.Text(
                    "LOCKED - recruit from Gacha",
                    "잠김 - 뽑기에서 획득하세요"));
        builder.AppendLine(
            $"{LocalizationManager.Text("Stars", "별")} {stars}/5  |  " +
            $"{LocalizationManager.Text("Attack", "공격력")} +{bonus}%");
        builder.AppendLine(
            $"{selectedCharacter.element} / {selectedCharacter.role}");
        if (owned > 0 && stars < 5)
        {
            builder.AppendLine(
                canPromote
                    ? LocalizationManager.Text(
                        "Promotion ready.",
                        "승급 가능.")
                    : $"{LocalizationManager.Text("Promotion needs", "승급 필요")} " +
                      $"{promotionCost} " +
                      $"{LocalizationManager.Text("duplicate(s)", "중복 캐릭터")}");
        }
        builder.AppendLine(selectedCharacter.description);
        builder.Append(
            LocalizationManager.Text("Party", "파티") + ": ");

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            if (slot > 0)
                builder.Append("  |  ");

            CharacterData equipped =
                companionManager.GetEquippedAtSlot(slot);
            string equippedName = equipped == null
                ? LocalizationManager.Text("Empty", "비어 있음")
                : equipped.characterName;
            builder.Append($"{slot + 1}. {equippedName}");
        }

        characterDetailText.text = builder.ToString();
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

    private void RefreshCompanionSlotButtons(
        CharacterData selectedCharacter,
        CompanionManager companionManager)
    {
        for (int slot = 0;
             slot < companionSlotButtons.Count;
             slot++)
        {
            Button button = companionSlotButtons[slot];
            CharacterData equipped =
                companionManager?.GetEquippedAtSlot(slot);
            bool selectedOwned =
                selectedCharacter != null &&
                companionManager != null &&
                companionManager.GetOwnedCount(
                    selectedCharacter.characterName) > 0;
            bool selectedInSlot =
                selectedCharacter != null &&
                equipped != null &&
                equipped.characterName ==
                selectedCharacter.characterName;

            string label = equipped == null
                ? $"{LocalizationManager.Text("SLOT", "슬롯")} {slot + 1}\n" +
                  LocalizationManager.Text("EMPTY", "비어 있음")
                : $"{LocalizationManager.Text("SLOT", "슬롯")} {slot + 1}\n" +
                  equipped.characterName;

            if (selectedInSlot)
            {
                label =
                    $"{LocalizationManager.Text("SLOT", "슬롯")} {slot + 1}\n" +
                    LocalizationManager.Text("REMOVE", "해제");
            }
            else if (selectedOwned)
            {
                string target = equipped == null
                    ? LocalizationManager.Text("EQUIP", "장착")
                    : $"{equipped.characterName} > " +
                      $"{selectedCharacter.characterName}";
                label =
                    $"{LocalizationManager.Text("SLOT", "슬롯")} {slot + 1}\n" +
                    target;
            }

            SetButtonLabel(button, label);
            button.interactable =
                selectedCharacter != null &&
                (selectedOwned || selectedInSlot);

            if (button.targetGraphic == null)
                continue;

            button.targetGraphic.color = selectedInSlot
                ? Success
                : selectedOwned
                    ? Accent
                    : equipped != null
                        ? Gold
                        : PanelLight;
        }
    }

    private void SetPortrait(Sprite portrait)
    {
        characterDetailPortraitImage.sprite = portrait;
        characterDetailPortraitImage.color =
            portrait == null ? Color.clear : Color.white;
    }

    private static bool IsCharacterEquipped(
        CharacterData character,
        CompanionManager companionManager)
    {
        if (character == null || companionManager == null)
            return false;

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            CharacterData equipped =
                companionManager.GetEquippedAtSlot(slot);
            if (equipped == null)
                continue;

            if (equipped == character ||
                equipped.characterName == character.characterName)
            {
                return true;
            }
        }

        return false;
    }

    private static void SetButtonLabel(Button button, string value)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = LocalizationManager.Translate(value);
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
