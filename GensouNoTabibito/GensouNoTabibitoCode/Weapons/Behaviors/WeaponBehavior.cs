using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons;

public abstract class WeaponBehavior : IWeaponBehavior
{
    private const string LocalizationTable = "weapons";
    private const string BehaviorSuffix = "Behavior";
    private const string ModPrefix = "GENSOUNOTABIBITO-";

    protected virtual string LocalizationId => GetLocalizationId(GetType());
    protected virtual string DescriptionKey => $"{LocalizationId}.description";
    protected virtual IEnumerable<WeaponDynamicVar> CanonicalVars => [];

    public virtual int MaxLevel => int.MaxValue;

    public virtual Task BeforeCombatStart(WeaponState weapon, Player player)
    {
        return Task.CompletedTask;
    }

    public virtual Task AfterRoomEntered(WeaponState weapon, Player player, AbstractRoom room)
    {
        return Task.CompletedTask;
    }

    public virtual Task AfterSideTurnStart(
        WeaponState weapon,
        Player player,
        CombatSide side,
        ICombatState combatState)
    {
        return Task.CompletedTask;
    }

    public virtual Task BeforeCardPlayed(WeaponState weapon, Player player, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    public virtual Task AfterCardPlayed(
        WeaponState weapon,
        Player player,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    public virtual Task BeforeTurnEnd(
        WeaponState weapon,
        Player player,
        PlayerChoiceContext choiceContext,
        CombatSide side)
    {
        return Task.CompletedTask;
    }

    public virtual decimal ModifyDamageAdditive(
        WeaponState weapon,
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return 0m;
    }

    public virtual IEnumerable<IHoverTip> GetHoverTips(WeaponState weapon)
    {
        yield return new HoverTip(new LocString(LocalizationTable, $"{LocalizationId}.name"), BuildDescription(weapon),
            null);
    }

    protected virtual string? GetProgressDescription(WeaponState weapon)
    {
        return null;
    }

    private string BuildDescription(WeaponState weapon)
    {
        var currentDescription = GetDescriptionForLevel(weapon.Level);
        var lines = new List<string>
        {
            FormatLine("GENSOUNOTABIBITO-WEAPON.current", currentDescription)
        };

        if (weapon.Level < MaxLevel)
            lines.Add(FormatLine(
                "GENSOUNOTABIBITO-WEAPON.next",
                GetDescriptionForLevel(weapon.Level + 1)));
        else
            lines.Add(new LocString(LocalizationTable, "GENSOUNOTABIBITO-WEAPON.maxLevel").GetFormattedText());

        var progressDescription = GetProgressDescription(weapon);
        if (!string.IsNullOrWhiteSpace(progressDescription))
            lines.Add(FormatLine("GENSOUNOTABIBITO-WEAPON.progress", progressDescription));

        return string.Join('\n', lines);
    }

    private string GetDescriptionForLevel(int level)
    {
        var description = new LocString(LocalizationTable, DescriptionKey);
        foreach (var variable in CanonicalVars)
            description.Add(variable.Name, variable.GetText(level));

        return description.GetFormattedText();
    }

    private static string FormatLine(string key, string value)
    {
        var locString = new LocString(LocalizationTable, key);
        locString.Add("0", value);
        return locString.GetFormattedText();
    }

    private static string GetLocalizationId(Type behaviorType)
    {
        var name = behaviorType.Name;
        if (name.EndsWith(BehaviorSuffix, StringComparison.Ordinal))
            name = name[..^BehaviorSuffix.Length];

        return ModPrefix + ToScreamingSnakeCase(name);
    }

    private static string ToScreamingSnakeCase(string value)
    {
        var chars = new List<char>(value.Length + 4);
        for (var i = 0; i < value.Length; ++i)
        {
            var c = value[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(value[i - 1]))
                chars.Add('_');

            chars.Add(char.ToUpperInvariant(c));
        }

        return new string(chars.ToArray());
    }
}