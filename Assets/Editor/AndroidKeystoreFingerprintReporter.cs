using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AndroidKeystoreFingerprintReporter
{
    [MenuItem("Tools/Print Android Debug SHA Fingerprints")]
    public static void PrintDebugFingerprints()
    {
        string keytool = Path.Combine(
            EditorApplication.applicationContentsPath,
            "PlaybackEngines",
            "AndroidPlayer",
            "OpenJDK",
            "bin",
            "keytool.exe");
        string debugKeystore = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile),
            ".android",
            "debug.keystore");

        if (!File.Exists(keytool))
            throw new FileNotFoundException("keytool.exe not found", keytool);
        if (!File.Exists(debugKeystore))
        {
            throw new FileNotFoundException(
                "Android debug keystore not found",
                debugKeystore);
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = keytool,
            Arguments =
                "-list -v -keystore " +
                $"\"{debugKeystore}\" -alias androiddebugkey " +
                "-storepass android -keypass android",
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using Process process = Process.Start(startInfo);
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(error);

        UnityEngine.Debug.Log("[Android SHA Fingerprints]\n" + output);
    }
}
