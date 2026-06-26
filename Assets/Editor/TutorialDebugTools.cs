using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class TutorialDebugTools
{
    [MenuItem("Tools/Tutorial/Reset Story Intro")]
    public static void ResetStoryIntro()
    {
        PlayerData data = GetRuntimeData();
        if (data == null)
            return;

        data.storyIntroCompleted = false;
        data.storyIntroCutIndex = 0;
        data.tutorialCompleted = false;
        data.tutorialStep = 0;
        PlayerDataManager.Instance.NotifyPlayerDataChanged();

        _ = SaveAsync(data);
        Debug.Log("[TutorialDebug] Story intro and tutorial state reset.");
    }

    [MenuItem("Tools/Tutorial/Complete Story Intro")]
    public static void CompleteStoryIntro()
    {
        PlayerData data = GetRuntimeData();
        if (data == null)
            return;

        int lastCutIndex = StoryIntroDatabase.GetCuts().Count - 1;
        data.storyIntroCompleted = true;
        data.storyIntroCutIndex = Mathf.Max(0, lastCutIndex);
        PlayerDataManager.Instance.NotifyPlayerDataChanged();

        _ = SaveAsync(data);
        Debug.Log("[TutorialDebug] Story intro marked complete.");
    }

    [MenuItem("Tools/Tutorial/Reset Story Intro", true)]
    [MenuItem("Tools/Tutorial/Complete Story Intro", true)]
    private static bool CanEditTutorialState()
    {
        return Application.isPlaying &&
               PlayerDataManager.Instance != null &&
               PlayerDataManager.Instance.playerData != null;
    }

    private static PlayerData GetRuntimeData()
    {
        if (!CanEditTutorialState())
        {
            Debug.LogWarning(
                "[TutorialDebug] Enter Play Mode and load player data first.");
            return null;
        }

        PlayerData data = PlayerDataManager.Instance.playerData;
        data.EnsureInitialized();
        return data;
    }

    private static async Task SaveAsync(PlayerData data)
    {
        if (FirestoreManager.Instance == null)
            return;

        await FirestoreManager.Instance.SavePlayerDataAsync(data);
    }
}
