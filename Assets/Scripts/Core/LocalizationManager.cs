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
        new Entry("MORE", "더보기"),
        new Entry("BACK", "뒤로"),
        new Entry("PLAYER HUB", "플레이어 허브"),
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
        new Entry("TOGGLE SOUND", "소리 전환"),
        new Entry("TOGGLE VIBRATION", "진동 전환"),
        new Entry("TOGGLE NOTIFICATIONS", "알림 전환"),
        new Entry("SWITCH 30 / 60 FPS", "30 / 60 FPS 전환"),
        new Entry("SWITCH LANGUAGE", "언어 전환"),
        new Entry("START", "시작"),
        new Entry("COLLECT", "받기"),
        new Entry("RETRY", "다시 시도"),
        new Entry("AUTO BATTLE", "자동 전투"),
        new Entry("AUTO ON", "자동 진행"),
        new Entry("BOSS", "보스"),
        new Entry("HERO", "영웅"),
        new Entry("HERO GROWTH", "영웅 성장"),
        new Entry("UPGRADE", "강화"),
        new Entry("LOGIN", "로그인"),
        new Entry("PLAY AS GUEST", "게스트로 시작"),
        new Entry("GOOGLE LOGIN", "구글 로그인"),
        new Entry("TITLE", "타이틀"),
        new Entry("LOADING", "불러오는 중"),
        new Entry("REPEAT", "반복"),
        new Entry("Companion skills preparing...", "동료 스킬 준비 중..."),
        new Entry("Stage", "스테이지"),
        new Entry("Gold", "골드"),
        new Entry("Power", "전투력"),
        new Entry("DMG", "피해"),
        new Entry("ATK", "공격"),
        new Entry("Cost", "비용"),
        new Entry("Interval", "간격"),
        new Entry("Paused", "일시정지"),
        new Entry("No companion skills equipped.", "장착한 동료 스킬이 없습니다."),
        new Entry("STAGE BOSS", "스테이지 보스"),
        new Entry("ART NEEDED / STAGE BACKGROUND", "아트 필요 / 스테이지 배경"),
        new Entry("Spend Gold to strengthen your hero.", "골드를 사용해 영웅을 강화하세요."),
        new Entry("Attack", "공격력"),
        new Entry("Health", "체력"),
        new Entry("Attack Speed", "공격 속도"),
        new Entry("Increase damage", "피해량 증가"),
        new Entry("Increase maximum HP", "최대 체력 증가"),
        new Entry("Attack more frequently", "더 빠르게 공격"),
        new Entry("RESOURCES", "재화"),
        new Entry("Inventory, companions, rewards, and account.", "가방, 동료, 보상, 계정을 관리합니다."),
        new Entry("Inventory", "가방"),
        new Entry("COMPANIONS", "동료"),
        new Entry("Companion", "동료"),
        new Entry("Account", "계정"),
        new Entry("Daily reward", "일일 보상"),
        new Entry("ACCOUNT", "계정"),
        new Entry("ACCOUNT LINK", "계정 연동"),
        new Entry("START NEW GUEST", "새 게스트로 시작"),
        new Entry("START WITH GOOGLE", "구글로 시작"),
        new Entry("Checking login...", "로그인 확인 중..."),
        new Entry("Android only. Guest progress can be linked to Google later.", "Android 전용. 게스트 진행도는 나중에 Google과 연동할 수 있습니다."),
        new Entry("Hero, companions, monsters, and world mood will be shown here.", "영웅, 동료, 몬스터, 세계 분위기가 여기에 표시됩니다."),
        new Entry("IDLE RPG\nPROTOTYPE", "방치형 RPG\n프로토타입"),
        new Entry("IDLE RPG PROTOTYPE", "방치형 RPG 프로토타입"),
        new Entry("Login and account linking placeholder.", "로그인과 계정 연동 상태입니다."),
        new Entry("LINK GOOGLE", "구글 연동"),
        new Entry("LOGOUT", "로그아웃"),
        new Entry("Select a companion, then equip it to a party slot.", "동료를 선택한 뒤 파티 슬롯에 장착하세요."),
        new Entry("DETAIL / PARTY SLOTS", "상세 정보 / 파티 슬롯"),
        new Entry("Select a companion.", "동료를 선택하세요."),
        new Entry("SLOT", "슬롯"),
        new Entry("EQUIP", "장착"),
        new Entry("REMOVE", "해제"),
        new Entry("EMPTY", "비어 있음"),
        new Entry("READY", "준비됨"),
        new Entry("EQUIPPED", "장착 중"),
        new Entry("LOCKED - recruit from Gacha", "잠김 - 뽑기에서 획득하세요"),
        new Entry("Promotion ready.", "승급 가능."),
        new Entry("Party", "파티"),
        new Entry("PARTY", "파티"),
        new Entry("Team Attack", "팀 공격력"),
        new Entry("Mailbox", "우편함"),
        new Entry("waiting", "개 대기 중"),
        new Entry("Monsters defeated", "처치한 몬스터"),
        new Entry("Recruit one in Gacha.", "뽑기에서 동료를 획득하세요."),
        new Entry("Linked account", "연동된 계정"),
        new Entry("Guest account", "게스트 계정"),
        new Entry("Highest", "최고"),
        new Entry("Daily Reward Day", "일일 보상"),
        new Entry("is ready", "수령 가능"),
        new Entry("already claimed", "수령 완료"),
        new Entry("Owned", "보유"),
        new Entry("Stars", "별"),
        new Entry("Promotion needs", "승급 필요"),
        new Entry("duplicate(s)", "중복 캐릭터"),
        new Entry("WEAPON", "무기"),
        new Entry("ARMOR", "방어구"),
        new Entry("None", "없음"),
        new Entry("Next cost", "다음 비용"),
        new Entry("Upgrade equipped gear to raise combat power.", "장착한 장비를 강화해 전투력을 올리세요."),
        new Entry("CURRENT LOADOUT", "현재 장착"),
        new Entry("No equipment.", "장비가 없습니다."),
        new Entry("Current quest changes immediately after claiming.", "보상을 받으면 다음 퀘스트가 바로 시작됩니다."),
        new Entry("Quest data unavailable.", "퀘스트 정보를 불러올 수 없습니다."),
        new Entry("MAIN QUEST", "메인 퀘스트"),
        new Entry("Complete one objective to unlock the next.", "목표 하나를 완료하면 다음 퀘스트가 열립니다."),
        new Entry("CURRENT OBJECTIVE", "현재 목표"),
        new Entry("Quest", "퀘스트"),
        new Entry("Reward", "보상"),
        new Entry("Ready to claim. Next quest starts immediately.", "수령 가능. 다음 퀘스트가 바로 시작됩니다."),
        new Entry("Keep playing to complete this objective.", "계속 플레이해서 목표를 완료하세요."),
        new Entry("Stage 5: Claimed", "스테이지 5: 수령 완료"),
        new Entry("50 kills: Claimed", "50마리 처치: 수령 완료"),
        new Entry("Total kills", "총 처치"),
        new Entry("SHOP & ADS", "상점과 광고"),
        new Entry("Store and rewarded ad placeholders.", "상점과 광고 보상 자리입니다."),
        new Entry("PRODUCTS", "상품"),
        new Entry("Shop data unavailable.", "상점 정보를 불러올 수 없습니다."),
        new Entry("Monetization service unavailable", "결제 서비스를 사용할 수 없습니다."),
        new Entry("STARTER PACK", "스타터 팩"),
        new Entry("STARTER PACK  OWNED", "스타터 팩  보유 중"),
        new Entry("WATCH AD", "광고 보기"),
        new Entry("WATCH AD  +100 GEMS", "광고 보기  +100 젬"),
        new Entry("1,200 GEMS", "1,200 젬"),
        new Entry("6,500 GEMS", "6,500 젬"),
        new Entry("5K GOLD\n100 GEM", "5K 골드\n100 젬"),
        new Entry("3 TICKETS\n250 GEM", "티켓 3개\n250 젬"),
        new Entry("30K GOLD\n500 GEM", "30K 골드\n500 젬"),
        new Entry("AD SDK PENDING", "광고 SDK 대기 중"),
        new Entry("Gems", "젬"),
        new Entry("GEMS", "젬"),
        new Entry("Gem", "젬"),
        new Entry("Tickets", "티켓"),
        new Entry("TICKETS", "티켓"),
        new Entry("Ticket", "티켓"),
        new Entry("GOLD", "골드"),
        new Entry("Real purchases and rewarded ads are SDK-ready placeholders.", "실제 결제와 광고 보상 SDK 연결 준비 화면입니다."),
        new Entry("BUY GOLD POUCH", "골드 주머니 구매"),
        new Entry("RESTORE PURCHASES", "구매 복원"),
        new Entry("EVENT MISSIONS", "이벤트 미션"),
        new Entry("Prototype event loop for future live ops.", "추후 라이브 운영을 위한 이벤트 기본 구조입니다."),
        new Entry("Limited-time mission and reward placeholder.", "기간 한정 미션과 보상 자리입니다."),
        new Entry("ACTIVE EVENT", "진행 중인 이벤트"),
        new Entry("Event data unavailable.", "이벤트 정보를 불러올 수 없습니다."),
        new Entry("DAILY PLAY EVENT", "일일 플레이 이벤트"),
        new Entry("Defeat monsters", "몬스터 처치"),
        new Entry("Recruit companions", "동료 모집"),
        new Entry("Reward claimed", "보상 수령 완료"),
        new Entry("Points", "포인트"),
        new Entry("pts", "점"),
        new Entry("Gacha Tickets", "뽑기 티켓"),
        new Entry("SETTINGS & OPTIONS", "설정과 옵션"),
        new Entry("Device, notification, language and frame-rate options.", "기기, 알림, 언어, 프레임 설정입니다."),
        new Entry("PREFERENCES", "환경 설정"),
        new Entry("Settings unavailable.", "설정을 불러올 수 없습니다."),
        new Entry("Settings", "설정"),
        new Entry("Sound", "소리"),
        new Entry("Vibration", "진동"),
        new Entry("Notifications", "알림"),
        new Entry("Frame Rate", "프레임"),
        new Entry("Language", "언어"),
        new Entry("ON", "켜짐"),
        new Entry("OFF", "꺼짐"),
        new Entry("WELCOME", "환영합니다"),
        new Entry("NEXT OBJECTIVE", "다음 목표"),
        new Entry("Welcome back!", "다시 오신 걸 환영합니다!"),
        new Entry("COLLECT REWARD", "보상 받기"),
        new Entry("Android build uses Google login or guest play.", "Android 빌드는 구글 로그인 또는 게스트 플레이를 사용합니다."),
        new Entry("Link Google to protect guest progress on Android.", "Android에서 게스트 진행도를 보호하려면 Google을 연동하세요."),
        new Entry("CURRENT ACCOUNT", "현재 계정"),
        new Entry("Account status", "계정 상태"),
        new Entry("Android build uses Google login only.\nLinking keeps the current UID and all guest progress.", "Android 빌드는 Google 로그인만 사용합니다.\n연동하면 현재 UID와 게스트 진행도가 유지됩니다."),
        new Entry("Companion banner placeholder. Art can be replaced later.", "동료 배너 자리입니다. 아트는 나중에 교체할 수 있습니다."),
        new Entry("ART NEEDED\nBanner Character / Featured Companions", "아트 필요\n배너 캐릭터 / 추천 동료"),
        new Entry("RESULT", "결과"),
        new Entry("Recruit companions to see results.", "동료를 모집하면 결과가 표시됩니다."),
        new Entry("Tickets are used before Gems.", "티켓을 먼저 사용하고, 부족하면 젬을 사용합니다."),
        new Entry("RECRUIT", "모집"),
        new Entry("Game data is not ready.", "게임 정보를 아직 불러오지 못했습니다."),
        new Entry("Need 1 Ticket or 100 Gems.", "티켓 1개 또는 젬 100개가 필요합니다."),
        new Entry("Need 10 Tickets or 900 Gems.", "티켓 10개 또는 젬 900개가 필요합니다."),
        new Entry("Used", "사용"),
        new Entry("ticket(s).", "티켓"),
        new Entry("Gems.", "젬"),
        new Entry("Recruitment failed. Cost refunded.", "모집에 실패했습니다. 비용을 돌려받았습니다."),
        new Entry("Could not return to battle.", "전투 화면으로 돌아갈 수 없습니다."),
        new Entry("SSR in", "SSR까지"),
        new Entry("RECRUIT 1\nTicket 1 / Gem 100", "1회 모집\n티켓 1 / 젬 100"),
        new Entry("RECRUIT 10\nTicket 10 / Gem 900", "10회 모집\n티켓 10 / 젬 900")
    };

    public static string Text(string english, string korean)
    {
        return GameSettingsManager.IsKoreanLanguage
            ? korean
            : english;
    }

    public static string Translate(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        foreach (Entry entry in Entries)
        {
            if (value == entry.English || value == entry.Korean)
            {
                return GameSettingsManager.IsKoreanLanguage
                    ? entry.Korean
                    : entry.English;
            }
        }

        return value;
    }

    public static void ApplyTo(Transform root)
    {
        if (root == null)
            return;

        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            text.text = Translate(text.text);
            GameFont.Apply(text);
        }
    }
}
