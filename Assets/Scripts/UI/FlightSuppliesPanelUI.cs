using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public sealed class FlightSuppliesPanelUI
{
    private RectTransform panel;
    private TMP_Text equipmentCountText;
    private TMP_Text materialsText;

    public GameObject GameObject => panel == null ? null : panel.gameObject;

    public FlightSuppliesPanelUI(
        RectTransform root,
        Action showEquipmentInventory)
    {
        if (!RuntimeUiBinder.TryInstantiatePrefab(
                "FlightSuppliesPanel", root, out panel))
        {
            Debug.LogError("FlightSuppliesPanel prefab is missing.");
            return;
        }

        equipmentCountText = RuntimeUiBinder.FindText(
            panel,
            "FlightSuppliesEquipmentCountText");
        materialsText = RuntimeUiBinder.FindText(
            panel,
            "FlightSuppliesMaterialsText");
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(
                panel,
                "FlightSuppliesEquipmentButton"),
            () => showEquipmentInventory?.Invoke());
    }

    public void Refresh(PlayerData data)
    {
        if (data == null)
        {
            SetText(equipmentCountText, "\uC7A5\uBE44 \uC815\uBCF4\uB97C \uBD88\uB7EC\uC624\uB294 \uC911\uC785\uB2C8\uB2E4.");
            SetText(materialsText, "-");
            return;
        }

        int equipmentCount = EquipmentManager.GetOwnedEquipment(data).Count;
        SetText(
            equipmentCountText,
            "\uBCF4\uC720 \uC7A5\uBE44 " + equipmentCount + "\uAC1C");
        SetText(materialsText, FormatMaterials(data));
    }

    private static string FormatMaterials(PlayerData data)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("\uBE44\uD589\uB2E8 \uC7A5\uBE44 \uCF54\uC778 x")
            .Append(data.flightEquipmentCoins);
        List<KeyValuePair<string, int>> materials =
            new List<KeyValuePair<string, int>>();
        if (data.inventory?.items != null)
        {
            foreach (KeyValuePair<string, int> entry in data.inventory.items)
            {
                if (entry.Value > 0 &&
                    EquipmentManager.GetEquipmentDefinition(entry.Key) == null)
                {
                    materials.Add(entry);
                }
            }
        }

        materials.Sort((left, right) => string.CompareOrdinal(
            left.Key,
            right.Key));
        foreach (KeyValuePair<string, int> material in materials)
        {
            builder.Append('\n')
                .Append(material.Key)
                .Append(" x")
                .Append(material.Value);
        }

        return builder.ToString();
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}
