using MegaCrit.Sts2.Core.Entities.Players;

namespace MoeSumika.MoeSumikaCode.Weapons;

public static class WeaponSaveSync
{
    public static void SyncPlayerWeaponToRelicCarriers(Player player)
    {
        foreach (var relic in player.Relics)
        {
            if (relic is IWeaponSlotSaveCarrier carrier)
                carrier.SyncSavedWeaponFromPlayerSlot(player);
        }
    }

    public static void RestorePlayerWeaponFromRelicCarriers(Player player)
    {
        foreach (var relic in player.Relics)
        {
            if (relic is IWeaponSlotSaveCarrier carrier)
            {
                carrier.RestorePlayerSlotFromSavedWeapon(player);
                return;
            }
        }
    }
}
