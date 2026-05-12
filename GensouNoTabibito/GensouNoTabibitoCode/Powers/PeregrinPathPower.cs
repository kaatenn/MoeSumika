using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GensouNoTabibito.GensouNoTabibitoCode.Powers;

public class PeregrinPathPower : GensouNoTabibitoPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || dealer == null || !props.IsPoweredAttack() || result.UnblockedDamage <= 0)
        {
            return;
        }

        await PowerCmd.Apply<PeregrinPathTempDexLoss>(
            choiceContext,
            Owner,
            Amount,
            Owner,
            cardSource);
    }
}