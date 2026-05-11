using MegaCrit.Sts2.Core.Entities.Players;

namespace MoeSumika.MoeSumikaCode.Weapons;

public static class PlayerWeaponExtensions
{
    public static WeaponSlotState GetWeaponSlot(this Player player)
    {
        return WeaponSlots.Get(player);
    }
}