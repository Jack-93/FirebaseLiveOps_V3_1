using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EquipmentItemActionModalUI
{
    private RectTransform overlay;
    private TMP_Text titleText;
    private Image icon;
    private TMP_Text typeText;
    private TMP_Text nameText;
    private TMP_Text starsText;
    private TMP_Text optionsText;
    private TMP_Text coinText;
    private TMP_Text instructionText;
    private Button equipButton;
    private Button dismantleButton;
    private TMP_Text equipButtonText;
    private TMP_Text dismantleButtonText;
    private readonly Func<string, bool> equipAction;
    private readonly Func<string, bool> dismantleAction;
    private readonly Action inventoryChanged;

    private EquipmentInstance selectedInstance;
    private bool isDismantleConfirm;

    public EquipmentItemActionModalUI(
        RectTransform root,
        Func<string, bool> equipAction,
        Func<string, bool> dismantleAction,
        Action inventoryChanged)
    {
        this.equipAction = equipAction;
        this.dismantleAction = dismantleAction;
        this.inventoryChanged = inventoryChanged;
        if (!RuntimeUiBinder.TryInstantiatePrefab(
                "EquipmentItemActionModal", root, out overlay))
        {
            Debug.LogError("EquipmentItemActionModal prefab is missing.");
            return;
        }

        titleText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentItemActionTitleText");
        icon = RuntimeUiBinder.FindImage(overlay, "EquipmentItemActionIcon");
        typeText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentItemActionTypeText");
        nameText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentItemActionNameText");
        starsText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentItemActionStarsText");
        optionsText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentItemActionOptionsText");
        coinText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentItemActionCoinText");
        instructionText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentItemActionInstructionText");
        equipButton = RuntimeUiBinder.FindButton(
            overlay,
            "EquipmentItemEquipButton");
        dismantleButton = RuntimeUiBinder.FindButton(
            overlay,
            "EquipmentItemDismantleButton");
        equipButtonText = RuntimeUiBinder.FindText(
            equipButton.transform as RectTransform,
            "Label");
        dismantleButtonText = RuntimeUiBinder.FindText(
            dismantleButton.transform as RectTransform,
            "Label");
        RuntimeUiBinder.ReplaceButtonAction(equipButton, Equip);
        RuntimeUiBinder.ReplaceButtonAction(dismantleButton, Dismantle);
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(
                overlay,
                "EquipmentItemActionCloseButton"),
            Hide);
        overlay.gameObject.SetActive(false);
    }

    public void Show(EquipmentInstance instance)
    {
        EquipmentDefinition definition = EquipmentManager.GetEquipmentDefinition(
            instance?.definitionId);
        if (definition == null)
            return;

        selectedInstance = instance;
        isDismantleConfirm = false;
        bool equipped = IsEquipped(instance.instanceId);
        titleText.text = "\uC7A5\uBE44 \uC120\uD0DD";
        icon.sprite = definition.icon;
        icon.color = icon.sprite == null ? Color.clear : Color.white;
        typeText.text = definition.slot == EquipmentSlot.Weapon
            ? "\uBB34\uAE30"
            : "\uBC29\uC5B4\uAD6C";
        nameText.text = definition.DisplayName;
        starsText.text = GetStarText(instance.enhancementLevel);
        optionsText.text = GetOptionsText(instance);
        coinText.text = "\uD574\uCCB4 \uBCF4\uC0C1: \uBE44\uD589\uB2E8 \uC7A5\uBE44 \uCF54\uC778 x" +
            EquipmentManager.GetDismantleCoinReward(definition.tier);
        instructionText.text = equipped
            ? "\uCC29\uC6A9 \uC911\uC778 \uC7A5\uBE44\uC785\uB2C8\uB2E4."
            : "\uC7A5\uCC29\uD558\uAC70\uB098 \uD574\uCCB4\uD560 \uC218 \uC788\uC2B5\uB2C8\uB2E4.";
        equipButton.interactable = !equipped;
        dismantleButton.interactable = !equipped;
        SetText(equipButtonText, "\uC7A5\uCC29\uD558\uAE30");
        SetText(dismantleButtonText, "\uC7A5\uBE44 \uD574\uCCB4");
        overlay.gameObject.SetActive(true);
        overlay.SetAsLastSibling();
    }

    private void Equip()
    {
        if (selectedInstance == null ||
            !(equipAction?.Invoke(selectedInstance.instanceId) ?? false))
        {
            return;
        }

        inventoryChanged?.Invoke();
        Hide();
    }

    private void Dismantle()
    {
        if (selectedInstance == null)
            return;

        if (!isDismantleConfirm)
        {
            isDismantleConfirm = true;
            titleText.text = "\uC7A5\uBE44 \uD574\uCCB4 \uD655\uC778";
            instructionText.text =
                "\uD574\uCCB4\uD558\uBA74 \uC7A5\uBE44\uAC00 \uC0AC\uB77C\uC9D1\uB2C8\uB2E4.";
            SetText(dismantleButtonText, "\uD574\uCCB4 \uD655\uC815");
            return;
        }

        if (!(dismantleAction?.Invoke(selectedInstance.instanceId) ?? false))
            return;

        inventoryChanged?.Invoke();
        Hide();
    }

    private static bool IsEquipped(string instanceId)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        return !string.IsNullOrWhiteSpace(instanceId) &&
            (data?.equippedWeaponInstanceId == instanceId ||
             data?.equippedArmorInstanceId == instanceId);
    }

    private static string GetStarText(int level)
    {
        int count = Mathf.Clamp(
            level,
            0,
            GameBalanceConfig.EquipmentStarForceMaxLevel);
        return count <= 0 ? "\u2606" : new string('\u2605', count);
    }

    private static string GetOptionsText(EquipmentInstance instance)
    {
        string options = EquipmentManager.FormatRolledOptions(
            instance?.rolledOptions);
        return string.IsNullOrWhiteSpace(options)
            ? "\uC635\uC158 \uC5C6\uC74C"
            : options.Replace(", ", "\n");
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private void Hide()
    {
        selectedInstance = null;
        overlay.gameObject.SetActive(false);
    }
}
