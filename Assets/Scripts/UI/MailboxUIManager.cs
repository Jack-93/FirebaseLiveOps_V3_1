using UnityEngine;

public class MailboxUIManager : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject mailSlotPrefab;

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

            slotUI.Setup(mail);
        }
    }

    public void AddTestMailAndRefresh()
    {
        Debug.Log("[MailboxUI] AddTestMailAndRefresh Called");

        MailboxManager.Instance.AddTestMail();

        RefreshUI();
    }

    public void ClaimAllAndRefresh()
    {
        Debug.Log("[MailboxUI] ClaimAllAndRefresh Called");

        MailboxManager.Instance
            .ClaimAllMails();

        RefreshUI();
    }

    private void ClearSlots()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }
}