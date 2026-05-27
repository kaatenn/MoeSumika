using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards;

public class BakedSweetPotato() : GensouNoTabibitoCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    private Decimal _extraHealth;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(1),
        new HealVar("Increase", 1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    private Decimal ExtraHealth
    {
        get => _extraHealth;
        set
        {
            AssertMutable();
            _extraHealth = value;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.IntValue);
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

    public override Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
        {
            return base.BeforeSideTurnEnd(choiceContext, side, participants);
        }

        var card = PileType.Hand.GetPile(Owner).Cards.Where(card => card is BakedSweetPotato);

        if (!card.Contains(this))
        {
            return base.OnTurnEndInHand(choiceContext);
        }

        DynamicVars.Heal.BaseValue += DynamicVars["Increase"].BaseValue;
        ExtraHealth += DynamicVars["Increase"].BaseValue;

        return base.OnTurnEndInHand(choiceContext);
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Heal.BaseValue += ExtraHealth;
    }

    protected override void OnUpgrade() => DynamicVars["Increase"].UpgradeValueBy(1);
}