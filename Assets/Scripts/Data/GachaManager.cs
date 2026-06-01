using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance;

    // 각 등급별 캐릭터 풀
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

        initializeCharacterPool();
        //// 캐릭터 풀 초기화
        //rCharacterPool.Add(new CharacterData("R_Character1", "R"));
        //rCharacterPool.Add(new CharacterData("R_Character2", "R"));
        //srCharacterPool.Add(new CharacterData("SR_Character1", "SR"));
        //srCharacterPool.Add(new CharacterData("SR_Character2", "SR"));
        //ssrCharacterPool.Add(new CharacterData("SSR_Character1", "SSR"));
        //ssrCharacterPool.Add(new CharacterData("SSR_Character2", "SSR"));
    }

    private void initializeCharacterPool()
    {
        // 실제 게임에서는 이 데이터를 외부에서 불러올 수 있도록 제작
        rCharacters.Add(new CharacterData("R_Character1", "R"));
        rCharacters.Add(new CharacterData("R_Character2", "R"));

        srCharacters.Add(new CharacterData("SR_Character1", "SR"));
        srCharacters.Add(new CharacterData("SR_Character2", "SR"));

        ssrCharacters.Add(new CharacterData("SSR_Character1", "SSR"));
        ssrCharacters.Add(new CharacterData("SSR_Character2", "SSR"));
    }

    // 확률 -> RemoteConfig에서 수치 관리
    public CharacterData RollCharacter()
    {
        int randomRoll = Random.Range(1, 101); // 1~100

        UnityEngine.Debug.Log(
            $"[Gacha] Random Value: {randomRoll}");

        if (randomRoll < GachaConfig.SSRRate)
        {
            // return ssrCharacterPool[Random.Range(0, ssrCharacterPool.Count)];
            return GetRandomCharacter(ssrCharacters);
        }
        else if (randomRoll < GachaConfig.SSRRate + GachaConfig.SRRate)
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

    // 10연차 뽑기 클래스
    public List<CharacterData> RollTen()
    {
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
        int index = Random.Range(1, list.Count);

        return list[index];
    }

    // 천장 시스템 클래스
    public CharacterData RollCharacterWithPity()
    {
        PlayerData data =
            PlayerDataManager.Instance.playerData;

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

}
