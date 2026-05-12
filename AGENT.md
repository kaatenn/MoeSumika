# AGENT.md

## Project Overview

This repository is a Slay the Spire 2 mod built as a Godot/.NET project.

- Solution: `MoeSumika.sln`
- Main project: `MoeSumika/MoeSumika.csproj`
- C# source: `MoeSumika/MoeSumikaCode/`
- Godot/assets/localization: `MoeSumika/MoeSumika/`
- Mod manifest: `MoeSumika/MoeSumika.json`

The project currently contains a custom character, relics, powers, and a weapon-slot system. The weapon-slot state is player-owned and persisted through the starter relic carrier.

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

## Weapon Slot Architecture

Important files:

- `MoeSumikaCode/Weapons/WeaponState.cs`
- `MoeSumikaCode/Weapons/WeaponSlotState.cs`
- `MoeSumikaCode/Weapons/WeaponSlots.cs`
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
- `MoeSumikaCode/Relics/BrokenSwordRelic.cs`

`BrokenSwordRelic` is the current example carrier:

- Ensures the player has a broken sword equipped.
- Receives relic/game hooks.
- Gets the owner's `WeaponSlotState`.
- Delegates to `WeaponBehaviorRegistry`.
- Carries saved weapon-slot state through `[SavedProperty]`.

## Weapon Behavior Hook Pattern

`IWeaponBehavior` hooks should receive enough context to perform real gameplay actions. In particular, pass `Player player` through hooks so behaviors can apply powers or use `player.Creature` as source/target.

Current pattern:

```csharp
Task AfterRoomEntered(WeaponState weapon, Player player, AbstractRoom room);
Task BeforeCardPlayed(WeaponState weapon, Player player, CardPlay cardPlay);
Task AfterCardPlayed(WeaponState weapon, Player player, PlayerChoiceContext choiceContext, CardPlay cardPlay);
Task BeforeTurnEnd(WeaponState weapon, Player player, PlayerChoiceContext choiceContext, CombatSide side);
```

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

Example already implemented: `BrokenSwordBehavior.AfterRoomEntered` checks for `CombatRoom` and applies 5 stacks of `SwordSkill` to the player.

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
