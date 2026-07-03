using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CompanionSlotButtonsUI
{
    private readonly List<Button> buttons = new List<Button>();
    private readonly List<Image> portraitImages = new List<Image>();
    private readonly List<Image> plusImages = new List<Image>();
    private readonly Color panelLight;
    private readonly Color success;

    public CompanionSlotButtonsUI(
        RectTransform parent,
        Action<int> toggleSelectedCharacterSlot,
        Color panelLight,
        Color accent,
        Color gold,
        Color success,
        bool bindExisting = false)
    {
        this.panelLight = panelLight;
        this.success = success;

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            int capturedSlot = slot;
            float xMin = 0.05f + slot * 0.32f;
            Button slotButton;
            if (bindExisting)
            {
                slotButton = RuntimeUiBinder.FindButton(
                    parent,
                    "EquipSlot" + (slot + 1));
                RuntimeUiBinder.ReplaceButtonAction(
                    slotButton,
                    () => toggleSelectedCharacterSlot?.Invoke(capturedSlot));
            }
            else
            {
                slotButton = RuntimeUiFactory.CreateButton(
                    "EquipSlot" + (slot + 1),
                    parent,
                    "+",
                    new Vector2(xMin, 0.08f),
                    new Vector2(xMin + 0.27f, 0.92f),
                    accent,
                    () => toggleSelectedCharacterSlot?.Invoke(capturedSlot));
            }
            ConfigureSlot(slotButton);
            buttons.Add(slotButton);
        }
    }

    public void Refresh(
        CharacterData selectedCharacter,
        CompanionManager companionManager)
    {
        for (int slot = 0; slot < buttons.Count; slot++)
        {
            Button button = buttons[slot];
            if (button == null)
                continue;

            CharacterData equipped =
                companionManager?.GetEquippedAtSlot(slot);
            bool hasEquipped = equipped != null;

            SetPortrait(slot, equipped);
            SetPlus(slot, !hasEquipped);
            SetButtonLabel(button, hasEquipped ? equipped.characterName : "+");
            button.interactable = true;

            if (button.targetGraphic == null)
                continue;

            button.targetGraphic.color = hasEquipped
                ? success
                : panelLight;
        }
    }

    private void ConfigureSlot(Button button)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.fontSizeMax = 24f;
            label.alignment = TextAlignmentOptions.Center;
            RectTransform labelRect =
                label.GetComponent<RectTransform>();
            if (labelRect != null)
            {
                labelRect.anchorMin = new UnityEngine.Vector2(0.08f, 0.02f);
                labelRect.anchorMax = new UnityEngine.Vector2(0.92f, 0.22f);
                labelRect.offsetMin = UnityEngine.Vector2.zero;
                labelRect.offsetMax = UnityEngine.Vector2.zero;
            }
        }

        Image portrait = RuntimeUiBinder.FindImage(
            button.transform,
            "SlotPortrait") ??
            RuntimeUiFactory.CreateSpriteImage(
                "SlotPortrait",
                button.transform,
                null,
                new UnityEngine.Vector2(0.12f, 0.24f),
                new UnityEngine.Vector2(0.88f, 0.92f));
        Image plus = RuntimeUiBinder.FindImage(
            button.transform,
            "SlotPlusIcon") ??
            RuntimeUiFactory.CreateSpriteImage(
                "SlotPlusIcon",
                button.transform,
                PrototypeUiArt.PlusIcon,
                new UnityEngine.Vector2(0.32f, 0.35f),
                new UnityEngine.Vector2(0.68f, 0.72f));
        portraitImages.Add(portrait);
        plusImages.Add(plus);
    }

    private void SetPortrait(int slot, CharacterData equipped)
    {
        if (slot < 0 || slot >= portraitImages.Count)
            return;

        Image image = portraitImages[slot];
        if (image == null)
            return;

        Sprite sprite = equipped == null
            ? null
            : equipped.ResolvePortraitSprite();
        image.sprite = sprite;
        image.color = sprite == null
            ? UnityEngine.Color.clear
            : UnityEngine.Color.white;
        image.gameObject.SetActive(sprite != null);
    }

    private void SetPlus(int slot, bool active)
    {
        if (slot < 0 || slot >= plusImages.Count)
            return;

        Image image = plusImages[slot];
        if (image == null)
            return;

        image.sprite = PrototypeUiArt.PlusIcon;
        image.color = active && image.sprite != null
            ? UnityEngine.Color.white
            : UnityEngine.Color.clear;
        image.gameObject.SetActive(active && image.sprite != null);
    }

    private void SetButtonLabel(Button button, string value)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = LocalizationManager.Translate(value);
            label.gameObject.SetActive(
                value == "+" || !string.IsNullOrEmpty(value));
        }
    }
}
