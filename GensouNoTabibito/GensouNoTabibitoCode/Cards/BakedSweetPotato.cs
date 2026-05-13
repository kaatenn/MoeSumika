using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards;

public class BakedSweetPotato(): GensouNoTabibitoCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new EnergyVar("Increase", 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    public override bool HasTurnEndInHandEffect => true;
    private Decimal _extraEnergy;

    private Decimal ExtraEnergy
    {
        get => _extraEnergy;
        set
        {
            AssertMutable();
            _extraEnergy = value;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await Cmd.Wait(0.5f);
        var debuffs = Owner.Creature.Powers
            .Where(power => power.Type is PowerType.Debuff)
            .ToList();

        foreach (var debuff in debuffs)
            await RemoveDebuff(choiceContext, (dynamic)debuff);
    }

    private async Task RemoveDebuff<TPower>(PlayerChoiceContext choiceContext, TPower debuff)
        where TPower : PowerModel
    {
        if (debuff.Amount == 0)
            return;

        await PowerCmd.Apply<TPower>(
            choiceContext,
            Owner.Creature,
            -debuff.Amount,
            Owner.Creature,
            this);
    }

    protected override Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        DynamicVars.Energy.BaseValue += DynamicVars["Increase"].BaseValue;
        ExtraEnergy += DynamicVars["Increase"].BaseValue;
        return base.OnTurnEndInHand(choiceContext);
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Energy.BaseValue += ExtraEnergy;
    }

    protected override void OnUpgrade() => DynamicVars["Increase"].UpgradeValueBy(1);
    protected override PileType GetResultPileTypeForOnTurnEndInHandEffect() => PileType.Hand;
}
