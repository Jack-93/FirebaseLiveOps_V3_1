using System.Collections.Generic;
using UnityEngine;

public static class PrototypeUiArt
{
    private const string Root = "PrototypeArt/UI/";
    private const string BannerRoot = "PrototypeArt/Banners/";

    private static readonly Dictionary<string, Sprite> Cache =
        new Dictionary<string, Sprite>();

    public static Sprite PanelFrame => Load("PanelFrame");
    public static Sprite ButtonNormal => Load("ButtonNormal");
    public static Sprite ButtonSelected => Load("ButtonSelected");
    public static Sprite SkillFrame => Load("SkillFrame");
    public static Sprite GoldIcon => Load("IconGold");
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
            return Load("IconTicket");
        if (buttonName.Contains("Mail"))
            return Load("IconEvent");
        if (buttonName.Contains("Google") ||
            buttonName.Contains("Guest") ||
            buttonName.Contains("Account"))
            return Load("IconMenu");
        if (buttonName.Contains("Sound") ||
            buttonName.Contains("Vibration") ||
            buttonName.Contains("Notifications") ||
            buttonName.Contains("FrameRate") ||
            buttonName.Contains("Language") ||
            buttonName.Contains("Settings"))
            return Load("IconMenu");
        if (buttonName.Contains("StarterPack") ||
            buttonName.Contains("RewardedAd"))
            return Load("IconShop");
        if (buttonName.Contains("Weapon") ||
            buttonName.Contains("Armor"))
            return Load("IconEquipment");
        if (buttonName.Contains("Battle"))
            return Load("IconBattle");
        if (buttonName.Contains("Growth"))
            return Load("IconGrowth");
        if (buttonName.Contains("Gacha"))
            return Load("IconGacha");
        if (buttonName.Contains("Collection") ||
            buttonName.Contains("Companion"))
            return Load("IconCompanions");
        if (buttonName.Contains("More") ||
            buttonName.Contains("Menu"))
            return Load("IconMenu");
        if (buttonName.Contains("Quest"))
            return Load("IconQuest");
        if (buttonName.Contains("Shop"))
            return Load("IconShop");
        if (buttonName.Contains("Equipment"))
            return Load("IconEquipment");
        if (buttonName.Contains("Event"))
            return Load("IconEvent");

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
        return LoadResource(Root + name);
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
