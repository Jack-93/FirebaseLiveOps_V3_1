using System;
using System.Collections.Generic;
using UnityEngine;

public class CompanionManager : MonoBehaviour
{
    public const int PartySize = 3;

    public static CompanionManager Instance;

    public event Action OnCompanionChanged;

    private CharacterDatabase database;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadDatabase();
    }

    public bool Initialize()
    {
        LoadDatabase();

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
            return false;

        data.EnsureInitialized();

        bool changed = RemoveInvalidPartyMembers(data);
        if (HasEquippedCompanion(data))
            return changed;

        return TryEquipBestOwned(out _);
    }

    public bool TryEquipBestOwned(out CharacterData equipped)
    {
        equipped = null;
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || database?.characters == null)
            return false;

        List<CharacterData> owned = GetOwnedCharacters();
        if (owned.Count == 0)
            return false;

        equipped = owned[0];
        return EquipParty(data, owned);
    }

    public bool TryEquipToSlot(CharacterData character, int slotIndex)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            character == null ||
            slotIndex < 0 ||
            slotIndex >= PartySize ||
            GetOwnedCount(character.characterName) <= 0)
        {
            return false;
        }

        data.EnsureInitialized();

        for (int index = 0; index < PartySize; index++)
        {
            if (data.equippedCompanions[index] ==
                character.characterName)
            {
                data.equippedCompanions[index] = "";
                data.equippedCompanionRarities[index] = "";
            }
        }

        data.equippedCompanions[slotIndex] = character.characterName;
        data.equippedCompanionRarities[slotIndex] = character.rarity;
        SyncLegacyEquipment(data);
        OnCompanionChanged?.Invoke();
        return true;
    }

    public bool TryUnequipSlot(int slotIndex)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null ||
            slotIndex < 0 ||
            slotIndex >= PartySize)
        {
            return false;
        }

        data.EnsureInitialized();
        if (string.IsNullOrEmpty(data.equippedCompanions[slotIndex]))
            return false;

        data.equippedCompanions[slotIndex] = "";
        data.equippedCompanionRarities[slotIndex] = "";
        SyncLegacyEquipment(data);
        OnCompanionChanged?.Invoke();
        return true;
    }

    public CharacterData GetEquippedAtSlot(int slotIndex)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data?.equippedCompanions == null ||
            slotIndex < 0 ||
            slotIndex >= data.equippedCompanions.Count)
        {
            return null;
        }

        return FindCharacter(data.equippedCompanions[slotIndex]);
    }

    public CharacterData GetEquipped()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        return data == null
            ? null
            : FindCharacter(data.equippedCompanion);
    }

    public List<CharacterData> GetEquippedParty()
    {
        List<CharacterData> party = new List<CharacterData>();
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data?.equippedCompanions == null)
            return party;

        foreach (string characterName in data.equippedCompanions)
        {
            CharacterData character = FindCharacter(characterName);
            if (character != null)
                party.Add(character);
        }

        return party;
    }

    public CompanionSynergyResult GetSynergyResult()
    {
        return CompanionSynergySystem.Calculate(GetEquippedParty());
    }

    public List<CharacterData> GetOwnedCharacters()
    {
        List<CharacterData> owned = new List<CharacterData>();
        if (database?.characters == null)
            return owned;

        foreach (CharacterData character in database.characters)
        {
            if (character != null &&
                GetOwnedCount(character.characterName) > 0)
            {
                owned.Add(character);
            }
        }

        owned.Sort((left, right) =>
        {
            int rarityComparison =
                GetRarityRank(right.rarity)
                .CompareTo(GetRarityRank(left.rarity));
            return rarityComparison != 0
                ? rarityComparison
                : string.Compare(
                    left.characterName,
                    right.characterName,
                    StringComparison.Ordinal);
        });

        return owned;
    }

    public List<CharacterData> GetAllCharacters()
    {
        return database?.characters == null
            ? new List<CharacterData>()
            : new List<CharacterData>(database.characters);
    }

    public int GetOwnedCount(string characterName)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data?.inventory?.items == null ||
            string.IsNullOrEmpty(characterName))
        {
            return 0;
        }

        return data.inventory.items.TryGetValue(
            characterName,
            out int count)
            ? Math.Max(0, count)
            : 0;
    }

    public bool IsCharacterItem(string itemName)
    {
        return FindCharacter(itemName) != null;
    }

    public static int GetAttackBonusPercent(string rarity)
    {
        switch (rarity)
        {
            case "SSR":
                return 50;
            case "SR":
                return 25;
            case "R":
                return 10;
            default:
                return 0;
        }
    }

    public static int GetAttackBonusPercent(
        string rarity,
        int stars)
    {
        return GetAttackBonusPercent(rarity) +
            Math.Max(0, stars - 1) * 5;
    }

    public int GetStars(string characterName)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || GetOwnedCount(characterName) <= 0)
            return 0;

        return data.companionStars.TryGetValue(
            characterName,
            out int stars)
            ? Mathf.Clamp(stars, 1, 5)
            : 1;
    }

    public int GetPromotionCost(string characterName)
    {
        int stars = GetStars(characterName);
        return stars <= 0 || stars >= 5 ? 0 : stars;
    }

    public bool TryPromote(CharacterData character)
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null || character == null)
            return false;

        int stars = GetStars(character.characterName);
        int cost = GetPromotionCost(character.characterName);
        int owned = GetOwnedCount(character.characterName);
        if (stars <= 0 || stars >= 5 || owned - 1 < cost)
            return false;

        data.inventory.items[character.characterName] = owned - cost;
        data.companionStars[character.characterName] = stars + 1;
        OnCompanionChanged?.Invoke();
        return true;
    }

    private void LoadDatabase()
    {
        if (database == null)
            database = Resources.Load<CharacterDatabase>(
                "CharacterDatabase");

        if (database == null)
        {
            Debug.LogError(
                "[Companion] Resources/CharacterDatabase is missing.");
        }
    }

    private CharacterData FindCharacter(string characterName)
    {
        if (string.IsNullOrEmpty(characterName) ||
            database?.characters == null)
        {
            return null;
        }

        return database.characters.Find(
            character =>
                character != null &&
                character.characterName == characterName);
    }

    private bool EquipParty(
        PlayerData data,
        List<CharacterData> characters)
    {
        List<string> names = new List<string> { "", "", "" };
        List<string> rarities = new List<string> { "", "", "" };
        for (int index = 0;
             index < characters.Count && index < PartySize;
             index++)
        {
            names[index] = characters[index].characterName;
            rarities[index] = characters[index].rarity;
        }

        bool changed = !ListsEqual(data.equippedCompanions, names);
        data.equippedCompanions = names;
        data.equippedCompanionRarities = rarities;
        SyncLegacyEquipment(data);

        if (changed)
            OnCompanionChanged?.Invoke();

        return changed;
    }

    private bool RemoveInvalidPartyMembers(PlayerData data)
    {
        bool changed = false;
        HashSet<string> used = new HashSet<string>();
        for (int index = 0; index < PartySize; index++)
        {
            string characterName = data.equippedCompanions[index];
            CharacterData character = FindCharacter(characterName);
            if (character == null ||
                GetOwnedCount(characterName) <= 0 ||
                !used.Add(characterName))
            {
                changed |= !string.IsNullOrEmpty(characterName);
                data.equippedCompanions[index] = "";
                data.equippedCompanionRarities[index] = "";
                continue;
            }

            data.equippedCompanionRarities[index] = character.rarity;
        }

        SyncLegacyEquipment(data);
        return changed;
    }

    private static bool HasEquippedCompanion(PlayerData data)
    {
        return data.equippedCompanions.Exists(
            name => !string.IsNullOrEmpty(name));
    }

    private static void SyncLegacyEquipment(PlayerData data)
    {
        int firstIndex = data.equippedCompanions.FindIndex(
            name => !string.IsNullOrEmpty(name));
        data.equippedCompanion = firstIndex >= 0
            ? data.equippedCompanions[firstIndex]
            : "";
        data.equippedCompanionRarity = firstIndex >= 0
            ? data.equippedCompanionRarities[firstIndex]
            : "";
    }

    private static bool ListsEqual(
        List<string> left,
        List<string> right)
    {
        if (left == null || left.Count != right.Count)
            return false;

        for (int index = 0; index < left.Count; index++)
        {
            if (left[index] != right[index])
                return false;
        }

        return true;
    }

    private static int GetRarityRank(string rarity)
    {
        switch (rarity)
        {
            case "SSR":
                return 3;
            case "SR":
                return 2;
            case "R":
                return 1;
            default:
                return 0;
        }
    }
}
