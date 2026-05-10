using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MoeSumika.MoeSumikaCode.Weapons.Effects;

public static class WeaponEffects
{
    private static readonly Dictionary<string, IWeaponEffect> EffectsById = new();
    private static readonly Dictionary<WeaponKind, IWeaponEffect> EffectsByKind = new()
    {
        [WeaponKind.Sword] = new SwordWeaponEffect(),
        [WeaponKind.Staff] = new StaffWeaponEffect(),
        [WeaponKind.Bow] = new BowWeaponEffect()
    };

    public static void Register(string weaponId, IWeaponEffect effect)
    {
        EffectsById[weaponId] = effect;
    }

    public static Task BeforeCombatStart(WeaponSlotState slot, Player player)
    {
        return ForEach(slot, weapon => Get(weapon).BeforeCombatStart(weapon, player));
    }

    public static Task AfterSideTurnStart(
        WeaponSlotState slot,
        Player player,
        CombatSide side,
        ICombatState combatState)
    {
        return ForEach(slot, weapon => Get(weapon).AfterSideTurnStart(weapon, player, side, combatState));
    }

    public static Task BeforeCardPlayed(WeaponSlotState slot, CardPlay cardPlay)
    {
        return ForEach(slot, weapon => Get(weapon).BeforeCardPlayed(weapon, cardPlay));
    }

    public static Task AfterCardPlayed(
        WeaponSlotState slot,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        return ForEach(slot, weapon => Get(weapon).AfterCardPlayed(weapon, choiceContext, cardPlay));
    }

    public static Task BeforeTurnEnd(
        WeaponSlotState slot,
        PlayerChoiceContext choiceContext,
        CombatSide side)
    {
        return ForEach(slot, weapon => Get(weapon).BeforeTurnEnd(weapon, choiceContext, side));
    }

    private static IWeaponEffect Get(WeaponState weapon)
    {
        return EffectsById.TryGetValue(weapon.Id, out var effect)
            ? effect
            : EffectsByKind[weapon.Kind];
    }

    private static async Task ForEach(WeaponSlotState slot, Func<WeaponState, Task> action)
    {
        foreach (var weapon in slot.Weapons)
            await action(weapon);
    }
}
