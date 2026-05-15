using GensouNoTabibito.GensouNoTabibitoCode.Rewards;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace GensouNoTabibito.GensouNoTabibitoCode.Relics;

public sealed class WeaponLibraryRelic : WeaponBagRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override string DescriptionKey => "GENSOUNOTABIBITO-WEAPON_LIBRARY_RELIC.description";

    public override bool TryModifyCardRewardAlternatives(
        Player player,
        CardReward cardReward,
        List<CardRewardAlternative> alternatives)
    {
        return false;
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (!ReferenceEquals(player, Owner))
            return false;

        rewards.Add(new WeaponLibraryReward(player));
        return true;
    }
}