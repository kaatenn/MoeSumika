using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using GensouNoTabibito.GensouNoTabibitoCode.Powers;
using GensouNoTabibito.GensouNoTabibitoCode.Tags;

namespace GensouNoTabibito.GensouNoTabibitoCode.Hooks;

[HarmonyPatch(typeof(CardModel))]
public static class SwordSkillTagHook
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CardModel.OnPlayWrapper))]
    private static async void OnCardPlayPostfix(
        Task __result,
        CardModel __instance,
        PlayerChoiceContext choiceContext,
        object target,
        bool isAutoPlay,
        object resources,
        bool skipCardPileVisuals)
    {
        await __result;

        if (__instance.Tags.Contains(GensouNoTabibitoTags.SwordSkill) && __instance.Owner != null)
        {
            await PowerCmd.Apply<SwordSkill>(
                choiceContext,
                __instance.Owner.Creature,
                1,
                __instance.Owner.Creature,
                __instance);
        }
    }
}
