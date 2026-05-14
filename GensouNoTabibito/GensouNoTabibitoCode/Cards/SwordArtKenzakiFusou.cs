using GensouNoTabibito.GensouNoTabibitoCode.Powers;
using GensouNoTabibito.GensouNoTabibitoCode.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards;

public class SwordArtKenzakiFusou() : GensouNoTabibitoCard(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    private const string PowerAmountKey = "PowerAmount";

    private decimal _observedIaiAmount;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        new(PowerAmountKey, 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<Iai>(),
        HoverTipFactory.FromPower<Battou>()
    ];

    protected override HashSet<CardTag> CanonicalTags => [GensouNoTabibitoTags.SwordSkill];

    private decimal ObservedIaiAmount
    {
        get => _observedIaiAmount;
        set
        {
            AssertMutable();
            _observedIaiAmount = value;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd
            .Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<Iai>(
            choiceContext,
            Owner.Creature,
            -DynamicVars[PowerAmountKey].BaseValue,
            Owner.Creature,
            this);

        await PowerCmd.Apply<Battou>(
            choiceContext,
            Owner.Creature,
            DynamicVars[PowerAmountKey].BaseValue,
            Owner.Creature,
            this);

        ObservedIaiAmount = GetIaiAmount();
    }

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || Pile?.Type == PileType.Hand)
            return;

        var currentIaiAmount = GetIaiAmount();
        if (currentIaiAmount <= ObservedIaiAmount || currentIaiAmount <= 0)
        {
            ObservedIaiAmount = currentIaiAmount;
            return;
        }

        ObservedIaiAmount = currentIaiAmount;
        await CardPileCmd.Add(this, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[PowerAmountKey].UpgradeValueBy(1);
    }

    private decimal GetIaiAmount()
    {
        return Owner.Creature.GetPower<Iai>()?.Amount ?? 0;
    }
}