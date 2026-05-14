using BaseLib.Abstracts;
using BaseLib.Utils;
using GensouNoTabibito.GensouNoTabibitoCode.Character;
using GensouNoTabibito.GensouNoTabibitoCode.Extensions;
using GensouNoTabibito.GensouNoTabibitoCode.Relics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards.Actions;

[Pool(typeof(GensouNoTabibitoCardPool))]
public abstract class WeaponRewardActionCard() : CustomCardModel(
    -1,
    CardType.Status,
    CardRarity.Status,
    TargetType.None,
    showInCardLibrary: false,
    autoAdd: false)
{
    public override int MaxUpgradeLevel => 0;
    public override bool CanBeGeneratedInCombat => false;
    public override CardPoolModel Pool => ModelDb.CardPool<GensouNoTabibitoCardPool>();
    public override CardPoolModel VisualCardPool => Pool;
    public override string CustomPortraitPath => "relic.png".BigRelicImagePath();
    public override string PortraitPath => "relic.png".RelicImagePath();
    public override string BetaPortraitPath => "relic.png".RelicImagePath();

    public abstract void Resolve(WeaponBagRelic weaponBag);

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }
}
