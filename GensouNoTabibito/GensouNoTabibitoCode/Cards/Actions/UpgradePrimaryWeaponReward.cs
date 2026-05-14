using GensouNoTabibito.GensouNoTabibitoCode.Relics;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards.Actions;

#pragma warning disable STS004
public sealed class UpgradePrimaryWeaponReward : WeaponRewardActionCard
#pragma warning restore STS004
{
    public override void Resolve(WeaponBagRelic weaponBag)
    {
        weaponBag.UpgradePrimaryWeaponFromReward();
    }
}