using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MoeSumika.MoeSumikaCode.Weapons.Effects;

public sealed class BrokenSwordWeaponEffect : SwordWeaponEffect
{
    public override async Task AfterSideTurnStart(
        WeaponState weapon,
        Player player,
        CombatSide side,
        ICombatState combatState)
    {
        if (side != player.Creature.Side || combatState.RoundNumber > 1)
            return;

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
}
