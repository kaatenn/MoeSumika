using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace MoeSumika.MoeSumikaCode.Weapons;

public interface IWeaponBehavior
{
    Task BeforeCombatStart(WeaponState weapon, Player player);
    Task AfterRoomEntered(WeaponState weapon, Player player, AbstractRoom room);
    Task AfterSideTurnStart(WeaponState weapon, Player player, CombatSide side, ICombatState combatState);
    Task BeforeCardPlayed(WeaponState weapon, Player player, CardPlay cardPlay);
    Task AfterCardPlayed(WeaponState weapon, Player player, PlayerChoiceContext choiceContext, CardPlay cardPlay);
    Task BeforeTurnEnd(WeaponState weapon, Player player, PlayerChoiceContext choiceContext, CombatSide side);
}