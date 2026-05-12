using MegaCrit.Sts2.Core.Localization;

namespace MoeSumika.MoeSumikaCode.Weapons;

public static class WeaponLocalization
{
    private const string Table = "relics";

    public static string GetTitle(WeaponState? weapon)
    {
        return GetTitleLocString(weapon)?.GetFormattedText() ?? "None";
    }

    public static string GetDescription(WeaponState? weapon)
    {
        return GetDescriptionLocString(weapon)?.GetFormattedText() ?? string.Empty;
    }

    public static LocString? GetTitleLocString(WeaponState? weapon)
    {
        return GetLocString(weapon?.Id ?? "MOESUMIKA-NONE", "title");
    }

    public static LocString? GetDescriptionLocString(WeaponState? weapon)
    {
        return weapon == null
            ? null
            : GetLocString(weapon.Id, "description");
    }

    private static LocString? GetLocString(string id, string suffix)
    {
        var key = $"{id}.weapon{char.ToUpperInvariant(suffix[0])}{suffix[1..]}";
        return LocString.Exists(Table, key)
            ? new LocString(Table, key)
            : null;
    }
}