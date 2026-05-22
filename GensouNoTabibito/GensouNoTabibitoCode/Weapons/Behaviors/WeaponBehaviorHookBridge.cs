using GensouNoTabibito.GensouNoTabibitoCode.Powers;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Bow;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Staff;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Sword;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors;

public static class WeaponBehaviorHookBridge
{
    private const int DualWieldSwordSkill = 10;

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
        if (IsDualWielding(slot))
            return ApplyDualWieldSwordSkill(player);

        return ForEach(slot, weapon => Get(weapon).BeforeCombatStart(weapon, player));
    }

    public static Task AfterRoomEntered(WeaponSlotState slot, Player player, AbstractRoom room)
    {
        if (IsDualWielding(slot))
            return Task.CompletedTask;

        return ForEach(slot, weapon => Get(weapon).AfterRoomEntered(weapon, player, room));
    }

    public static Task AfterSideTurnStart(
        WeaponSlotState slot,
        Player player,
        CombatSide side,
        ICombatState combatState)
    {
        if (IsDualWielding(slot))
            return Task.CompletedTask;

        return ForEach(slot, weapon => Get(weapon).AfterSideTurnStart(weapon, player, side, combatState));
    }

    public static Task BeforeCardPlayed(WeaponSlotState slot, Player player, CardPlay cardPlay)
    {
        if (IsDualWielding(slot))
            return Task.CompletedTask;

        return ForEach(slot, weapon => Get(weapon).BeforeCardPlayed(weapon, player, cardPlay));
    }

    public static Task AfterCardPlayed(
        WeaponSlotState slot,
        Player player,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (IsDualWielding(slot))
            return Task.CompletedTask;

        return ForEach(slot, weapon => Get(weapon).AfterCardPlayed(weapon, player, choiceContext, cardPlay));
    }

    public static Task BeforeTurnEnd(
        WeaponSlotState slot,
        Player player,
        PlayerChoiceContext choiceContext,
        CombatSide side)
    {
        if (IsDualWielding(slot))
            return Task.CompletedTask;

        return ForEach(slot, weapon => Get(weapon).BeforeTurnEnd(weapon, player, choiceContext, side));
    }

    public static decimal ModifyDamageAdditive(
        WeaponSlotState slot,
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (IsDualWielding(slot))
            return 0m;

        return slot.Weapons.Sum(weapon => Get(weapon).ModifyDamageAdditive(
            weapon,
            target,
            amount,
            props,
            dealer,
            cardSource));
    }

    public static IEnumerable<IHoverTip> GetHoverTips(WeaponState weapon)
    {
        return Get(weapon).GetHoverTips(weapon);
    }

    public static IEnumerable<IHoverTip> GetHoverTips(EquippedWeapon equippedWeapon)
    {
        var isWeaponTip = true;
        foreach (var tip in Get(equippedWeapon.Weapon).GetHoverTips(equippedWeapon.Weapon))
        {
            if (isWeaponTip && tip is HoverTip hoverTip)
            {
                yield return CreateEquippedWeaponHoverTip(equippedWeapon, hoverTip);
                isWeaponTip = false;
                continue;
            }

            yield return tip;
            isWeaponTip = false;
        }
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

    public static IWeaponBehavior Get(WeaponState weapon)
    {
        return BehaviorsById.TryGetValue(weapon.Id, out var behavior)
            ? behavior
            : BehaviorsByKind[weapon.Kind];
    }

    private static bool IsDualWielding(WeaponSlotState slot)
    {
        return slot.PrimaryWeapon != null && slot.SecondaryWeapon != null;
    }

    private static HoverTip CreateEquippedWeaponHoverTip(EquippedWeapon equippedWeapon, HoverTip source)
    {
        var title = new LocString("weapons", GetEquippedWeaponTitleKey(equippedWeapon.Slot));
        title.Add("0", GetWeaponName(equippedWeapon.Weapon));

        var tip = new HoverTip(title, source.Description, source.Icon)
        {
            Id = $"{source.Id}.{equippedWeapon.Slot}",
            IsSmart = source.IsSmart,
            IsDebuff = source.IsDebuff,
            IsInstanced = source.IsInstanced,
            ShouldOverrideTextOverflow = source.ShouldOverrideTextOverflow
        };

        if (source.CanonicalModel != null)
            tip.SetCanonicalModel(source.CanonicalModel);

        return tip;
    }

    private static string GetEquippedWeaponTitleKey(WeaponSlot slot)
    {
        return slot switch
        {
            WeaponSlot.Primary => "GENSOUNOTABIBITO-WEAPON.primaryTitle",
            WeaponSlot.Secondary => "GENSOUNOTABIBITO-WEAPON.secondaryTitle",
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
        };
    }

    private static string GetWeaponName(WeaponState weapon)
    {
        return new LocString("weapons", $"{weapon.Id}.name").GetFormattedText();
    }

    private static Task ApplyDualWieldSwordSkill(Player player)
    {
        return PowerCmd.Apply<SwordSkill>(
            new ThrowingPlayerChoiceContext(),
            player.Creature,
            DualWieldSwordSkill,
            player.Creature,
            null);
    }

    private static async Task ForEach(WeaponSlotState slot, Func<WeaponState, Task> action)
    {
        foreach (var weapon in slot.Weapons)
            await action(weapon);
    }
}