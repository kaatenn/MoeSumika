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

## Power Implementation Guidelines

Important files:

- `GensouNoTabibitoCode/Powers/Battou.cs`
- `GensouNoTabibitoCode/Powers/SwordSkill.cs`
- `GensouNoTabibitoCode/Powers/PeregrinPathPower.cs`
- `GensouNoTabibitoCode/Tags/GensouNoTabibitoTags.cs`
- `GensouNoTabibitoCode/Hooks/SwordSkillTagHook.cs`

**Using directives requirements**:

```csharp
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
```

**Access modifiers**:

- Override `ExtraHoverTips` as `protected override`, not `public override`
- Follow existing Power patterns in the codebase

**Localization key naming**:

- Power class `BattouPower` uses localization keys `GENSOUNOTABIBITO-BATTOU.*`
- The framework automatically handles the `Power` suffix mapping
- Always include `title`, `description`, and `smartDescription` keys

**Damage modification powers**:

Use `ModifyDamageMultiplicative` for damage scaling:

```csharp
public override decimal ModifyDamageMultiplicative(
    Creature? target,
    Decimal amount,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource)
{
    // Only affect owner's attack cards
    if (!props.IsPoweredAttack() || cardSource == null || cardSource.Owner.Creature != Owner)
        return 1m;

    // Check conditions
    var swordSkillPower = Owner.GetPower<SwordSkill>();
    if (swordSkillPower == null || swordSkillPower.Amount < RequiredSwordSkillAmount)
        return 1m;

    // Return damage multiplier (1m = no change)
    return (decimal)Math.Pow(1.1, Amount);
}
```

**DynamicVar access**:

- Define DynamicVars in `CanonicalVars` for localization: `new DynamicVar("RequiredSwordSkill", 4m)`
- Access in code via constants or properties: `private const int RequiredSwordSkillAmount = 4;`
- Use `{RequiredSwordSkill}` placeholder in localization strings

**Smart description format**:

Follow this concise format for Power smart descriptions:

- English: `"[gold]Sword Skill Required[/gold] [blue]{RequiredSwordSkill}[/blue]\nAttack damage becomes [blue]{1.1^Amount}[/blue]x"`
- Chinese: `"[gold]剑技需求[/gold] [blue]{RequiredSwordSkill}[/blue]\n攻击所造成的伤害变为 [blue]{1.1^Amount}[/blue] 倍"`

**Code simplification patterns**:
- Remove unnecessary using statements: only include what's actually used
- Simplify math operations: `(decimal)Math.Pow(1.1, Amount)` instead of `(decimal)Math.Pow(1.1, (double)Amount)`
- Use direct references when the framework auto-handles type conversions
- Prefer helper methods over complex inline logic

**Card best practices**:
- Use `ShouldGlowGoldInternal` for custom gold glow effects based on game state
- Add helper methods for complex conditions to improve readability
- Keep `OnPlay` methods focused on core card logic, delegate complex checks to helpers
- Use LINQ and pattern matching for cleaner conditional logic

**Getting other powers**:

Use `Owner.GetPower<TPower>()` to retrieve power instances from the owner creature:

```csharp
var swordSkillPower = Owner.GetPower<SwordSkill>();
if (swordSkillPower == null || swordSkillPower.Amount < RequiredAmount)
    return 1m;
```

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

## Common APIs

BaseLib model APIs:

- `CustomCardModel`, `CustomPowerModel`, `CustomTemporaryPowerModel`, `CustomRelicModel`, `CustomPotionModel`, `CustomCardPoolModel`, `CustomRelicPoolModel`, and `CustomPotionPoolModel` are the main extension points for mod content.
- Use `[Pool(typeof(...Pool))]` on custom card/relic/potion base classes so BaseLib associates content with the correct character pool.
- Use `ModelDb.Card<T>()`, `ModelDb.Power<T>()`, `ModelDb.Relic<T>()`, `ModelDb.CardPool<T>()`, `ModelDb.RelicPool<T>()`, and `ModelDb.PotionPool<T>()` when referencing registered models instead of constructing models directly.

Gameplay command APIs:

- Use `PowerCmd.Apply<TPower>(choiceContext, target, amount, source, sourceCard)` to apply powers. For self-buffs, both `target` and `source` are usually `player.Creature`.
- Use the hook-provided `PlayerChoiceContext` when available. If a hook has no context but must run a command, use `new ThrowingPlayerChoiceContext()`.
- Card play implementations usually override `OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)` and read the acting player through `cardPlay.Owner`.

Hover tip APIs:

- Use `ExtraHoverTips` on cards/relics for additional UI tips.
- Use `HoverTipFactory.FromPower<TPower>()`, `HoverTipFactory.FromCard<TCard>(upgraded)`, and `HoverTipFactory.FromKeyword(keyword)` for standard localized tips.
- Use `new HoverTip(title, description)` only when the title and description already come from localization or another stable presentation source.

Reward and relic APIs:

- Override relic lifecycle hooks such as `AfterCardPlayed`, `BeforeTurnEnd`, and room/combat hooks when the relic owns the gameplay trigger.
- Override `TryModifyCardRewardAlternatives(CardReward cardReward, List<CardRewardAlternative> alternatives)` to add card-reward replacement actions.
- Create alternatives with `new CardRewardAlternative(id, callback, PostAlternateCardRewardAction.EndSelectionAndCompleteReward)` when selecting the alternative should finish the reward flow.

Persistence and patch APIs:

