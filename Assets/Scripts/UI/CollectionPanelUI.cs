using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CollectionPanelUI
{
    private const string NumberResourceRoot =
        "PrototypeArt/Numbers/DamageGold";
    private const int CharactersPerPage = 12;
    private const int CharacterColumns = 4;

    private RectTransform panel;
    private RectTransform slotBar;
    private RectTransform detailInfoRoot;
    private TMP_Text selectionPromptText;
    private TMP_Text pageText;
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
    private Button previousPageButton;
    private Button nextPageButton;
    private readonly Button[] characterButtons =
        new Button[CharactersPerPage];
    private readonly Image[] characterPortraitImages =
        new Image[CharactersPerPage];
    private readonly Image[] characterLockImages =
        new Image[CharactersPerPage];
    private readonly TMP_Text[] characterButtonLabels =
        new TMP_Text[CharactersPerPage];
    private CompanionSlotButtonsUI slotButtons;
    private Action<CharacterData> selectCharacterAction;
    private int characterPage;

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
            "+ 슬롯을 누른 뒤 장착할 동료를 선택하세요.",
            24,
            new Vector2(0.24f, 0.86f),
            new Vector2(0.96f, 0.9f),
            TextAlignmentOptions.Center,
            MutedText);

        slotBar = CreateSlotBar();
        slotButtons = new CompanionSlotButtonsUI(
            slotBar,
            toggleSelectedCharacterSlot,
            PanelLight,
            Accent,
            Gold,
            Success);

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
            "상세 정보",
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

    }

    public void Refresh(
        CharacterData selectedCharacter,
        CompanionManager companionManager)
    {
        RefreshCharacterButtons(companionManager);

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
        SetPortrait(selectedCharacter.ResolvePortraitSprite());
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
        TMP_Text subtitleText =
            RuntimeUiBinder.FindText(panel, "CollectionSubtitle");
        SetText(
            subtitleText,
            "+ 슬롯을 누른 뒤 장착할 동료를 선택하세요.");
        BindCharacterButtons(companionManager, selectCharacter);
        HideLegacyDetailSlotButtons();

        selectionPromptText =
            RuntimeUiBinder.FindText(
                panel,
                "CharacterSelectionPromptText");
        detailInfoRoot =
            RuntimeUiBinder.FindRect(panel, "CharacterDetailInfo");
        characterTitleText =
            RuntimeUiBinder.FindText(panel, "CharacterTitleText");
        SetText(
            RuntimeUiBinder.FindText(panel, "CharacterDetailTitle"),
            "상세 정보");
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
        slotBar = RuntimeUiBinder.FindRect(panel, "CompanionSlotBar") ??
            CreateSlotBar();
        slotButtons = new CompanionSlotButtonsUI(
            slotBar,
            toggleSelectedCharacterSlot,
            PanelLight,
            Accent,
            Gold,
            Success);
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
        selectCharacterAction = selectCharacter;

        for (int index = 0; index < CharactersPerPage; index++)
        {
            Vector2 anchorMin;
            Vector2 anchorMax;
            GetSlotAnchors(index, out anchorMin, out anchorMax);

            Button characterButton = RuntimeUiFactory.CreateButton(
                "CharacterSlot_" + (index + 1),
                panel,
                "",
                anchorMin,
                anchorMax,
                PanelLight,
                () => { });
            characterButtons[index] = characterButton;

            characterPortraitImages[index] = RuntimeUiFactory.CreateSpriteImage(
                "Portrait",
                characterButton.transform,
                null,
                new Vector2(0.12f, 0.28f),
                new Vector2(0.88f, 0.9f));
            characterLockImages[index] = CreateLockImage(
                characterButton.transform);

            TMP_Text labelText = characterButton.GetComponentInChildren<TMP_Text>(
                true);
            characterButtonLabels[index] = labelText;
            ConfigureCharacterLabel(labelText);
        }

        BuildPageControls();
        RefreshCharacterButtons(companionManager);
    }

    private void BindCharacterButtons(
        CompanionManager companionManager,
        Action<CharacterData> selectCharacter)
    {
        selectCharacterAction = selectCharacter;
        List<Button> existingButtons = GetExistingCharacterButtons();

        for (int index = 0; index < CharactersPerPage; index++)
        {
            Button characterButton = index < existingButtons.Count
                ? existingButtons[index]
                : CreateRuntimeCharacterSlot(index);
            characterButtons[index] = characterButton;
            if (characterButton == null)
                continue;

            characterButton.name = "CharacterSlot_" + (index + 1);
            RectTransform rect =
                characterButton.GetComponent<RectTransform>();
            Vector2 anchorMin;
            Vector2 anchorMax;
            GetSlotAnchors(index, out anchorMin, out anchorMax);
            ApplyAnchors(rect, anchorMin, anchorMax);

            characterPortraitImages[index] =
                RuntimeUiBinder.FindImage(
                    characterButton.transform,
                    "Portrait") ??
                RuntimeUiFactory.CreateSpriteImage(
                    "Portrait",
                    characterButton.transform,
                    null,
                    new Vector2(0.12f, 0.28f),
                    new Vector2(0.88f, 0.9f));
            ApplyAnchors(
                characterPortraitImages[index]
                    ?.GetComponent<RectTransform>(),
                new Vector2(0.12f, 0.28f),
                new Vector2(0.88f, 0.9f));
            characterLockImages[index] =
                RuntimeUiBinder.FindImage(
                    characterButton.transform,
                    "LockIcon") ??
                CreateLockImage(characterButton.transform);

            TMP_Text labelText = RuntimeUiBinder.FindText(
                characterButton.transform,
                "Label") ??
                characterButton.GetComponentInChildren<TMP_Text>(true);
            characterButtonLabels[index] = labelText;
            ConfigureCharacterLabel(labelText);
        }

        BuildPageControls();
        RefreshCharacterButtons(companionManager);
    }

    private void RefreshCharacterButtons(CompanionManager companionManager)
    {
        List<CharacterData> characters =
            companionManager?.GetAllCharacters() ??
            new List<CharacterData>();
        int maxPage = characters.Count == 0
            ? 0
            : (characters.Count - 1) / CharactersPerPage;
        characterPage = Mathf.Clamp(characterPage, 0, maxPage);

        for (int index = 0; index < CharactersPerPage; index++)
        {
            Button button = characterButtons[index];
            if (button == null)
                continue;

            int characterIndex =
                characterPage * CharactersPerPage + index;
            bool hasCharacter = characterIndex < characters.Count;
            button.gameObject.SetActive(hasCharacter);
            if (!hasCharacter)
                continue;

            CharacterData character = characters[characterIndex];
            bool owned = companionManager != null &&
                companionManager.GetOwnedCount(
                    character.characterName) > 0;
            CharacterData capturedCharacter = character;
            RuntimeUiBinder.ReplaceButtonAction(
                button,
                owned
                    ? () => selectCharacterAction?.Invoke(capturedCharacter)
                    : (UnityEngine.Events.UnityAction)null);
            button.interactable = owned;

            TMP_Text label = characterButtonLabels[index];
            SetText(
                label,
                owned
                    ? character.characterName
                    : "?");

            Sprite portrait = owned
                ? character.ResolveGachaSprite()
                : null;
            Image portraitImage = characterPortraitImages[index];
            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.color =
                    portrait == null ? Color.clear : Color.white;
            }
            Image lockImage = characterLockImages[index];
            if (lockImage != null)
            {
                lockImage.sprite = PrototypeUiArt.LockIcon;
                lockImage.color = owned ? Color.clear : Color.white;
                lockImage.gameObject.SetActive(!owned);
            }

            ApplyCharacterButtonColor(
                button,
                character.rarity,
                owned);
        }

        RefreshPageControls(maxPage);
    }

    private List<Button> GetExistingCharacterButtons()
    {
        List<Button> buttons = new List<Button>();
        if (panel == null)
            return buttons;

        foreach (Button button in panel.GetComponentsInChildren<Button>(true))
        {
            if (button == null ||
                !button.name.StartsWith(
                    "Character_",
                    StringComparison.Ordinal))
            {
                continue;
            }

            buttons.Add(button);
        }

        buttons.Sort(CompareCharacterButtonPosition);
        return buttons;
    }

    private static int CompareCharacterButtonPosition(
        Button left,
        Button right)
    {
        RectTransform leftRect = left.GetComponent<RectTransform>();
        RectTransform rightRect = right.GetComponent<RectTransform>();
        if (leftRect == null || rightRect == null)
            return 0;

        int rowComparison =
            rightRect.anchorMax.y.CompareTo(leftRect.anchorMax.y);
        if (rowComparison != 0)
            return rowComparison;

        return leftRect.anchorMin.x.CompareTo(rightRect.anchorMin.x);
    }

    private Button CreateRuntimeCharacterSlot(int index)
    {
        Vector2 anchorMin;
        Vector2 anchorMax;
        GetSlotAnchors(index, out anchorMin, out anchorMax);
        return RuntimeUiFactory.CreateButton(
            "CharacterSlot_" + (index + 1),
            panel,
            "",
            anchorMin,
            anchorMax,
            PanelLight,
            () => { });
    }

    private void BuildPageControls()
    {
        previousPageButton =
            RuntimeUiBinder.FindButton(
                panel,
                "CollectionPreviousPageButton") ??
            RuntimeUiFactory.CreateButton(
                "CollectionPreviousPageButton",
                panel,
                "<",
                new Vector2(0.005f, 0.38f),
                new Vector2(0.08f, 0.75f),
                PanelLight,
                () => ChangeCharacterPage(-1));
        RuntimeUiBinder.ReplaceButtonAction(
            previousPageButton,
            () => ChangeCharacterPage(-1));

        nextPageButton =
            RuntimeUiBinder.FindButton(
                panel,
                "CollectionNextPageButton") ??
            RuntimeUiFactory.CreateButton(
                "CollectionNextPageButton",
                panel,
                ">",
                new Vector2(0.92f, 0.38f),
                new Vector2(0.995f, 0.75f),
                PanelLight,
                () => ChangeCharacterPage(1));
        RuntimeUiBinder.ReplaceButtonAction(
            nextPageButton,
            () => ChangeCharacterPage(1));

        pageText =
            RuntimeUiBinder.FindText(panel, "CollectionPageText") ??
            RuntimeUiFactory.CreateText(
                "CollectionPageText",
                panel,
                "",
                20,
                new Vector2(0.39f, 0.34f),
                new Vector2(0.61f, 0.38f),
                TextAlignmentOptions.Center,
                MutedText);
    }

    private void ChangeCharacterPage(int direction)
    {
        characterPage += direction;
        RefreshCharacterButtons(CompanionManager.Instance);
    }

    private void RefreshPageControls(int maxPage)
    {
        bool showPages = maxPage > 0;
        if (previousPageButton != null)
        {
            previousPageButton.gameObject.SetActive(showPages);
            previousPageButton.interactable = characterPage > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.gameObject.SetActive(showPages);
            nextPageButton.interactable = characterPage < maxPage;
        }

        if (pageText != null)
        {
            pageText.gameObject.SetActive(showPages);
            pageText.text = (characterPage + 1) + " / " + (maxPage + 1);
        }
    }

    private static void GetSlotAnchors(
        int index,
        out Vector2 anchorMin,
        out Vector2 anchorMax)
    {
        int column = index % CharacterColumns;
        int row = index / CharacterColumns;
        float width = 0.18f;
        float gap = 0.025f;
        float xMin = 0.11f + column * (width + gap);
        float yMax = 0.715f - row * 0.125f;
        anchorMin = new Vector2(xMin, yMax - 0.11f);
        anchorMax = new Vector2(xMin + width, yMax);
    }

    private static void ApplyAnchors(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        if (rect == null)
            return;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void ConfigureCharacterLabel(TMP_Text labelText)
    {
        if (labelText == null)
            return;

        labelText.alignment = TextAlignmentOptions.Center;
        labelText.fontSizeMax = 19f;
        RectTransform labelRect =
            labelText.GetComponent<RectTransform>();
        ApplyAnchors(
            labelRect,
            new Vector2(0.06f, 0.02f),
            new Vector2(0.94f, 0.24f));
    }

    private RectTransform CreateSlotBar()
    {
        RectTransform bar = RuntimeUiFactory.CreatePanel(
            "CompanionSlotBar",
            panel,
            new Color32(0, 0, 0, 0),
            new Vector2(0.04f, 0.735f),
            new Vector2(0.96f, 0.855f));
        Image image = bar.GetComponent<Image>();
        if (image != null)
            image.raycastTarget = false;
        return bar;
    }

    private Image CreateLockImage(Transform parent)
    {
        return RuntimeUiFactory.CreateSpriteImage(
            "LockIcon",
            parent,
            PrototypeUiArt.LockIcon,
            new Vector2(0.32f, 0.38f),
            new Vector2(0.68f, 0.76f));
    }

    private void HideLegacyDetailSlotButtons()
    {
        RectTransform detailCard =
            RuntimeUiBinder.FindRect(panel, "CharacterDetailCard");
        if (detailCard == null)
            return;

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            Button button = RuntimeUiBinder.FindButton(
                detailCard,
                "EquipSlot" + (slot + 1));
            if (button != null)
                button.gameObject.SetActive(false);
        }
    }

    private static void ApplyCharacterButtonColor(
        Button button,
        string rarity,
        bool owned)
    {
        Color color = GetRarityColor(rarity);
        if (!owned)
            color = Color.Lerp(color, new Color32(30, 30, 30, 255), 0.58f);

        Image art = RuntimeUiBinder.FindImage(
            button.transform,
            "ButtonArt") ??
            (button.targetGraphic as Image);
        if (art != null)
            art.color = color;
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
