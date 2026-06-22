using UnityEngine;

public static class BattleLayoutConfig
{
    // Normalized positions inside the full portrait battle artwork.
    public static readonly Vector2 SupportSparrowAnchor =
        new Vector2(0.18f, 0.43f);

    public static readonly Vector2[] CompanionAnchors =
    {
        new Vector2(0.50f, 0.62f),
        new Vector2(0.56f, 0.51f),
        new Vector2(0.61f, 0.40f)
    };

    public static readonly Vector2 EnemyAnchor =
        new Vector2(0.84f, 0.71f);

    public static Vector2 GetCompanionAnchor(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < CompanionAnchors.Length
            ? CompanionAnchors[slotIndex]
            : CompanionAnchors[0];
    }
}
