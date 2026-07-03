using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class ProductionArtImporter : AssetPostprocessor
{
    private const string Root = "Assets/Art/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Root))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = GetMaxTextureSize(assetPath);

        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        if (assetPath.Contains("/UI/Frames/") ||
            fileName.EndsWith("Frame"))
        {
            importer.spriteBorder = new Vector4(32f, 32f, 32f, 32f);
        }
    }

    private static int GetMaxTextureSize(string path)
    {
        if (path.Contains("/Backgrounds/"))
            return 2048;

        if (path.Contains("/UI/") || path.Contains("/Projectiles/"))
            return 512;

        return 256;
    }
}
