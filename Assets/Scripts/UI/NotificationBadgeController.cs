using TMPro;
using UnityEngine;

public sealed class NotificationBadgeController
{
    private RectTransform questQuickBadge;
    private RectTransform eventQuickBadge;
    private RectTransform shopQuickBadge;
    private RectTransform equipmentQuickBadge;
    private RectTransform dailyRewardBadge;
    private RectTransform questMenuBadge;
    private RectTransform eventMenuBadge;
    private RectTransform growthNavBadge;
    private RectTransform gachaNavBadge;
    private RectTransform collectionNavBadge;
    private RectTransform moreNavBadge;

    public void RegisterBattleHud(BattleHudUI battleHud)
    {
        if (battleHud == null)
            return;

        questQuickBadge = battleHud.QuestQuickBadge;
        eventQuickBadge = battleHud.EventQuickBadge;
        shopQuickBadge = battleHud.ShopQuickBadge;
        equipmentQuickBadge = battleHud.EquipmentQuickBadge;
    }

    public void RegisterMorePanel(MorePanelUI morePanel)
    {
        if (morePanel == null)
            return;

        dailyRewardBadge = morePanel.DailyRewardBadge;
        questMenuBadge = morePanel.QuestMenuBadge;
        eventMenuBadge = morePanel.EventMenuBadge;
    }

    public void RegisterBottomNavigation(BottomNavigationUI bottomNavigation)
    {
        if (bottomNavigation == null)
            return;

        growthNavBadge = bottomNavigation.GrowthBadge;
        gachaNavBadge = bottomNavigation.GachaBadge;
        collectionNavBadge = bottomNavigation.CollectionBadge;
        moreNavBadge = bottomNavigation.MoreBadge;
    }

    public void Refresh(NotificationBadgeState state)
    {
        SetBadgeVisible(questQuickBadge, state.QuestReady);
        SetBadgeVisible(eventQuickBadge, state.EventReady);
        SetBadgeVisible(shopQuickBadge, state.ShopReady);
        SetBadgeVisible(equipmentQuickBadge, state.EquipmentReady);
        SetBadgeVisible(dailyRewardBadge, state.DailyReady);
        SetBadgeVisible(questMenuBadge, state.QuestReady);
        SetBadgeVisible(eventMenuBadge, state.EventReady);
        SetBadgeVisible(growthNavBadge, state.GrowthReady);
        SetBadgeVisible(gachaNavBadge, state.GachaReady);
        SetBadgeVisible(collectionNavBadge, state.CollectionReady);
        SetBadgeVisible(moreNavBadge, state.MoreReady);
    }

    private static void SetBadgeVisible(
        RectTransform badge,
        bool visible,
        string label = "!")
    {
        if (badge == null)
            return;

        badge.gameObject.SetActive(visible);
        if (!visible)
            return;

        badge.SetAsLastSibling();
        TMP_Text text = badge.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = label;
    }
}
