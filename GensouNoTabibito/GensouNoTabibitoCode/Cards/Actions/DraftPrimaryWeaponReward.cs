using GensouNoTabibito.GensouNoTabibitoCode.Weapons;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards.Actions;

#pragma warning disable STS004
public sealed class DraftPrimaryWeaponReward : DraftWeaponRewardCard
#pragma warning restore STS004
{
    public override void Resolve(IWeaponRewardOwner weaponRewardOwner)
    {
        weaponRewardOwner.DraftPrimaryWeaponFromReward(GetDraftedWeapon());
    }
}