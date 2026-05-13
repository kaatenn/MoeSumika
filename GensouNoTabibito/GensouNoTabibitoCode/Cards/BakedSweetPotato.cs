using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

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