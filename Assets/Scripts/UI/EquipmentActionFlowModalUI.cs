using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EquipmentActionKind
{
    Enhancement,
    OptionReset
}

public sealed class EquipmentActionFlowModalUI
{
    private RectTransform overlay;
    private TMP_Text titleText;
    private TMP_Text instructionText;
    private Button weaponButton;
    private Button armorButton;
    private TMP_Text weaponButtonText;
    private TMP_Text armorButtonText;
    private Button confirmButton;
    private Button cancelButton;
    private TMP_Text confirmButtonText;
    private TMP_Text cancelButtonText;
    private readonly Func<EquipmentActionKind, EquipmentSlot, bool> confirmAction;

    private EquipmentActionKind action;
    private EquipmentSlot selectedSlot;
    private bool isConfirming;
    private bool actionCompleted;

    public EquipmentActionFlowModalUI(
        RectTransform root,
        Func<EquipmentActionKind, EquipmentSlot, bool> confirmAction)
    {
        this.confirmAction = confirmAction;
        if (!RuntimeUiBinder.TryInstantiatePrefab(
                "EquipmentActionFlowModal", root, out overlay))
        {
            Debug.LogError("EquipmentActionFlowModal prefab is missing.");
            return;
        }

        titleText = RuntimeUiBinder.FindText(overlay, "EquipmentActionTitleText");
        instructionText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentActionInstructionText");
        weaponButton = RuntimeUiBinder.FindButton(
            overlay,
            "EquipmentActionWeaponButton");
        armorButton = RuntimeUiBinder.FindButton(
            overlay,
            "EquipmentActionArmorButton");
        weaponButtonText = RuntimeUiBinder.FindText(
            weaponButton.transform as RectTransform,
            "Label");
        armorButtonText = RuntimeUiBinder.FindText(
            armorButton.transform as RectTransform,
            "Label");

        confirmButton = RuntimeUiBinder.FindButton(
            overlay,
            "EquipmentActionConfirmButton");
        cancelButton = RuntimeUiBinder.FindButton(
            overlay,
            "EquipmentActionCancelButton");
        confirmButtonText = RuntimeUiBinder.FindText(
            confirmButton.transform as RectTransform,
            "Label");
        cancelButtonText = RuntimeUiBinder.FindText(
            cancelButton.transform as RectTransform,
            "Label");
        RuntimeUiBinder.ReplaceButtonAction(
            weaponButton,
            () => Select(EquipmentSlot.Weapon));
        RuntimeUiBinder.ReplaceButtonAction(
            armorButton,
            () => Select(EquipmentSlot.Armor));
        RuntimeUiBinder.ReplaceButtonAction(confirmButton, Confirm);
        RuntimeUiBinder.ReplaceButtonAction(cancelButton, Cancel);
        overlay.gameObject.SetActive(false);
    }

    public void ShowSelection(EquipmentActionKind nextAction)
    {
        action = nextAction;
        isConfirming = false;
        actionCompleted = false;
        titleText.text = GetActionTitle();
        instructionText.text =
            "\uC9C4\uD589\uD560 \uC7A5\uBE44\uB97C \uB20C\uB7EC\uC8FC\uC138\uC694.";
        RefreshEquipmentButtons();
        SetSelectionVisible(true);
        overlay.gameObject.SetActive(true);
        overlay.SetAsLastSibling();
    }

    private void Select(EquipmentSlot slot)
    {
        if (!CanUse(slot))
            return;

        selectedSlot = slot;
        isConfirming = true;
        titleText.text = GetActionTitle();
        instructionText.text = GetConfirmationText(slot);
        SetSelectionVisible(false);
    }

    private void Confirm()
    {
        if (!isConfirming)
            return;

        if (confirmAction?.Invoke(action, selectedSlot) ?? false)
            actionCompleted = true;
        RefreshConfirmation();
    }

    private void Cancel()
    {
        if (isConfirming)
        {
            ShowSelection(action);
            return;
        }

        overlay.gameObject.SetActive(false);
    }

    private void RefreshEquipmentButtons()
    {
        SetButtonText(
            weaponButtonText,
            GetSelectionLabel(EquipmentSlot.Weapon));
        SetButtonText(
            armorButtonText,
            GetSelectionLabel(EquipmentSlot.Armor));
        weaponButton.interactable = CanUse(EquipmentSlot.Weapon);
        armorButton.interactable = CanUse(EquipmentSlot.Armor);
    }

    private void SetSelectionVisible(bool visible)
    {
        weaponButton.gameObject.SetActive(visible);
        armorButton.gameObject.SetActive(visible);
        confirmButton.gameObject.SetActive(!visible);
        cancelButton.gameObject.SetActive(true);
        SetButtonText(
            cancelButtonText,
            visible ? "\uB2EB\uAE30" : "\uC774\uC804");
        if (!visible)
            RefreshConfirmation();
    }

