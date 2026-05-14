using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons;

public abstract class WeaponBehavior : IWeaponBehavior
{
    public virtual int MaxLevel => int.MaxValue;

    public virtual Task BeforeCombatStart(WeaponState weapon, Player player)
    {
        return Task.CompletedTask;
    }

    public virtual Task AfterRoomEntered(WeaponState weapon, Player player, AbstractRoom room)
    {
        return Task.CompletedTask;
    }

    public virtual Task AfterSideTurnStart(
        WeaponState weapon,
        Player player,
        CombatSide side,
        ICombatState combatState)
    {
        return Task.CompletedTask;
    }

    public virtual Task BeforeCardPlayed(WeaponState weapon, Player player, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    public virtual Task AfterCardPlayed(
        WeaponState weapon,
        Player player,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    public virtual Task BeforeTurnEnd(
        WeaponState weapon,
        Player player,
        PlayerChoiceContext choiceContext,
        CombatSide side)
    {
        return Task.CompletedTask;
    }

    public virtual decimal ModifyDamageAdditive(
        WeaponState weapon,
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return 0m;
    }

    public virtual IEnumerable<IHoverTip> GetHoverTips(WeaponState weapon)
    {
        var title = WeaponLocalization.GetTitleLocString(weapon);
        var description = WeaponLocalization.GetDescriptionLocString(weapon);
        if (title != null && description != null && !description.IsEmpty)
            yield return new HoverTip(title, description);
    }
}