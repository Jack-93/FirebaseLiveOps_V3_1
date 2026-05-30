using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance;

    // 각 등급별 캐릭터 풀
    private List<CharacterData> rCharacterPool = new List<CharacterData>();
    private List<CharacterData> srCharacterPool = new List<CharacterData>();
    private List<CharacterData> ssrCharacterPool = new List<CharacterData>();

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
    
    // 캐릭터 풀
    private void initializeCharacterPool()
    {
        // 실제 게임에서는 이 데이터를 외부에서 불러올 수 있도록 구현
        rCharacterPool.Add(new CharacterData("R_Character1", "R"));
        rCharacterPool.Add(new CharacterData("R_Character2", "R"));

        srCharacterPool.Add(new CharacterData("SR_Character1", "SR"));
        srCharacterPool.Add(new CharacterData("SR_Character2", "SR"));

        ssrCharacterPool.Add(new CharacterData("SSR_Character1", "SSR"));
        ssrCharacterPool.Add(new CharacterData("SSR_Character2", "SSR"));
    }

    public CharacterData RollCharacter()
    {
        int randomRoll = Random.Range(1, 101); // 1~100

        UnityEngine.Debug.Log(
            $"[Gacha] Random Value: {randomRoll}");

        if (randomRoll <= 5) // SSR 5%
        {
            // return ssrCharacterPool[Random.Range(0, ssrCharacterPool.Count)];
            return GetRandomRoll(ssrCharacterPool);
        }
        else if (randomRoll <= 20) // SR 15%
        {
            //return srCharacterPool[Random.Range(0, srCharacterPool.Count)];
            return GetRandomRoll(srCharacterPool);
        }
        else // R 80%
        {
            //return rCharacterPool[Random.Range(0, rCharacterPool.Count)];
            return GetRandomRoll(rCharacterPool);
        }
    }

    private CharacterData GetRandomRoll(List<CharacterData> list)
    {
        // r, sr, ssr ( r 몇개, sr 몇개, ssr 몇개 ... )
        int index = Random.Range(0, list.Count);

        return list[index];
    }

}
