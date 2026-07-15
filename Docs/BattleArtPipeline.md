# Battle Art Pipeline

## Goal

New companion or enemy art should connect to battle, gacha, and collection with one editor action.

Use `Tools > Battle Art > Sync Battle Art Pipeline`.

## Companion Folder Rule

Path:

```text
Assets/Art/Battle/Companions/{CharacterName}/
```

Required files:

```text
{CharacterName}.png
BasicProjectile.png
SkillProjectile.png
Idle/{CharacterName}_Idle_01.png ...
Attack/{CharacterName}_Attack_01.png ...
Skill/{CharacterName}_Skill_01.png ...
Hit/{CharacterName}_Hit_01.png ...
Death/{CharacterName}_Death_01.png ...
```

Frame count rule:

```text
Idle 6
Attack 8
Skill 8
Hit 4
Death 6
```

Sync result:

```text
CharacterData auto-created if missing
CharacterDatabase auto-registered
BattleVisual auto-linked
Gacha result uses first Idle frame
Collection portrait uses icon, then battle sprite fallback
Battle uses full animation set
```

If a new auto-created character appears as `R`, edit its
`Assets/Characters/{CharacterName}.asset` rarity, role, element, skill, and
description before final balancing.

## Enemy Folder Rule

Normal enemy path:

```text
Assets/Art/Battle/Enemies/{EnemyName}/
```

Boss path:

```text
Assets/Art/Battle/Bosses/{BossName}/
```

Use the same frame folder rule as companions. Sync auto-links these into
`BattleVisualDatabase.asset`. Stage range stays editable in the database.

## Report

Run:

```text
Tools > Battle Art > Write Art Readiness Report
```

Output:

```text
Logs/BattleArtReadinessReport.txt
```

Use this report to find missing sprites, wrong frame counts, missing
projectiles, or missing database registration.
