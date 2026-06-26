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
        if (selectedCharacter != null || companionManager == null)
            return;

        List<CharacterData> characters =
            companionManager.GetAllCharacters();
        if (characters.Count > 0)
            selectedCharacter = characters[0];
    }

    public void Select(CharacterData character)
    {
        selectedCharacter = character;
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
        if (selectedCharacter == null || companionManager == null)
            return;

        CharacterData equipped =
            companionManager.GetEquippedAtSlot(slotIndex);
        bool selectedInSlot =
            equipped != null &&
            equipped.characterName == selectedCharacter.characterName;
        bool changed = selectedInSlot
            ? companionManager.TryUnequipSlot(slotIndex)
            : companionManager.TryEquipToSlot(
                selectedCharacter,
                slotIndex);

        if (!changed)
        {
            showToast?.Invoke("This companion is not owned.");
            return;
        }

        RefreshPlayerData();
        refreshCollection?.Invoke();
        showToast?.Invoke(
            selectedInSlot
                ? $"{selectedCharacter.characterName} removed."
                : $"{selectedCharacter.characterName} set to slot " +
                  $"{slotIndex + 1}.");
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
            $"{equipped.characterName} equipped. Attack +{bonus}%.");
    }

    private void RefreshPlayerData()
    {
        battleManager?.RefreshPlayerStats();
        PlayerDataManager.Instance?.NotifyPlayerDataChanged();
        if (bootstrap != null)
            _ = bootstrap.SaveNowAsync();
    }
}
