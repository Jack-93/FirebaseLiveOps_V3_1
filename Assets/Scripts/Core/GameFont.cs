using TMPro;
using UnityEngine;

public static class GameFont
{
    private const string JuaResourcePath = "Fonts/Jua-Regular";

    private static TMP_FontAsset primaryFontAsset;

    public static TMP_FontAsset Primary
    {
        get
        {
            if (primaryFontAsset != null)
                return primaryFontAsset;

            Font font = Resources.Load<Font>(JuaResourcePath);
            if (font == null)
                return null;

            primaryFontAsset = TMP_FontAsset.CreateFontAsset(font);
            primaryFontAsset.name = "Jua Runtime TMP Font";
            primaryFontAsset.atlasPopulationMode =
                AtlasPopulationMode.Dynamic;
            return primaryFontAsset;
        }
    }

    public static void Apply(TMP_Text text)
    {
        TMP_FontAsset font = Primary;
        if (text != null && font != null)
            text.font = font;
    }
}
