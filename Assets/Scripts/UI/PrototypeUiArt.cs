using System.Collections.Generic;
using UnityEngine;

public static class PrototypeUiArt
{
    private const string Root = "PrototypeArt/UI/";
    private const string BannerRoot = "PrototypeArt/Banners/";
    private const string KenneyGameRoot = Root + "KenneyIcons/Game/";
    private const string KenneyBoardRoot = Root + "KenneyIcons/Board/";

    private static readonly Dictionary<string, Sprite> Cache =
        new Dictionary<string, Sprite>();

    public static Sprite PanelFrame => Load("PanelFrame");
    public static Sprite ButtonNormal => Load("ButtonNormal");
    public static Sprite ButtonSelected => Load("ButtonSelected");
    public static Sprite SkillFrame => Load("SkillFrame");
    public static Sprite ActorShadow => Load("ActorShadow");
    public static Sprite LockIcon => Load("IconLock");
    public static Sprite PlusIcon =>
        LoadFirst(GameIconPath("Game_plus"), UiPath("IconPlus"));
    public static Sprite GoldIcon =>
        LoadFirst(BoardIconPath("Board_pouch"), UiPath("IconGold"));
    public static Sprite StandardGachaBanner =>
        LoadResource(BannerRoot + "StandardRecruitment");

    public static Sprite GetButtonIcon(string buttonName)
    {
        if (string.IsNullOrEmpty(buttonName))
            return null;

        if (IsSkillButton(buttonName))
            return null;

        if (buttonName.Contains("Gold"))
            return Load("IconGold");
        if (buttonName.Contains("Gem"))
            return Load("IconGem");
        if (buttonName.Contains("Ticket"))
            return LoadFirst(
                BoardIconPath("Board_cards_collection"),
                UiPath("IconTicket"));
        if (buttonName.Contains("Mail"))
            return LoadFirst(
                GameIconPath("Game_exclamation"),
                UiPath("IconEvent"));
        if (buttonName.Contains("Google") ||
            buttonName.Contains("Guest") ||
            buttonName.Contains("Account"))
            return LoadFirst(
                GameIconPath("Game_home"),
                UiPath("IconMenu"));
        if (buttonName.Contains("Sound"))
            return LoadFirst(
                GameIconPath("Game_audioOn"),
                UiPath("IconMenu"));
        if (buttonName.Contains("Vibration") ||
            buttonName.Contains("Notifications") ||
            buttonName.Contains("FrameRate") ||
            buttonName.Contains("Language") ||
            buttonName.Contains("Settings"))
            return LoadFirst(
                GameIconPath("Game_gear"),
                UiPath("IconMenu"));
        if (buttonName.Contains("StarterPack") ||
            buttonName.Contains("RewardedAd"))
            return LoadFirst(
                BoardIconPath("Board_pouch"),
                UiPath("IconShop"));
        if (buttonName.Contains("Weapon"))
            return LoadFirst(
                BoardIconPath("Board_sword"),
                UiPath("IconEquipment"));
        if (buttonName.Contains("Armor"))
            return LoadFirst(
                BoardIconPath("Board_shield"),
                UiPath("IconEquipment"));
        if (buttonName.Contains("Battle"))
            return LoadFirst(
                BoardIconPath("Board_sword"),
                UiPath("IconBattle"));
        if (buttonName.Contains("Growth"))
            return LoadFirst(
                BoardIconPath("Board_award"),
                UiPath("IconGrowth"));
        if (buttonName.Contains("Gacha"))
            return LoadFirst(
                BoardIconPath("Board_cards_stack"),
                UiPath("IconGacha"));
        if (buttonName.Contains("Collection") ||
            buttonName.Contains("Companion"))
            return LoadFirst(
                BoardIconPath("Board_character"),
                UiPath("IconCompanions"));
        if (buttonName.Contains("More") ||
            buttonName.Contains("Menu"))
            return LoadFirst(
                GameIconPath("Game_barsHorizontal"),
                UiPath("IconMenu"));
        if (buttonName.Contains("Quest"))
            return LoadFirst(
                BoardIconPath("Board_book_open"),
                UiPath("IconQuest"));
        if (buttonName.Contains("Shop"))
            return LoadFirst(
                BoardIconPath("Board_pouch"),
                UiPath("IconShop"));
        if (buttonName.Contains("Equipment"))
            return LoadFirst(
                BoardIconPath("Board_sword"),
                UiPath("IconEquipment"));
        if (buttonName.Contains("Event"))
            return LoadFirst(
                GameIconPath("Game_star"),
                BoardIconPath("Board_crown_a"),
                UiPath("IconEvent"));

        return null;
    }

    public static bool IsSkillButton(string buttonName)
    {
        return !string.IsNullOrEmpty(buttonName) &&
               buttonName.Contains("SkillButton");
    }

    public static bool ShouldDecoratePanel(string panelName)
    {
        if (string.IsNullOrEmpty(panelName))
            return false;

        if (panelName == "EnemyCard" || panelName == "PlayerCard")
            return false;

        return panelName == "TopBar" ||
               panelName == "BottomNavigation" ||
               panelName == "Banner" ||
               panelName == "TutorialPanel" ||
               panelName == "DialoguePanel" ||
               panelName.EndsWith("Card") ||
               panelName.EndsWith("Popup") ||
               panelName.EndsWith("Row");
    }

    private static Sprite Load(string name)
    {
        return LoadResource(UiPath(name));
    }

    private static string UiPath(string name)
    {
        return Root + name;
    }

    private static string GameIconPath(string name)
    {
        return KenneyGameRoot + name;
    }

    private static string BoardIconPath(string name)
    {
        return KenneyBoardRoot + name;
    }

    private static Sprite LoadFirst(params string[] paths)
    {
        foreach (string path in paths)
        {
            Sprite sprite = LoadResource(path);
            if (sprite != null)
                return sprite;
        }

        return null;
    }

    private static Sprite LoadResource(string path)
    {
        if (Cache.TryGetValue(path, out Sprite sprite))
            return sprite;

        sprite = Resources.Load<Sprite>(path);
        Cache[path] = sprite;
        return sprite;
    }
}
