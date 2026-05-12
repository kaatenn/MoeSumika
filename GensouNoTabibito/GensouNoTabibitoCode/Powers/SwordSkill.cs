using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace GensouNoTabibito.GensouNoTabibitoCode.Powers;

public class SwordSkill : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}