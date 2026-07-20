using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EquipmentCubeModalUI
{
    private RectTransform overlay;
    private TMP_Text equipmentNameText;
    private TMP_Text coinCostText;
    private TMP_Text currentOptionsText;
    private TMP_Text newOptionsText;
    private readonly Action<bool> chooseResult;

    public EquipmentCubeModalUI(
        RectTransform root,
        Action<bool> chooseResult)
    {
        this.chooseResult = chooseResult;
        if (!RuntimeUiBinder.TryInstantiatePrefab(
                "EquipmentCubeModal", root, out overlay))
        {
            Debug.LogError("EquipmentCubeModal prefab is missing.");
            return;
        }

        equipmentNameText = RuntimeUiBinder.FindText(
            overlay,
            "CubeEquipmentNameText");
        coinCostText = RuntimeUiBinder.FindText(
            overlay,
            "CubeCoinCostText");
        currentOptionsText = RuntimeUiBinder.FindText(
            overlay,
            "CubeCurrentColumnOptions");
        newOptionsText = RuntimeUiBinder.FindText(
            overlay,
            "CubeNewColumnOptions");
        Button keepCurrentButton = RuntimeUiBinder.FindButton(
            overlay,
            "CubeKeepCurrentButton");
        Button useNewButton = RuntimeUiBinder.FindButton(
            overlay,
            "CubeUseNewButton");
        RuntimeUiBinder.ReplaceButtonAction(
            keepCurrentButton,
            () => Choose(false));
        RuntimeUiBinder.ReplaceButtonAction(
            useNewButton,
            () => Choose(true));
        overlay.gameObject.SetActive(false);
    }

    public void Show(EquipmentCubePreview preview)
    {
        if (preview == null)
            return;

        equipmentNameText.text = preview.equipmentName;
        coinCostText.text =
            "\uBE44\uD589\uB2E8 \uC7A5\uBE44 \uCF54\uC778 x" +
            preview.coinCost;
        currentOptionsText.text = FormatOptions(preview.currentOptions);
        newOptionsText.text = FormatOptions(preview.newOptions);
        overlay.gameObject.SetActive(true);
        overlay.SetAsLastSibling();
    }

    private void Choose(bool applyNew)
    {
        overlay.gameObject.SetActive(false);
        chooseResult?.Invoke(applyNew);
    }

    private static string FormatOptions(
        System.Collections.Generic.List<EquipmentRolledOption> options)
    {
        string text = EquipmentManager.FormatRolledOptions(options);
        return string.IsNullOrWhiteSpace(text)
            ? "-"
            : text.Replace(", ", "\n");
    }
}
