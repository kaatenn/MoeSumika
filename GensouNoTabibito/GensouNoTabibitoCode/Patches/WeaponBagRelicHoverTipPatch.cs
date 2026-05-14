using GensouNoTabibito.GensouNoTabibitoCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace GensouNoTabibito.GensouNoTabibitoCode.Patches;

[HarmonyPatch(typeof(RelicModel), "get_HoverTip")]
public static class WeaponBagRelicHoverTipPatch
{
    public static void Postfix(RelicModel __instance, ref HoverTip __result)
    {
        if (__instance is WeaponBagRelic weaponBag)
            __result = weaponBag.CreateCurrentHoverTip();
    }
}
