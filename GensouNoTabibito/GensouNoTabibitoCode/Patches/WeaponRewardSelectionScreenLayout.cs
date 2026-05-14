using System.Reflection;
using System.Runtime.CompilerServices;
using GensouNoTabibito.GensouNoTabibitoCode.Cards.Actions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace GensouNoTabibito.GensouNoTabibitoCode.Patches;

public static class WeaponRewardSelectionScreenLayout
{
    private const string WeaponRewardHeaderKey = "GENSOUNOTABIBITO-WEAPON_REWARD_HEADER";
    private const float DefaultCardRewardSpacing = 350f;
    private const float MaxWeaponRewardCardRowWidth = 1400f;

    private static readonly object WeaponRewardScreenMarker = new();
    private static readonly ConditionalWeakTable<NCardRewardSelectionScreen, object> WeaponRewardScreens = new();

    private static readonly FieldInfo CardRewardScreenCardTweenField =
        typeof(NCardRewardSelectionScreen).GetField("_cardTween", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException(typeof(NCardRewardSelectionScreen).FullName, "_cardTween");

    private static void Mark(NCardRewardSelectionScreen screen)
    {
        WeaponRewardScreens.Remove(screen);
        WeaponRewardScreens.Add(screen, WeaponRewardScreenMarker);
    }

    private static void ApplyStatic(NCardRewardSelectionScreen screen)
    {
        Apply(screen, animated: false);
    }

    private static void ApplyAnimated(NCardRewardSelectionScreen screen)
    {
        Apply(screen, animated: true);
    }

    private static void Apply(NCardRewardSelectionScreen screen, bool animated)
    {
        if (!WeaponRewardScreens.TryGetValue(screen, out _))
            return;

        var holders = screen
            .GetNode<Control>("UI/CardRow")
            .GetChildren()
            .OfType<NGridCardHolder>()
            .ToList();

        if (holders.Count <= 1)
            return;

        var spacing = Math.Min(DefaultCardRewardSpacing, MaxWeaponRewardCardRowWidth / (holders.Count - 1));
        var start = Vector2.Left * (float)(holders.Count - 1) * spacing * 0.5f;

        if (!animated)
        {
            for (var index = 0; index < holders.Count; ++index)
                holders[index].Position = start + Vector2.Right * spacing * index;

            return;
        }

        if (CardRewardScreenCardTweenField.GetValue(screen) is Tween currentTween)
            currentTween.Kill();

        var tween = screen.CreateTween().SetParallel();
        CardRewardScreenCardTweenField.SetValue(screen, tween);

        for (var index = 0; index < holders.Count; ++index)
        {
            var holder = holders[index];
            var targetPosition = start + Vector2.Right * spacing * index;

            tween
                .TweenProperty(holder, "position", targetPosition, 0.5)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Expo);

            tween
                .TweenProperty(holder, "modulate", Colors.White, 1.0)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic)
                .From(Colors.Black);
        }
    }

    private static bool ContainsWeaponRewardAction(IReadOnlyList<CardCreationResult> options)
    {
        return options.Any(option => option.Card is WeaponRewardActionCard);
    }

    private static void UpdateWeaponRewardScreenHeader(NCardRewardSelectionScreen screen)
    {
        var banner = screen.GetNode<NCommonBanner>("UI/Banner");
        var title = new LocString("gameplay_ui", WeaponRewardHeaderKey);
        banner.label.SetTextAutoSize(title.GetFormattedText());
    }

    [HarmonyPatch(typeof(NCardRewardSelectionScreen), nameof(NCardRewardSelectionScreen.RefreshOptions))]
    private static class RefreshOptionsPatch
    {
        private static void Postfix(
            NCardRewardSelectionScreen __instance,
            IReadOnlyList<CardCreationResult> options)
        {
            if (!ContainsWeaponRewardAction(options))
                return;

            Mark(__instance);
            UpdateWeaponRewardScreenHeader(__instance);
            ApplyAnimated(__instance);
        }
    }

    [HarmonyPatch(typeof(NCardRewardSelectionScreen), nameof(NCardRewardSelectionScreen.AfterOverlayShown))]
    private static class AfterOverlayShownPatch
    {
        private static void Postfix(NCardRewardSelectionScreen __instance)
        {
            ApplyStatic(__instance);
        }
    }

    [HarmonyPatch(typeof(NCardRewardSelectionScreen), nameof(NCardRewardSelectionScreen.AfterOverlayOpened))]
    private static class AfterOverlayOpenedPatch
    {
        private static void Postfix(NCardRewardSelectionScreen __instance)
        {
            ApplyStatic(__instance);
        }
    }
}