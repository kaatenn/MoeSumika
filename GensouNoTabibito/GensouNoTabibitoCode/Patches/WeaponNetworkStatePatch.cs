using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons;

namespace GensouNoTabibito.GensouNoTabibitoCode.Patches;

[HarmonyPatch(typeof(NetFullCombatState), nameof(NetFullCombatState.FromRun))]
public static class NetFullCombatStateFromRunWeaponPatch
{
    public static void Postfix(IRunState runState, GameAction? justFinishedAction, NetFullCombatState __result)
    {
        WeaponNetworkStates.Set(__result, WeaponNetworkState.FromRun(runState));
    }
}

[HarmonyPatch(typeof(NetFullCombatState), nameof(NetFullCombatState.Serialize))]
public static class NetFullCombatStateSerializeWeaponPatch
{
    public static void Postfix(NetFullCombatState __instance, PacketWriter writer)
    {
        writer.WriteBool(true);
        (WeaponNetworkStates.Get(__instance) ?? new WeaponNetworkState()).Serialize(writer);
    }
}

[HarmonyPatch(typeof(NetFullCombatState), nameof(NetFullCombatState.Deserialize))]
public static class NetFullCombatStateDeserializeWeaponPatch
{
    public static void Postfix(NetFullCombatState __instance, PacketReader reader)
    {
        if (!reader.ReadBool())
            return;

        WeaponNetworkStates.Set(__instance, WeaponNetworkState.Deserialize(reader));
    }
}

[HarmonyPatch(typeof(NetFullCombatState), nameof(NetFullCombatState.ToString))]
public static class NetFullCombatStateToStringWeaponPatch
{
    public static void Postfix(NetFullCombatState __instance, ref string __result)
    {
        var weaponState = WeaponNetworkStates.Get(__instance);
        if (weaponState == null)
            return;

        __result += Environment.NewLine + "Weapon state:" + Environment.NewLine + weaponState.ToDebugString();
    }
}