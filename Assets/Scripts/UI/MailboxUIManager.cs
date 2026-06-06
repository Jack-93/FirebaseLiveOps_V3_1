using System;
using UnityEngine;

public class MailboxUIManager : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject mailSlotPrefab;

    private void Start()
    {
        RefreshUI();

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataChanged += RefreshUI;
        }
    }

    public void RefreshUI()
    {
        if (content == null)
        {
            Debug.LogError("[MailboxUI] Content is not assigned.");
            return;
        }

        if (mailSlotPrefab == null)
        {
            Debug.LogError("[MailboxUI] MailSlotPrefab is not assigned.");
            return;
        }

        if (PlayerDataManager.Instance == null ||
            PlayerDataManager.Instance.playerData == null)
        {
            Debug.LogError("[MailboxUI] PlayerData is not ready.");
            return;
        }

        ClearSlots();

        var mails =
            PlayerDataManager.Instance
            .playerData
            .mailbox;

        foreach (MailData mail in mails)
        {
            GameObject slot =
                Instantiate(
                    mailSlotPrefab,
                    content,
                    false);

            MailSlotUI slotUI =
                slot.GetComponent<MailSlotUI>();

            if (slotUI == null)
            {
                Debug.LogError(
                    "[MailboxUI] MailSlot prefab has no MailSlotUI.");
                Destroy(slot);
                continue;
            }

            slotUI.Setup(mail);
        }
    }

    public async void AddTestMailAndRefresh()
    {
        try
        {
            await MailboxManager.Instance.AddTestMailAsync();
            RefreshUI();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async void ClaimAllAndRefresh()
    {
        try
        {
            await MailboxManager.Instance.ClaimAllMailsAsync();
            RefreshUI();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void ClearSlots()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataChanged -= RefreshUI;
        }
    }
}
