namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons;

public readonly record struct EquippedWeapon(WeaponSlot Slot, WeaponState Weapon);

public enum WeaponSlot
{
    Primary,
    Secondary
}