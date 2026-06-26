using System;

public static class EventPanelFormatter
{
    public static string DataUnavailable =>
        LocalizationManager.Translate("Event data unavailable.");

    public static string EmptyProgress => "0 / 0";

    public static string FormatProgress(PlayerData data)
    {
        return
            $"{LocalizationManager.Translate("Kills")} " +
            $"{Math.Min(data.eventKillCount, EventMissionManager.KillTarget)}/" +
            $"{EventMissionManager.KillTarget}   " +
            $"{LocalizationManager.Translate("Gacha")} " +
            $"{Math.Min(data.eventGachaCount, EventMissionManager.GachaTarget)}/" +
            $"{EventMissionManager.GachaTarget}   " +
            $"{LocalizationManager.Translate("Points")} " +
            $"{Math.Min(data.eventMissionPoints, EventMissionManager.RewardPointTarget)}/" +
            $"{EventMissionManager.RewardPointTarget}";
    }
}
