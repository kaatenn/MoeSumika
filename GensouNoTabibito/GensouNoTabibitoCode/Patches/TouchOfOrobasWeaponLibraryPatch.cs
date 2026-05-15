using GensouNoTabibito.GensouNoTabibitoCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace GensouNoTabibito.GensouNoTabibitoCode.Patches;

[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
public static class TouchOfOrobasWeaponLibraryPatch
{
    public static bool Prefix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic.Id != ModelDb.Relic<WeaponBagRelic>().Id)
            return true;

        __result = ModelDb.Relic<WeaponLibraryRelic>();
        return false;
    }
}