namespace MoeSumika.MoeSumikaCode.Weapons;

public sealed class WeaponSlotState
{
    public WeaponState? PrimaryWeapon { get; private set; }
    public WeaponState? SecondaryWeapon { get; private set; }

    public WeaponState? CurrentWeapon => PrimaryWeapon;

    public bool HasWeapon => PrimaryWeapon != null;
    public bool CanHoldSecondaryWeapon => PrimaryWeapon?.Kind == WeaponKind.Sword;

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

    public void UpgradeCurrentWeapon()
    {
        PrimaryWeapon?.Upgrade();

        // TODO: Flash relic or play a weapon-upgrade VFX/SFX.
    }

    public void UpgradeSecondaryWeapon()
    {
        SecondaryWeapon?.Upgrade();

        // TODO: Let weapon-upgrade rewards choose which weapon to upgrade when dual-wielding.
    }

    private void EnforceCapacity()
    {
        if (!CanHoldSecondaryWeapon)
            SecondaryWeapon = null;
    }
}
