using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleVisualProfile
{
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;
}

[CreateAssetMenu(
    fileName = "BattleVisualDatabase",
    menuName = "Game/Battle Visual Database")]
public class BattleVisualDatabase : ScriptableObject
{
    public BattleVisualProfile hero = new BattleVisualProfile();
    public List<BattleVisualProfile> normalEnemies =
        new List<BattleVisualProfile>();
    public List<BattleVisualProfile> bosses =
        new List<BattleVisualProfile>();

    public BattleVisualProfile GetEnemy(int stage, bool boss)
    {
        List<BattleVisualProfile> profiles =
            boss ? bosses : normalEnemies;
        if (profiles == null || profiles.Count == 0)
            return null;

        int index = Mathf.Abs(stage - 1) % profiles.Count;
        return profiles[index];
    }
}

public static class BattleVisualResolver
{
    private static BattleVisualDatabase database;

    public static BattleVisualProfile GetHero()
    {
        LoadDatabase();
        if (database?.hero != null &&
            (database.hero.sprite != null ||
             database.hero.animatorController != null))
        {
            return database.hero;
        }

        return LoadProfile("Battle/Hero");
    }

    public static BattleVisualProfile GetEnemy(int stage, bool boss)
    {
        LoadDatabase();
        BattleVisualProfile profile =
            database?.GetEnemy(stage, boss);
        if (profile != null &&
            (profile.sprite != null ||
             profile.animatorController != null))
        {
            return profile;
        }

        string type = boss ? "Bosses/Boss" : "Enemies/Enemy";
        int index = Mathf.Abs(stage - 1) % 4 + 1;
        return LoadProfile($"Battle/{type}_{index}");
    }

    private static BattleVisualProfile LoadProfile(string resourcePath)
    {
        return new BattleVisualProfile
        {
            sprite = Resources.Load<Sprite>(resourcePath),
            animatorController =
                Resources.Load<RuntimeAnimatorController>(
                    resourcePath + "_Animator")
        };
    }

    private static void LoadDatabase()
    {
        if (database == null)
        {
            database = Resources.Load<BattleVisualDatabase>(
                "BattleVisualDatabase");
        }
    }
}
