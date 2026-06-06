using System;
using System.Threading.Tasks;
using UnityEngine;

public class MailboxManager : MonoBehaviour
{
    public static MailboxManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task AddTestMailAsync()
    {
        PlayerData data = GetPlayerData();
        if (data == null)
            return;

        data.mailbox.Add(new MailData
        {
            mailId = Guid.NewGuid().ToString(),
            title = "Maintenance Reward",
            itemName = "Gem",
            amount = 500,
            isClaimed = false
        });

        await FirestoreManager.Instance.SavePlayerDataAsync(data);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();

        Debug.Log("[Mailbox] Test Mail Added");
    }

    public async Task<bool> ClaimMailAsync(MailData mail)
    {
        PlayerData data = GetPlayerData();
        if (data == null || mail == null || mail.isClaimed)
            return false;

        InventoryManager.Instance.AddItem(
            mail.itemName,
            mail.amount,
            false);

        mail.isClaimed = true;
        RememberClaimedMail(data, mail.mailId);
        data.mailbox.Remove(mail);

        await FirestoreManager.Instance.SavePlayerDataAsync(data);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();

        Debug.Log($"[Mailbox] Claimed: {mail.title}");
        return true;
    }

    public async Task<int> ClaimAllMailsAsync()
    {
        PlayerData data = GetPlayerData();
        if (data == null || data.mailbox.Count == 0)
        {
            Debug.Log("[Mailbox] No mails to claim");
            return 0;
        }

        int claimedCount = 0;

        for (int index = data.mailbox.Count - 1; index >= 0; index--)
        {
            MailData mail = data.mailbox[index];
            if (mail == null || mail.isClaimed)
                continue;

            InventoryManager.Instance.AddItem(
                mail.itemName,
                mail.amount,
                false);

            mail.isClaimed = true;
            RememberClaimedMail(data, mail.mailId);
            data.mailbox.RemoveAt(index);
            claimedCount++;
        }

        await FirestoreManager.Instance.SavePlayerDataAsync(data);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();

        Debug.Log($"[Mailbox] Claim All Complete: {claimedCount}");
        return claimedCount;
    }

    public async void AddTestMail()
    {
        try
        {
            await AddTestMailAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public async void ClaimAllMails()
    {
        try
        {
            await ClaimAllMailsAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static PlayerData GetPlayerData()
    {
        PlayerData data = PlayerDataManager.Instance?.playerData;

        if (data == null)
        {
            Debug.LogError("[Mailbox] PlayerData is not ready.");
            return null;
        }

        data.EnsureInitialized();
        return data;
    }

    private static void RememberClaimedMail(PlayerData data, string mailId)
    {
        if (!string.IsNullOrEmpty(mailId) &&
            !data.claimedMailIds.Contains(mailId))
        {
            data.claimedMailIds.Add(mailId);
        }
    }
}
