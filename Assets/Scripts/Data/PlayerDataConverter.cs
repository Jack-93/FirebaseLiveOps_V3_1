using System;
using System.Collections;
using System.Collections.Generic;

/*
 * PlayerData와 Dictionary<string, object> 간의 변환을 담당하는 유틸리티 클래스
 * Firestore에서 데이터를 읽고 쓸 때 사용
 */
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
            { "loginDay", data.loginDay },
            { "currentStage", data.currentStage },
            { "highestStage", data.highestStage },
            { "stageEnemyIndex", data.stageEnemyIndex },
            { "attackLevel", data.attackLevel },
            { "healthLevel", data.healthLevel },
            { "attackSpeedLevel", data.attackSpeedLevel },
            { "tutorialStep", data.tutorialStep },
            { "totalMonstersDefeated", data.totalMonstersDefeated },
            { "lastOnlineUnixTime", data.lastOnlineUnixTime },
            { "autoAdvance", data.autoAdvance },
            { "equippedCompanion", data.equippedCompanion },
            {
                "equippedCompanionRarity",
                data.equippedCompanionRarity
            },
            {
                "equippedCompanions",
                new List<string>(data.equippedCompanions)
            },
            {
                "equippedCompanionRarities",
                new List<string>(data.equippedCompanionRarities)
            },
            { "companionStars", ConvertIntDictionary(data.companionStars) },
            { "equippedWeapon", data.equippedWeapon },
            { "equippedArmor", data.equippedArmor },
            { "weaponUpgradeLevel", data.weaponUpgradeLevel },
            { "armorUpgradeLevel", data.armorUpgradeLevel },
            { "dailyQuestDate", data.dailyQuestDate },
            { "dailyQuestKills", data.dailyQuestKills },
            { "dailyQuestClaimed", data.dailyQuestClaimed },
            { "sequentialQuestIndex", data.sequentialQuestIndex },
            { "sequentialQuestProgress", data.sequentialQuestProgress },
            { "sequentialQuestCycles", data.sequentialQuestCycles },
            { "eventMissionDate", data.eventMissionDate },
            { "eventKillCount", data.eventKillCount },
            { "eventGachaCount", data.eventGachaCount },
            { "eventMissionPoints", data.eventMissionPoints },
            { "eventRewardClaimed", data.eventRewardClaimed },
            {
                "claimedAchievementIds",
                new List<string>(data.claimedAchievementIds)
            }
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
            loginDay = GetInt(values, "loginDay", 0),
            currentStage = GetInt(values, "currentStage", 1),
            highestStage = GetInt(values, "highestStage", 1),
            stageEnemyIndex = GetInt(values, "stageEnemyIndex", 0),
            attackLevel = GetInt(values, "attackLevel", 1),
            healthLevel = GetInt(values, "healthLevel", 1),
            attackSpeedLevel = GetInt(values, "attackSpeedLevel", 1),
            tutorialStep = GetInt(values, "tutorialStep", 0),
            totalMonstersDefeated =
                GetInt(values, "totalMonstersDefeated", 0),
            lastOnlineUnixTime =
                GetLong(values, "lastOnlineUnixTime", 0),
            autoAdvance = GetBool(values, "autoAdvance", true),
            equippedWeapon = GetString(values, "equippedWeapon", ""),
            equippedArmor = GetString(values, "equippedArmor", ""),
            weaponUpgradeLevel =
                GetInt(values, "weaponUpgradeLevel", 0),
            armorUpgradeLevel =
                GetInt(values, "armorUpgradeLevel", 0),
            dailyQuestDate = GetString(values, "dailyQuestDate", ""),
            dailyQuestKills = GetInt(values, "dailyQuestKills", 0),
            dailyQuestClaimed =
                GetBool(values, "dailyQuestClaimed", false),
            sequentialQuestIndex =
                GetInt(values, "sequentialQuestIndex", 0),
            sequentialQuestProgress =
                GetInt(values, "sequentialQuestProgress", 0),
            sequentialQuestCycles =
                GetInt(values, "sequentialQuestCycles", 0),
            eventMissionDate =
                GetString(values, "eventMissionDate", ""),
            eventKillCount = GetInt(values, "eventKillCount", 0),
            eventGachaCount = GetInt(values, "eventGachaCount", 0),
            eventMissionPoints =
                GetInt(values, "eventMissionPoints", 0),
            eventRewardClaimed =
                GetBool(values, "eventRewardClaimed", false),
            equippedCompanion =
                GetString(values, "equippedCompanion", ""),
            equippedCompanionRarity =
                GetString(values, "equippedCompanionRarity", "")
        };

        /*
         * 안전하게 체크, 변환
         */
        if (values.TryGetValue("inventory", out object inventoryValue))
            data.inventory = ConvertInventory(inventoryValue);

        if (values.TryGetValue("mailbox", out object mailboxValue))
            data.mailbox = ConvertMails(mailboxValue);

        if (values.TryGetValue("claimedMailIds", out object claimedValue))
            data.claimedMailIds = ConvertStrings(claimedValue);

        if (values.TryGetValue(
                "claimedAchievementIds",
                out object achievementsValue))
        {
            data.claimedAchievementIds =
                ConvertStrings(achievementsValue);
        }

        if (values.TryGetValue(
                "equippedCompanions",
                out object companionsValue))
        {
            data.equippedCompanions =
                ConvertStrings(companionsValue, true);
        }

        if (values.TryGetValue(
                "equippedCompanionRarities",
                out object raritiesValue))
        {
            data.equippedCompanionRarities =
                ConvertStrings(raritiesValue, true);
        }

        if (values.TryGetValue(
                "companionStars",
                out object starsValue))
        {
            data.companionStars = ConvertIntDictionary(starsValue);
        }

        data.EnsureInitialized();
        return data;
    }
    // Dictionary<string, int> -> Dictionary<string, object>
    private static Dictionary<string, object> ConvertInventoryToDictionary(
        InventoryData inventory)
    {
        Dictionary<string, object> result =
            new Dictionary<string, object>();

        foreach (KeyValuePair<string, int> item in inventory.items)
            result[item.Key] = item.Value;

        return result;
    }

    // Dictionary<string, int> <- Dictionary<string, object>
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

    private static Dictionary<string, object> ConvertIntDictionary(
        Dictionary<string, int> values)
    {
        Dictionary<string, object> result =
            new Dictionary<string, object>();

        foreach (KeyValuePair<string, int> entry in values)
            result[entry.Key] = entry.Value;

        return result;
    }

    private static Dictionary<string, int> ConvertIntDictionary(object raw)
    {
        Dictionary<string, int> result =
            new Dictionary<string, int>();

        if (!(raw is IDictionary dictionary))
            return result;

        foreach (DictionaryEntry entry in dictionary)
        {
            string key = entry.Key?.ToString();
            if (!string.IsNullOrEmpty(key))
                result[key] = ConvertToInt(entry.Value, 1);
        }

        return result;
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
                { "isGlobalMail", mail.isGlobalMail },
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
                isGlobalMail =
                    GetDictionaryBool(dictionary, "isGlobalMail"),
                title = GetDictionaryString(dictionary, "title"),
                itemName = GetDictionaryString(dictionary, "itemName"),
                amount = GetDictionaryInt(dictionary, "amount"),
                isClaimed = GetDictionaryBool(dictionary, "isClaimed")
            });
        }

        return mails;
    }

    private static List<string> ConvertStrings(
        object raw,
        bool keepEmpty = false)
    {
        List<string> result = new List<string>();

        if (!(raw is IEnumerable list) || raw is string)
            return result;

        foreach (object item in list)
        {
            string value = item?.ToString();
            if (keepEmpty || !string.IsNullOrEmpty(value))
                result.Add(value);
        }

        return result;
    }

    private static string GetString(
        Dictionary<string, object> values,
        string key,
        string fallback)
    {
        // fallback : 값이 없거나 null인 경우 사용할 기본값 (1 or 값)
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

    private static long GetLong(
        Dictionary<string, object> values,
        string key,
        long fallback)
    {
        if (!values.TryGetValue(key, out object value) || value == null)
            return fallback;

        try
        {
            return Convert.ToInt64(value);
        }
        catch
        {
            return fallback;
        }
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
