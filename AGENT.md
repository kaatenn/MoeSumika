# AGENT.md

## Project Overview

This repository is a Slay the Spire 2 mod built as a Godot/.NET project.

- Solution: `GensouNoTabibito.sln`
- Main project: `GensouNoTabibito/GensouNoTabibito.csproj`
- C# source: `GensouNoTabibito/GensouNoTabibitoCode/`
- Godot/assets/localization: `GensouNoTabibito/GensouNoTabibito/`
- Mod manifest: `GensouNoTabibito/GensouNoTabibito.json`

The project currently contains a custom character, cards, relics, powers, and a weapon-slot system. The weapon-slot state is player-owned and persisted through the starter relic carrier.

## Build And Verification

Prefer building the project after C# changes:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' build GensouNoTabibito\GensouNoTabibito.csproj --no-restore
```

Notes from this environment:

- `dotnet` may not be on `PATH`; use the absolute path above.
- `git` may not be on `PATH`; `C:\Program Files\Git\cmd\git.exe` exists.
- PowerShell startup prints an `oh-my-posh` error. It is profile noise and not related to the repo.
- Building may need access outside the sandbox for Godot SDK/NuGet/game references. If sandboxed build fails with `Godot.NET.Sdk` or NuGet source resolution errors, rerun with appropriate escalation.
- A successful build runs the project post-build copy target and copies mod outputs to the configured Slay the Spire 2 mods folder.

## Dependency Paths

`GensouNoTabibito/Sts2PathDiscovery.props` attempts to discover the Slay the Spire 2 install and defines:

- `Sts2Path`
- `Sts2DataDir`
- `ModsPath`

`GensouNoTabibito/Directory.Build.props` defines the local Godot/MegaDot path. If build or publish fails because Godot or StS2 cannot be found, inspect these props files first.

`Directory.Build.props` is intentionally ignored by `GensouNoTabibito/.gitignore`, so treat it as local machine configuration.

## Code Conventions

- Keep C# nullable-aware; the project has `<Nullable>enable</Nullable>`.
- Prefer existing BaseLib and StS2 APIs over new infrastructure.
- Keep gameplay wiring close to the owning model: relics receive game hooks, then delegate to weapon behavior services.
- Do not rewrite generated `.uid`, `.import`, `.godot`, or asset metadata files unless the task explicitly requires it.
- The working tree may contain user edits. Do not revert unrelated changes.

## Cards, Powers, And Reward UI

Card model files live under `GensouNoTabibitoCode/Cards/`. Current custom-card work includes:

- `GensouNoTabibitoCode/Cards/DinIn.cs`
- `GensouNoTabibitoCode/Cards/PeregrinPath.cs`
- `GensouNoTabibitoCode/Cards/Regroup.cs`
- `GensouNoTabibitoCode/Cards/SwordArtWindrend.cs`

Power model files live under `GensouNoTabibitoCode/Powers/`. Current Peregrin Path power work includes:

- `GensouNoTabibitoCode/Powers/PeregrinPathPower.cs`
- `GensouNoTabibitoCode/Powers/PeregrinPathTempDexLoss.cs`

The shared sword resource power lives in `GensouNoTabibitoCode/Powers/SwordSkill.cs`.

For temporary stat-down powers like `PeregrinPathTempDexLoss`, prefer BaseLib's `CustomTemporaryPowerModel` instead of hand-rolling cleanup. The temporary dexterity-loss shape is:

- `InternallyAppliedPower` is `ModelDb.Power<DexterityPower>()`.
- `OriginModel` is the source card, currently `ModelDb.Card<PeregrinPath>()`.
- `InvertInternalPowerAmount` is `true`.
- `ApplyPowerFunc` delegates to `PowerCmd.Apply<DexterityPower>`.
- Reuse vanilla temporary-dexterity localization keys from the `powers` table: `TEMPORARY_DEXTERITY_DOWN.description` and `TEMPORARY_DEXTERITY_DOWN.smartDescription`.
- Use the origin card title for the power title when the power is card-specific.

Card reward option localization uses `localization/{locale}/card_reward_ui.json`. Keep these option keys present for both `eng` and `zhs`:

- `OPTION_GENSOUNOTABIBITO-DRAFT_WEAPON.name`
- `OPTION_GENSOUNOTABIBITO-UPGRADE_WEAPON.name`

## Weapon Slot Architecture

Important files:

- `GensouNoTabibitoCode/Weapons/WeaponState.cs`
- `GensouNoTabibitoCode/Weapons/WeaponSlotState.cs`
- `GensouNoTabibitoCode/Weapons/WeaponSlots.cs`
- `GensouNoTabibitoCode/Weapons/WeaponLocalization.cs`
- `GensouNoTabibitoCode/Weapons/Sync/WeaponSaveSync.cs`
- `GensouNoTabibitoCode/Weapons/Sync/IWeaponSlotSaveCarrier.cs`
- `GensouNoTabibitoCode/Weapons/Sync/WeaponNetworkState.cs`
- `GensouNoTabibitoCode/Weapons/Behaviors/IWeaponBehavior.cs`
- `GensouNoTabibitoCode/Weapons/Behaviors/WeaponBehavior.cs`
- `GensouNoTabibitoCode/Weapons/Behaviors/WeaponBehaviorRegistry.cs`
- `GensouNoTabibitoCode/Weapons/Behaviors/Sword/SwordBehavior.cs`
- `GensouNoTabibitoCode/Weapons/Behaviors/Sword/BrokenSwordBehavior.cs`
- `GensouNoTabibitoCode/Weapons/Behaviors/Staff/StaffBehavior.cs`
- `GensouNoTabibitoCode/Weapons/Behaviors/Bow/BowBehavior.cs`
- `GensouNoTabibitoCode/Keywords/GensouNoTabibitoKeywords.cs`
- `GensouNoTabibitoCode/Localization/StringDynamicVar.cs`
- `GensouNoTabibitoCode/Relics/WeaponBagRelic.cs`

`WeaponBagRelic` is the current starter weapon-slot carrier:

- Ensures the player has a broken sword equipped by default.
- Receives relic/game hooks.
- Gets the owner's `WeaponSlotState`.
- Delegates to `WeaponBehaviorRegistry`.
- Carries saved weapon-slot state through `[SavedProperty]`.
- Uses dynamic relic description variables for weapon names and levels only.
- Aggregates weapon-specific hover tips from `WeaponBehaviorRegistry`; long weapon descriptions and weapon effect details belong to behaviors, not the relic description body.

Weapon localization currently lives in the `relics` localization table, not a custom `weapons` table. The game does not automatically create arbitrary localization tables, so `WeaponLocalization` reads keys like:

- `GENSOUNOTABIBITO-BROKEN_SWORD.weaponTitle`
- `GENSOUNOTABIBITO-BROKEN_SWORD.weaponDescription`
- `GENSOUNOTABIBITO-NONE.weaponTitle`

## Weapon Behavior Hook Pattern

`IWeaponBehavior` hooks should receive enough context to perform real gameplay actions. In particular, pass `Player player` through hooks so behaviors can apply powers or use `player.Creature` as source/target.

Current pattern:

```csharp
Task BeforeCombatStart(WeaponState weapon, Player player);
Task AfterSideTurnStart(WeaponState weapon, Player player, CombatSide side, ICombatState combatState);
Task AfterRoomEntered(WeaponState weapon, Player player, AbstractRoom room);
Task BeforeCardPlayed(WeaponState weapon, Player player, CardPlay cardPlay);
Task AfterCardPlayed(WeaponState weapon, Player player, PlayerChoiceContext choiceContext, CardPlay cardPlay);
Task BeforeTurnEnd(WeaponState weapon, Player player, PlayerChoiceContext choiceContext, CombatSide side);
IEnumerable<IHoverTip> GetHoverTips(WeaponState weapon);
```

`GetHoverTips` is part of the weapon presentation contract. `WeaponBehavior.GetHoverTips` returns the generic weapon title/description hover tip. Derived behaviors should call `base.GetHoverTips(weapon)` before adding type-specific or weapon-specific tips.

Examples:

- `SwordBehavior.GetHoverTips` adds the sword-weapon keyword and `SwordSkill` power tip after the base weapon description.
- `BrokenSwordBehavior.GetHoverTips` calls `SwordBehavior` and then adds a `DramaticEntrance` card hover tip. This keeps Dramatic Entrance visible only for broken sword, not all swords.
- `StaffBehavior` and `BowBehavior` add only their own weapon-type keyword tips after the base weapon description.

When the original game hook provides a `PlayerChoiceContext`, pass it through. When it does not, effects that need commands can use:

```csharp
new ThrowingPlayerChoiceContext()
```

This matches vanilla-style relic code such as room-entry power application.

## Adding New Weapons

To add a new weapon:

1. Create a new `XXXBehavior` class in the appropriate `Behaviors/{WeaponKind}/` subfolder, extending `WeaponBehavior` (or a more specific subclass like `SwordBehavior`).
2. If the weapon uses a custom ID, register it in `WeaponBehaviorRegistry.BehaviorsById` via `Register(id, behavior)`.
3. If it is the default for a new `WeaponKind`, add it to `BehaviorsByKind`.
4. Add `*.weaponTitle` and `*.weaponDescription` entries to `localization/eng/relics.json` and `localization/zhs/relics.json`.
5. Put long effect explanations in the weapon hover tip via behavior/localization; keep `WeaponBagRelic.description` short enough to show only equipped weapon names and levels.

Weapon-type hover tips use custom `CardKeyword` values from `GensouNoTabibitoKeywords`:

- `SwordWeapon`
- `StaffWeapon`
- `BowWeapon`

Add new weapon type keywords to `GensouNoTabibitoKeywords` and both `card_keywords.json` files. `CustomEnum` names should omit the mod prefix because BaseLib applies it automatically.

## Applying Powers

Use `PowerCmd.Apply<TPower>` for powers. For player self-buffs, the usual shape is:

```csharp
await PowerCmd.Apply<MyPower>(
    new ThrowingPlayerChoiceContext(),
    player.Creature,
    amount,
    player.Creature,
    null);
```

Example already implemented: `SwordBehavior.BeforeCombatStart` applies 5 stacks of `SwordSkill` to the player, so all sword weapons share the same start-of-combat sword-skill baseline. Broken sword's unique behavior is separate: it gives `DramaticEntrance` at the start of the first combat round, upgrading the generated card at weapon level 2 or higher.

## Common Pitfalls

- `AbstractRoom` lives under `MegaCrit.Sts2.Core.Rooms`.
- `CombatRoom` is also under the room namespace.
- `PlayerChoiceContext`/`ThrowingPlayerChoiceContext` are under `MegaCrit.Sts2.Core.GameActions.Multiplayer`.
- `PowerCmd` is under `MegaCrit.Sts2.Core.Commands`.
- If a hook signature is changed in `IWeaponBehavior`, update all of:
  - `WeaponBehavior`
  - `WeaponBehaviorRegistry`
  - every concrete behavior
  - every relic or caller that dispatches to `WeaponBehaviorRegistry`
