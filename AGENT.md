# AGENT.md

## Project Overview

This repository is a Slay the Spire 2 mod built as a Godot/.NET project.

- Solution: `MoeSumika.sln`
- Main project: `MoeSumika/MoeSumika.csproj`
- C# source: `MoeSumika/MoeSumikaCode/`
- Godot/assets/localization: `MoeSumika/MoeSumika/`
- Mod manifest: `MoeSumika/MoeSumika.json`

The project currently contains a custom character, cards, relics, powers, and a weapon-slot system. The weapon-slot state is player-owned and persisted through the starter relic carrier.

## Build And Verification

Prefer building the project after C# changes:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' build MoeSumika\MoeSumika.csproj --no-restore
```

Notes from this environment:

- `dotnet` may not be on `PATH`; use the absolute path above.
- `git` may not be on `PATH`; `C:\Program Files\Git\cmd\git.exe` exists.
- PowerShell startup prints an `oh-my-posh` error. It is profile noise and not related to the repo.
- Building may need access outside the sandbox for Godot SDK/NuGet/game references. If sandboxed build fails with `Godot.NET.Sdk` or NuGet source resolution errors, rerun with appropriate escalation.
- A successful build runs the project post-build copy target and copies mod outputs to the configured Slay the Spire 2 mods folder.

## Dependency Paths

`MoeSumika/Sts2PathDiscovery.props` attempts to discover the Slay the Spire 2 install and defines:

- `Sts2Path`
- `Sts2DataDir`
- `ModsPath`

`MoeSumika/Directory.Build.props` defines the local Godot/MegaDot path. If build or publish fails because Godot or StS2 cannot be found, inspect these props files first.

## Code Conventions

- Keep C# nullable-aware; the project has `<Nullable>enable</Nullable>`.
- Prefer existing BaseLib and StS2 APIs over new infrastructure.
- Keep gameplay wiring close to the owning model: relics receive game hooks, then delegate to weapon behavior services.
- Do not rewrite generated `.uid`, `.import`, `.godot`, or asset metadata files unless the task explicitly requires it.
- The working tree may contain user edits. Do not revert unrelated changes.

## Cards, Powers, And Reward UI

Card model files live under `MoeSumikaCode/Cards/`. Current custom-card work includes:

- `MoeSumikaCode/Cards/DinIn.cs`
- `MoeSumikaCode/Cards/PeregrinPath.cs`
- `MoeSumikaCode/Cards/Regroup.cs`

Power model files live under `MoeSumikaCode/Powers/`. Current Peregrin Path power work includes:

- `MoeSumikaCode/Powers/PeregrinPathPower.cs`
- `MoeSumikaCode/Powers/PeregrinPathTempDexLoss.cs`

For temporary stat-down powers like `PeregrinPathTempDexLoss`, prefer BaseLib's `CustomTemporaryPowerModel` instead of hand-rolling cleanup. The temporary dexterity-loss shape is:

- `InternallyAppliedPower` is `ModelDb.Power<DexterityPower>()`.
- `OriginModel` is the source card, currently `ModelDb.Card<PeregrinPath>()`.
- `InvertInternalPowerAmount` is `true`.
- `ApplyPowerFunc` delegates to `PowerCmd.Apply<DexterityPower>`.
- Reuse vanilla temporary-dexterity localization keys from the `powers` table: `TEMPORARY_DEXTERITY_DOWN.description` and `TEMPORARY_DEXTERITY_DOWN.smartDescription`.
- Use the origin card title for the power title when the power is card-specific.

Card reward option localization uses `localization/{locale}/card_reward_ui.json`. Keep these option keys present for both `eng` and `zhs`:

- `OPTION_MOESUMIKA-DRAFT_WEAPON.name`
- `OPTION_MOESUMIKA-UPGRADE_WEAPON.name`

## Weapon Slot Architecture

Important files:

- `MoeSumikaCode/Weapons/WeaponState.cs`
- `MoeSumikaCode/Weapons/WeaponSlotState.cs`
- `MoeSumikaCode/Weapons/WeaponSlots.cs`
- `MoeSumikaCode/Weapons/WeaponLocalization.cs`
- `MoeSumikaCode/Weapons/Sync/WeaponSaveSync.cs`
- `MoeSumikaCode/Weapons/Sync/IWeaponSlotSaveCarrier.cs`
- `MoeSumikaCode/Weapons/Sync/WeaponNetworkState.cs`
- `MoeSumikaCode/Weapons/Behaviors/IWeaponBehavior.cs`
- `MoeSumikaCode/Weapons/Behaviors/WeaponBehavior.cs`
- `MoeSumikaCode/Weapons/Behaviors/WeaponBehaviorRegistry.cs`
- `MoeSumikaCode/Weapons/Behaviors/Sword/SwordBehavior.cs`
- `MoeSumikaCode/Weapons/Behaviors/Sword/BrokenSwordBehavior.cs`
- `MoeSumikaCode/Weapons/Behaviors/Staff/StaffBehavior.cs`
- `MoeSumikaCode/Weapons/Behaviors/Bow/BowBehavior.cs`
- `MoeSumikaCode/Keywords/MoeSumikaKeywords.cs`
- `MoeSumikaCode/Localization/StringDynamicVar.cs`
- `MoeSumikaCode/Relics/WeaponBagRelic.cs`

`WeaponBagRelic` is the current starter weapon-slot carrier:

- Ensures the player has a broken sword equipped by default.
- Receives relic/game hooks.
- Gets the owner's `WeaponSlotState`.
- Delegates to `WeaponBehaviorRegistry`.
- Carries saved weapon-slot state through `[SavedProperty]`.
- Uses dynamic relic description variables for weapon names and levels only.
- Aggregates weapon-specific hover tips from `WeaponBehaviorRegistry`; long weapon descriptions and weapon effect details belong to behaviors, not the relic description body.

Weapon localization currently lives in the `relics` localization table, not a custom `weapons` table. The game does not automatically create arbitrary localization tables, so `WeaponLocalization` reads keys like:

- `MOESUMIKA-BROKEN_SWORD.weaponTitle`
- `MOESUMIKA-BROKEN_SWORD.weaponDescription`
- `MOESUMIKA-NONE.weaponTitle`

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

Weapon-type hover tips use custom `CardKeyword` values from `MoeSumikaKeywords`:

- `SwordWeapon`
- `StaffWeapon`
- `BowWeapon`

Add new weapon type keywords to `MoeSumikaKeywords` and both `card_keywords.json` files. `CustomEnum` names should omit the mod prefix because BaseLib applies it automatically.

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
