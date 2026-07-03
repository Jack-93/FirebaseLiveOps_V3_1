using System;
using System.Collections.Generic;

public sealed class CompanionActionController
{
    private readonly CompanionManager companionManager;
    private readonly BattleManager battleManager;
    private readonly MainGameBootstrap bootstrap;
    private readonly Action<string> showToast;
    private readonly Action refreshCollection;
    private readonly Action refreshBattle;

    private CharacterData selectedCharacter;
    private int pendingEquipSlot = -1;

    public CompanionActionController(
        CompanionManager companionManager,
        BattleManager battleManager,
        MainGameBootstrap bootstrap,
        Action<string> showToast,
        Action refreshCollection,
        Action refreshBattle)
    {
        this.companionManager = companionManager;
        this.battleManager = battleManager;
        this.bootstrap = bootstrap;
        this.showToast = showToast;
        this.refreshCollection = refreshCollection;
        this.refreshBattle = refreshBattle;
    }

    public CharacterData SelectedCharacter => selectedCharacter;

    public void EnsureSelected()
    {
        if (companionManager == null)
            return;

        if (selectedCharacter != null &&
            companionManager.GetOwnedCount(
                selectedCharacter.characterName) > 0)
        {
            return;
        }

        List<CharacterData> ownedCharacters =
            companionManager.GetOwnedCharacters();
        selectedCharacter = ownedCharacters.Count > 0
            ? ownedCharacters[0]
            : null;
    }

    public void Select(CharacterData character)
    {
        if (character != null &&
            companionManager != null &&
            companionManager.GetOwnedCount(character.characterName) <= 0)
        {
            showToast?.Invoke("Recruit this companion first.");
            return;
        }

        selectedCharacter = character;

        if (pendingEquipSlot >= 0)
        {
            EquipSelectedToPendingSlot();
            return;
        }

        refreshCollection?.Invoke();
    }

    public void PromoteSelected()
    {
        if (selectedCharacter == null || companionManager == null)
            return;

        if (!companionManager.TryPromote(selectedCharacter))
        {
            showToast?.Invoke("Not enough duplicate copies.");
            return;
        }

        RefreshPlayerData();
        refreshCollection?.Invoke();
        showToast?.Invoke(
            $"{selectedCharacter.characterName} promoted.");
    }

    public void AutoEquip()
    {
        if (companionManager == null)
            return;

        bool changed = companionManager.TryEquipBestOwned(
            out CharacterData equipped);
        if (!changed && equipped == null)
        {
            showToast?.Invoke("Recruit a companion first.");
            return;
        }

        ApplyCompanionSelection(equipped);
    }

    public void ToggleSelectedSlot(int slotIndex)
    {
        if (companionManager == null)
            return;

        pendingEquipSlot = slotIndex;
        CharacterData equipped =
            companionManager.GetEquippedAtSlot(slotIndex);
        selectedCharacter = equipped ?? selectedCharacter;
        refreshCollection?.Invoke();
        showToast?.Invoke(
            equipped == null
                ? $"{slotIndex + 1}번 슬롯: 장착할 동료를 선택하세요."
                : $"{slotIndex + 1}번 슬롯: 교체할 동료를 선택하세요.");
    }

    private void EquipSelectedToPendingSlot()
    {
        if (selectedCharacter == null ||
            companionManager == null ||
            pendingEquipSlot < 0)
        {
            return;
        }

        int targetSlot = pendingEquipSlot;
        pendingEquipSlot = -1;

        bool changed = companionManager.TryEquipToSlot(
            selectedCharacter,
            targetSlot);
        if (!changed)
        {
            showToast?.Invoke("This companion is not owned.");
            return;
        }

        RefreshPlayerData();
        refreshCollection?.Invoke();
        refreshBattle?.Invoke();
        showToast?.Invoke(
            $"{selectedCharacter.characterName} {targetSlot + 1}번 슬롯 장착.");
    }

    private void ApplyCompanionSelection(CharacterData equipped)
    {
        if (equipped == null)
        {
            showToast?.Invoke("Recruit a companion first.");
            return;
        }

        RefreshPlayerData();
        int bonus =
            CompanionManager.GetAttackBonusPercent(
                equipped.rarity,
                companionManager.GetStars(equipped.characterName));
        selectedCharacter = equipped;
        refreshBattle?.Invoke();
        refreshCollection?.Invoke();
        showToast?.Invoke(
            $"{equipped.characterName} equipped. Attack " +
            $"{CompactNumberFormatter.Format(bonus, "+")}%.");
    }

    private void RefreshPlayerData()
    {
        PlayerDataManager.Instance?.NotifyPlayerDataChanged(true);
        if (bootstrap != null)
            _ = bootstrap.SaveNowAsync();

        try
        {
            battleManager?.RefreshPlayerStats();
        }
        catch (Exception exception)
        {
            showToast?.Invoke("Companion saved. Battle refresh failed.");
            UnityEngine.Debug.LogException(exception);
        }
    }
}
