using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using GensouNoTabibito.GensouNoTabibitoCode.Character.CardPools;
using GensouNoTabibito.GensouNoTabibitoCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards.Staff;

[Pool(typeof(GensouNoTabibitoCardPool))]
public abstract class StaffGeneratedCard(
    int cost,
    CardType type,
    CardRarity rarity,
    TargetType target)
    : CustomCardModel(
        cost,
        type,
        rarity,
        target,
        showInCardLibrary: false,
        autoAdd: false)
{
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override CardPoolModel Pool => ModelDb.CardPool<GensouNoTabibitoCardPool>();
    public override CardPoolModel VisualCardPool => Pool;
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
}