using UnityEngine;
using System;

public class MailboxManager : MonoBehaviour
{
    public static MailboxManager Instance;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void AddTestMail()
    {
        MailData mail =
            new MailData();

        mail.mailId =
            Guid.NewGuid().ToString();

        mail.title =
            "Maintenance Reward";

        mail.itemName =
            "Gem";

        mail.amount =
            500;

        mail.isClaimed =
            false;

        PlayerDataManager.Instance
            .playerData.mailbox
            .Add(mail);

        FirestoreManager.Instance
            .SavePlayerData(
                PlayerDataManager.Instance
                .playerData);

        Debug.Log(
            "[Mailbox] Test Mail Added");
    }

    public void ClaimMail(MailData mail)
    {
        if (mail == null)
            return;

        if (mail.isClaimed)
            return;

        InventoryManager.Instance
            .AddItem(
                mail.itemName,
                mail.amount);

        mail.isClaimed = true;

        PlayerDataManager.Instance
            .playerData.mailbox
            .Remove(mail);

        FirestoreManager.Instance
            .SavePlayerData(
                PlayerDataManager.Instance
                .playerData);

        Debug.Log(
            $"[Mailbox] Claimed : {mail.title}");
    }
}