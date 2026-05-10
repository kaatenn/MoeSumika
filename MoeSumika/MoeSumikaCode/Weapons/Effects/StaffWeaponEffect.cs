using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MoeSumika.MoeSumikaCode.Weapons.Effects;

public sealed class StaffWeaponEffect : WeaponEffect
{
    public override Task AfterCardPlayed(WeaponState weapon, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: Read element-producing cards and add elemental state to the player.
        return Task.CompletedTask;
    }
}
