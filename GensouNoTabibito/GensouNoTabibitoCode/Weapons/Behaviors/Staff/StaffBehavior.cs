using GensouNoTabibito.GensouNoTabibitoCode.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Staff;

public sealed class StaffBehavior : WeaponBehavior
{
    public override int MaxLevel => 2;
    private int MagicLevel { get; set; }

    public override Task AfterCardPlayed(
        WeaponState weapon,
        Player player,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // TODO: Read element-producing cards and add elemental state to the player.
        return Task.CompletedTask;
    }

    public override IEnumerable<IHoverTip> GetHoverTips(WeaponState weapon)
    {
        foreach (var tip in base.GetHoverTips(weapon))
            yield return tip;

        yield return HoverTipFactory.FromKeyword(GensouNoTabibitoKeywords.StaffWeapon);
    }
}