using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MoeSumika.MoeSumikaCode.Weapons.Effects;

public interface IWeaponEffect
{
    Task BeforeCombatStart(WeaponState weapon, Player player);
    Task AfterSideTurnStart(WeaponState weapon, Player player, CombatSide side, ICombatState combatState);
    Task BeforeCardPlayed(WeaponState weapon, CardPlay cardPlay);
    Task AfterCardPlayed(WeaponState weapon, PlayerChoiceContext choiceContext, CardPlay cardPlay);
    Task BeforeTurnEnd(WeaponState weapon, PlayerChoiceContext choiceContext, CombatSide side);
}
