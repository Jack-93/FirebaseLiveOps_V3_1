using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class PlayerDataLocalCache
{
    private const string FilePrefix = "player_cache_";
    private const string FileExtension = ".json";
    private const string MetaExtension = ".meta.json";

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
            TouchMeta(data.uid);
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

    public static bool HasPendingRemoteSave(string uid)
    {
        return TryLoadMeta(uid, out CacheMeta meta) &&
            meta.pendingRemoteSave;
    }

    public static void MarkPendingRemoteSave(
        string uid,
        bool pendingRemoteSave)
    {
        try
        {
            CacheMeta meta = TryLoadMeta(uid, out CacheMeta loaded)
                ? loaded
                : new CacheMeta();
            meta.pendingRemoteSave = pendingRemoteSave;
            meta.updatedUnixTime =
                DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            WriteMeta(uid, meta);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[PlayerDataLocalCache] Pending marker update failed: " +
                exception.Message);
        }
    }

    private static string GetPath(string uid)
    {
        string fileName =
            FilePrefix + Sanitize(uid) + FileExtension;
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    private static string GetMetaPath(string uid)
    {
        string fileName =
            FilePrefix + Sanitize(uid) + MetaExtension;
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    private static void TouchMeta(string uid)
    {
        CacheMeta meta = TryLoadMeta(uid, out CacheMeta loaded)
            ? loaded
            : new CacheMeta();
        meta.cachedAtUnixTime =
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        WriteMeta(uid, meta);
    }

    private static bool TryLoadMeta(string uid, out CacheMeta meta)
    {
        meta = null;
        string path = GetMetaPath(uid);
        if (!File.Exists(path))
            return false;

        try
        {
            string json = File.ReadAllText(path);
            meta = JsonConvert.DeserializeObject<CacheMeta>(json);
            return meta != null;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[PlayerDataLocalCache] Meta load failed: " +
                exception.Message);
            return false;
        }
    }

    private static void WriteMeta(string uid, CacheMeta meta)
    {
        string path = GetMetaPath(uid);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string json = JsonConvert.SerializeObject(meta, Formatting.None);
        WriteAllTextAtomic(path, json);
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

    [Serializable]
    private sealed class CacheMeta
    {
        public bool pendingRemoteSave;
        public long cachedAtUnixTime;
        public long updatedUnixTime;
    }
}
