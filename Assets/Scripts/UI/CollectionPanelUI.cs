using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CollectionPanelUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";

    private RectTransform panel;
    private RectTransform detailInfoRoot;
    private TMP_Text selectionPromptText;
    private TMP_Text characterTitleText;
    private TMP_Text ownershipLabelText;
    private TMP_Text ownershipStateText;
    private TMP_Text starsLabelText;
    private TMP_Text attackLabelText;
    private TMP_Text attackPercentText;
    private TMP_Text elementRoleText;
    private TMP_Text promotionText;
    private TMP_Text promotionSuffixText;
    private TMP_Text descriptionText;
    private TMP_Text partyText;
    private SpriteNumberText ownedNumberText;
    private SpriteNumberText starsNumberText;
    private SpriteNumberText maxStarsNumberText;
    private SpriteNumberText attackBonusNumberText;
    private SpriteNumberText promotionCostNumberText;
    private Image characterDetailPortraitImage;
    private CompanionSlotButtonsUI slotButtons;

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
        Action<int> toggleSelectedCharacterSlot,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "CollectionPanel",
                root,
                out panel))
        {
            Bind(
                companionManager,
                showMore,
                selectCharacter,
                promoteSelectedCharacter,
                toggleSelectedCharacterSlot);
            return;
        }

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

        selectionPromptText = RuntimeUiFactory.CreateText(
            "CharacterSelectionPromptText",
            detailCard,
            CollectionDetailFormatter.SelectionPrompt,
            25,
            new Vector2(0.05f, 0.29f),
            new Vector2(0.67f, 0.8f),
            TextAlignmentOptions.TopLeft,
            Color.white);
        detailInfoRoot = RuntimeUiFactory.CreatePanel(
            "CharacterDetailInfo",
            detailCard,
            new Color32(0, 0, 0, 0),
            new Vector2(0.05f, 0.24f),
            new Vector2(0.67f, 0.8f));
        detailInfoRoot.GetComponent<UnityEngine.UI.Image>().raycastTarget =
            false;

        characterTitleText = RuntimeUiFactory.CreateText(
            "CharacterTitleText",
            detailInfoRoot,
            "",
            24,
            new Vector2(0f, 0.82f),
            new Vector2(1f, 1f),
            TextAlignmentOptions.Left,
            Color.white);
        ownershipLabelText = RuntimeUiFactory.CreateText(
            "OwnershipLabelText",
            detailInfoRoot,
            "Owned x",
            18,
            new Vector2(0f, 0.62f),
            new Vector2(0.18f, 0.8f),
            TextAlignmentOptions.Left,
            MutedText);
        ownedNumberText = new SpriteNumberText(
            detailInfoRoot,
            "OwnedNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.18f, 0.62f),
            new Vector2(0.3f, 0.8f));
        ownershipStateText = RuntimeUiFactory.CreateText(
            "OwnershipStateText",
            detailInfoRoot,
            "",
            18,
            new Vector2(0.32f, 0.62f),
            new Vector2(0.58f, 0.8f),
            TextAlignmentOptions.Left,
            Gold);
        starsLabelText = RuntimeUiFactory.CreateText(
            "StarsLabelText",
            detailInfoRoot,
            "Stars",
            18,
            new Vector2(0f, 0.42f),
            new Vector2(0.16f, 0.6f),
            TextAlignmentOptions.Left,
            MutedText);
        starsNumberText = new SpriteNumberText(
            detailInfoRoot,
            "StarsNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.16f, 0.42f),
            new Vector2(0.24f, 0.6f));
        RuntimeUiFactory.CreateText(
            "StarsSlashText",
            detailInfoRoot,
            "/",
            18,
            new Vector2(0.24f, 0.42f),
            new Vector2(0.28f, 0.6f),
            TextAlignmentOptions.Center,
            Color.white);
        maxStarsNumberText = new SpriteNumberText(
            detailInfoRoot,
            "MaxStarsNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.28f, 0.42f),
            new Vector2(0.36f, 0.6f));
        attackLabelText = RuntimeUiFactory.CreateText(
            "AttackBonusLabelText",
            detailInfoRoot,
            "Attack",
            18,
            new Vector2(0.42f, 0.42f),
            new Vector2(0.58f, 0.6f),
            TextAlignmentOptions.Left,
            MutedText);
        attackBonusNumberText = new SpriteNumberText(
            detailInfoRoot,
            "AttackBonusNumberText",
            NumberResourceRoot,
            18f,
            new Vector2(0.58f, 0.42f),
            new Vector2(0.77f, 0.6f));
        attackPercentText = RuntimeUiFactory.CreateText(
            "AttackBonusPercentText",
            detailInfoRoot,
            "%",
            18,
            new Vector2(0.77f, 0.42f),
            new Vector2(0.83f, 0.6f),
            TextAlignmentOptions.Left,
            Color.white);
        elementRoleText = RuntimeUiFactory.CreateText(
            "ElementRoleText",
            detailInfoRoot,
            "",
            17,
            new Vector2(0f, 0.24f),
            new Vector2(0.45f, 0.4f),
            TextAlignmentOptions.Left,
            Color.white);
        promotionText = RuntimeUiFactory.CreateText(
            "PromotionText",
            detailInfoRoot,
            "",
            17,
            new Vector2(0.46f, 0.24f),
            new Vector2(0.7f, 0.4f),
            TextAlignmentOptions.Left,
            Gold);
        promotionCostNumberText = new SpriteNumberText(
            detailInfoRoot,
            "PromotionCostNumberText",
            NumberResourceRoot,
            17f,
            new Vector2(0.7f, 0.24f),
            new Vector2(0.82f, 0.4f));
        promotionSuffixText = RuntimeUiFactory.CreateText(
            "PromotionSuffixText",
            detailInfoRoot,
            "",
            15,
            new Vector2(0.82f, 0.24f),
            new Vector2(1f, 0.4f),
            TextAlignmentOptions.Left,
            MutedText);
        descriptionText = RuntimeUiFactory.CreateText(
            "CharacterDescriptionText",
            detailInfoRoot,
            "",
            15,
            new Vector2(0f, 0.04f),
            new Vector2(0.72f, 0.22f),
            TextAlignmentOptions.TopLeft,
            Color.white);
        partyText = RuntimeUiFactory.CreateText(
            "PartySummaryText",
            detailInfoRoot,
            "",
            15,
            new Vector2(0.72f, 0.04f),
            new Vector2(1f, 0.22f),
            TextAlignmentOptions.TopLeft,
            MutedText);
        detailInfoRoot.gameObject.SetActive(false);

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
            if (selectionPromptText != null)
            {
                selectionPromptText.text =
                    CollectionDetailFormatter.SelectionPrompt;
                selectionPromptText.gameObject.SetActive(true);
            }
            if (detailInfoRoot != null)
                detailInfoRoot.gameObject.SetActive(false);
            SetPortrait(null);
            slotButtons?.Refresh(null, companionManager);
            return;
        }

        if (selectionPromptText != null)
            selectionPromptText.gameObject.SetActive(false);
        if (detailInfoRoot != null)
            detailInfoRoot.gameObject.SetActive(true);
        RefreshDetailInfo(selectedCharacter, companionManager);
        SetPortrait(
            selectedCharacter.icon ??
            selectedCharacter.battleSprite);
        slotButtons?.Refresh(
            selectedCharacter,
            companionManager);
    }

    private void Bind(
        CompanionManager companionManager,
        Action showMore,
        Action<CharacterData> selectCharacter,
        Action promoteSelectedCharacter,
        Action<int> toggleSelectedCharacterSlot)
    {
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(panel, "CollectionBackButton"),
            () => showMore?.Invoke());
        BindCharacterButtons(companionManager, selectCharacter);

        selectionPromptText =
            RuntimeUiBinder.FindText(
                panel,
                "CharacterSelectionPromptText");
        detailInfoRoot =
            RuntimeUiBinder.FindRect(panel, "CharacterDetailInfo");
        characterTitleText =
            RuntimeUiBinder.FindText(panel, "CharacterTitleText");
        ownershipLabelText =
            RuntimeUiBinder.FindText(panel, "OwnershipLabelText");
        ownershipStateText =
            RuntimeUiBinder.FindText(panel, "OwnershipStateText");
        starsLabelText =
            RuntimeUiBinder.FindText(panel, "StarsLabelText");
        attackLabelText =
            RuntimeUiBinder.FindText(panel, "AttackBonusLabelText");
        attackPercentText =
            RuntimeUiBinder.FindText(panel, "AttackBonusPercentText");
        elementRoleText =
            RuntimeUiBinder.FindText(panel, "ElementRoleText");
        promotionText =
            RuntimeUiBinder.FindText(panel, "PromotionText");
        promotionSuffixText =
            RuntimeUiBinder.FindText(panel, "PromotionSuffixText");
        descriptionText =
            RuntimeUiBinder.FindText(panel, "CharacterDescriptionText");
        partyText =
            RuntimeUiBinder.FindText(panel, "PartySummaryText");
        ownedNumberText = BindNumber("OwnedNumberText", 18f);
        starsNumberText = BindNumber("StarsNumberText", 18f);
        maxStarsNumberText = BindNumber("MaxStarsNumberText", 18f);
        attackBonusNumberText = BindNumber(
            "AttackBonusNumberText",
            18f);
        promotionCostNumberText = BindNumber(
            "PromotionCostNumberText",
            17f);
        characterDetailPortraitImage =
            RuntimeUiBinder.FindImage(panel, "CharacterDetailPortrait");

        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(panel, "PromoteButton"),
            () => promoteSelectedCharacter?.Invoke());
        slotButtons = new CompanionSlotButtonsUI(
            RuntimeUiBinder.FindRect(panel, "CharacterDetailCard"),
            toggleSelectedCharacterSlot,
            PanelLight,
            Accent,
            Gold,
            Success,
            true);
        if (detailInfoRoot != null)
            detailInfoRoot.gameObject.SetActive(false);
    }

    private SpriteNumberText BindNumber(string name, float height)
    {
        return RuntimeUiBinder.BindNumber(
            panel,
            name,
            NumberResourceRoot,
            height);
    }

    private void RefreshDetailInfo(
        CharacterData selectedCharacter,
        CompanionManager companionManager)
    {
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

        SetText(
            characterTitleText,
            $"[{selectedCharacter.rarity}] {selectedCharacter.characterName}");
        SetText(
            ownershipLabelText,
            LocalizationManager.Translate("Owned") + " x");
        ownedNumberText?.SetText(CompactNumberFormatter.Format(owned));
        SetText(
            ownershipStateText,
            owned <= 0
                ? LocalizationManager.Translate("LOCKED - recruit from Gacha")
                : equippedSelected
                    ? LocalizationManager.Translate("EQUIPPED")
                    : LocalizationManager.Translate("READY"));

        SetText(starsLabelText, LocalizationManager.Translate("Stars"));
        starsNumberText?.SetText(CompactNumberFormatter.Format(stars));
        maxStarsNumberText?.SetText("5");
        SetText(attackLabelText, LocalizationManager.Translate("Attack"));
        attackBonusNumberText?.SetText(
            CompactNumberFormatter.Format(bonus, "+"));
        SetText(attackPercentText, "%");

        SetText(
            elementRoleText,
            $"{selectedCharacter.element} / {selectedCharacter.role}");
        RefreshPromotionInfo(owned, stars, promotionCost);
        SetText(descriptionText, selectedCharacter.description);
        SetText(partyText, BuildPartyText(companionManager));
    }

    private void RefreshPromotionInfo(
        int owned,
        int stars,
        int promotionCost)
    {
        bool showPromotion = owned > 0 && stars < 5;
        SetTextActive(promotionText, showPromotion);
        promotionCostNumberText?.SetActive(showPromotion);
        SetTextActive(promotionSuffixText, showPromotion);
        if (!showPromotion)
            return;

        bool canPromote = owned - 1 >= promotionCost &&
            promotionCost > 0;
        SetText(
            promotionText,
            canPromote
                ? LocalizationManager.Translate("Promotion ready.")
                : LocalizationManager.Translate("Promotion needs"));
        promotionCostNumberText?.SetActive(!canPromote);
        SetTextActive(promotionSuffixText, !canPromote);
        if (canPromote)
            return;

        promotionCostNumberText?.SetText(
            CompactNumberFormatter.Format(promotionCost));
        SetText(
            promotionSuffixText,
            LocalizationManager.Translate("duplicate(s)"));
    }

    private static string BuildPartyText(CompanionManager companionManager)
    {
        List<string> slots = new List<string>();
        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            CharacterData equipped =
                companionManager.GetEquippedAtSlot(slot);
            string equippedName = equipped == null
                ? LocalizationManager.Translate("EMPTY")
                : equipped.characterName;
            slots.Add($"{(char)('A' + slot)}. {equippedName}");
        }

        return LocalizationManager.Translate("Party") + ": " +
            string.Join("  |  ", slots);
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

    private void BindCharacterButtons(
        CompanionManager companionManager,
        Action<CharacterData> selectCharacter)
    {
        List<CharacterData> characters =
            companionManager?.GetAllCharacters() ??
            new List<CharacterData>();
        for (int index = 0; index < characters.Count; index++)
        {
            CharacterData character = characters[index];
            Button characterButton = RuntimeUiBinder.FindButton(
                panel,
                "Character_" + character.characterName);
            RuntimeUiBinder.ReplaceButtonAction(
                characterButton,
                () => selectCharacter?.Invoke(character));

            Image portraitImage = characterButton == null
                ? null
                : RuntimeUiBinder.FindImage(characterButton.transform, "Portrait");
            if (portraitImage == null)
                continue;

            Sprite portrait = character.icon ?? character.battleSprite;
            portraitImage.sprite = portrait;
            portraitImage.color =
                portrait == null ? Color.clear : Color.white;
        }
    }

    private void SetPortrait(Sprite portrait)
    {
        if (characterDetailPortraitImage == null)
            return;

        characterDetailPortraitImage.sprite = portrait;
        characterDetailPortraitImage.color =
            portrait == null ? Color.clear : Color.white;
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private static void SetTextActive(TMP_Text text, bool active)
    {
        if (text != null)
            text.gameObject.SetActive(active);
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
