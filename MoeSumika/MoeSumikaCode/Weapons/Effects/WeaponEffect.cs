using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MoeSumika.MoeSumikaCode.Weapons.Effects;

public abstract class WeaponEffect : IWeaponEffect
{
    public virtual Task BeforeCombatStart(WeaponState weapon, Player player)
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

    public virtual Task BeforeCardPlayed(WeaponState weapon, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    public virtual Task AfterCardPlayed(WeaponState weapon, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    public virtual Task BeforeTurnEnd(WeaponState weapon, PlayerChoiceContext choiceContext, CombatSide side)
    {
        return Task.CompletedTask;
    }
}
