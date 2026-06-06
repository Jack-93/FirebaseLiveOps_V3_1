using System;
using System.Collections.Generic;
using UnityEngine;

public class GachaUIManager : MonoBehaviour
{
    public Transform content;
    public GameObject resultSlotPrefab;

    public async void SingleRoll()
    {
        try
        {
            ValidateReferences();
            ClearSlots();

            CharacterData result =
                GachaManager.Instance.RollCharacterWithPity();

            InventoryManager.Instance.AddItem(
                result.characterName,
                1,
                false);

            await FirestoreManager.Instance.SavePlayerDataAsync(
                PlayerDataManager.Instance.playerData);

            LogResult(result);
            CreateSlot(result);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async void TenRoll()
    {
        try
        {
            ValidateReferences();
            ClearSlots();

            List<CharacterData> results =
                GachaManager.Instance.RollTen();

            foreach (CharacterData result in results)
            {
                InventoryManager.Instance.AddItem(
                    result.characterName,
                    1,
                    false);
            }

            await FirestoreManager.Instance.SavePlayerDataAsync(
                PlayerDataManager.Instance.playerData);

            foreach (CharacterData result in results)
            {
                LogResult(result);
                CreateSlot(result);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static void LogResult(CharacterData result)
    {
        AnalyticsManager.Instance?.LogGachaRoll(result);

        if (result.rarity == "SSR")
            AnalyticsManager.Instance?.LogSSR(result);
    }

    private void CreateSlot(CharacterData result)
    {
        GameObject slot = Instantiate(
            resultSlotPrefab,
            content,
            false);

        ResultSlotUIManager slotUI =
            slot.GetComponent<ResultSlotUIManager>();

        if (slotUI == null)
        {
            Destroy(slot);
            throw new InvalidOperationException(
                "[GachaUI] ResultSlot prefab has no ResultSlotUIManager.");
        }

        slotUI.Setup(result);
    }

    private void ClearSlots()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);
    }

    private void ValidateReferences()
    {
        if (GachaManager.Instance == null)
            throw new InvalidOperationException("[GachaUI] GachaManager is missing.");

        if (InventoryManager.Instance == null)
            throw new InvalidOperationException("[GachaUI] InventoryManager is missing.");

        if (FirestoreManager.Instance == null)
            throw new InvalidOperationException("[GachaUI] FirestoreManager is missing.");

        if (content == null)
            throw new InvalidOperationException("[GachaUI] Content is not assigned.");

        if (resultSlotPrefab == null)
        {
            throw new InvalidOperationException(
                "[GachaUI] ResultSlot prefab is not assigned.");
        }
    }
}
