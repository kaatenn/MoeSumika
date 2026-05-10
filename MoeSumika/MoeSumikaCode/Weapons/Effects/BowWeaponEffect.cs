using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MoeSumika.MoeSumikaCode.Weapons.Effects;

public sealed class BowWeaponEffect : WeaponEffect
{
    public override Task BeforeCardPlayed(WeaponState weapon, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack)
        {
            // TODO: Delay this attack until end-turn instead of letting it resolve immediately.
            // This likely needs a command-level patch or a supported cancel/redirect hook.
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(WeaponState weapon, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: Track attack cards for delayed bow playback if the play was not cancelled earlier.
        return Task.CompletedTask;
    }

    public override Task BeforeTurnEnd(WeaponState weapon, PlayerChoiceContext choiceContext, CombatSide side)
    {
        // TODO: Randomly choose valid enemies and autoplay delayed bow attacks here.
        return Task.CompletedTask;
    }
}
