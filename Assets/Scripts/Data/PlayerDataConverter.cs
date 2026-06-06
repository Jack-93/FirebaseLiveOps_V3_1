using System;
using System.Collections;
using System.Collections.Generic;

public static class PlayerDataConverter
{
    public static Dictionary<string, object> ToDictionary(PlayerData data)
    {
        data.EnsureInitialized();

        return new Dictionary<string, object>
        {
            { "uid", data.uid ?? "" },
            { "nickname", data.nickname },
            { "level", data.level },
            { "gold", data.gold },
            { "tutorialCompleted", data.tutorialCompleted },
            { "inventory", ConvertInventoryToDictionary(data.inventory) },
            { "pityCount", data.pityCount },
            { "mailbox", ConvertMailsToList(data.mailbox) },
            { "claimedMailIds", new List<string>(data.claimedMailIds) },
            { "lastLoginDate", data.lastLoginDate },
            { "lastRewardDate", data.lastRewardDate },
            { "loginDay", data.loginDay }
        };
    }

    public static PlayerData FromDictionary(Dictionary<string, object> values)
    {
        PlayerData data = new PlayerData
        {
            uid = GetString(values, "uid", ""),
            nickname = GetString(values, "nickname", "NewPlayer"),
            level = GetInt(values, "level", 1),
            gold = GetInt(values, "gold", 1000),
            tutorialCompleted = GetBool(values, "tutorialCompleted", false),
            pityCount = GetInt(values, "pityCount", 0),
            lastLoginDate = GetString(values, "lastLoginDate", ""),
            lastRewardDate = GetString(values, "lastRewardDate", ""),
            loginDay = GetInt(values, "loginDay", 0)
        };

        if (values.TryGetValue("inventory", out object inventoryValue))
            data.inventory = ConvertInventory(inventoryValue);

        if (values.TryGetValue("mailbox", out object mailboxValue))
            data.mailbox = ConvertMails(mailboxValue);

        if (values.TryGetValue("claimedMailIds", out object claimedValue))
            data.claimedMailIds = ConvertStrings(claimedValue);

        data.EnsureInitialized();
        return data;
    }

    private static Dictionary<string, object> ConvertInventoryToDictionary(
        InventoryData inventory)
    {
        Dictionary<string, object> result =
            new Dictionary<string, object>();

        foreach (KeyValuePair<string, int> item in inventory.items)
            result[item.Key] = item.Value;

        return result;
    }

    private static InventoryData ConvertInventory(object raw)
    {
        InventoryData inventory = new InventoryData();
        inventory.items.Clear();

        if (!(raw is IDictionary dictionary))
            return inventory;

        foreach (DictionaryEntry entry in dictionary)
        {
            string key = entry.Key?.ToString();
            if (!string.IsNullOrEmpty(key))
                inventory.items[key] = ConvertToInt(entry.Value, 0);
        }

        return inventory;
    }

    private static List<object> ConvertMailsToList(List<MailData> mails)
    {
        List<object> result = new List<object>();

        foreach (MailData mail in mails)
        {
            if (mail == null)
                continue;

            result.Add(new Dictionary<string, object>
            {
                { "mailId", mail.mailId ?? "" },
                { "title", mail.title ?? "" },
                { "itemName", mail.itemName ?? "" },
                { "amount", mail.amount },
                { "isClaimed", mail.isClaimed }
            });
        }

        return result;
    }

    private static List<MailData> ConvertMails(object raw)
    {
        List<MailData> mails = new List<MailData>();

        if (!(raw is IEnumerable list))
            return mails;

        foreach (object item in list)
        {
            if (!(item is IDictionary dictionary))
                continue;

            mails.Add(new MailData
            {
                mailId = GetDictionaryString(dictionary, "mailId"),
                title = GetDictionaryString(dictionary, "title"),
                itemName = GetDictionaryString(dictionary, "itemName"),
                amount = GetDictionaryInt(dictionary, "amount"),
                isClaimed = GetDictionaryBool(dictionary, "isClaimed")
            });
        }

        return mails;
    }

    private static List<string> ConvertStrings(object raw)
    {
        List<string> result = new List<string>();

        if (!(raw is IEnumerable list) || raw is string)
            return result;

        foreach (object item in list)
        {
            string value = item?.ToString();
            if (!string.IsNullOrEmpty(value))
                result.Add(value);
        }

        return result;
    }

    private static string GetString(
        Dictionary<string, object> values,
        string key,
        string fallback)
    {
        return values.TryGetValue(key, out object value) && value != null
            ? value.ToString()
            : fallback;
    }

    private static int GetInt(
        Dictionary<string, object> values,
        string key,
        int fallback)
    {
        return values.TryGetValue(key, out object value)
            ? ConvertToInt(value, fallback)
            : fallback;
    }

    private static bool GetBool(
        Dictionary<string, object> values,
        string key,
        bool fallback)
    {
        return values.TryGetValue(key, out object value)
            ? ConvertToBool(value, fallback)
            : fallback;
    }

    private static string GetDictionaryString(
        IDictionary dictionary,
        string key)
    {
        return dictionary.Contains(key) && dictionary[key] != null
            ? dictionary[key].ToString()
            : "";
    }

    private static int GetDictionaryInt(IDictionary dictionary, string key)
    {
        return dictionary.Contains(key)
            ? ConvertToInt(dictionary[key], 0)
            : 0;
    }

    private static bool GetDictionaryBool(
        IDictionary dictionary,
        string key)
    {
        return dictionary.Contains(key) &&
            ConvertToBool(dictionary[key], false);
    }

    private static int ConvertToInt(object value, int fallback)
    {
        try
        {
            return value == null ? fallback : Convert.ToInt32(value);
        }
        catch
        {
            return fallback;
        }
    }

    private static bool ConvertToBool(object value, bool fallback)
    {
        try
        {
            return value == null ? fallback : Convert.ToBoolean(value);
        }
        catch
        {
            return fallback;
        }
    }
}