    public void RefreshCurrentSelection()
    {
        if (!isConfirming)
            return;

        RefreshConfirmation();
    }

    private void RefreshConfirmation()
    {
        instructionText.text = GetConfirmationText(selectedSlot);
        bool canContinue = CanUse(selectedSlot);
        confirmButton.interactable = canContinue;
        if (!canContinue)
        {
            SetButtonText(
                confirmButtonText,
                action == EquipmentActionKind.Enhancement
                    ? "\uCD5C\uB300 \uAC15\uD654"
                    : "\uC7AC\uC124\uC815 \uBD88\uAC00");
            return;
        }

        SetButtonText(
            confirmButtonText,
            action == EquipmentActionKind.Enhancement
                ? (actionCompleted
                    ? "\uACC4\uC18D \uAC15\uD654"
                    : "\uAC15\uD654\uD558\uAE30")
                : (actionCompleted
                    ? "\uACC4\uC18D \uC7AC\uC124\uC815"
                    : "\uC7AC\uC124\uC815\uD558\uAE30"));
    }

    private bool CanUse(EquipmentSlot slot)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || string.IsNullOrEmpty(GetEquipmentId(data, slot)))
            return false;

        if (action == EquipmentActionKind.Enhancement)
            return GetEnhancementLevel(data, slot) <
                GameBalanceConfig.EquipmentStarForceMaxLevel;

        return EquipmentManager.GetOptionResetCoinCost(data, slot) >= 0;
    }

    private string GetSelectionLabel(EquipmentSlot slot)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return "-";

        string equipmentId = GetEquipmentId(data, slot);
        string name = string.IsNullOrEmpty(equipmentId)
            ? "\uC7A5\uBE44 \uC5C6\uC74C"
            : EquipmentManager.GetEquipmentDisplayName(equipmentId);
        if (action == EquipmentActionKind.Enhancement)
        {
            int level = GetEnhancementLevel(data, slot);
            if (level >= GameBalanceConfig.EquipmentStarForceMaxLevel)
                return name + "\nMAX";

            return name + "\n" + level + "\uC131 -> " +
                (level + 1) + "\uC131 / \uC131\uACF5\uB960 " +
                EquipmentManager.GetStarForceSuccessPercent(level)
                    .ToString("0") + "%";
        }

        int coinCost = EquipmentManager.GetOptionResetCoinCost(data, slot);
        if (coinCost < 0)
            return name + "\n\uC7AC\uC124\uC815\uD560 \uC635\uC158 \uC5C6\uC74C";

        return name + "\n" +
            GetOptionText(data, slot) +
            "\n\uBE44\uD589\uB2E8 \uC7A5\uBE44 \uCF54\uC778 x" + coinCost;
    }

    private string GetConfirmationText(EquipmentSlot slot)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        string name = EquipmentManager.GetEquipmentDisplayName(
            GetEquipmentId(data, slot));
        if (action == EquipmentActionKind.Enhancement)
        {
            int level = GetEnhancementLevel(data, slot);
            return name + "\n" + level + "\uC131 -> " +
                (level + 1) + "\uC131\n\uC131\uACF5\uB960 " +
                EquipmentManager.GetStarForceSuccessPercent(level)
                    .ToString("0") + "% / \uACE8\uB4DC " +
                EquipmentManager.GetStarForceCost(level) + "\n\n" +
                "\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654\uD558\uACA0\uC2B5\uB2C8\uAE4C?";
        }

        return name + "\n" +
            GetOptionText(data, slot) +
            "\n\uBE44\uD589\uB2E8 \uC7A5\uBE44 \uCF54\uC778 x" +
            EquipmentManager.GetOptionResetCoinCost(data, slot) + "\n\n" +
            "\uC635\uC158\uC744 \uC7AC\uC124\uC815\uD558\uACA0\uC2B5\uB2C8\uAE4C?";
    }

    private string GetActionTitle()
    {
        return action == EquipmentActionKind.Enhancement
            ? "\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654"
            : "\uC635\uC158 \uC7AC\uC124\uC815";
    }

    private static string GetEquipmentId(
        PlayerData data,
        EquipmentSlot slot)
    {
        if (data == null)
            return "";

        return slot == EquipmentSlot.Weapon
            ? data.equippedWeapon
            : data.equippedArmor;
    }

    private static int GetEnhancementLevel(
        PlayerData data,
        EquipmentSlot slot)
    {
        return EquipmentManager.GetEnhancementLevel(data, slot);
    }

    private static string GetOptionText(
        PlayerData data,
        EquipmentSlot slot)
    {
        string options = EquipmentManager.GetEquipmentOptionSummary(
            data,
            slot);
        return string.IsNullOrWhiteSpace(options)
            ? "\uC635\uC158 \uC5C6\uC74C"
            : options;
    }

    private static void SetButtonText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}
