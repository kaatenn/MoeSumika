using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MoeSumika.MoeSumikaCode.Weapons;

public static class WeaponSlots
{
    private static readonly ConditionalWeakTable<Player, WeaponSlotState> Slots = new();

    public static WeaponSlotState GetWeaponSlot(this Player player)
    {
        return Get(player);
    }

    public static WeaponSlotState Get(Player player)
    {
        return Slots.GetValue(player, static _ => new WeaponSlotState());
    }

    // Weapon slot state is player-owned at runtime. Persistence is carried by WeaponBagRelic,
    // which saves and restores this state through IWeaponSlotSaveCarrier.
}