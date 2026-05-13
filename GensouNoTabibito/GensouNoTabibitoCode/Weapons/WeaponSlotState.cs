namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons;

public sealed class WeaponSlotState
{
    public WeaponState? PrimaryWeapon { get; private set; }
    public WeaponState? SecondaryWeapon { get; private set; }

    public WeaponState? CurrentWeapon => PrimaryWeapon;

    public bool HasWeapon => PrimaryWeapon != null;
    public bool CanHoldSecondaryWeapon => PrimaryWeapon?.Kind == WeaponKind.Sword;
    public bool CanUpgradeCurrentWeapon => PrimaryWeapon != null && WeaponBehaviorRegistry.CanUpgrade(PrimaryWeapon);
    public bool CanUpgradeSecondaryWeapon => SecondaryWeapon != null && WeaponBehaviorRegistry.CanUpgrade(SecondaryWeapon);

    public IEnumerable<WeaponState> Weapons
    {
        get
        {
            if (PrimaryWeapon != null)
                yield return PrimaryWeapon;

            if (SecondaryWeapon != null)
                yield return SecondaryWeapon;
        }
    }

    public WeaponState EnsureWeaponEquipped(WeaponState fallbackWeapon)
    {
        PrimaryWeapon ??= fallbackWeapon;
        EnforceCapacity();
        return PrimaryWeapon;
    }

    public void ReplaceWeapon(WeaponState weapon)
    {
        PrimaryWeapon = weapon;
        EnforceCapacity();

        // TODO: Notify/update the custom weapon-slot UI once that UI exists.
    }

    public void ReplaceSecondaryWeapon(WeaponState? weapon)
    {
        SecondaryWeapon = CanHoldSecondaryWeapon ? weapon : null;

        // TODO: Notify/update the custom weapon-slot UI once that UI exists.
    }

    public void EquipWeapon(WeaponState weapon)
    {
        if (PrimaryWeapon == null)
        {
            PrimaryWeapon = weapon;
        }
        else if (CanHoldSecondaryWeapon && SecondaryWeapon == null)
        {
            SecondaryWeapon = weapon;
        }
        else
        {
            PrimaryWeapon = weapon;
        }

        EnforceCapacity();

        // TODO: When weapon drafting UI exists, ask which occupied slot to replace.
    }

    public bool UpgradeCurrentWeapon()
    {
        if (PrimaryWeapon == null)
            return false;

        return WeaponBehaviorRegistry.TryUpgrade(PrimaryWeapon);
    }

    public bool UpgradeSecondaryWeapon()
    {
        if (SecondaryWeapon == null)
            return false;

        return WeaponBehaviorRegistry.TryUpgrade(SecondaryWeapon);
    }

    private void EnforceCapacity()
    {
        if (!CanHoldSecondaryWeapon)
            SecondaryWeapon = null;
    }
}
