using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MoeSumika.MoeSumikaCode.Weapons.Behaviors.Sword;

public class SwordBehavior : WeaponBehavior
{
    public override Task AfterCardPlayed(
        WeaponState weapon,
        Player player,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // TODO: Apply sword-skill rules for cards that consume or scale with weapon.SwordSkill.
        return Task.CompletedTask;
    }
}