- Use `[SavedProperty]` on relic properties that must be serialized with the run. This project stores weapon-slot state on `WeaponBagRelic`, then syncs it to player-owned runtime state.
- Use Harmony attributes like `[HarmonyPatch(typeof(Player), nameof(Player.ToSerializable))]` for serialization/network hooks, and call `harmony.PatchAll()` from `MainFile.Initialize()`.
- Keep Harmony patches thin: move project-specific state transfer into helpers such as `WeaponSaveSync` or `WeaponNetworkState`.

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
- **Missing using directives**: When creating new Powers, ensure you have all required namespaces:
  - `MegaCrit.Sts2.Core.Entities.Powers` for `PowerType`, `PowerStackType`
  - `MegaCrit.Sts2.Core.Models` for `CardModel` and other model types
- **Incorrect access modifiers**: Power overrides like `ExtraHoverTips` must be `protected override`, not `public override`
- **DynamicVar access**: Don't use `DynamicVars.Var("name")` - this method doesn't exist. Define constants or properties to access DynamicVar values in code
- **Localization key errors**: The framework provides clear error messages like `Localization GENSOUNOTABIBITO-BATTOU.title not found`. Ensure keys match the class name (with automatic suffix handling)

## CardTag System

The project uses a custom CardTag system for card classification and automatic effects, implemented through `GensouNoTabibitoTags.cs`.

**Tag definition**:
```csharp
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace GensouNoTabibito.GensouNoTabibitoCode.Tags;

public static class GensouNoTabibitoTags
{
    [CustomEnum("SWORD_SKILL")]
    public static CardTag SwordSkill;
}
```

**Tag usage in cards**:
```csharp
protected override HashSet<CardTag> CanonicalTags => [GensouNoTabibitoTags.SwordSkill];
```

**Tag vs Keyword**:
- **CardTag**: Internal logic classification, not displayed to players, used for system hooks
- **CardKeyword**: Displayed to players, has UI tooltips, affects card behavior directly
- Use Tag when you need internal logic triggers without player-visible keywords
- Use Keyword when you want players to see the card property and understand its effect

## Harmony Patch Hook System

Global card effects are implemented through Harmony patches to `CardModel.OnPlayWrapper`.

**Hook pattern**:
```csharp
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using GensouNoTabibito.GensouNoTabibitoCode.Powers;
using GensouNoTabibito.GensouNoTabibitoCode.Tags;

namespace GensouNoTabibito.GensouNoTabibitoCode.Hooks;

[HarmonyPatch(typeof(CardModel))]
public static class SwordSkillTagHook
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CardModel.OnPlayWrapper))]
    private static async void OnCardPlayPostfix(
        Task __result,
        CardModel __instance,
        PlayerChoiceContext choiceContext,
        object target,
        bool isAutoPlay,
        object resources,
        bool skipCardPileVisuals)
    {
        await __result;

        if (__instance.Tags.Contains(GensouNoTabibitoTags.SwordSkill) && __instance.Owner != null)
        {
            await PowerCmd.Apply<SwordSkill>(
                choiceContext,
                __instance.Owner.Creature,
                1,
                __instance.Owner.Creature,
                __instance);
        }
    }
}
```

**Key points**:
- Harmony patches are automatically registered via `harmony.PatchAll()` in `MainFile.Initialize()`
- Use `[HarmonyPostfix]` to execute code after the original method completes
- Use `__instance` to access the patched instance
- Always `await __result` before post-fix logic to ensure original execution completes
- This pattern enables global effects without requiring manual registration by individual cards

## File Naming Conventions

**Simplified naming**: Prefer shorter class names over descriptive suffixes
- ✅ `Battou.cs` (class `Battou`) 
- ❌ `BattouPower.cs` (class `BattouPower`)
- The framework auto-handles naming, so `Battou` becomes `GENSOUNOTABIBITO-BATTOU` in localization

**Consistency with existing patterns**:
- Powers: Simple names matching the concept
- Cards: Descriptive names matching the card effect
- Behaviors: Functional names describing behavior type

## Localization Translation Style

**English localization principles**:
- Keep descriptions concise and action-oriented
- Use standard StS2 terminology: "Attack damage becomes Xx" not "is multiplied by"
- Avoid redundancy in smartDescription; title appears elsewhere
- Examples from reference: "Attack deals double damage", "Gain X Block"
- Use "required cards" for clarity when appropriate: "A resource used by some of Sword Weapons and Sword Skill required cards"

**Chinese localization principles**:
- Follow native Chinese expression patterns, not English translation structures
- Use standard phrases: "每当你...时，获得" (not "打出...时获得")
- Damage expressions: "攻击造成...倍伤害" (not "攻击伤害变为...倍")
- Time expressions: "在本回合" (not "该回合")
- Conditionals: "如果有敌人的意图是攻击" (follows reference pattern)
- Maintain rhythm and flow of Chinese language, avoiding translation stiffness

**Key translation patterns learned from `.refer` reference files**:
- English: "Whenever you play a card, gain X" → Chinese: "每当你打出一张牌时，都获得X"
- English: "Attack deals double damage" → Chinese: "攻击造成双倍伤害"  
- English: "If enemy intends to attack" → Chinese: "如果有敌人的意图是攻击"
- Use shorter, more natural Chinese expressions that match native speaker patterns

**Consistency improvements made during development**:
- English: "Sword Skill" instead of "Sword Skill Level" - more concise, matches reference style
- Chinese: "剑技等级" for the resource name, but "剑技牌" in descriptions - avoids repetition
- Both: "造成...倍伤害" instead of "变为...倍" - more natural Chinese phrasing
- Both: Using "每当你...时，获得" instead of "打出...时获得" - better rhythm and clarity
