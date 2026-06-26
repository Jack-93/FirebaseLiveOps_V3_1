using System.Text;

public static class CollectionDetailFormatter
{
    public static string SelectionPrompt =>
        LocalizationManager.Translate("Select a companion.");

    public static string Format(
        CharacterData selectedCharacter,
        CompanionManager companionManager)
    {
        int owned = companionManager.GetOwnedCount(
            selectedCharacter.characterName);
        int stars =
            companionManager.GetStars(selectedCharacter.characterName);
        int bonus =
            CompanionManager.GetAttackBonusPercent(
                selectedCharacter.rarity,
                stars);
        int promotionCost =
            companionManager.GetPromotionCost(
                selectedCharacter.characterName);
        bool equippedSelected = IsCharacterEquipped(
            selectedCharacter,
            companionManager);
        bool canPromote =
            owned > 0 &&
            stars < 5 &&
            owned - 1 >= promotionCost &&
            promotionCost > 0;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(
            $"[{selectedCharacter.rarity}] " +
            $"{selectedCharacter.characterName}");
        builder.AppendLine(
            FormatOwnership(owned, equippedSelected));
        builder.AppendLine(
            $"{LocalizationManager.Translate("Stars")} {stars}/5  |  " +
            $"{LocalizationManager.Translate("Attack")} +{bonus}%");
        builder.AppendLine(
            $"{selectedCharacter.element} / {selectedCharacter.role}");

        if (owned > 0 && stars < 5)
        {
            builder.AppendLine(
                canPromote
                    ? LocalizationManager.Translate("Promotion ready.")
                    : $"{LocalizationManager.Translate("Promotion needs")} " +
                      $"{promotionCost} " +
                      $"{LocalizationManager.Translate("duplicate(s)")}");
        }

        builder.AppendLine(selectedCharacter.description);
        builder.Append(FormatParty(companionManager));
        return builder.ToString();
    }

    private static string FormatOwnership(
        int owned,
        bool equippedSelected)
    {
        if (owned <= 0)
            return LocalizationManager.Translate(
                "LOCKED - recruit from Gacha");

        string state = equippedSelected
            ? LocalizationManager.Translate("EQUIPPED")
            : LocalizationManager.Translate("READY");
        return $"{LocalizationManager.Translate("Owned")} x{owned}  |  " +
               state;
    }

    private static string FormatParty(CompanionManager companionManager)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(LocalizationManager.Translate("Party") + ": ");

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            if (slot > 0)
                builder.Append("  |  ");

            CharacterData equipped =
                companionManager.GetEquippedAtSlot(slot);
            string equippedName = equipped == null
                ? LocalizationManager.Translate("EMPTY")
                : equipped.characterName;
            builder.Append($"{slot + 1}. {equippedName}");
        }

        return builder.ToString();
    }

    private static bool IsCharacterEquipped(
        CharacterData character,
        CompanionManager companionManager)
    {
        if (character == null || companionManager == null)
            return false;

        for (int slot = 0; slot < CompanionManager.PartySize; slot++)
        {
            CharacterData equipped =
                companionManager.GetEquippedAtSlot(slot);
            if (equipped == null)
                continue;

            if (equipped == character ||
                equipped.characterName == character.characterName)
            {
                return true;
            }
        }

        return false;
    }
}
