using GensouNoTabibito.GensouNoTabibitoCode.Patches;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Sword;

namespace GensouNoTabibito.GensouNoTabibitoCode;

public static class Registry
{
    public static void Register()
    {
        WeaponLocalizationTablePatch.RegisterCurrentLanguage();
        RegisterWeapons();
    }

    private static void RegisterWeapons()
    {
        WeaponBehaviorHookBridge.Register("GENSOUNOTABIBITO-BROKEN_SWORD", new BrokenSwordBehavior());
        WeaponBehaviorHookBridge.Register("GENSOUNOTABIBITO-LIGHT_SWORD", new LightSwordBehavior());
    }
}