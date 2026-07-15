# Firebase LiveOps V3.1

Unity 2D portrait idle RPG. The player commands a sparrow squad to defend a utility pole from cat enemies.

## Stack

- Unity `6000.4.8f1`
- Firebase Unity SDK `13.11.0`
- Firebase Authentication, Firestore, Analytics, Remote Config, Crashlytics, Messaging
- Unity IAP and rewarded-ad integration points
- Android target package: `com.DoOurGame.gameliveops`

## Current Game Loop

1. Defeat the current enemy.
2. Clear stages and bosses for Gold, equipment, and progression rewards.
3. Upgrade player growth, companions, and equipment.
4. Protect utility-pole durability. When durability reaches zero, the stage ends and the pole death visual plays.
5. Build parties around companion roles, elements, skills, and synergies.

## Battle

- Portrait battle HUD with auto-advance and power-charge controls.
- Enemies follow stage health, attack, boss, and reward scaling rules.
- Utility pole uses `durability` terminology throughout the battle rules.
- Power Charge grants `5` power per tap.
- Pole damage reduction, repair, recovery speed, and durability options are supported.
- Stage ranges can select map prefabs through `mapPrefabPath` in the stage theme database.
- Day and night map bases can be reused or replaced per stage range.

## Companions And Art

- Characters use `CharacterData`, `BattleVisualDatabase`, and per-character battle art folders.
- Battle visuals support sprite, Animator, idle, attack, skill, death, hit, and projectile assignments.
- Hit animation is not used for new companion production.
- New standard animation layout:
  - Idle: 8 frames, 2 x 4 layout.
  - Attack: 8 frames, 2 x 4 layout.
  - Skill: 8 frames, 2 x 4 layout.
  - Death: 8 frames, 2 x 4 layout.
  - Frame crop size: `400 x 320` or larger when effects need more room.
- Art folders live under `Assets/Art/Battle/Companions/{CharacterName}`.
- `BasicProjectile.png` and `SkillProjectile.png` can be assigned from `CharacterData`.

## Equipment

Equipment definitions live in `Assets/Resources/EquipmentDatabase.asset`.

- Weapon keys: `equip101` through `equip104`.
- Armor keys: `equip201` through `equip204`.
- Equipment uses individual instances for rolled options while equipped weapon and armor selections remain player-controlled.
- Higher equipment is never auto-equipped.
- Equipment rows cycle owned equipment manually.
- Duplicate drops are dismantled into `Flight Equipment Coins`.
- Duplicate coin rewards by tier: `5 / 15 / 40 / 100`.

### Random Options

- Tier 0: no random option.
- Tier 1 and 2: one random option.
- Tier 3: two distinct random options.
- Each rolled value is `1%` through `15%`.
- Weapon pool: attack, skill damage, boss damage.
- Armor pool: pole durability, pole damage reduction, pole repair, pole recovery speed.

### Buriburi Enhancement

The player-facing equipment enhancement name is **Buriburi Enhancement**.

- `0` through `20` enhancement levels.
- Success rates scale by level.
- Levels `5`, `10`, and `15` are guaranteed success milestones.
- A failure below level `10` keeps the current level.
- A failure at level `10` or higher reduces one level.
- Two consecutive downgrade failures trigger a guaranteed next success.
- Equipment is never destroyed.
- Enhancement level belongs to the weapon or armor slot so changing equipped gear does not erase progress.

### Option Reset

- Uses `Flight Equipment Coins` only. Player-facing text does not use the word "cube".
- Tier reset costs: `10 / 30 / 75` coins for tiers `1 / 2 / 3`.
- Reset spends coins, rolls a candidate option set, and opens a comparison modal.
- The player chooses either **Keep Current Options** or **Apply New Options**.
- Existing options remain untouched until the new option choice is confirmed.

## UI Workflow

- Runtime UI prefers prefabs in `Assets/Resources/Prefabs/UI`.
- `RuntimeUiBinder` reconnects named buttons, text, number sprites, and progress fills at runtime.
- UI preview scenes are under `Assets/Scenes/UIPreviews`.
- Primary editor commands:
  - `Tools > UI > Open UI Preview Scene`
  - `Tools > UI > Rebuild UI Preview Scene`
  - `Tools > UI > Rebuild Individual UI Preview Scenes`
  - `Tools > UI > Apply Selected Preview UI Override To Prefab`
- Bottom navigation is prefab-owned. Do not use anchors or runtime layout code to override button size and position.

## Map Workflow

- Build maps by composing sprite assets, not by placing one flattened PNG background.
- Source assets:
  - `Assets/Art/Battle/MapAssets`
  - `Assets/Art/Battle/Backgrounds`
- Create stage map prefabs in `Assets/Resources/Prefabs/Maps`.
- Assign stage-range prefab paths in `BattleStageThemeDatabase`.
- Recommended layers: sky, distant buildings, midground, ground tiles, platforms, poles, and foreground effects.

## Save Data

`PlayerData` is serialized through `PlayerDataConverter` for Firestore and local fallback saves.

- Equipment instances and rolled options are persisted.
- Flight Equipment Coins are persisted.
- Existing legacy inventory equipment is migrated to instances at runtime.
- Legacy duplicate counts are converted to coins during migration.
- Buriburi Enhancement levels and guaranteed-success counters are persisted.

## Important Paths

- Core balance: `Assets/Scripts/Core/GameBalanceConfig.cs`
- Battle rules: `Assets/Scripts/Battle/BattleManager.cs`
- Equipment rules: `Assets/Scripts/Data/EquipmentManager.cs`
- Equipment definitions: `Assets/Resources/EquipmentDatabase.asset`
- Character data: `Assets/Resources/CharacterDatabase.asset`
- Stage themes: `Assets/Resources/BattleStageThemeDatabase.asset`
- Main UI setup: `Assets/Scripts/UI/MainGameUI.cs`
- Equipment UI: `Assets/Scripts/UI/EquipmentPanelUI.cs`

## Validation

- Run the game from `MainGameScene` in Unity Play Mode.
- Validate battle, equipment selection, Buriburi Enhancement, option reset selection, stage maps, and UI preview scenes.
- `dotnet build` may fail outside Unity when Firebase temporary assemblies are absent. Unity regeneration is required for a full external build.

## Current Follow-Up Work

- Tune battle progression from playtest data.
- Expand stage-map prefab variations.
- Add more companions, enemies, boss patterns, equipment tiers, and option pools.
- Replace remaining prototype UI and audio placeholders.
- Complete Android device validation and Firebase release configuration.
