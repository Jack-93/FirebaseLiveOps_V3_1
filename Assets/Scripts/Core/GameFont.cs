using System;
using TMPro;
using UnityEngine;

public static class GameFont
{
    private const string PrimaryResourcePath = "Fonts/ONE-Mobile-POP";
    private const string TitleResourcePath = "Fonts/Jalnan2";
    private const string FallbackResourcePath = "Fonts/Jua-Regular";

    private static TMP_FontAsset primaryFontAsset;
    private static TMP_FontAsset titleFontAsset;

    public static TMP_FontAsset Primary
    {
        get
        {
            if (primaryFontAsset != null)
                return primaryFontAsset;

            primaryFontAsset = LoadRuntimeFont(
                PrimaryResourcePath,
                "ONE Mobile POP Runtime TMP Font") ??
                LoadRuntimeFont(
                    FallbackResourcePath,
                    "Jua Runtime TMP Font");
            return primaryFontAsset;
        }
    }

    public static TMP_FontAsset Title
    {
        get
        {
            if (titleFontAsset != null)
                return titleFontAsset;

            titleFontAsset = LoadRuntimeFont(
                TitleResourcePath,
                "Jalnan2 Runtime TMP Font") ??
                Primary;
            return titleFontAsset;
        }
    }

    public static TMP_FontAsset Damage => Primary;

    public static void Apply(TMP_Text text, string objectName = null)
    {
        TMP_FontAsset font = ShouldUseTitleFont(objectName)
            ? Title
            : Primary;
        if (text != null && font != null)
            text.font = font;
    }

    public static void ApplyDamage(TMP_Text text)
    {
        TMP_FontAsset font = Damage;
        if (text != null && font != null)
            text.font = font;
    }

    private static TMP_FontAsset LoadRuntimeFont(
        string resourcePath,
        string assetName)
    {
        Font font = Resources.Load<Font>(resourcePath);
        if (font == null)
            return null;

        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font);
        asset.name = assetName;
        asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        return asset;
    }

    private static bool ShouldUseTitleFont(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return false;

        return objectName.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0 ||
            objectName.IndexOf("Header", StringComparison.OrdinalIgnoreCase) >= 0 ||
            objectName.IndexOf("Stage", StringComparison.OrdinalIgnoreCase) >= 0 ||
            objectName.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
