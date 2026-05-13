using BaseLib.Abstracts;
using GensouNoTabibito.GensouNoTabibitoCode.Keywords;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace GensouNoTabibito.GensouNoTabibitoCode.Powers;

public class SwordSkill : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}