using System.Globalization;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons;

public sealed class WeaponDynamicVar
{
    private readonly Func<int, object?> _getValue;

    public WeaponDynamicVar(string name, object? value)
        : this(name, _ => value)
    {
    }

    public WeaponDynamicVar(string name, Func<int, object?> getValue)
    {
        Name = name;
        _getValue = getValue;
    }

    public string Name { get; }

    public string GetText(int level)
    {
        return FormatValue(_getValue(level));
    }

    public static WeaponDynamicVar FromLevels<T>(string name, IReadOnlyList<T> values)
    {
        return new WeaponDynamicVar(name, level =>
        {
            var index = Math.Clamp(level, 1, values.Count) - 1;
            return values[index];
        });
    }

    public static WeaponDynamicVar FromCard<TCard>(string name, Func<int, bool> isUpgraded)
        where TCard : CardModel
    {
        return new WeaponDynamicVar(
            name,
            level => ((CardHoverTip)HoverTipFactory.FromCard<TCard>(isUpgraded(level))).Card.Title);
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}