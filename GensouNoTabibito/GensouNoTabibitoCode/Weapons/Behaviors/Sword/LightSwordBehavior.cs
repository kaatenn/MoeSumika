using GensouNoTabibito.GensouNoTabibitoCode.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Sword;

public sealed class LightSwordBehavior : SwordBehavior
{
    private const decimal SwordSkillDamageBonus = 2m;
    private static readonly int[] DexterityByLevel = [1, 1, 2, 2, 3];

    public override int MaxLevel => 5;

    protected override IEnumerable<WeaponDynamicVar> CanonicalVars =>
    [
        WeaponDynamicVar.FromLevels("Dexterity", DexterityByLevel),
        new("SwordSkillDamageBonus", SwordSkillDamageBonus)
    ];

    public override async Task BeforeCombatStart(WeaponState weapon, Player player)
    {
        await base.BeforeCombatStart(weapon, player);

        await PowerCmd.Apply<DexterityPower>(
            new ThrowingPlayerChoiceContext(),
            player.Creature,
            GetDexterity(weapon),
            player.Creature,
            null);
    }

    public override IEnumerable<IHoverTip> GetHoverTips(WeaponState weapon)
    {
        foreach (var tip in base.GetHoverTips(weapon))
            yield return tip;

        yield return HoverTipFactory.FromPower<DexterityPower>();
    }

    protected override string GetProgressDescription(WeaponState weapon) => string.Join(" / ", DexterityByLevel);

    public override decimal ModifyDamageAdditive(
        WeaponState weapon,
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return cardSource?.Tags.Contains(GensouNoTabibitoTags.SwordSkill) == true
            ? SwordSkillDamageBonus
            : 0m;
    }

    private static int GetDexterity(WeaponState weapon)
    {
        var index = Math.Clamp(weapon.Level, 1, DexterityByLevel.Length) - 1;
        return DexterityByLevel[index];
    }
}