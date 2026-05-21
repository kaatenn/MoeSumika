using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using FileAccess = Godot.FileAccess;

namespace GensouNoTabibito.GensouNoTabibitoCode.Patches;

[HarmonyPatch(typeof(LocManager))]
public static class WeaponLocalizationTablePatch
{
    private const string Table = "weapons";
    private const string DefaultLocale = "eng";
    private const string ResourcePath = "res://GensouNoTabibito/localization";

    private static readonly FieldInfo LocManagerTablesField =
        typeof(LocManager).GetField("_tables", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException(typeof(LocManager).FullName, "_tables");

    public static void RegisterCurrentLanguage()
    {
        var locManager = LocManager.Instance;
        if (locManager == null)
            return;

        if (LocManagerTablesField.GetValue(locManager) is Dictionary<string, LocTable> tables)
            Register(locManager.Language, tables);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetLanguageInternal")]
    private static void SetLanguageInternalPostfix(string language, Dictionary<string, LocTable> tables)
    {
        Register(language, tables);
    }

    private static void Register(string language, Dictionary<string, LocTable> tables)
    {
        if (tables.ContainsKey(Table))
            return;

        var englishTable = CreateTable(DefaultLocale, null);
        var currentTable = language == DefaultLocale
            ? englishTable
            : CreateTable(language, englishTable);

        tables[Table] = currentTable;
    }

    private static LocTable CreateTable(string locale, LocTable? fallback)
    {
        return new LocTable(Table, Load(locale), fallback);
    }

    private static Dictionary<string, string> Load(string locale)
    {
        var path = $"{ResourcePath}/{locale}/weapons.json";
        if (!FileAccess.FileExists(path))
        {
            MainFile.Logger.Info($"Weapon localization file not found: {path}");
            return [];
        }

        var json = FileAccess.GetFileAsString(path);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }
}