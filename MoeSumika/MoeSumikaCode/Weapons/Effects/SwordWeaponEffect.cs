using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MoeSumika.MoeSumikaCode.Weapons.Effects;

public class SwordWeaponEffect : WeaponEffect
{
    public override Task AfterCardPlayed(WeaponState weapon, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: Apply sword-skill rules for cards that consume or scale with weapon.SwordSkillLevel.
        return Task.CompletedTask;
    }
}
