using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public CharacterDatabase database;

    public static GachaManager Instance;

    // 媛??깃툒蹂?罹먮┃???
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

        //initializeCharacterPool();
        //// 罹먮┃??? 珥덇린??
        //rCharacterPool.Add(new CharacterData("R_Character1", "R"));
        //rCharacterPool.Add(new CharacterData("R_Character2", "R"));
        //srCharacterPool.Add(new CharacterData("SR_Character1", "SR"));
        //srCharacterPool.Add(new CharacterData("SR_Character2", "SR"));
        //ssrCharacterPool.Add(new CharacterData("SSR_Character1", "SSR"));
        //ssrCharacterPool.Add(new CharacterData("SSR_Character2", "SSR"));
    }
    /*
    private void initializeCharacterPool()
    {
        // ?ㅼ젣 寃뚯엫?먯꽌?????곗씠?곕? ?몃??먯꽌 遺덈윭?????덈룄濡??쒖옉
        // 罹먮┃???곗씠???먯뀑 異붽? ScriptableObject
         
        rCharacters.Add(new CharacterData("R_Character1", "R"));
        rCharacters.Add(new CharacterData("R_Character2", "R"));

        srCharacters.Add(new CharacterData("SR_Character1", "SR"));
        srCharacters.Add(new CharacterData("SR_Character2", "SR"));

        ssrCharacters.Add(new CharacterData("Scarlet", "SSR"));
        ssrCharacters.Add(new CharacterData("Modernia", "SSR"));
    }
    */

    // 罹먮┃???곗씠???먯뀑 異붽? ScriptableObject
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


    // ?뺣쪧 -> RemoteConfig?먯꽌 ?섏튂 愿由?
    public CharacterData RollCharacter()
    {
        ValidateRollPools();

        int randomRoll = Random.Range(1, 101); // 1~100

        UnityEngine.Debug.Log(
            $"[Gacha] Random Value: {randomRoll}");

        if (randomRoll <= GachaConfig.SSRRate)
        {
            // return ssrCharacterPool[Random.Range(0, ssrCharacterPool.Count)];
            return GetRandomCharacter(ssrCharacters);
        }
        else if (randomRoll <=
            GachaConfig.SSRRate + GachaConfig.SRRate)
        {
            //return srCharacterPool[Random.Range(0, srCharacterPool.Count)];
            return GetRandomCharacter(srCharacters);
        }
        else // R 80%
        {
            //return rCharacterPool[Random.Range(0, rCharacterPool.Count)];
            return GetRandomCharacter(rCharacters);
        }
    }

    // 10?곗감 戮묎린 ?대옒??
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

    // 泥쒖옣 ?쒖뒪???대옒??
    public CharacterData RollCharacterWithPity()
    {
        ValidateRollPools();

        PlayerData data = PlayerDataManager.Instance?.playerData;
        if (data == null)
        {
            throw new System.InvalidOperationException(
                "[Gacha] PlayerData is not ready.");
        }

        if (data.pityCount >= 99)
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
             data != null && data.pityCount >= 99) &&
            ssrCharacters.Count == 0)
        {
            throw new System.InvalidOperationException(
                "[Gacha] SSR can be selected, but the SSR pool is empty.");
        }
    }
}
