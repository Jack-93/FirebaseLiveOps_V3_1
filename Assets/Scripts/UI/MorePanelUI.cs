using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MorePanelUI
{
    private RectTransform panel;
    private TMP_Text inventoryText;
    private TMP_Text companionText;
    private TMP_Text dailyRewardText;
    private TMP_Text accountText;

    public GameObject GameObject => panel == null ? null : panel.gameObject;
    public RectTransform DailyRewardBadge { get; private set; }
    public RectTransform QuestMenuBadge { get; private set; }
    public RectTransform EventMenuBadge { get; private set; }

    private static readonly Color OverlayBackground =
        new Color32(12, 18, 30, 218);
    private static readonly Color Panel =
        new Color32(37, 49, 73, 245);
    private static readonly Color PanelLight =
        new Color32(52, 68, 96, 255);
    private static readonly Color Accent =
        new Color32(82, 188, 255, 255);
    private static readonly Color Gold =
        new Color32(255, 201, 77, 255);
    private static readonly Color Success =
        new Color32(76, 205, 145, 255);
    private static readonly Color Danger =
        new Color32(238, 83, 106, 255);
    private static readonly Color MutedText =
        new Color32(190, 203, 225, 255);

    public MorePanelUI(
        RectTransform root,
        Action claimAllMail,
        Action showCollection,
        Action autoEquip,
        Action claimDailyReward,
        Action showQuests,
        Action showEvent,
        Action showShop,
        Action save,
        Action showSettings,
        Action showAccount,
        bool usePrefab = true)
    {
        if (usePrefab &&
            RuntimeUiBinder.TryInstantiatePrefab(
                "MorePanel",
                root,
                out panel))
        {
            Bind(
                claimAllMail,
                showCollection,
                autoEquip,
                claimDailyReward,
                showQuests,
                showEvent,
                showShop,
                save,
                showSettings,
                showAccount);
            return;
        }

        BuildGenerated(
            root,
            claimAllMail,
            showCollection,
            autoEquip,
            claimDailyReward,
            showQuests,
            showEvent,
            showShop,
            save,
            showSettings,
            showAccount);
    }

    public void BuildGenerated(
        RectTransform root,
        Action claimAllMail,
        Action showCollection,
        Action autoEquip,
        Action claimDailyReward,
        Action showQuests,
        Action showEvent,
        Action showShop,
        Action save,
        Action showSettings,
        Action showAccount)
    {
        panel = RuntimeUiFactory.CreatePanel(
            "MorePanel",
            root,
            OverlayBackground,
            new Vector2(0f, 0.12f),
            new Vector2(1f, 0.9f));

        RuntimeUiFactory.CreateText(
            "MoreTitle",
            panel,
            "PLAYER HUB",
            48,
            new Vector2(0.05f, 0.9f),
            new Vector2(0.95f, 0.98f),
            TextAlignmentOptions.Center,
            Accent);

        RuntimeUiFactory.CreateText(
            "MoreSubtitle",
            panel,
            "Inventory, companions, rewards, and account.",
            25,
            new Vector2(0.06f, 0.85f),
            new Vector2(0.94f, 0.9f),
            TextAlignmentOptions.Center,
            MutedText);

        RectTransform inventoryCard = BuildInventoryCard(
            claimAllMail);
        inventoryText = RuntimeUiFactory.CreateText(
            "InventoryText",
            inventoryCard,
            "Inventory",
            27,
            new Vector2(0.05f, 0.08f),
            new Vector2(0.68f, 0.72f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        RectTransform companionCard = BuildCompanionCard(
            showCollection,
            autoEquip);
        companionText = RuntimeUiFactory.CreateText(
            "CompanionText",
            companionCard,
            "Companion",
            25,
            new Vector2(0.05f, 0.08f),
            new Vector2(0.68f, 0.7f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        RectTransform rewardCard = BuildRewardCard(claimDailyReward);
        dailyRewardText = RuntimeUiFactory.CreateText(
            "DailyRewardText",
            rewardCard,
            "Daily reward",
            29,
            new Vector2(0.05f, 0.45f),
            new Vector2(0.64f, 0.9f),
            TextAlignmentOptions.Left,
            Color.white);
        accountText = RuntimeUiFactory.CreateText(
            "AccountText",
            rewardCard,
            "Account",
            22,
            new Vector2(0.05f, 0.05f),
            new Vector2(0.64f, 0.4f),
            TextAlignmentOptions.Left,
            new Color32(174, 189, 214, 255));

        BuildMenuButtons(
            showQuests,
            showEvent,
            showShop,
            save,
            showSettings,
            showAccount);
    }

    public void Refresh(
        PlayerData data,
        CompanionManager companionManager,
        AccountLinkManager accounts,
        DailyRewardManager dailyRewards)
    {
        if (data == null)
        {
            SetText(inventoryText, MorePanelSummaryFormatter.InventoryUnavailable);
            SetText(companionText, string.Empty);
            SetText(dailyRewardText, string.Empty);
            SetText(accountText, string.Empty);
            return;
        }

        SetText(
            inventoryText,
            MorePanelSummaryFormatter.FormatInventory(
                data,
                companionManager));
        SetText(
            companionText,
            MorePanelSummaryFormatter.FormatCompanions(companionManager));
        SetText(
            accountText,
            MorePanelSummaryFormatter.FormatAccount(data, accounts));
        SetText(
            dailyRewardText,
            MorePanelSummaryFormatter.FormatDailyReward(dailyRewards));
    }

    private RectTransform BuildInventoryCard(
        Action claimAllMail)
    {
        RectTransform card = RuntimeUiFactory.CreatePanel(
            "InventoryCard",
            panel,
            Panel,
            new Vector2(0.05f, 0.62f),
            new Vector2(0.95f, 0.84f));
        RuntimeUiFactory.CreateText(
            "InventoryTitle",
            card,
            "RESOURCES",
            27,
            new Vector2(0.05f, 0.74f),
            new Vector2(0.68f, 0.94f),
            TextAlignmentOptions.Left,
            Gold);
        RuntimeUiFactory.CreateButton(
            "ClaimMailButton",
            card,
            "MAIL",
            new Vector2(0.71f, 0.54f),
            new Vector2(0.95f, 0.88f),
            Gold,
            () => claimAllMail?.Invoke());
        return card;
    }

    private RectTransform BuildCompanionCard(
        Action showCollection,
        Action autoEquip)
    {
        RectTransform card = RuntimeUiFactory.CreatePanel(
            "CompanionCard",
            panel,
            Panel,
            new Vector2(0.05f, 0.39f),
            new Vector2(0.95f, 0.59f));
        RuntimeUiFactory.CreateText(
            "CompanionTitle",
            card,
            "COMPANIONS",
            27,
            new Vector2(0.05f, 0.72f),
            new Vector2(0.68f, 0.94f),
            TextAlignmentOptions.Left,
            Gold);
        RuntimeUiFactory.CreateButton(
            "CollectionButton",
            card,
            "COLLECTION",
            new Vector2(0.71f, 0.53f),
            new Vector2(0.95f, 0.88f),
            PanelLight,
            () => showCollection?.Invoke());
        RuntimeUiFactory.CreateButton(
            "BestCompanionButton",
            card,
            "BEST",
            new Vector2(0.71f, 0.12f),
            new Vector2(0.95f, 0.47f),
            Accent,
            () => autoEquip?.Invoke());
        return card;
    }

    private RectTransform BuildRewardCard(Action claimDailyReward)
    {
        RectTransform card = RuntimeUiFactory.CreatePanel(
            "RewardCard",
            panel,
            Panel,
            new Vector2(0.05f, 0.23f),
            new Vector2(0.95f, 0.36f));
        Button dailyRewardButton = RuntimeUiFactory.CreateButton(
            "DailyRewardButton",
            card,
            "CLAIM",
            new Vector2(0.69f, 0.2f),
            new Vector2(0.95f, 0.8f),
            Success,
            () => claimDailyReward?.Invoke());
        DailyRewardBadge = BattleHudUiFactory.CreateBadge(
            dailyRewardButton,
            "DailyRewardBadge",
            Danger);
        return card;
    }

    private void BuildMenuButtons(
        Action showQuests,
        Action showEvent,
        Action showShop,
        Action save,
        Action showSettings,
        Action showAccount)
    {
        Button questMenuButton = RuntimeUiFactory.CreateButton(
            "QuestButton",
            panel,
            "QUESTS",
            new Vector2(0.06f, 0.13f),
            new Vector2(0.31f, 0.21f),
            Gold,
            () => showQuests?.Invoke());
        QuestMenuBadge = BattleHudUiFactory.CreateBadge(
            questMenuButton,
            "QuestMenuBadge",
            Danger);

        Button eventMenuButton = RuntimeUiFactory.CreateButton(
            "EventButton",
            panel,
            "EVENT",
            new Vector2(0.36f, 0.13f),
            new Vector2(0.64f, 0.21f),
            Success,
            () => showEvent?.Invoke());
        EventMenuBadge = BattleHudUiFactory.CreateBadge(
            eventMenuButton,
            "EventMenuBadge",
            Danger);

        RuntimeUiFactory.CreateButton(
            "ShopButton",
            panel,
            "SHOP",
            new Vector2(0.69f, 0.13f),
            new Vector2(0.94f, 0.21f),
            Accent,
            () => showShop?.Invoke());
        RuntimeUiFactory.CreateButton(
            "SaveButton",
            panel,
            "SAVE",
            new Vector2(0.06f, 0.03f),
            new Vector2(0.31f, 0.12f),
            PanelLight,
            () => save?.Invoke());
        RuntimeUiFactory.CreateButton(
            "SettingsButton",
            panel,
            "SETTINGS",
            new Vector2(0.36f, 0.03f),
            new Vector2(0.64f, 0.12f),
            PanelLight,
            () => showSettings?.Invoke());
        RuntimeUiFactory.CreateButton(
            "AccountButton",
            panel,
            "ACCOUNT",
            new Vector2(0.69f, 0.03f),
            new Vector2(0.94f, 0.12f),
            Accent,
            () => showAccount?.Invoke());
    }

    private void Bind(
        Action claimAllMail,
        Action showCollection,
        Action autoEquip,
        Action claimDailyReward,
        Action showQuests,
        Action showEvent,
        Action showShop,
        Action save,
        Action showSettings,
        Action showAccount)
    {
        inventoryText = RuntimeUiBinder.FindText(panel, "InventoryText");
        companionText = RuntimeUiBinder.FindText(panel, "CompanionText");
        dailyRewardText = RuntimeUiBinder.FindText(panel, "DailyRewardText");
        accountText = RuntimeUiBinder.FindText(panel, "AccountText");

        Replace("ClaimMailButton", claimAllMail);
        Hide("EquipmentButton");
        Replace("CollectionButton", showCollection);
        Replace("BestCompanionButton", autoEquip);
        Replace("DailyRewardButton", claimDailyReward);
        Replace("QuestButton", showQuests);
        Replace("EventButton", showEvent);
        Replace("ShopButton", showShop);
        Replace("SaveButton", save);
        Replace("SettingsButton", showSettings);
        Replace("AccountButton", showAccount);

        DailyRewardBadge =
            RuntimeUiBinder.FindRect(panel, "DailyRewardBadge");
        QuestMenuBadge =
            RuntimeUiBinder.FindRect(panel, "QuestMenuBadge");
        EventMenuBadge =
            RuntimeUiBinder.FindRect(panel, "EventMenuBadge");
    }

    private void Replace(string buttonName, Action action)
    {
        RuntimeUiBinder.ReplaceButtonAction(
            RuntimeUiBinder.FindButton(panel, buttonName),
            () => action?.Invoke());
    }

    private void Hide(string objectName)
    {
        Transform target = RuntimeUiBinder.FindTransform(panel, objectName);
        if (target != null)
            target.gameObject.SetActive(false);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}
