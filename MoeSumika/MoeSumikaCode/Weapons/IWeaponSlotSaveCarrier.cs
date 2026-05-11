using MegaCrit.Sts2.Core.Entities.Players;

namespace MoeSumika.MoeSumikaCode.Weapons;

public interface IWeaponSlotSaveCarrier
{
    void SyncSavedWeaponFromPlayerSlot(Player player);
    void RestorePlayerSlotFromSavedWeapon(Player player);
}