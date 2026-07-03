using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class JackTestCharacterCreator
{
    private const int CanvasSize = 64;
    private const string CharacterName = "Jack";
    private const string CharacterAssetPath = "Assets/Characters/Jack.asset";
    private const string CharacterDatabasePath =
        "Assets/Resources/CharacterDatabase.asset";
    private const string JackArtRoot =
        "Assets/Art/Battle/Companions/Jack";

    private static readonly Color32 Clear = new Color32(0, 0, 0, 0);
    private static readonly Color32 Outline = new Color32(30, 36, 48, 255);
    private static readonly Color32 FeatherBlack = new Color32(43, 49, 67, 255);
    private static readonly Color32 FeatherBlue = new Color32(50, 114, 150, 255);
    private static readonly Color32 FeatherWhite = new Color32(237, 237, 221, 255);
    private static readonly Color32 Belly = new Color32(252, 248, 229, 255);
    private static readonly Color32 Beak = new Color32(238, 171, 61, 255);
    private static readonly Color32 Foot = new Color32(182, 120, 54, 255);
    private static readonly Color32 Electric = new Color32(92, 231, 255, 255);
    private static readonly Color32 ElectricLight = new Color32(204, 249, 255, 255);
    private static readonly Color32 HitTint = new Color32(255, 104, 111, 180);

    [MenuItem("Tools/Battle Art/Create Jack Test Character")]
    public static void CreateJackTestCharacter()
    {
        EnsureFolders();
        CreateSprites();

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        CharacterData jack = LoadOrCreateJack();
        ApplyJackData(jack);
        AddToCharacterDatabase(jack);

        EditorUtility.SetDirty(jack);
        AssetDatabase.SaveAssets();

        BattleArtPipelineTools.AutoLinkBattleVisuals();

        Debug.Log(
            "[JackTestCharacterCreator] Created SR magpie test character: Jack");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Characters");
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Art");
        EnsureFolder("Assets/Art/Battle");
        EnsureFolder("Assets/Art/Battle/Companions");
        EnsureFolder(JackArtRoot);

        foreach (BattleAnimationCue cue in
                 System.Enum.GetValues(typeof(BattleAnimationCue)))
        {
            EnsureFolder(JackArtRoot + "/" + cue);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string name = Path.GetFileName(path);
        if (!string.IsNullOrWhiteSpace(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, name);
    }

    private static void CreateSprites()
    {
        if (File.Exists(ToFullPath(JackArtRoot + "/Jack.png")))
        {
            Debug.Log(
                "[JackTestCharacterCreator] Existing Jack art found. " +
                "Skipping prototype sprite generation.");
            return;
        }

        SaveFrame(JackArtRoot + "/Jack.png", PoseKind.Idle, 0);
        SaveProjectile(JackArtRoot + "/BasicProjectile.png", false);
        SaveProjectile(JackArtRoot + "/SkillProjectile.png", true);

        SaveCue(BattleAnimationCue.Idle, PoseKind.Idle, 4);
        SaveCue(BattleAnimationCue.Attack, PoseKind.Attack, 4);
        SaveCue(BattleAnimationCue.Hit, PoseKind.Hit, 4);
        SaveCue(BattleAnimationCue.Death, PoseKind.Death, 4);
        SaveCue(BattleAnimationCue.Skill, PoseKind.Skill, 6);
    }

    private static void SaveCue(
        BattleAnimationCue cue,
        PoseKind pose,
        int frameCount)
    {
        for (int i = 0; i < frameCount; i++)
        {
            SaveFrame(
                JackArtRoot + "/" + cue + "/Jack_" +
                cue + "_" + (i + 1).ToString("00") + ".png",
                pose,
                i);
        }
    }

    private static void SaveFrame(string path, PoseKind pose, int frame)
    {
        Texture2D texture = NewTexture();
        if (pose == PoseKind.Death)
            DrawDeath(texture, frame);
        else
            DrawMagpie(texture, pose, frame);

        WritePng(texture, path);
        Object.DestroyImmediate(texture);
    }

    private static Texture2D NewTexture()
    {
        Texture2D texture = new Texture2D(
            CanvasSize,
            CanvasSize,
            TextureFormat.RGBA32,
            false);
        texture.filterMode = FilterMode.Point;

        Color32[] pixels = new Color32[CanvasSize * CanvasSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Clear;

        texture.SetPixels32(pixels);
        return texture;
    }

    private static void DrawMagpie(
        Texture2D texture,
        PoseKind pose,
        int frame)
    {
        int bob = pose == PoseKind.Idle
            ? new[] { 0, 1, 0, -1 }[frame % 4]
            : 0;
        int lean = pose == PoseKind.Attack
            ? new[] { 0, 2, 5, 2 }[frame % 4]
            : 0;
        int hitShake = pose == PoseKind.Hit
            ? new[] { -2, 2, -1, 1 }[frame % 4]
            : 0;
        int x = 31 + lean + hitShake;
        int y = 30 + bob;

        if (pose == PoseKind.Skill)
            DrawSkillAura(texture, x, y, frame);

        DrawTail(texture, x - 16, y - 3, pose, frame);
        DrawWing(texture, x - 6, y + 2, pose, frame);

        FillEllipse(texture, x - 2, y, 15, 12, Outline);
        FillEllipse(texture, x - 2, y + 1, 13, 10, FeatherWhite);
        FillEllipse(texture, x + 3, y - 1, 8, 6, Belly);

        FillCircle(texture, x + 9, y + 10, 10, Outline);
        FillCircle(texture, x + 9, y + 10, 8, FeatherBlack);
        FillEllipse(texture, x + 12, y + 7, 5, 4, FeatherWhite);

        FillRect(texture, x + 14, y + 13, 2, 2, Electric);
        SetPixel(texture, x + 13, y + 15, ElectricLight);
        FillCircle(texture, x + 12, y + 12, 2, Color.white);
        SetPixel(texture, x + 13, y + 12, Outline);

        FillTriangle(
            texture,
            new Vector2Int(x + 17, y + 9),
            new Vector2Int(x + 25, y + 11),
            new Vector2Int(x + 17, y + 6),
            Beak);

        DrawScarf(texture, x, y, frame);
        DrawFeet(texture, x, y);

        if (pose == PoseKind.Attack)
            DrawAttackEffect(texture, x + 20, y + 9, frame);
        else if (pose == PoseKind.Hit)
            DrawHitEffect(texture, x, y, frame);
    }

    private static void DrawTail(
        Texture2D texture,
        int x,
        int y,
        PoseKind pose,
        int frame)
    {
        int lift = pose == PoseKind.Skill ? 4 : 0;
        int wag = pose == PoseKind.Idle ? new[] { 0, 1, 0, -1 }[frame % 4] : 0;
        FillTriangle(
            texture,
            new Vector2Int(x - 17, y + 5 + wag + lift),
            new Vector2Int(x + 2, y + 8 + lift),
            new Vector2Int(x - 2, y + 1 + wag),
            Outline);
        FillTriangle(
            texture,
            new Vector2Int(x - 15, y + 5 + wag + lift),
            new Vector2Int(x + 1, y + 7 + lift),
            new Vector2Int(x - 3, y + 2 + wag),
            FeatherBlue);
        FillRect(texture, x - 12, y + 6 + lift, 10, 2, FeatherBlack);
    }

    private static void DrawWing(
        Texture2D texture,
        int x,
        int y,
        PoseKind pose,
        int frame)
    {
        if (pose == PoseKind.Skill)
        {
            FillTriangle(
                texture,
                new Vector2Int(x - 7, y + 10),
                new Vector2Int(x + 10, y + 17),
                new Vector2Int(x + 2, y - 2),
                Outline);
            FillTriangle(
                texture,
                new Vector2Int(x - 5, y + 10),
                new Vector2Int(x + 8, y + 15),
                new Vector2Int(x + 2, y),
                FeatherBlue);
            return;
        }

        if (pose == PoseKind.Attack && frame % 2 == 1)
        {
            FillTriangle(
                texture,
                new Vector2Int(x - 11, y + 8),
                new Vector2Int(x + 8, y + 14),
                new Vector2Int(x + 4, y - 3),
                Outline);
            FillTriangle(
                texture,
                new Vector2Int(x - 9, y + 8),
                new Vector2Int(x + 6, y + 12),
                new Vector2Int(x + 3, y - 1),
                FeatherBlue);
            return;
        }

        FillEllipse(texture, x, y, 8, 12, Outline);
        FillEllipse(texture, x + 1, y, 6, 10, FeatherBlue);
        FillRect(texture, x - 2, y - 3, 7, 3, FeatherBlack);
    }

    private static void DrawScarf(Texture2D texture, int x, int y, int frame)
    {
        FillRect(texture, x + 3, y + 6, 11, 3, Electric);
        FillRect(texture, x - 2, y + 5, 7, 2, ElectricLight);
        if (frame % 2 == 0)
            FillRect(texture, x - 8, y + 4, 7, 2, Electric);
    }

    private static void DrawFeet(Texture2D texture, int x, int y)
    {
        FillRect(texture, x - 3, y - 12, 3, 6, Foot);
        FillRect(texture, x + 7, y - 12, 3, 6, Foot);
        FillRect(texture, x - 5, y - 13, 6, 2, Foot);
        FillRect(texture, x + 5, y - 13, 6, 2, Foot);
    }

    private static void DrawAttackEffect(
        Texture2D texture,
        int x,
        int y,
        int frame)
    {
        int length = 8 + frame * 4;
        DrawLine(texture, x, y, x + length, y + 3, ElectricLight);
        DrawLine(texture, x + 2, y - 2, x + length - 2, y + 1, Electric);
        FillCircle(texture, x + length, y + 3, 2, ElectricLight);
    }

    private static void DrawHitEffect(
        Texture2D texture,
        int x,
        int y,
        int frame)
    {
        FillEllipse(texture, x + 4, y + 4, 18, 14, HitTint);
        DrawLine(texture, x - 16, y + 12, x - 9, y + 18, Color.red);
        if (frame % 2 == 0)
            DrawLine(texture, x + 14, y + 18, x + 20, y + 23, Color.red);
    }

    private static void DrawSkillAura(
        Texture2D texture,
        int x,
        int y,
        int frame)
    {
        FillCircle(texture, x + 4, y + 5, 24, new Color32(31, 164, 212, 48));
        DrawLine(texture, x - 17, y + 20, x - 8, y + 28, Electric);
        DrawLine(texture, x - 8, y + 28, x - 2, y + 19, ElectricLight);
        DrawLine(texture, x + 16, y + 21, x + 23, y + 30, Electric);
        DrawLine(texture, x + 23, y + 30, x + 31, y + 18, ElectricLight);
        if (frame % 2 == 0)
            FillCircle(texture, x - 17, y + 20, 2, ElectricLight);
    }

    private static void DrawDeath(Texture2D texture, int frame)
    {
        int x = 32;
        int y = 20 + Mathf.Max(0, 2 - frame);

        DrawLine(texture, x - 22, y - 2, x - 10, y + 2, Outline);
        DrawLine(texture, x - 22, y - 1, x - 9, y + 3, FeatherBlue);
        FillEllipse(texture, x - 1, y, 16, 9, Outline);
        FillEllipse(texture, x - 1, y + 1, 14, 7, FeatherWhite);
        FillCircle(texture, x + 14, y + 5, 8, Outline);
        FillCircle(texture, x + 14, y + 5, 6, FeatherBlack);
        DrawLine(texture, x + 11, y + 8, x + 15, y + 4, Color.white);
        DrawLine(texture, x + 15, y + 8, x + 11, y + 4, Color.white);
        FillTriangle(
            texture,
            new Vector2Int(x + 19, y + 5),
            new Vector2Int(x + 25, y + 7),
            new Vector2Int(x + 19, y + 3),
            Beak);
        FillRect(texture, x - 2, y - 7, 9, 2, Foot);
        if (frame == 0)
            DrawLine(texture, x + 5, y + 16, x + 11, y + 21, Electric);
    }

    private static void SaveProjectile(string path, bool isSkill)
    {
        Texture2D texture = NewTexture();

        if (isSkill)
        {
            FillCircle(texture, 31, 32, 10, new Color32(54, 177, 224, 190));
            FillCircle(texture, 31, 32, 6, ElectricLight);
            DrawLine(texture, 18, 32, 27, 42, Electric);
            DrawLine(texture, 27, 42, 24, 33, Color.white);
            DrawLine(texture, 37, 22, 44, 32, Electric);
            DrawLine(texture, 44, 32, 38, 31, Color.white);
        }
        else
        {
            FillTriangle(
                texture,
                new Vector2Int(14, 26),
                new Vector2Int(43, 37),
                new Vector2Int(24, 39),
                ElectricLight);
            DrawLine(texture, 17, 25, 41, 36, Electric);
            DrawLine(texture, 17, 38, 41, 36, Electric);
            FillCircle(texture, 44, 36, 2, Color.white);
        }

        WritePng(texture, path);
        Object.DestroyImmediate(texture);
    }

    private static CharacterData LoadOrCreateJack()
    {
        CharacterData jack =
            AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterAssetPath);
        if (jack != null)
            return jack;

        jack = ScriptableObject.CreateInstance<CharacterData>();
        AssetDatabase.CreateAsset(jack, CharacterAssetPath);
        return jack;
    }

    private static void ApplyJackData(CharacterData jack)
    {
        jack.characterName = CharacterName;
        jack.rarity = "SR";
        jack.element = CompanionElement.Light;
        jack.role = CompanionRole.Striker;
        jack.synergyTags = new List<string>
        {
            "Bird",
            "Electric",
            "Magpie"
        };
        jack.description =
            "SR magpie companion prototype. A fast rooftop striker that fires charged feather shots.";
        jack.skillName = "Magpie Spark";
        jack.skillCooldown = 7.5f;
        jack.skillDamageMultiplier = 2.2f;
    }

    private static void AddToCharacterDatabase(CharacterData jack)
    {
        CharacterDatabase database =
            AssetDatabase.LoadAssetAtPath<CharacterDatabase>(
                CharacterDatabasePath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<CharacterDatabase>();
            AssetDatabase.CreateAsset(database, CharacterDatabasePath);
        }

        if (database.characters == null)
            database.characters = new List<CharacterData>();

        if (!database.characters.Contains(jack))
            database.characters.Add(jack);

        EditorUtility.SetDirty(database);
    }

    private static void WritePng(Texture2D texture, string assetPath)
    {
        string fullPath = ToFullPath(assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
    }

    private static string ToFullPath(string assetPath)
    {
        return Path.Combine(
            Directory.GetCurrentDirectory(),
            assetPath);
    }

    private static void FillRect(
        Texture2D texture,
        int x,
        int y,
        int width,
        int height,
        Color color)
    {
        for (int px = x; px < x + width; px++)
        for (int py = y; py < y + height; py++)
            SetPixel(texture, px, py, color);
    }

    private static void FillCircle(
        Texture2D texture,
        int cx,
        int cy,
        int radius,
        Color color)
    {
        int rr = radius * radius;
        for (int x = cx - radius; x <= cx + radius; x++)
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            int dx = x - cx;
            int dy = y - cy;
            if (dx * dx + dy * dy <= rr)
                SetPixel(texture, x, y, color);
        }
    }

    private static void FillEllipse(
        Texture2D texture,
        int cx,
        int cy,
        int rx,
        int ry,
        Color color)
    {
        int rxx = rx * rx;
        int ryy = ry * ry;
        int threshold = rxx * ryy;
        for (int x = cx - rx; x <= cx + rx; x++)
        for (int y = cy - ry; y <= cy + ry; y++)
        {
            int dx = x - cx;
            int dy = y - cy;
            if (dx * dx * ryy + dy * dy * rxx <= threshold)
                SetPixel(texture, x, y, color);
        }
    }

    private static void FillTriangle(
        Texture2D texture,
        Vector2Int a,
        Vector2Int b,
        Vector2Int c,
        Color color)
    {
        int minX = Mathf.Min(a.x, Mathf.Min(b.x, c.x));
        int maxX = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
        int minY = Mathf.Min(a.y, Mathf.Min(b.y, c.y));
        int maxY = Mathf.Max(a.y, Mathf.Max(b.y, c.y));

        for (int x = minX; x <= maxX; x++)
        for (int y = minY; y <= maxY; y++)
        {
            Vector2Int p = new Vector2Int(x, y);
            if (IsInsideTriangle(p, a, b, c))
                SetPixel(texture, x, y, color);
        }
    }

    private static bool IsInsideTriangle(
        Vector2Int p,
        Vector2Int a,
        Vector2Int b,
        Vector2Int c)
    {
        int d1 = Sign(p, a, b);
        int d2 = Sign(p, b, c);
        int d3 = Sign(p, c, a);

        bool hasNegative = d1 < 0 || d2 < 0 || d3 < 0;
        bool hasPositive = d1 > 0 || d2 > 0 || d3 > 0;
        return !(hasNegative && hasPositive);
    }

    private static int Sign(Vector2Int p1, Vector2Int p2, Vector2Int p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) -
               (p2.x - p3.x) * (p1.y - p3.y);
    }

    private static void DrawLine(
        Texture2D texture,
        int x0,
        int y0,
        int x1,
        int y1,
        Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;

        while (true)
        {
            SetPixel(texture, x0, y0, color);
            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    private static void SetPixel(
        Texture2D texture,
        int x,
        int y,
        Color color)
    {
        if (x < 0 || y < 0 || x >= CanvasSize || y >= CanvasSize)
            return;

        texture.SetPixel(x, y, color);
    }

    private enum PoseKind
    {
        Idle,
        Attack,
        Hit,
        Death,
        Skill
    }
}
