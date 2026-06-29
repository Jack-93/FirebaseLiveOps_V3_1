public struct QuestProgressView
{
    public int Progress;
    public int Target;
    public int QuestIndex;
}

public static class QuestPanelFormatter
{
    public static string DataUnavailable =>
        LocalizationManager.Translate("Quest data unavailable.");

    public static QuestProgressView BuildProgress(PlayerData data)
    {
        if (data == null || QuestManager.QuestCount <= 0)
            return Build(0, 1, 0);

        int questIndex = Clamp(
            data.sequentialQuestIndex,
            0,
            QuestManager.QuestCount - 1);
        int target = QuestManager.GetTargetForIndex(questIndex);
        int progress = Clamp(
            data.sequentialQuestProgress,
            0,
            target);
        return Build(progress, target, questIndex);
    }

    private static QuestProgressView Build(
        int progress,
        int target,
        int questIndex)
    {
        return new QuestProgressView
        {
            Progress = progress,
            Target = target,
            QuestIndex = questIndex
        };
    }

    private static int Clamp(int value, int minimum, int maximum)
    {
        if (value < minimum)
            return minimum;
        return value > maximum ? maximum : value;
    }
}
