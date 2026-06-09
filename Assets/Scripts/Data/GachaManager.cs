using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public const int PityLimit =
        GameBalanceConfig.GachaPityLimit;

    public CharacterDatabase database;

    public static GachaManager Instance;

    private List<CharacterData> rCharacters = new List<CharacterData>();
    private List<CharacterData> srCharacters = new List<CharacterData>();
    private List<CharacterData> ssrCharacters = new List<CharacterData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeCharacterPoolFromDatabase();
    }

    private void InitializeCharacterPoolFromDatabase()
    {
        if (database == null)
        {
            Debug.LogError("[Gacha] Character database is not assigned.");
            return;
        }

        if (database.characters == null)
        {
            Debug.LogError("[Gacha] Character database list is null.");
            return;
        }

        rCharacters.Clear();
        srCharacters.Clear();
        ssrCharacters.Clear();

        foreach (CharacterData character in database.characters)
        {
            if (character == null)
                continue;

            switch (character.rarity)
            {
                case "SSR":
                    ssrCharacters.Add(character);
                    break;

                case "SR":
                    srCharacters.Add(character);
                    break;

                default:
                    rCharacters.Add(character);
                    break;
            }
        }

        Debug.Log(
            $"R:{rCharacters.Count} " +
            $"SR:{srCharacters.Count} " +
            $"SSR:{ssrCharacters.Count}");
    }


    public CharacterData RollCharacter()
    {
        ValidateRollPools();

        int randomRoll = Random.Range(1, 101); // 1~100

        UnityEngine.Debug.Log(
            $"[Gacha] Random Value: {randomRoll}");

        if (randomRoll <= GachaConfig.SSRRate)
        {
            return GetRandomCharacter(ssrCharacters);
        }
        else if (randomRoll <=
            GachaConfig.SSRRate + GachaConfig.SRRate)
        {
            return GetRandomCharacter(srCharacters);
        }
        else
        {
            return GetRandomCharacter(rCharacters);
        }
    }

    public List<CharacterData> RollTen()
    {
        ValidateRollPools();

        if (srCharacters.Count == 0)
        {
            throw new System.InvalidOperationException(
                "[Gacha] Ten-roll guarantee requires at least one SR.");
        }

        List<CharacterData> results =
            new List<CharacterData>();

        bool hasSRorHigher = false;

        for (int i = 0; i < 10; i++)
        {
            CharacterData result =
                RollCharacterWithPity();

            results.Add(result);

            if (result.rarity != "R")
            {
                hasSRorHigher = true;
            }
        }

        if (!hasSRorHigher)
        {
            results[9] =
                GetRandomCharacter(
                    srCharacters);
        }

        return results;
    }

    private CharacterData GetRandomCharacter(List<CharacterData> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new System.InvalidOperationException(
                "[Gacha] The selected rarity pool is empty.");
        }

        int index = Random.Range(0, list.Count);

        return list[index];
    }

    public CharacterData RollCharacterWithPity()
    {
        ValidateRollPools();

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
        {
            throw new System.InvalidOperationException(
                "[Gacha] PlayerData is not ready.");
        }

        if (data.pityCount >= PityLimit - 1)
        {
            data.pityCount = 0;

            Debug.Log("[Pity] Guaranteed SSR");

            return GetRandomCharacter(ssrCharacters);
        }

        CharacterData result = RollCharacter();
        if (result.rarity == "SSR")
        {
            data.pityCount = 0;
        }
        else
        {
            data.pityCount++;
        }

        return result;
    }

    private void ValidateRollPools()
    {
        int rRate = 100 - GachaConfig.SSRRate - GachaConfig.SRRate;
        if (rRate > 0 && rCharacters.Count == 0)
        {
            throw new System.InvalidOperationException(
                "[Gacha] At least one R character is required.");
        }

        if (GachaConfig.SRRate > 0 && srCharacters.Count == 0)
        {
            throw new System.InvalidOperationException(
                "[Gacha] SR rate is enabled, but the SR pool is empty.");
        }

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if ((GachaConfig.SSRRate > 0 ||
             data != null && data.pityCount >= PityLimit - 1) &&
            ssrCharacters.Count == 0)
        {
            throw new System.InvalidOperationException(
                "[Gacha] SSR can be selected, but the SSR pool is empty.");
        }
    }
}
