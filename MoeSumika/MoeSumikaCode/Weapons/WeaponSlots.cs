using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MoeSumika.MoeSumikaCode.Weapons;

public static class WeaponSlots
{
    private static readonly ConditionalWeakTable<Player, WeaponSlotState> Slots = new();

    public static WeaponSlotState Get(Player player)
    {
        return Slots.GetValue(player, static _ => new WeaponSlotState());
    }

    // TODO: Persist and restore weapon slot state with Player save data. This service deliberately
    // models the weapon as player-owned state; save/load wiring still needs a supported API or patch.
}