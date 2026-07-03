using UnityEditor;
using UnityEngine;

public static class BattleVisualMigrationTools
{
    [MenuItem("Tools/Battle/Migrate Legacy Visual Fields")]
    public static void MigrateLegacyVisualFields()
    {
        int changed = 0;
        changed += MigrateCharacters();
        changed += MigrateBattleVisualDatabases();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BattleVisual] Migrated visual assets: " + changed);
    }

    private static int MigrateCharacters()
    {
        int changed = 0;
        string[] guids = AssetDatabase.FindAssets("t:CharacterData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterData character =
                AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (character == null)
                continue;

            if (character.battleVisual == null)
                character.battleVisual = new BattleActorVisualSet();

            if (BattleActorVisualSet.IsConfigured(character.battleVisual))
                continue;

            character.battleVisual = BattleActorVisualSet.FromLegacy(
                character.battleSprite ?? character.icon,
                character.battleAnimator,
                character.basicProjectileSprite,
                character.skillProjectileSprite,
                character.basicProjectileTint,
                character.skillProjectileTint);
            EditorUtility.SetDirty(character);
            changed++;
        }

        return changed;
    }

    private static int MigrateBattleVisualDatabases()
    {
        int changed = 0;
        string[] guids = AssetDatabase.FindAssets("t:BattleVisualDatabase");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BattleVisualDatabase database =
                AssetDatabase.LoadAssetAtPath<BattleVisualDatabase>(path);
            if (database == null)
                continue;

            bool databaseChanged = false;
            databaseChanged |= MigrateProfile(database.hero);
            if (database.normalEnemies != null)
            {
                foreach (BattleVisualProfile profile in database.normalEnemies)
                    databaseChanged |= MigrateProfile(profile);
            }
            if (database.bosses != null)
            {
                foreach (BattleVisualProfile profile in database.bosses)
                    databaseChanged |= MigrateProfile(profile);
            }

            if (!databaseChanged)
                continue;

            EditorUtility.SetDirty(database);
            changed++;
        }

        return changed;
    }

    private static bool MigrateProfile(BattleVisualProfile profile)
    {
        if (profile == null)
            return false;

        if (profile.visual == null)
            profile.visual = new BattleActorVisualSet();

        if (BattleActorVisualSet.IsConfigured(profile.visual) ||
            (profile.sprite == null && profile.animatorController == null))
        {
            return false;
        }

        profile.visual = BattleActorVisualSet.FromLegacy(
            profile.sprite,
            profile.animatorController);
        return true;
    }
}
