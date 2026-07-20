using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EquipmentEnhancementResultModalUI
{
    private RectTransform overlay;
    private TMP_Text titleText;
    private Image equipmentIcon;
    private TMP_Text equipmentTypeText;
    private TMP_Text equipmentNameText;
    private TMP_Text enhancementText;
    private TMP_Text resultText;

    public EquipmentEnhancementResultModalUI(RectTransform root)
    {
        if (!RuntimeUiBinder.TryInstantiatePrefab(
                "EquipmentEnhancementResultModal", root, out overlay))
        {
            Debug.LogError("EquipmentEnhancementResultModal prefab is missing.");
            return;
        }

        titleText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentEnhancementResultTitleText");
        equipmentIcon = RuntimeUiBinder.FindImage(
            overlay,
            "EquipmentEnhancementResultIcon");
        equipmentTypeText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentEnhancementResultTypeText");
        equipmentNameText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentEnhancementResultNameText");
        enhancementText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentEnhancementResultLevelText");
        resultText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentEnhancementResultText");
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(
                overlay,
                "EquipmentEnhancementResultConfirmButton"),
            Hide);
        overlay.gameObject.SetActive(false);
    }

    public void Show(EquipmentStarForceResult result)
    {
        if (result == null)
            return;

        PlayerData data = PlayerDataManager.Instance?.playerData;
        string equipmentId = result.slot == EquipmentSlot.Weapon
            ? data?.equippedWeapon
            : data?.equippedArmor;
        bool succeeded = result.success;
        titleText.text = succeeded
            ? (result.chanceTime
                ? "\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654 \uD655\uC815 \uC131\uACF5"
                : "\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654 \uC131\uACF5")
            : "\uBD80\uB9AC\uBD80\uB9AC \uAC15\uD654 \uC2E4\uD328";
        titleText.color = succeeded
            ? new Color32(255, 211, 100, 255)
            : new Color32(255, 129, 129, 255);
        equipmentIcon.sprite = EquipmentManager.GetEquipmentIcon(equipmentId);
        equipmentIcon.color = equipmentIcon.sprite == null
            ? Color.clear
            : Color.white;
        equipmentTypeText.text = result.slot == EquipmentSlot.Weapon
            ? "\uBB34\uAE30"
            : "\uBC29\uC5B4\uAD6C";
        equipmentNameText.text = EquipmentManager.GetEquipmentDisplayName(
            equipmentId);
        enhancementText.text = result.previousStar + "\uC131 -> " +
            result.currentStar + "\uC131";
        resultText.text = GetResultText(result);
        overlay.gameObject.SetActive(true);
        overlay.SetAsLastSibling();
    }

    private void Hide()
    {
        overlay.gameObject.SetActive(false);
    }

    private static string GetResultText(EquipmentStarForceResult result)
    {
        if (result.success)
        {
            return "\uC131\uACF5\uB960 " +
                result.successPercent.ToString("0") + "% / \uACE8\uB4DC -" +
                result.goldCost;
        }

        return result.downgraded
            ? "\uC2E4\uD328: 1\uC131 \uD558\uB77D / \uACE8\uB4DC -" +
                result.goldCost
            : "\uC2E4\uD328: \uB2E8\uACC4 \uC720\uC9C0 / \uACE8\uB4DC -" +
                result.goldCost;
    }
}
