using GensouNoTabibito.GensouNoTabibitoCode.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GensouNoTabibito.GensouNoTabibitoCode.Powers;

public class Iai : GensouNoTabibitoPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private const int RequiredSwordSkillAmount = 4;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("RequiredSwordSkill", RequiredSwordSkillAmount)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SwordSkill>(),
        HoverTipFactory.FromKeyword(GensouNoTabibitoKeywords.SwordSkillRequirement)
    ];

    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource == null || cardSource.Owner.Creature != Owner)
            return 1m;

        var swordSkillPower = Owner.GetPower<SwordSkill>();
        if (swordSkillPower == null || swordSkillPower.Amount < RequiredSwordSkillAmount)
            return 1m;

        return (decimal)Math.Pow(1.1, Amount);
    }
}
