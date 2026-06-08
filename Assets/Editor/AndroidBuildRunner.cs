using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuildRunner
{
    private const string OutputPath =
        "Builds/Android/FirebaseLiveOps-debug.apk";

    [MenuItem("Tools/Build Android Debug APK")]
    public static void BuildDebugApk()
    {
        AndroidBuildReadinessValidator.Run();

        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Android,
            BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.androidBuildSystem =
            AndroidBuildSystem.Gradle;

        string outputDirectory = Path.GetDirectoryName(OutputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.Android,
            options = BuildOptions.Development
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Android build failed: {report.summary.result}");
        }

        Debug.Log(
            $"[Build] Android APK created: {Path.GetFullPath(OutputPath)}");
    }
}
