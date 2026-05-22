using BaseLib.Abstracts;
using BaseLib.Utils;
using GensouNoTabibito.GensouNoTabibitoCode.Character.CardPools;
using GensouNoTabibito.GensouNoTabibitoCode.Extensions;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards.Actions;

[Pool(typeof(WeaponActionCardPool))]
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
    public override CardPoolModel Pool => ModelDb.CardPool<WeaponActionCardPool>();
    public override CardPoolModel VisualCardPool => Pool;
    public override string CustomPortraitPath => "relic.png".BigRelicImagePath();
    public override string PortraitPath => "relic.png".RelicImagePath();
    public override string BetaPortraitPath => "relic.png".RelicImagePath();

    public abstract void Resolve(IWeaponRewardOwner weaponRewardOwner);

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }
}