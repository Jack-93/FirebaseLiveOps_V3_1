using System;
using TMPro;
using UnityEngine;

public sealed class EquipmentCubeModalUI
{
    private readonly RectTransform overlay;
    private readonly TMP_Text equipmentNameText;
    private readonly TMP_Text coinCostText;
    private readonly TMP_Text currentOptionsText;
    private readonly TMP_Text newOptionsText;
    private readonly Action<bool> chooseResult;

    public EquipmentCubeModalUI(
        RectTransform root,
        Action<bool> chooseResult)
    {
        this.chooseResult = chooseResult;
        overlay = RuntimeUiFactory.CreatePanel(
            "EquipmentCubeModal",
            root,
            new Color32(5, 8, 16, 220),
            Vector2.zero,
            Vector2.one);

        RectTransform dialog = RuntimeUiFactory.CreatePanel(
            "CubeResultDialog",
            overlay,
            new Color32(35, 48, 76, 255),
            new Vector2(0.1f, 0.22f),
            new Vector2(0.9f, 0.78f));
        RuntimeUiFactory.CreateText(
            "CubeTitleText",
            dialog,
            "\uC635\uC158 \uC7AC\uC124\uC815",
            34f,
            new Vector2(0.08f, 0.84f),
            new Vector2(0.92f, 0.96f),
            TextAlignmentOptions.Center,
            new Color32(255, 211, 100, 255));
        equipmentNameText = RuntimeUiFactory.CreateText(
            "CubeEquipmentNameText",
            dialog,
            "",
            26f,
            new Vector2(0.08f, 0.73f),
            new Vector2(0.92f, 0.84f),
            TextAlignmentOptions.Center,
            Color.white);
        coinCostText = RuntimeUiFactory.CreateText(
            "CubeCoinCostText",
            dialog,
            "",
            20f,
            new Vector2(0.08f, 0.67f),
            new Vector2(0.92f, 0.73f),
            TextAlignmentOptions.Center,
            new Color32(255, 211, 100, 255));

        CreateOptionColumn(
            dialog,
            "CubeCurrentColumn",
            "\uAE30\uC874 \uC635\uC158",
            new Vector2(0.07f, 0.3f),
            new Vector2(0.47f, 0.64f),
            out currentOptionsText);
        CreateOptionColumn(
            dialog,
            "CubeNewColumn",
            "\uC0C8 \uC635\uC158",
            new Vector2(0.53f, 0.3f),
            new Vector2(0.93f, 0.64f),
            out newOptionsText);

        RuntimeUiFactory.CreateButton(
            "CubeKeepCurrentButton",
            dialog,
            "\uAE30\uC874 \uC720\uC9C0",
            new Vector2(0.07f, 0.08f),
            new Vector2(0.47f, 0.23f),
            new Color32(84, 111, 149, 255),
            () => Choose(false));
        RuntimeUiFactory.CreateButton(
            "CubeUseNewButton",
            dialog,
            "\uC0C8 \uC635\uC158 \uC801\uC6A9",
            new Vector2(0.53f, 0.08f),
            new Vector2(0.93f, 0.23f),
            new Color32(72, 169, 139, 255),
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

    private static void CreateOptionColumn(
        RectTransform parent,
        string name,
        string heading,
        Vector2 anchorMin,
        Vector2 anchorMax,
        out TMP_Text optionsText)
    {
        RectTransform column = RuntimeUiFactory.CreatePanel(
            name,
            parent,
            new Color32(21, 31, 52, 255),
            anchorMin,
            anchorMax);
        RuntimeUiFactory.CreateText(
            name + "Heading",
            column,
            heading,
            22f,
            new Vector2(0.08f, 0.7f),
            new Vector2(0.92f, 0.94f),
            TextAlignmentOptions.Center,
            new Color32(174, 210, 255, 255));
        optionsText = RuntimeUiFactory.CreateText(
            name + "Options",
            column,
            "",
            21f,
            new Vector2(0.08f, 0.1f),
            new Vector2(0.92f, 0.66f),
            TextAlignmentOptions.Center,
            Color.white);
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
