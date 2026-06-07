using System.Collections.Generic;
using System.Text;

public sealed class CompanionSynergyResult
{
    public int AttackPercent;
    public int HealthPercent;
    public int AttackSpeedPercent;
    public readonly List<string> ActiveEffects = new List<string>();

    public string GetSummary()
    {
        if (ActiveEffects.Count == 0)
            return "Synergy: None";

        StringBuilder builder = new StringBuilder("Synergy: ");
        for (int index = 0; index < ActiveEffects.Count; index++)
        {
            if (index > 0)
                builder.Append(", ");
            builder.Append(ActiveEffects[index]);
        }

        return builder.ToString();
    }
}

public static class CompanionSynergySystem
{
    public static CompanionSynergyResult Calculate(
        IReadOnlyList<CharacterData> party)
    {
        CompanionSynergyResult result = new CompanionSynergyResult();
        if (party == null || party.Count == 0)
            return result;

        Dictionary<CompanionElement, int> elements =
            new Dictionary<CompanionElement, int>();
        Dictionary<CompanionRole, int> roles =
            new Dictionary<CompanionRole, int>();
        Dictionary<string, int> tags =
            new Dictionary<string, int>();

        foreach (CharacterData character in party)
        {
            if (character == null)
                continue;

            AddCount(elements, character.element);
            AddCount(roles, character.role);

            if (character.synergyTags == null)
                continue;

            foreach (string tag in character.synergyTags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                    AddCount(tags, tag);
            }
        }

        foreach (KeyValuePair<CompanionElement, int> entry in elements)
        {
            if (entry.Key == CompanionElement.None || entry.Value < 2)
                continue;

            int bonus = entry.Value >= 3 ? 18 : 8;
            result.AttackPercent += bonus;
            result.ActiveEffects.Add(
                $"{entry.Key} {entry.Value}: ATK +{bonus}%");
        }

        ApplyRoleSynergy(result, roles, CompanionRole.Striker);
        ApplyRoleSynergy(result, roles, CompanionRole.Guardian);
        ApplyRoleSynergy(result, roles, CompanionRole.Support);

        foreach (KeyValuePair<string, int> entry in tags)
        {
            if (entry.Value < 2)
                continue;

            result.AttackPercent += 5;
            result.ActiveEffects.Add($"{entry.Key}: ATK +5%");
        }

        if (party.Count >= 3 && elements.Count >= 3)
        {
            result.HealthPercent += 10;
            result.ActiveEffects.Add("Elemental Harmony: HP +10%");
        }

        return result;
    }

    private static void ApplyRoleSynergy(
        CompanionSynergyResult result,
        Dictionary<CompanionRole, int> roles,
        CompanionRole role)
    {
        if (!roles.TryGetValue(role, out int count) || count < 2)
            return;

        bool fullParty = count >= 3;
        switch (role)
        {
            case CompanionRole.Striker:
                int attack = fullParty ? 15 : 7;
                result.AttackPercent += attack;
                result.ActiveEffects.Add(
                    $"Strikers {count}: ATK +{attack}%");
                break;
            case CompanionRole.Guardian:
                int health = fullParty ? 30 : 15;
                result.HealthPercent += health;
                result.ActiveEffects.Add(
                    $"Guardians {count}: HP +{health}%");
                break;
            case CompanionRole.Support:
                int speed = fullParty ? 25 : 12;
                result.AttackSpeedPercent += speed;
                result.ActiveEffects.Add(
                    $"Supports {count}: SPD +{speed}%");
                break;
        }
    }

    private static void AddCount<TKey>(
        Dictionary<TKey, int> counts,
        TKey key)
    {
        counts[key] = counts.TryGetValue(key, out int count)
            ? count + 1
            : 1;
    }
}
