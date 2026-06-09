using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuildRunner
{
    private const string DebugApkPath =
        "Builds/Android/FirebaseLiveOps-debug.apk";
    private const string ReleaseBundlePath =
        "Builds/Android/FirebaseLiveOps-release.aab";

    [MenuItem("Tools/Build Android Debug APK")]
    public static void BuildDebugApk()
    {
        BuildAndroid(
            DebugApkPath,
            false,
            true,
            BuildOptions.Development);
    }

    [MenuItem("Tools/Build Android Release AAB")]
    public static void BuildReleaseAab()
    {
        BuildAndroid(
            ReleaseBundlePath,
            true,
            false,
            BuildOptions.None);
    }

    private static void BuildAndroid(
        string outputPath,
        bool appBundle,
        bool development,
        BuildOptions buildOptions)
    {
        AndroidBuildReadinessValidator.Run();

        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Android,
            BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = appBundle;
        EditorUserBuildSettings.development = development;
        EditorUserBuildSettings.androidBuildSystem =
            AndroidBuildSystem.Gradle;

        string outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = buildOptions
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Android build failed: {report.summary.result}");
        }

        Debug.Log(
            $"[Build] Android output created: " +
            $"{Path.GetFullPath(outputPath)}");
    }
}
