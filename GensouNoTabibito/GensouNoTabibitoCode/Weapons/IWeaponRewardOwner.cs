namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons;

public interface IWeaponRewardOwner
{
    void DraftWeaponFromReward(WeaponState weapon);
    void DraftPrimaryWeaponFromReward(WeaponState weapon);
    void DraftSecondaryWeaponFromReward(WeaponState weapon);
    void UpgradeWeaponFromReward();
    void UpgradePrimaryWeaponFromReward();
    void UpgradeSecondaryWeaponFromReward();
    void DiscardPrimaryWeaponFromReward();
    void DiscardSecondaryWeaponFromReward();
}