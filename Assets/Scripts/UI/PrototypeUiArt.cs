using System.Collections.Generic;
using UnityEngine;

public static class PrototypeUiArt
{
    private const string Root = "PrototypeArt/UI/";

    private static readonly Dictionary<string, Sprite> Cache =
        new Dictionary<string, Sprite>();

    public static Sprite PanelFrame => Load("PanelFrame");
    public static Sprite ButtonNormal => Load("ButtonNormal");
    public static Sprite ButtonSelected => Load("ButtonSelected");
    public static Sprite SkillFrame => Load("SkillFrame");
    public static Sprite GoldIcon => Load("IconGold");

    public static Sprite GetButtonIcon(string buttonName)
    {
        if (string.IsNullOrEmpty(buttonName))
            return null;

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
        if (Cache.TryGetValue(name, out Sprite sprite))
            return sprite;

        sprite = Resources.Load<Sprite>(Root + name);
        Cache[name] = sprite;
        return sprite;
    }
}
