using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EquipmentInventoryModalUI
{
    private const int MinimumSlotCount = 45;
    private const int SlotsPerRow = 5;

    private RectTransform overlay;
    private RectTransform cardArea;
    private RectTransform cardContent;
    private TMP_Text titleText;
    private TMP_Text countText;
    private TMP_Text emptyText;
    private ScrollRect scrollRect;
    private readonly Action<EquipmentInstance> selectAction;

    public EquipmentInventoryModalUI(
        RectTransform root,
        Action<EquipmentInstance> selectAction)
    {
        this.selectAction = selectAction;
        if (!RuntimeUiBinder.TryInstantiatePrefab(
                "EquipmentInventoryModal", root, out overlay))
        {
            Debug.LogError("EquipmentInventoryModal prefab is missing.");
            return;
        }

        titleText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentInventoryTitleText");
        cardArea = RuntimeUiBinder.FindRect(
            overlay,
            "EquipmentInventoryCardArea");
        scrollRect = cardArea != null
            ? cardArea.GetComponent<ScrollRect>()
            : null;
        cardContent = RuntimeUiBinder.FindRect(
            overlay,
            "EquipmentInventoryContent");
        countText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentInventoryCountText");
        emptyText = RuntimeUiBinder.FindText(
            overlay,
            "EquipmentInventoryEmptyText");
        ConfigureLegacyGrid();
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(
                overlay,
                "EquipmentInventoryCloseButton"),
            Hide);
        overlay.gameObject.SetActive(false);
    }

    public void Show()
    {
        overlay.gameObject.SetActive(true);
        overlay.SetAsLastSibling();
        titleText.text = "\uBE44\uD589\uB2E8 \uC7A5\uBE44";
        RebuildCards();
        Canvas.ForceUpdateCanvases();
        if (cardContent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardContent);
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    public void Refresh()
    {
        if (overlay != null && overlay.gameObject.activeSelf)
            RebuildCards();
    }

    private void RebuildCards()
    {
        RectTransform content = cardContent ?? cardArea;
        for (int index = content.childCount - 1; index >= 0; index--)
        {
            Transform child = content.GetChild(index);
            if (child.name != "EquipmentInventoryEmptyText")
                UnityEngine.Object.Destroy(child.gameObject);
        }

        PlayerData data = PlayerDataManager.Instance?.playerData;
        List<EquipmentInstance> owned =
            EquipmentManager.GetOwnedEquipment(data);
        int slotCount = Mathf.Max(
            MinimumSlotCount,
            Mathf.CeilToInt(owned.Count / (float)SlotsPerRow) * SlotsPerRow);
        SetText(countText, owned.Count + " / " + slotCount);
        if (emptyText != null)
            emptyText.gameObject.SetActive(owned.Count == 0);

        for (int index = 0; index < slotCount; index++)
        {
            CreateEquipmentSlot(
                data,
                index < owned.Count ? owned[index] : null,
                index);
        }
    }

    private void CreateEquipmentSlot(
        PlayerData data,
        EquipmentInstance instance,
        int index)
    {
        RectTransform content = cardContent ?? cardArea;
        string slotPrefab = Resources.Load<GameObject>(
            "Prefabs/UI/EquipmentInventorySlot") != null
            ? "EquipmentInventorySlot"
            : "EquipmentInventoryItemCard";
        if (!RuntimeUiBinder.TryInstantiatePrefab(
                slotPrefab, content, out RectTransform cardRect))
        {
            Debug.LogError("Equipment inventory slot prefab is missing.");
            return;
        }

        cardRect.name = "EquipmentInventorySlot_" + index;
        Button card = cardRect.GetComponent<Button>();
        Image cardArt = RuntimeUiBinder.FindImage(cardRect, "ButtonArt");
        Image icon = RuntimeUiBinder.FindImage(
            cardRect,
            "EquipmentInventorySlotIcon");
        if (icon == null)
        {
            icon = RuntimeUiBinder.FindImage(
                cardRect,
                "EquipmentInventoryItemIcon");
        }
        TMP_Text starsText = RuntimeUiBinder.FindText(
            cardRect,
            "EquipmentInventorySlotStarsText");
        if (starsText == null)
        {
            starsText = RuntimeUiBinder.FindText(
                cardRect,
                "EquipmentInventoryItemStars");
        }
        TMP_Text equippedText = RuntimeUiBinder.FindText(
            cardRect,
            "EquipmentInventorySlotEquippedText");
        if (equippedText == null)
        {
            equippedText = RuntimeUiBinder.FindText(
                cardRect,
                "EquipmentInventoryEquippedText");
        }
        SetActive(cardRect, "EquipmentInventoryItemName", false);
        SetActive(cardRect, "EquipmentInventoryItemOptions", false);

        if (instance == null)
        {
            card.interactable = false;
            if (cardArt != null)
                cardArt.color = new Color32(96, 82, 58, 150);
            if (icon != null)
                icon.color = Color.clear;
            SetText(starsText, "");
            if (equippedText != null)
                equippedText.gameObject.SetActive(false);
            return;
        }

        bool equipped = IsEquipped(data, instance.instanceId);
        EquipmentDefinition definition =
            EquipmentManager.GetEquipmentDefinition(instance.definitionId);
        RuntimeUiBinder.ReplaceButtonAction(card, () => Select(instance));
        if (cardArt != null)
        {
            Color tierColor = GetSlotColor(definition?.tier ?? 0);
            cardArt.color = equipped
                ? Color.Lerp(tierColor, Color.white, 0.14f)
                : tierColor;
        }

        if (icon != null)
        {
            icon.sprite = EquipmentManager.GetEquipmentIcon(instance.definitionId);
            icon.color = icon.sprite == null ? Color.clear : Color.white;
        }
        SetText(
            starsText,
            GetSlotHeader(
                definition?.tier ?? 0,
                instance.enhancementLevel,
                equipped));
        if (equippedText != null)
        {
            equippedText.text = definition?.DisplayName ?? instance.definitionId;
            equippedText.gameObject.SetActive(true);
        }
    }

    private void Select(EquipmentInstance instance)
    {
        selectAction?.Invoke(instance);
    }

    private static bool IsEquipped(PlayerData data, string instanceId)
    {
        return !string.IsNullOrWhiteSpace(instanceId) &&
            (data?.equippedWeaponInstanceId == instanceId ||
             data?.equippedArmorInstanceId == instanceId);
    }

    private static string GetSlotHeader(
        int tier,
        int level,
        bool equipped)
    {
        int starCount = Mathf.Clamp(
            level,
            0,
            GameBalanceConfig.EquipmentStarForceMaxLevel);
        string stars = starCount > 0 ? "  \u2605" + starCount : "";
        string equippedLabel = equipped ? "  \uCC29\uC6A9" : "";
        return GetTierLabel(tier) + stars + equippedLabel;
    }

    private static string GetTierLabel(int tier)
    {
        string[] labels =
        {
            "D", "G", "B", "A", "S", "SS", "SSS", "X", "XX", "XXX"
        };
        return labels[Mathf.Clamp(tier, 0, labels.Length - 1)];
    }

    private static Color GetSlotColor(int tier)
    {
        switch (Mathf.Max(0, tier))
        {
            case 1:
                return new Color32(238, 166, 56, 255);
            case 2:
                return new Color32(58, 168, 190, 255);
            case 3:
                return new Color32(75, 146, 65, 255);
            case 4:
                return new Color32(133, 125, 45, 255);
            case 5:
                return new Color32(126, 52, 47, 255);
            case 6:
                return new Color32(94, 54, 121, 255);
            case 7:
                return new Color32(28, 111, 111, 255);
            case 8:
                return new Color32(119, 35, 100, 255);
            case 9:
                return new Color32(108, 81, 18, 255);
            default:
                return new Color32(202, 139, 77, 255);
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private void ConfigureLegacyGrid()
    {
        if (RuntimeUiBinder.FindTransform(
                overlay,
                "EquipmentInventoryGridV2") != null ||
            cardContent == null)
        {
            return;
        }

        GridLayoutGroup grid = cardContent.GetComponent<GridLayoutGroup>();
        if (grid == null)
            return;

        grid.padding = new RectOffset(14, 14, 14, 14);
        grid.cellSize = new Vector2(170f, 126f);
        grid.spacing = new Vector2(12f, 12f);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = SlotsPerRow;
    }

    private static void SetActive(
        RectTransform root,
        string name,
        bool active)
    {
        Transform target = RuntimeUiBinder.FindTransform(root, name);
        if (target != null)
            target.gameObject.SetActive(active);
    }

    private void Hide()
    {
        overlay.gameObject.SetActive(false);
    }
}
