using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors;

namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons;

public sealed class WeaponSlotState
{
    public WeaponState? PrimaryWeapon { get; private set; }
    public WeaponState? SecondaryWeapon { get; private set; }

    public WeaponState? CurrentWeapon => PrimaryWeapon;

    public bool HasWeapon => PrimaryWeapon != null;
    public bool CanHoldSecondaryWeapon => PrimaryWeapon?.Kind == WeaponKind.Sword;
    public bool CanUpgradeCurrentWeapon => PrimaryWeapon != null && WeaponBehaviorHookBridge.CanUpgrade(PrimaryWeapon);

    public bool CanUpgradeSecondaryWeapon =>
        SecondaryWeapon != null && WeaponBehaviorHookBridge.CanUpgrade(SecondaryWeapon);

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

    public IEnumerable<EquippedWeapon> EquippedWeapons
    {
        get
        {
            if (PrimaryWeapon != null)
                yield return new EquippedWeapon(WeaponSlot.Primary, PrimaryWeapon);

            if (SecondaryWeapon != null)
                yield return new EquippedWeapon(WeaponSlot.Secondary, SecondaryWeapon);
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

    public bool DiscardPrimaryWeapon()
    {
        if (PrimaryWeapon == null)
            return false;

        PrimaryWeapon = SecondaryWeapon;
        SecondaryWeapon = null;
        EnforceCapacity();

        // TODO: Notify/update the custom weapon-slot UI once that UI exists.
        return true;
    }

    public bool DiscardSecondaryWeapon()
    {
        if (SecondaryWeapon == null)
            return false;

        SecondaryWeapon = null;

        // TODO: Notify/update the custom weapon-slot UI once that UI exists.
        return true;
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

        return WeaponBehaviorHookBridge.TryUpgrade(PrimaryWeapon);
    }

    public bool UpgradeSecondaryWeapon()
    {
        if (SecondaryWeapon == null)
            return false;

        return WeaponBehaviorHookBridge.TryUpgrade(SecondaryWeapon);
    }

    private void EnforceCapacity()
    {
        if (!CanHoldSecondaryWeapon)
            SecondaryWeapon = null;
    }
}