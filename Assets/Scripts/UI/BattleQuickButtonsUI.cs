using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleQuickButtonsUI
{
    private readonly Action onQuest;
    private readonly Action onEvent;
    private readonly Action onShop;
    private readonly Action onEquipment;
    private readonly Color badgeColor;

    public RectTransform QuestQuickBadge { get; private set; }
    public RectTransform EventQuickBadge { get; private set; }
    public RectTransform ShopQuickBadge { get; private set; }
    public RectTransform EquipmentQuickBadge { get; private set; }

    public BattleQuickButtonsUI(
        Action onQuest,
        Action onEvent,
        Action onShop,
        Action onEquipment,
        Color badgeColor)
    {
        this.onQuest = onQuest;
        this.onEvent = onEvent;
        this.onShop = onShop;
        this.onEquipment = onEquipment;
        this.badgeColor = badgeColor;
    }

    public void Build(RectTransform parent)
    {
        Button questQuickButton = RuntimeUiFactory.CreateButton(
            "QuestQuickButton",
            parent,
            "QUEST",
            new Vector2(0.02f, 0.82f),
            new Vector2(0.19f, 0.89f),
            new Color32(24, 35, 58, 210),
            () => onQuest?.Invoke());
        QuestQuickBadge = BattleHudUiFactory.CreateBadge(
            questQuickButton,
            "QuestQuickBadge",
            badgeColor);

        Button eventQuickButton = RuntimeUiFactory.CreateButton(
            "EventQuickButton",
            parent,
            "EVENT",
            new Vector2(0.81f, 0.82f),
            new Vector2(0.98f, 0.89f),
            new Color32(24, 35, 58, 210),
            () => onEvent?.Invoke());
        EventQuickBadge = BattleHudUiFactory.CreateBadge(
            eventQuickButton,
            "EventQuickBadge",
            badgeColor);

        Button shopQuickButton = RuntimeUiFactory.CreateButton(
            "ShopQuickButton",
            parent,
            "SHOP",
            new Vector2(0.02f, 0.74f),
            new Vector2(0.19f, 0.81f),
            new Color32(24, 35, 58, 210),
            () => onShop?.Invoke());
        ShopQuickBadge = BattleHudUiFactory.CreateBadge(
            shopQuickButton,
            "ShopQuickBadge",
            badgeColor);

        Button equipmentQuickButton = RuntimeUiFactory.CreateButton(
            "EquipmentQuickButton",
            parent,
            "EQUIPMENT",
            new Vector2(0.81f, 0.74f),
            new Vector2(0.98f, 0.81f),
            new Color32(24, 35, 58, 210),
            () => onEquipment?.Invoke());
        EquipmentQuickBadge = BattleHudUiFactory.CreateBadge(
            equipmentQuickButton,
            "EquipmentQuickBadge",
            badgeColor);
    }

    public void Bind(RectTransform parent)
    {
        Button questQuickButton = FindOrCreateQuickButton(
            parent,
            "QuestQuickButton",
            "QUEST",
            new Vector2(0.02f, 0.82f),
            new Vector2(0.19f, 0.89f),
            () => onQuest?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            questQuickButton,
            () => onQuest?.Invoke());
        QuestQuickBadge =
            FindOrCreateBadge(questQuickButton, "QuestQuickBadge");

        Button eventQuickButton = FindOrCreateQuickButton(
            parent,
            "EventQuickButton",
            "EVENT",
            new Vector2(0.81f, 0.82f),
            new Vector2(0.98f, 0.89f),
            () => onEvent?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            eventQuickButton,
            () => onEvent?.Invoke());
        EventQuickBadge =
            FindOrCreateBadge(eventQuickButton, "EventQuickBadge");

        Button shopQuickButton = FindOrCreateQuickButton(
            parent,
            "ShopQuickButton",
            "SHOP",
            new Vector2(0.02f, 0.74f),
            new Vector2(0.19f, 0.81f),
            () => onShop?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            shopQuickButton,
            () => onShop?.Invoke());
        ShopQuickBadge =
            FindOrCreateBadge(shopQuickButton, "ShopQuickBadge");

        Button equipmentQuickButton = FindOrCreateQuickButton(
            parent,
            "EquipmentQuickButton",
            "EQUIPMENT",
            new Vector2(0.81f, 0.74f),
            new Vector2(0.98f, 0.81f),
            () => onEquipment?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            equipmentQuickButton,
            () => onEquipment?.Invoke());
        EquipmentQuickBadge =
            FindOrCreateBadge(equipmentQuickButton, "EquipmentQuickBadge");
    }

    private static Button FindOrCreateQuickButton(
        RectTransform parent,
        string name,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Action action)
    {
        Button button = RuntimeUiBinder.FindButton(parent, name);
        if (button != null)
            return button;

        return RuntimeUiFactory.CreateButton(
            name,
            parent,
            label,
            anchorMin,
            anchorMax,
            new Color32(24, 35, 58, 210),
            () => action?.Invoke());
    }

    private RectTransform FindOrCreateBadge(Button button, string badgeName)
    {
        if (button == null)
            return null;

        RectTransform badge = RuntimeUiBinder.FindRect(
            button.transform,
            badgeName);
        return badge != null
            ? badge
            : BattleHudUiFactory.CreateBadge(
                button,
                badgeName,
                badgeColor);
    }
}
