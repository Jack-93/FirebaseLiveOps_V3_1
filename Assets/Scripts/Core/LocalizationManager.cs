using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class LocalizationManager
{
    private sealed class Entry
    {
        public string English;
        public string Korean;

        public Entry(string english, string korean)
        {
            English = english;
            Korean = korean;
        }
    }

    private static readonly List<Entry> Entries = new List<Entry>
    {
        new Entry("BATTLE", "전투"),
        new Entry("GROWTH", "성장"),
        new Entry("GACHA", "뽑기"),
        new Entry("MORE", "메뉴"),
        new Entry("BACK", "뒤로"),
        new Entry("PLAYER HUB", "플레이어 메뉴"),
        new Entry("MAIL", "우편"),
        new Entry("EQUIPMENT", "장비"),
        new Entry("COLLECTION", "도감"),
        new Entry("BEST", "자동 편성"),
        new Entry("CLAIM", "받기"),
        new Entry("QUESTS", "퀘스트"),
        new Entry("EVENT", "이벤트"),
        new Entry("SHOP", "상점"),
        new Entry("SAVE", "저장"),
        new Entry("SETTINGS", "설정"),
        new Entry("NEW GUEST", "새 게스트"),
        new Entry("COMPANION COLLECTION", "동료 도감"),
        new Entry("COMPANION GACHA", "동료 뽑기"),
        new Entry("STANDARD RECRUITMENT", "일반 동료 모집"),
        new Entry("PROMOTE", "승급"),
        new Entry("UPGRADE WEAPON", "무기 강화"),
        new Entry("UPGRADE ARMOR", "방어구 강화"),
        new Entry("QUESTS & ACHIEVEMENTS", "퀘스트와 업적"),
        new Entry("CLAIM QUEST", "퀘스트 보상"),
        new Entry("CLAIM ACHIEVEMENTS", "업적 보상"),
        new Entry("CLAIM EVENT REWARD", "이벤트 보상"),
        new Entry("TOGGLE SOUND", "사운드 전환"),
        new Entry("TOGGLE VIBRATION", "진동 전환"),
        new Entry("TOGGLE NOTIFICATIONS", "알림 전환"),
        new Entry("SWITCH 30 / 60 FPS", "30 / 60 FPS 전환"),
        new Entry("SWITCH LANGUAGE", "언어 전환"),
        new Entry("START", "시작"),
        new Entry("COLLECT", "받기"),
        new Entry("RETRY", "재시도"),
        new Entry("AUTO BATTLE", "자동 전투"),
        new Entry("AUTO ON", "자동 진행"),
        new Entry("BOSS", "보스"),
        new Entry("HERO", "영웅"),
        new Entry("HERO GROWTH", "영웅 성장"),
        new Entry("UPGRADE", "강화")
    };

    private static TMP_FontAsset koreanFont;

    public static string Text(string english, string korean)
    {
        return GameSettingsManager.IsKoreanLanguage
            ? korean
            : english;
    }

    public static void ApplyTo(Transform root)
    {
        if (root == null)
            return;

        bool korean = GameSettingsManager.IsKoreanLanguage;
        TMP_FontAsset targetFont =
            korean ? GetKoreanFont() : TMP_Settings.defaultFontAsset;

        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            foreach (Entry entry in Entries)
            {
                if (text.text == entry.English ||
                    text.text == entry.Korean)
                {
                    text.text = korean
                        ? entry.Korean
                        : entry.English;
                    break;
                }
            }

            if (targetFont != null)
                text.font = targetFont;
        }
    }

    private static TMP_FontAsset GetKoreanFont()
    {
        if (koreanFont != null)
            return koreanFont;

        Font font = Font.CreateDynamicFontFromOSFont(
            new[]
            {
                "Noto Sans KR",
                "Malgun Gothic",
                "Noto Sans CJK KR",
                "sans-serif"
            },
            36);
        if (font == null)
            return null;

        koreanFont = TMP_FontAsset.CreateFontAsset(font);
        return koreanFont;
    }
}
