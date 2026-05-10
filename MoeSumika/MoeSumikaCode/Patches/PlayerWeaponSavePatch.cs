using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MoeSumika.MoeSumikaCode.Weapons;

namespace MoeSumika.MoeSumikaCode.Patches;

[HarmonyPatch(typeof(Player), nameof(Player.ToSerializable))]
public static class PlayerToSerializableWeaponPatch
{
    public static void Prefix(Player __instance)
    {
        WeaponSaveSync.SyncPlayerWeaponToRelicCarriers(__instance);
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.FromSerializable))]
public static class PlayerFromSerializableWeaponPatch
{
    public static void Postfix(Player __result)
    {
        WeaponSaveSync.RestorePlayerWeaponFromRelicCarriers(__result);
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.SyncWithSerializedPlayer))]
public static class PlayerSyncWithSerializedPlayerWeaponPatch
{
    public static void Postfix(Player __instance)
    {
        WeaponSaveSync.RestorePlayerWeaponFromRelicCarriers(__instance);
    }
}
