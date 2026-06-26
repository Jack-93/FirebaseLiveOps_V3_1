using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MorePanelUI
{
    private readonly RectTransform panel;
    private readonly TMP_Text inventoryText;
    private readonly TMP_Text companionText;
    private readonly TMP_Text dailyRewardText;
    private readonly TMP_Text accountText;

    public GameObject GameObject => panel.gameObject;
    public RectTransform DailyRewardBadge { get; }
    public RectTransform QuestMenuBadge { get; }
    public RectTransform EventMenuBadge { get; }

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
        Action showEquipment,
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

        RectTransform inventoryCard = RuntimeUiFactory.CreatePanel(
            "InventoryCard",
            panel,
            Panel,
            new Vector2(0.05f, 0.62f),
            new Vector2(0.95f, 0.84f));

        RuntimeUiFactory.CreateText(
            "InventoryTitle",
            inventoryCard,
            "RESOURCES",
            27,
            new Vector2(0.05f, 0.74f),
            new Vector2(0.68f, 0.94f),
            TextAlignmentOptions.Left,
            Gold);

        inventoryText = RuntimeUiFactory.CreateText(
            "InventoryText",
            inventoryCard,
            "Inventory",
            27,
            new Vector2(0.05f, 0.08f),
            new Vector2(0.68f, 0.72f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        RuntimeUiFactory.CreateButton(
            "ClaimMailButton",
            inventoryCard,
            "MAIL",
            new Vector2(0.71f, 0.54f),
            new Vector2(0.95f, 0.88f),
            Gold,
            () => claimAllMail?.Invoke());

        RuntimeUiFactory.CreateButton(
            "EquipmentButton",
            inventoryCard,
            "EQUIPMENT",
            new Vector2(0.71f, 0.12f),
            new Vector2(0.95f, 0.47f),
            PanelLight,
            () => showEquipment?.Invoke());

        RectTransform companionCard = RuntimeUiFactory.CreatePanel(
            "CompanionCard",
            panel,
            Panel,
            new Vector2(0.05f, 0.39f),
            new Vector2(0.95f, 0.59f));

        RuntimeUiFactory.CreateText(
            "CompanionTitle",
            companionCard,
            "COMPANIONS",
            27,
            new Vector2(0.05f, 0.72f),
            new Vector2(0.68f, 0.94f),
            TextAlignmentOptions.Left,
            Gold);

        companionText = RuntimeUiFactory.CreateText(
            "CompanionText",
            companionCard,
            "Companion",
            25,
            new Vector2(0.05f, 0.08f),
            new Vector2(0.68f, 0.7f),
            TextAlignmentOptions.TopLeft,
            Color.white);

        RuntimeUiFactory.CreateButton(
            "CollectionButton",
            companionCard,
            "COLLECTION",
            new Vector2(0.71f, 0.53f),
            new Vector2(0.95f, 0.88f),
            PanelLight,
            () => showCollection?.Invoke());

        RuntimeUiFactory.CreateButton(
            "BestCompanionButton",
            companionCard,
            "BEST",
            new Vector2(0.71f, 0.12f),
            new Vector2(0.95f, 0.47f),
            Accent,
            () => autoEquip?.Invoke());

        RectTransform rewardCard = RuntimeUiFactory.CreatePanel(
            "RewardCard",
            panel,
            Panel,
            new Vector2(0.05f, 0.23f),
            new Vector2(0.95f, 0.36f));

        dailyRewardText = RuntimeUiFactory.CreateText(
            "DailyRewardText",
            rewardCard,
            "Daily reward",
            29,
            new Vector2(0.05f, 0.45f),
            new Vector2(0.64f, 0.9f),
            TextAlignmentOptions.Left,
            Color.white);

        Button dailyRewardButton = RuntimeUiFactory.CreateButton(
            "DailyRewardButton",
            rewardCard,
            "CLAIM",
            new Vector2(0.69f, 0.2f),
            new Vector2(0.95f, 0.8f),
            Success,
            () => claimDailyReward?.Invoke());
        DailyRewardBadge = CreateBadge(
            dailyRewardButton,
            "DailyRewardBadge");

        accountText = RuntimeUiFactory.CreateText(
            "AccountText",
            rewardCard,
            "Account",
            22,
            new Vector2(0.05f, 0.05f),
            new Vector2(0.64f, 0.4f),
            TextAlignmentOptions.Left,
            new Color32(174, 189, 214, 255));

        Button questMenuButton = RuntimeUiFactory.CreateButton(
            "QuestButton",
            panel,
            "QUESTS",
            new Vector2(0.06f, 0.13f),
            new Vector2(0.31f, 0.21f),
            Gold,
            () => showQuests?.Invoke());
        QuestMenuBadge = CreateBadge(
            questMenuButton,
            "QuestMenuBadge");

        Button eventMenuButton = RuntimeUiFactory.CreateButton(
            "EventButton",
            panel,
            "EVENT",
            new Vector2(0.36f, 0.13f),
            new Vector2(0.64f, 0.21f),
            Success,
            () => showEvent?.Invoke());
        EventMenuBadge = CreateBadge(
            eventMenuButton,
            "EventMenuBadge");

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

    public void Refresh(
        PlayerData data,
        CompanionManager companionManager,
        AccountLinkManager accounts,
        DailyRewardManager dailyRewards)
    {
        if (data == null)
        {
            inventoryText.text = LocalizationManager.Text(
                "Inventory data unavailable.",
                "인벤토리 정보를 불러올 수 없습니다.");
            companionText.text = string.Empty;
            dailyRewardText.text = string.Empty;
            accountText.text = string.Empty;
            return;
        }

        RefreshInventory(data, companionManager);
        RefreshCompanions(companionManager);
        RefreshAccount(data, accounts);
        RefreshDailyReward(dailyRewards);
    }

    private void RefreshInventory(
        PlayerData data,
        CompanionManager companionManager)
    {
        StringBuilder builder = new StringBuilder();
        if (data.inventory?.items != null)
        {
            foreach (var item in data.inventory.items)
            {
                if (companionManager == null ||
                    !companionManager.IsCharacterItem(item.Key))
                {
                    builder.AppendLine($"{item.Key}   x{item.Value}");
                }
            }
        }

        builder.AppendLine(
            $"{LocalizationManager.Text("Mailbox", "우편함")}   " +
            $"{data.mailbox.Count} " +
            $"{LocalizationManager.Text("waiting", "개 대기 중")}");
        builder.AppendLine(
            $"{LocalizationManager.Text("Monsters defeated", "처치한 몬스터")}   " +
            $"{data.totalMonstersDefeated:N0}");
        inventoryText.text = builder.ToString();
    }

    private void RefreshCompanions(CompanionManager companionManager)
    {
        StringBuilder companionBuilder = new StringBuilder();
        var party = companionManager?.GetEquippedParty();
        if (party == null || party.Count == 0)
        {
            companionBuilder.AppendLine(
                $"{LocalizationManager.Text("Party", "파티")} 0/3");
            companionBuilder.Append(
                LocalizationManager.Text(
                    "Recruit one in Gacha.",
                    "뽑기에서 동료를 획득하세요."));
        }
        else
        {
            int bonus = 0;
            companionBuilder.AppendLine(
                $"{LocalizationManager.Text("PARTY", "파티")} " +
                $"{party.Count}/{CompanionManager.PartySize}");
            for (int i = 0; i < party.Count; i++)
            {
                CharacterData character = party[i];
                if (i > 0)
                    companionBuilder.Append(", ");

                companionBuilder.Append(
                    $"[{character.rarity}] {character.characterName}");
                bonus += CompanionManager.GetAttackBonusPercent(
                    character.rarity);
            }

            companionBuilder.AppendLine();
            companionBuilder.Append(
                $"{LocalizationManager.Text("Team Attack", "팀 공격력")} " +
                $"+{bonus}%");
            CompanionSynergyResult synergy =
                companionManager.GetSynergyResult();
            companionBuilder.AppendLine();
            companionBuilder.Append(synergy.GetSummary());
        }

        companionText.text = companionBuilder.ToString();
    }

    private void RefreshAccount(
        PlayerData data,
        AccountLinkManager accounts)
    {
        string accountType = accounts != null &&
            accounts.IsLinked(AccountLinkProvider.Google)
                ? LocalizationManager.Text(
                    "Linked account",
                    "연동된 계정")
                : LocalizationManager.Text(
                    "Guest account",
                    "게스트 계정");
        accountText.text =
            $"{accountType}  |  " +
            $"{LocalizationManager.Text("Highest", "최고")} " +
            $"{data.highestStage}";
    }

    private void RefreshDailyReward(DailyRewardManager dailyRewards)
    {
        if (dailyRewards == null)
        {
            dailyRewardText.text = LocalizationManager.Text(
                "Daily reward unavailable.",
                "일일 보상을 사용할 수 없습니다.");
            return;
        }

        int day = dailyRewards.GetNextRewardDay();
        dailyRewardText.text = dailyRewards.CanClaimReward()
            ? $"{LocalizationManager.Text("Daily Reward Day", "일일 보상")} " +
              $"{day} " +
              $"{LocalizationManager.Text("is ready", "수령 가능")}"
            : $"{LocalizationManager.Text("Daily Reward Day", "일일 보상")} " +
              $"{day} " +
              $"{LocalizationManager.Text("already claimed", "수령 완료")}";
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
}
