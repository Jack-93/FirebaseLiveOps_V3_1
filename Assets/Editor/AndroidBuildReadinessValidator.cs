using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class AndroidBuildReadinessValidator
{
    private const string AndroidPackageName = "com.DoOurGame.gameliveops";
    private const string FirebaseProjectId = "gameliveops-1449b";
    private const string WebClientId =
        "74044493321-8bua9p1feup1cs7bacqqkflmum5mob28.apps.googleusercontent.com";

    [MenuItem("Tools/Validate Android Build Readiness")]
    public static void Run()
    {
        ValidatePlayerSettings();
        ValidateFirebaseConfig();
        ValidateAndroidManifest();
        ValidateGradleTemplates();
        ValidateCredentialManagerConfig();
        ValidateReleaseBalanceFlags();

        Debug.Log("[Validation] Android build readiness checks passed.");
    }

    private static void ValidatePlayerSettings()
    {
        string packageName =
            PlayerSettings.GetApplicationIdentifier(
                NamedBuildTarget.Android);

        Require(
            packageName == AndroidPackageName,
            $"Android package mismatch: {packageName}");
        Require(
            PlayerSettings.GetScriptingBackend(
                NamedBuildTarget.Android) ==
            ScriptingImplementation.IL2CPP,
            "Android scripting backend must be IL2CPP.");
        Require(
            (PlayerSettings.Android.targetArchitectures &
             AndroidArchitecture.ARM64) != 0,
            "Android build must include ARM64.");
        Require(
            (int)PlayerSettings.Android.minSdkVersion >= 23,
            "Android min SDK must be 23 or higher.");
    }

    private static void ValidateFirebaseConfig()
    {
        string googleServices =
            ReadRequired("Assets/google-services.json");

        Require(
            googleServices.Contains($"\"project_id\": \"{FirebaseProjectId}\""),
            "google-services.json project_id mismatch.");
        Require(
            googleServices.Contains(
                $"\"package_name\": \"{AndroidPackageName}\""),
            "google-services.json package_name mismatch.");
        Require(
            googleServices.Contains(WebClientId),
            "google-services.json web client ID missing.");

        string generatedXml =
            ReadRequired(
                "Assets/Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml");
        Require(
            generatedXml.Contains(
                $"<string name=\"project_id\" translatable=\"false\">{FirebaseProjectId}</string>"),
            "Generated google-services.xml project_id mismatch.");
        Require(
            generatedXml.Contains(WebClientId),
            "Generated google-services.xml web client ID missing.");
    }

    private static void ValidateAndroidManifest()
    {
        string manifest =
            ReadRequired("Assets/Plugins/Android/AndroidManifest.xml");

        Require(
            manifest.Contains(
                "com.google.firebase.MessagingUnityPlayerActivity"),
            "Firebase Messaging activity is missing.");
        Require(
            manifest.Contains(
                "com.google.firebase.messaging.MessageForwardingService"),
            "Firebase Messaging forwarding service is missing.");
        Require(
            manifest.Contains("android.permission.POST_NOTIFICATIONS"),
            "POST_NOTIFICATIONS permission is missing.");
    }

    private static void ValidateGradleTemplates()
    {
        string gradleProperties =
            ReadRequired(
                "Assets/Plugins/Android/gradleTemplate.properties");
        Require(
            gradleProperties.Contains("android.useAndroidX=true"),
            "AndroidX must be enabled.");
        Require(
            gradleProperties.Contains("android.enableJetifier=true"),
            "Jetifier must be enabled.");

        string mainGradle =
            ReadRequired("Assets/Plugins/Android/mainTemplate.gradle");
        Require(
            mainGradle.Contains("androidx.credentials:credentials:1.3.0"),
            "Credential Manager dependency is missing.");
        Require(
            mainGradle.Contains("com.google.firebase:firebase-auth"),
            "Firebase Auth dependency is missing.");
        Require(
            mainGradle.Contains("com.google.firebase:firebase-messaging"),
            "Firebase Messaging dependency is missing.");
        Require(
            mainGradle.Contains(
                "com.google.firebase:firebase-crashlytics-unity"),
            "Firebase Crashlytics Unity dependency is missing.");
    }

    private static void ValidateCredentialManagerConfig()
    {
        string config =
            ReadRequired("Assets/Resources/GoogleSignInConfig.json");
        Require(
            config.Contains(WebClientId),
            "GoogleSignInConfig web client ID mismatch.");

        string dependencies =
            ReadRequired(
                "Assets/GoogleCredentialManager/Editor/GoogleCredentialManagerDependencies.xml");
        Require(
            dependencies.Contains(
                "com.google.android.libraries.identity.googleid:googleid"),
            "Google ID dependency is missing.");
    }

    private static void ValidateReleaseBalanceFlags()
    {
        if (GameBalanceConfig.PrototypeMinimumGold > 0 ||
            GameBalanceConfig.PrototypeMinimumGems > 0)
        {
            Debug.LogWarning(
                "[Validation] PrototypeMinimumGold/Gems are enabled. " +
                "Set them to 0 before release or closed testing with real economy.");
        }

        Require(
            GameBalanceConfig.GachaSingleGemCost > 0 &&
            GameBalanceConfig.GachaTenGemCost > 0,
            "Gacha Gem costs must be positive.");
        Require(
            GameBalanceConfig.GachaTenGemCost <
            GameBalanceConfig.GachaSingleGemCost * 10,
            "Ten-pull Gem cost should preserve a bundle discount.");
        Require(
            GameBalanceConfig.RewardedAdDailyLimit > 0 &&
            GameBalanceConfig.RewardedAdGemAmount > 0,
            "Rewarded ad limits/rewards must be positive.");
    }

    private static string ReadRequired(string path)
    {
        Require(File.Exists(path), $"Required file is missing: {path}");
        return File.ReadAllText(path);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
