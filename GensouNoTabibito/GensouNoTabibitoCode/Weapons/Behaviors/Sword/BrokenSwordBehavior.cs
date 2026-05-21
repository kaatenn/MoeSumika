using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Sword;

public sealed class BrokenSwordBehavior : SwordBehavior
{
    public override int MaxLevel => 2;

    protected override IEnumerable<WeaponDynamicVar> CanonicalVars =>
    [
        WeaponDynamicVar.FromCard<DramaticEntrance>("DramaticEntranceName", level => level >= 2)
    ];

    public override async Task AfterSideTurnStart(
        WeaponState weapon,
        Player player,
        CombatSide side,
        ICombatState combatState)
    {
        if (side != player.Creature.Side || combatState.RoundNumber > 1)
            return;

        await GiveDramaticEntrance(weapon, player);
    }

    private async Task GiveDramaticEntrance(WeaponState weapon, Player player)
    {
        var card = CardFactory.GetForCombat(
                player,
                [ModelDb.Card<DramaticEntrance>()],
                1,
                player.RunState.Rng.CombatCardGeneration)
            .Single();

        if (weapon.Level >= 2)
            CardCmd.Upgrade(card, CardPreviewStyle.None);

        await CardPileCmd.AddGeneratedCardsToCombat(
            [card],
            PileType.Hand,
            player,
            CardPilePosition.Top);
    }

    public override IEnumerable<IHoverTip> GetHoverTips(WeaponState weapon)
    {
        foreach (var tip in base.GetHoverTips(weapon))
            yield return tip;

        foreach (var hoverTip in HoverTipFactory.FromCardWithCardHoverTips<DramaticEntrance>(weapon.Level >= 2))
        {
            yield return hoverTip;
        }
    }
}