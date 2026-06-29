using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GachaFlowController
{
    private readonly GachaPanelUI panel;
    private readonly CompanionManager companionManager;
    private readonly Action refreshGacha;
    private readonly Action showBattle;

    private bool isRolling;
    private bool isResultVisible;
    private bool returnToBattleAfterResult;
    private Action externalResultCleared;

    public GachaFlowController(
        GachaPanelUI panel,
        CompanionManager companionManager,
        Action refreshGacha,
        Action showBattle = null)
    {
        this.panel = panel;
        this.companionManager = companionManager;
        this.refreshGacha = refreshGacha;
        this.showBattle = showBattle;
    }

    public void Roll(int count)
    {
        if (isRolling || isResultVisible)
            return;

        if (count != 1 && count != 10)
        {
            panel?.SetStatus(
                LocalizationManager.Translate(
                    "Invalid recruitment count."));
            return;
        }

        if (GachaManager.Instance == null ||
            PlayerDataManager.Instance?.playerData == null ||
            InventoryManager.Instance == null)
        {
            panel?.SetStatus(
                LocalizationManager.Translate("Game data is not ready."));
            return;
        }

        PlayerData data = PlayerDataManager.Instance.playerData;
        TutorialManager tutorial = TutorialManager.Instance;
        if (tutorial?.IsWaitingForTutorialGacha == true)
        {
            if (count != TutorialManager.TutorialGachaTicketCount)
            {
                panel?.SetStatus(
                    LocalizationManager.Text(
                        "Use the 10 tickets for 10x recruitment.",
                        "\uBC1B\uC740 \uD2F0\uCF13 10\uC7A5\uC73C\uB85C 10\uD68C \uBAA8\uC9D1\uC744 \uB20C\uB7EC\uC8FC\uC138\uC694."));
                return;
            }

            if (GachaEconomy.GetItemCount(data, "GachaTicket") <
                TutorialManager.TutorialGachaTicketCount)
            {
                panel?.SetStatus(
                    LocalizationManager.Text(
                        "10 tickets are required for tutorial recruitment.",
                        "\uD29C\uD1A0\uB9AC\uC5BC \uBAA8\uC9D1\uC5D0 \uD2F0\uCF13 10\uC7A5\uC774 \uD544\uC694\uD569\uB2C8\uB2E4."));
                return;
            }
        }

        GachaRollSnapshot snapshot = GachaRollSnapshot.Capture(data);
        if (!GachaEconomy.TrySpend(
                data,
                count,
                out GachaPayment payment))
        {
            panel?.SetStatus(
                count == 1
                    ? LocalizationManager.Translate(
                        "Need 1 Ticket or 100 Gems.")
                    : LocalizationManager.Translate(
                        "Need 10 Tickets or 900 Gems."));
            return;
        }

        isRolling = true;
        SetButtonsInteractable(false);

        try
        {
            List<CharacterData> results = count == 1
                ? new List<CharacterData>
                {
                    GachaManager.Instance.RollCharacterWithPity()
                }
                : GachaManager.Instance.RollTen();

            Dictionary<string, int> ownedBefore = GetOwnedCounts(results);
            foreach (CharacterData character in results)
            {
                InventoryManager.Instance.AddItem(
                    character.characterName,
                    1,
                    false);
                LogRollAnalytics(character);
            }

            panel?.ShowResults(results, ownedBefore);
            SetResultMode(panel?.IsResultVisible == true);
            panel?.SetStatus(
                payment.UsedTickets
                    ? $"{LocalizationManager.Translate("Used")} " +
                      $"{CompactNumberFormatter.Format(payment.Amount)} " +
                      $"{LocalizationManager.Translate("ticket(s).")}"
                    : $"{LocalizationManager.Translate("Used")} " +
                      $"{CompactNumberFormatter.Format(payment.Amount)} " +
                      $"{LocalizationManager.Translate("Gems.")}");

            ReportGachaProgress(results.Count);
            bool completedTutorialGacha =
                tutorial?.TryCompleteTutorialGacha(count) == true;
            if (completedTutorialGacha)
                returnToBattleAfterResult = true;

            PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
            refreshGacha?.Invoke();
        }
        catch (Exception exception)
        {
            snapshot.Restore(data);
            PlayerDataManager.Instance.NotifyPlayerDataChanged(true);
            panel?.SetStatus(
                LocalizationManager.Translate(
                    "Recruitment failed. State restored."));
            refreshGacha?.Invoke();
            Debug.LogException(exception);
        }
        finally
        {
            isRolling = false;
            SetButtonsInteractable(!isResultVisible);
        }
    }

    public void ClearResult()
    {
        bool shouldReturnToBattle = returnToBattleAfterResult;
        Action clearExternalResult = externalResultCleared;
        returnToBattleAfterResult = false;
        externalResultCleared = null;

        SetResultMode(false);
        panel?.ClearResult(
            LocalizationManager.Translate(
                "Tickets are used before Gems."));
        clearExternalResult?.Invoke();

        if (shouldReturnToBattle)
            showBattle?.Invoke();
    }

    public bool ShowExternalResults(
        List<CharacterData> results,
        Dictionary<string, int> ownedBefore,
        string status,
        bool returnToBattleAfterConfirm,
        Action onResultCleared = null)
    {
        if (results == null || results.Count == 0)
            return false;

        isRolling = false;
        returnToBattleAfterResult = returnToBattleAfterConfirm;
        panel?.ShowResults(results, ownedBefore);
        SetResultMode(panel?.IsResultVisible == true);
        externalResultCleared = isResultVisible ? onResultCleared : null;
        panel?.SetStatus(status);
        refreshGacha?.Invoke();
        return isResultVisible;
    }

    private Dictionary<string, int> GetOwnedCounts(
        List<CharacterData> results)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        if (results == null || companionManager == null)
            return counts;

        foreach (CharacterData character in results)
        {
            if (character == null ||
                counts.ContainsKey(character.characterName))
            {
                continue;
            }

            counts[character.characterName] =
                companionManager.GetOwnedCount(character.characterName);
        }

        return counts;
    }

    private static void LogRollAnalytics(CharacterData character)
    {
        try
        {
            AnalyticsManager.Instance?.LogGachaRoll(character);

            if (character.rarity == "SSR")
                AnalyticsManager.Instance?.LogSSR(character);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[Gacha] Analytics logging failed: " +
                exception.Message);
        }
    }

    private static void ReportGachaProgress(int count)
    {
        try
        {
            QuestManager.Instance?.ReportGacha(count);
            EventMissionManager.Instance?.ReportGacha(count);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[Gacha] Progress report failed: " +
                exception.Message);
        }
    }

    private void SetResultMode(bool visible)
    {
        isResultVisible = visible;
        panel?.SetResultMode(visible, isRolling);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        panel?.SetButtonsInteractable(
            interactable && !isResultVisible && !isRolling);
    }

    private sealed class GachaRollSnapshot
    {
        private readonly int pityCount;
        private readonly Dictionary<string, int> inventoryItems;

        private GachaRollSnapshot(PlayerData data)
        {
            pityCount = data.pityCount;
            inventoryItems =
                new Dictionary<string, int>(data.inventory.items);
        }

        public static GachaRollSnapshot Capture(PlayerData data)
        {
            data.EnsureInitialized();
            return new GachaRollSnapshot(data);
        }

        public void Restore(PlayerData data)
        {
            if (data == null)
                return;

            data.EnsureInitialized();
            data.pityCount = pityCount;
            data.inventory.items =
                new Dictionary<string, int>(inventoryItems);
        }
    }
}
