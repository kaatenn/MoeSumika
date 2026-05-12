using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace MoeSumika.MoeSumikaCode.Weapons;

public abstract class WeaponBehavior : IWeaponBehavior
{
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
}