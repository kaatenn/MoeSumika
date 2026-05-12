using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using GensouNoTabibito.GensouNoTabibitoCode.Keywords;

namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Bow;

public sealed class BowBehavior : WeaponBehavior
{
    public override Task BeforeCardPlayed(WeaponState weapon, Player player, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack)
        {
            // TODO: Delay this attack until end-turn instead of letting it resolve immediately.
            // This likely needs a command-level patch or a supported cancel/redirect hook.
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(
        WeaponState weapon,
        Player player,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // TODO: Track attack cards for delayed bow playback if the play was not cancelled earlier.
        return Task.CompletedTask;
    }

    public override Task BeforeTurnEnd(
        WeaponState weapon,
        Player player,
        PlayerChoiceContext choiceContext,
        CombatSide side)
    {
        // TODO: Randomly choose valid enemies and autoplay delayed bow attacks here.
        return Task.CompletedTask;
    }

    public override IEnumerable<IHoverTip> GetHoverTips(WeaponState weapon)
    {
        foreach (var tip in base.GetHoverTips(weapon))
            yield return tip;

        yield return HoverTipFactory.FromKeyword(GensouNoTabibitoKeywords.BowWeapon);
    }
}