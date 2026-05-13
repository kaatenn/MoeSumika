using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Bow;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Staff;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Sword;

namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons;

public static class WeaponBehaviorRegistry
{
    private static readonly Dictionary<string, IWeaponBehavior> BehaviorsById = new();

    private static readonly Dictionary<WeaponKind, IWeaponBehavior> BehaviorsByKind = new()
    {
        [WeaponKind.Sword] = new SwordBehavior(),
        [WeaponKind.Staff] = new StaffBehavior(),
        [WeaponKind.Bow] = new BowBehavior()
    };

    public static void Register(string weaponId, IWeaponBehavior behavior)
    {
        BehaviorsById[weaponId] = behavior;
    }

    public static Task BeforeCombatStart(WeaponSlotState slot, Player player)
    {
        return ForEach(slot, weapon => Get(weapon).BeforeCombatStart(weapon, player));
    }

    public static Task AfterRoomEntered(WeaponSlotState slot, Player player, AbstractRoom room)
    {
        return ForEach(slot, weapon => Get(weapon).AfterRoomEntered(weapon, player, room));
    }

    public static Task AfterSideTurnStart(
        WeaponSlotState slot,
        Player player,
        CombatSide side,
        ICombatState combatState)
    {
        return ForEach(slot, weapon => Get(weapon).AfterSideTurnStart(weapon, player, side, combatState));
    }

    public static Task BeforeCardPlayed(WeaponSlotState slot, Player player, CardPlay cardPlay)
    {
        return ForEach(slot, weapon => Get(weapon).BeforeCardPlayed(weapon, player, cardPlay));
    }

    public static Task AfterCardPlayed(
        WeaponSlotState slot,
        Player player,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        return ForEach(slot, weapon => Get(weapon).AfterCardPlayed(weapon, player, choiceContext, cardPlay));
    }

    public static Task BeforeTurnEnd(
        WeaponSlotState slot,
        Player player,
        PlayerChoiceContext choiceContext,
        CombatSide side)
    {
        return ForEach(slot, weapon => Get(weapon).BeforeTurnEnd(weapon, player, choiceContext, side));
    }

    public static IEnumerable<IHoverTip> GetHoverTips(WeaponState weapon)
    {
        return Get(weapon).GetHoverTips(weapon);
    }

    public static bool CanUpgrade(WeaponState weapon)
    {
        return weapon.Level < Get(weapon).MaxLevel;
    }

    public static bool TryUpgrade(WeaponState weapon)
    {
        if (!CanUpgrade(weapon))
            return false;

        weapon.Upgrade();
        return true;
    }

    private static IWeaponBehavior Get(WeaponState weapon)
    {
        return BehaviorsById.TryGetValue(weapon.Id, out var behavior)
            ? behavior
            : BehaviorsByKind[weapon.Kind];
    }

    private static async Task ForEach(WeaponSlotState slot, Func<WeaponState, Task> action)
    {
        foreach (var weapon in slot.Weapons)
            await action(weapon);
    }
}
