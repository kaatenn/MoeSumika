using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using GensouNoTabibito.GensouNoTabibitoCode.Cards;

namespace GensouNoTabibito.GensouNoTabibitoCode.Powers;

public class PeregrinPathTempDexLoss : CustomTemporaryPowerModel
{
    public override PowerModel InternallyAppliedPower => ModelDb.Power<DexterityPower>();
    public override AbstractModel OriginModel => ModelDb.Card<PeregrinPath>();

    protected override bool InvertInternalPowerAmount => true;

    protected override Func<PlayerChoiceContext, Creature, decimal, Creature?, CardModel?, bool, Task> ApplyPowerFunc =>
        PowerCmd.Apply<DexterityPower>;

    public override LocString Description => new("powers", "TEMPORARY_DEXTERITY_DOWN.description");

    protected override string SmartDescriptionLocKey => "TEMPORARY_DEXTERITY_DOWN.smartDescription";

    public override LocString Title => ((CardModel)OriginModel).TitleLocString;
}