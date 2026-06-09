using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FirebaseSecurityReadinessValidator
{
    private const string UsersCollection = "users";
    private const string GlobalMailsCollection = "global_mails";
    private const string PackageName = "com.DoOurGame.gameliveops";

    [MenuItem("Tools/Validate Firebase Security Readiness")]
    public static void Run()
    {
        AndroidBuildReadinessValidator.Run();
        ValidateFirestoreUsage();
        ValidateFirebaseFiles();
        LogConsoleChecklist();

        Debug.Log(
            "[Validation] Firebase security readiness local checks passed. " +
            "Review the Firebase Console checklist items before release.");
    }

    private static void ValidateFirestoreUsage()
    {
        string firestoreManager =
            ReadRequired("Assets/Scripts/Firebase/FirestoreManager.cs");

        Require(
            firestoreManager.Contains($"Collection(\"{UsersCollection}\")") &&
            firestoreManager.Contains("Document(user.UserId)"),
            "User saves must stay under users/{uid}.");
        Require(
            firestoreManager.Contains(
                $"Collection(\"{GlobalMailsCollection}\")"),
            "Global mail collection usage is missing.");
    }

    private static void ValidateFirebaseFiles()
    {
        string googleServices =
            ReadRequired("Assets/google-services.json");
        Require(
            googleServices.Contains($"\"package_name\": \"{PackageName}\""),
            "google-services.json package_name mismatch.");

        Require(
            File.Exists(
                "Assets/Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml"),
            "Generated Firebase Android resources are missing.");
    }

    private static void LogConsoleChecklist()
    {
        Debug.Log(
            "[Firebase Console Checklist]\n" +
            "1. Authentication: enable Google only for Android release.\n" +
            "2. Authentication: register release SHA-1 and SHA-256.\n" +
            "3. Firestore Rules: users/{uid} readable/writable only by request.auth.uid == uid.\n" +
            "4. Firestore Rules: global_mails readable by signed-in users, writable only by admin/server.\n" +
            "5. App Check: enable Play Integrity before public release.\n" +
            "6. Crashlytics: upload release symbols/mapping after release builds.\n" +
            "7. Cloud Messaging: test foreground/background notification on Android 13+.");
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
