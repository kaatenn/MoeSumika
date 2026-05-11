using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MoeSumika.MoeSumikaCode.Powers;

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

        await GiveDramaticEntrance(weapon, player);
    }

    public override async Task AfterRoomEntered(WeaponState weapon, Player player, AbstractRoom room)
    {
        if (room is not CombatRoom)
        {
            return;
        }

        await GiveSwordSkill(player);
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

    private async Task GiveSwordSkill(Player player)
    {
        await PowerCmd.Apply<SwordSkill>(
            new ThrowingPlayerChoiceContext(),
            player.Creature,
            5,
            player.Creature,
            null);
    }
}