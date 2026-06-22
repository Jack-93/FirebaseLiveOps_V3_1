using UnityEditor;
using UnityEngine;

public sealed class PrototypeArtImporter : AssetPostprocessor
{
    private const string Root =
        "Assets/Resources/PrototypeArt/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Root))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        bool isActor = assetPath.Contains("/Heroes/") ||
                       assetPath.Contains("/Companions/") ||
                       assetPath.Contains("/Enemies/");
        bool isUi = assetPath.Contains("/UI/");
        importer.maxTextureSize = isActor || isUi
            ? 256
            : 1024;
        importer.filterMode = isActor || isUi
            ? FilterMode.Point
            : FilterMode.Bilinear;
        importer.textureCompression = isUi
            ? TextureImporterCompression.Uncompressed
            : TextureImporterCompression.Compressed;

        string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (fileName == "PanelFrame")
            importer.spriteBorder = new Vector4(58f, 58f, 58f, 58f);
        else if (fileName == "ButtonNormal" || fileName == "ButtonSelected")
            importer.spriteBorder = new Vector4(48f, 48f, 48f, 48f);
    }

    [MenuItem("Tools/Prototype Art/Reimport UI Sprites")]
    private static void ReimportUiSprites()
    {
        ReimportSprites(Root + "UI");
    }

    [MenuItem("Tools/Prototype Art/Reimport All Sprites")]
    private static void ReimportAllSprites()
    {
        ReimportSprites(Root);
    }

    private static void ReimportSprites(string folder)
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:Texture2D",
            new[] { folder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.Refresh();
    }
}
