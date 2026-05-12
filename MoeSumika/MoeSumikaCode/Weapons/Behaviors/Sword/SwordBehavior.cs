using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MoeSumika.MoeSumikaCode.Keywords;
using MoeSumika.MoeSumikaCode.Powers;

namespace MoeSumika.MoeSumikaCode.Weapons.Behaviors.Sword;

public class SwordBehavior : WeaponBehavior
{
    private const int StartingSwordSkill = 5;

    public override async Task BeforeCombatStart(WeaponState weapon, Player player)
    {
        await PowerCmd.Apply<SwordSkill>(
            new ThrowingPlayerChoiceContext(),
            player.Creature,
            StartingSwordSkill,
            player.Creature,
            null);
    }

    public override Task AfterCardPlayed(
        WeaponState weapon,
        Player player,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // TODO: Apply sword-skill rules for cards that consume or scale with weapon.SwordSkill.
        return Task.CompletedTask;
    }

    public override IEnumerable<IHoverTip> GetHoverTips(WeaponState weapon)
    {
        foreach (var tip in base.GetHoverTips(weapon))
            yield return tip;

        yield return HoverTipFactory.FromKeyword(MoeSumikaKeywords.SwordWeapon);
        yield return HoverTipFactory.FromPower<SwordSkill>();
    }
}