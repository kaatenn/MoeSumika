using GensouNoTabibito.GensouNoTabibitoCode.Weapons;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards.Actions;

#pragma warning disable STS004
public sealed class DiscardPrimaryWeaponReward : WeaponRewardActionCard
#pragma warning restore STS004
{
    public override void Resolve(IWeaponRewardOwner weaponRewardOwner)
    {
        weaponRewardOwner.DiscardPrimaryWeaponFromReward();
    }
}