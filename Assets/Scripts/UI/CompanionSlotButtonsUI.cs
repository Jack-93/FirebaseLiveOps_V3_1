using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CompanionSlotButtonsUI
{
    private readonly List<Button> buttons = new List<Button>();
    private readonly Color panelLight;
    private readonly Color accent;
    private readonly Color gold;
    private readonly Color success;

    public CompanionSlotButtonsUI(
        RectTransform parent,
        Action<int> toggleSelectedCharacterSlot,
        Color panelLight,
        Color accent,
        Color gold,
        Color success)
    {
        this.panelLight = panelLight;
        this.accent = accent;
        this.gold = gold;
        this.success = success;

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            int capturedSlot = slot;
            float xMin = 0.05f + slot * 0.32f;
            Button slotButton = RuntimeUiFactory.CreateButton(
                "EquipSlot" + (slot + 1),
                parent,
                "SLOT " + (slot + 1),
                new Vector2(xMin, 0.05f),
                new Vector2(xMin + 0.27f, 0.23f),
                accent,
                () => toggleSelectedCharacterSlot?.Invoke(capturedSlot));
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

            SetButtonLabel(
                button,
                BuildLabel(
                    slot,
                    equipped,
                    selectedCharacter,
                    selectedOwned,
                    selectedInSlot));
            button.interactable =
                selectedCharacter != null &&
                (selectedOwned || selectedInSlot);

            if (button.targetGraphic == null)
                continue;

            button.targetGraphic.color = selectedInSlot
                ? success
                : selectedOwned
                    ? accent
                    : equipped != null
                        ? gold
                        : panelLight;
        }
    }

    private static string BuildLabel(
        int slot,
        CharacterData equipped,
        CharacterData selectedCharacter,
        bool selectedOwned,
        bool selectedInSlot)
    {
        string slotLabel =
            $"{LocalizationManager.Text("SLOT", "SLOT")} {slot + 1}\n";

        if (selectedInSlot)
            return slotLabel + LocalizationManager.Text("REMOVE", "REMOVE");

        if (!selectedOwned)
        {
            return equipped == null
                ? slotLabel + LocalizationManager.Text("EMPTY", "EMPTY")
                : slotLabel + equipped.characterName;
        }

        string target = equipped == null
            ? LocalizationManager.Text("EQUIP", "EQUIP")
            : $"{equipped.characterName} > " +
              $"{selectedCharacter.characterName}";
        return slotLabel + target;
    }

    private static void SetButtonLabel(Button button, string value)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = LocalizationManager.Translate(value);
    }
}
