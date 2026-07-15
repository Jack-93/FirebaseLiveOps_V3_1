using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BottomNavigationTab
{
    Battle,
    Growth,
    Gacha,
    Collection,
    Equipment,
    More
}

public sealed class BottomNavigationUI
{
    private RectTransform bottom;
    private Button battleButton;
    private Button growthButton;
    private Button gachaButton;
    private Button collectionButton;
    private Button equipmentButton;
    private Button moreButton;

    public GameObject GameObject => bottom == null ? null : bottom.gameObject;
    public RectTransform GrowthBadge { get; private set; }
    public RectTransform GachaBadge { get; private set; }
    public RectTransform CollectionBadge { get; private set; }
    public RectTransform MoreBadge { get; private set; }

    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Danger =
        new Color32(238, 83, 106, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public BottomNavigationUI(
        RectTransform root,
        Action showBattle,
        Action showGrowth,
        Action showGacha,
        Action showCollection,
        Action showEquipment,
        Action showMore,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "BottomNavigation",
                root,
                out bottom))
        {
            Bind(
                showBattle,
                showGrowth,
                showGacha,
                showCollection,
                showEquipment,
                showMore);
            return;
        }

        BuildGenerated(
            root,
            showBattle,
            showGrowth,
            showGacha,
            showCollection,
            showEquipment,
            showMore);
    }

    public void BuildGenerated(
        RectTransform root,
        Action showBattle,
        Action showGrowth,
        Action showGacha,
        Action showCollection,
        Action showEquipment,
        Action showMore)
    {
        bottom = RuntimeUiFactory.CreatePanel(
            "BottomNavigation",
            root,
            new Color32(24, 35, 58, 210),
            Vector2.zero,
            new Vector2(1f, 0.105f));

        battleButton = RuntimeUiFactory.CreateButton(
            "BattleNav",
            bottom,
            "BATTLE",
            new Vector2(0.015f, 0.08f),
            new Vector2(0.16f, 0.92f),
            PanelLight,
            () => showBattle?.Invoke());

        growthButton = RuntimeUiFactory.CreateButton(
            "GrowthNav",
            bottom,
            "GROWTH",
            new Vector2(0.175f, 0.08f),
            new Vector2(0.32f, 0.92f),
            PanelLight,
            () => showGrowth?.Invoke());
        GrowthBadge = CreateBadge(growthButton, "GrowthNavBadge");

        gachaButton = RuntimeUiFactory.CreateButton(
            "GachaNav",
            bottom,
            "GACHA",
            new Vector2(0.335f, 0.08f),
            new Vector2(0.48f, 0.92f),
            Accent,
            () => showGacha?.Invoke());
        GachaBadge = CreateBadge(gachaButton, "GachaNavBadge");

        collectionButton = RuntimeUiFactory.CreateButton(
            "CollectionNav",
            bottom,
            "COMPANIONS",
            new Vector2(0.495f, 0.08f),
            new Vector2(0.64f, 0.92f),
            PanelLight,
            () => showCollection?.Invoke());
        CollectionBadge = CreateBadge(
            collectionButton,
            "CollectionNavBadge");

        equipmentButton = RuntimeUiFactory.CreateButton(
            "EquipmentNav",
            bottom,
            "EQUIP",
            new Vector2(0.655f, 0.08f),
            new Vector2(0.8f, 0.92f),
            PanelLight,
            () => showEquipment?.Invoke());

        moreButton = RuntimeUiFactory.CreateButton(
            "MoreNav",
            bottom,
            "MORE",
            new Vector2(0.815f, 0.08f),
            new Vector2(0.985f, 0.92f),
            PanelLight,
            () => showMore?.Invoke());
        MoreBadge = CreateBadge(moreButton, "MoreNavBadge");
    }

    public void SetActive(BottomNavigationTab active)
    {
        SetNavigationColor(
            battleButton,
            active == BottomNavigationTab.Battle);
        SetNavigationColor(
            growthButton,
            active == BottomNavigationTab.Growth);
        SetNavigationColor(
            gachaButton,
            active == BottomNavigationTab.Gacha);
        SetNavigationColor(
            collectionButton,
            active == BottomNavigationTab.Collection);
        SetNavigationColor(
            equipmentButton,
            active == BottomNavigationTab.Equipment);
        SetNavigationColor(
            moreButton,
            active == BottomNavigationTab.More);
    }

    private void Bind(
        Action showBattle,
        Action showGrowth,
        Action showGacha,
        Action showCollection,
        Action showEquipment,
        Action showMore)
    {
        battleButton = RuntimeUiBinder.FindButton(bottom, "BattleNav");
        growthButton = RuntimeUiBinder.FindButton(bottom, "GrowthNav");
        gachaButton = RuntimeUiBinder.FindButton(bottom, "GachaNav");
        collectionButton =
            RuntimeUiBinder.FindButton(bottom, "CollectionNav");
        equipmentButton =
            RuntimeUiBinder.FindButton(bottom, "EquipmentNav");
        moreButton = RuntimeUiBinder.FindButton(bottom, "MoreNav");

        RuntimeUiBinder.ReplaceButtonAction(
            battleButton,
            () => showBattle?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            growthButton,
            () => showGrowth?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            gachaButton,
            () => showGacha?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            collectionButton,
            () => showCollection?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            equipmentButton,
            () => showEquipment?.Invoke());
        RuntimeUiBinder.ReplaceButtonAction(
            moreButton,
            () => showMore?.Invoke());

        GrowthBadge = RuntimeUiBinder.FindRect(bottom, "GrowthNavBadge");
        GachaBadge = RuntimeUiBinder.FindRect(bottom, "GachaNavBadge");
        CollectionBadge =
            RuntimeUiBinder.FindRect(bottom, "CollectionNavBadge");
        MoreBadge = RuntimeUiBinder.FindRect(bottom, "MoreNavBadge");
    }

    private static RectTransform CreateBadge(Button button, string name)
    {
        if (button == null)
            return null;

        RectTransform badge = RuntimeUiFactory.CreatePanel(
            name,
            button.transform,
            Danger,
            new Vector2(0.74f, 0.68f),
            new Vector2(0.98f, 0.98f));
        badge.GetComponent<Image>().raycastTarget = false;
        RuntimeUiFactory.CreateText(
            "BadgeText",
            badge,
            "!",
            22,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center,
            Color.white);
        badge.SetAsLastSibling();
        badge.gameObject.SetActive(false);
        return badge;
    }

    private static void SetNavigationColor(Button button, bool isActive)
    {
        if (button == null)
            return;

        Image background = button.GetComponent<Image>();
        if (background != null)
        {
            background.color = isActive
                ? Accent
                : PanelLight;
        }

        Transform artTransform = button.transform.Find("ButtonArt");
        if (artTransform != null &&
            artTransform.TryGetComponent(out Image artImage))
        {
            artImage.sprite = isActive
                ? PrototypeUiArt.ButtonSelected
                : PrototypeUiArt.ButtonNormal;
            artImage.color = Color.white;
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.color = isActive ? Color.white : MutedText;
    }
}
