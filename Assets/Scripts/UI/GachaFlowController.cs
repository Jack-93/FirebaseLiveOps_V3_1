using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GachaFlowController
{
    private readonly GachaPanelUI panel;
    private readonly CompanionManager companionManager;
    private readonly Action refreshGacha;

    private bool isRolling;
    private bool isResultVisible;

    public GachaFlowController(
        GachaPanelUI panel,
        CompanionManager companionManager,
        Action refreshGacha)
    {
        this.panel = panel;
        this.companionManager = companionManager;
        this.refreshGacha = refreshGacha;
    }

    public async void Roll(int count)
    {
        if (isRolling)
            return;

        if (GachaManager.Instance == null ||
            PlayerDataManager.Instance?.playerData == null ||
            InventoryManager.Instance == null)
        {
            panel?.SetStatus(
                LocalizationManager.Text(
                    "Game data is not ready.",
                    "Game data is not ready."));
            return;
        }

        PlayerData data = PlayerDataManager.Instance.playerData;
        if (!GachaEconomy.TrySpend(
                data,
                count,
                out GachaPayment payment))
        {
            panel?.SetStatus(
                count == 1
                    ? LocalizationManager.Text(
                        "Need 1 Ticket or 100 Gems.",
                        $"Need 1 Ticket or {GachaEconomy.SingleGemCost} Gems.")
                    : LocalizationManager.Text(
                        "Need 10 Tickets or 900 Gems.",
                        $"Need 10 Tickets or {GachaEconomy.TenGemCost} Gems."));
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
                AnalyticsManager.Instance?.LogGachaRoll(character);

                if (character.rarity == "SSR")
                    AnalyticsManager.Instance?.LogSSR(character);
            }

            panel?.ShowResults(results, ownedBefore);
            SetResultMode(panel?.IsResultVisible == true);
            panel?.SetStatus(
                payment.UsedTickets
                    ? $"{LocalizationManager.Text("Used", "Used")} " +
                      $"{payment.Amount} " +
                      $"{LocalizationManager.Text("ticket(s).", "ticket(s).")}"
                    : $"{LocalizationManager.Text("Used", "Used")} " +
                      $"{payment.Amount:N0} " +
                      $"{LocalizationManager.Text("Gems.", "Gems.")}");

            QuestManager.Instance?.ReportGacha(results.Count);
            EventMissionManager.Instance?.ReportGacha(results.Count);

            if (FirestoreManager.Instance != null)
                await FirestoreManager.Instance.SavePlayerDataAsync(data);

            PlayerDataManager.Instance.NotifyPlayerDataChanged();
            refreshGacha?.Invoke();
        }
        catch (Exception exception)
        {
            GachaEconomy.Refund(data, payment);
            panel?.SetStatus(
                LocalizationManager.Text(
                    "Recruitment failed. Cost refunded.",
                    "Recruitment failed. Cost refunded."));
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
        SetResultMode(false);
        panel?.ClearResult(
            LocalizationManager.Text(
                "Tickets are used before Gems.",
                "Tickets are used before Gems."));
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
}
