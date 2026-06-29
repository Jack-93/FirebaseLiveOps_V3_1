using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class PlayerDataLocalCache
{
    private const string FilePrefix = "player_cache_";
    private const string FileExtension = ".json";

    public static bool TryLoad(string uid, out PlayerData data)
    {
        data = null;
        string path = GetPath(uid);
        if (!File.Exists(path))
            return false;

        try
        {
            string json = File.ReadAllText(path);
            data = JsonConvert.DeserializeObject<PlayerData>(json);
            data?.EnsureInitialized();
            return data != null;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[PlayerDataLocalCache] Load failed: {exception.Message}");
            data = null;
            return false;
        }
    }

    public static void Save(PlayerData data)
    {
        if (data == null)
            return;

        try
        {
            data.EnsureInitialized();
            string path = GetPath(data.uid);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string json = JsonConvert.SerializeObject(
                data,
                Formatting.None);
            WriteAllTextAtomic(path, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[PlayerDataLocalCache] Save failed: {exception.Message}");
        }
    }

    public static bool IsNewerThan(
        PlayerData localData,
        PlayerData serverData)
    {
        return localData != null &&
            serverData != null &&
            localData.lastOnlineUnixTime >
            serverData.lastOnlineUnixTime;
    }

    private static string GetPath(string uid)
    {
        string fileName =
            FilePrefix + Sanitize(uid) + FileExtension;
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    private static string Sanitize(string uid)
    {
        if (string.IsNullOrWhiteSpace(uid))
            return "guest";

        foreach (char invalid in Path.GetInvalidFileNameChars())
            uid = uid.Replace(invalid, '_');

        return uid;
    }

    private static void WriteAllTextAtomic(string path, string contents)
    {
        string tempPath = path + ".tmp";
        File.WriteAllText(tempPath, contents);

        if (!File.Exists(path))
        {
            File.Move(tempPath, path);
            return;
        }

        try
        {
            File.Replace(tempPath, path, null);
        }
        catch
        {
            File.Delete(path);
            File.Move(tempPath, path);
        }
    }
}
