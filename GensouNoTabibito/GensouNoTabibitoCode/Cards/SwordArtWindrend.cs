using BaseLib.Extensions;
using GensouNoTabibito.GensouNoTabibitoCode.Powers;
using GensouNoTabibito.GensouNoTabibitoCode.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards;

public class SwordArtWindrend() : GensouNoTabibitoCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4, ValueProp.Move),
        new PowerVar<BlurPower>(1M)
    ];

    protected override HashSet<CardTag> CanonicalTags => [GensouNoTabibitoTags.SwordSkill];


    protected override bool ShouldGlowGoldInternal => HasEnemyIntendToAttack();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(
            Owner.Creature,
            DynamicVars.Block,
            cardPlay
        );
        if (HasEnemyIntendToAttack())
        {
            await PowerCmd.Apply<BlurPower>(
                choiceContext,
                Owner.Creature,
                DynamicVars.Power<BlurPower>().BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars.Power<BlurPower>().UpgradeValueBy(1);
    }

    private bool HasEnemyIntendToAttack()
    {
        return CombatState != null && CombatState.HittableEnemies.Any(e =>
            e.Monster is { IntendsToAttack: true });
    }
}