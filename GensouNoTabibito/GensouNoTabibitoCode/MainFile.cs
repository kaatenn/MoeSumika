using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors.Sword;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace GensouNoTabibito.GensouNoTabibitoCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "GensouNoTabibito"; //Used for resource filepath
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; } =
        new(ModId, LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);

        harmony.PatchAll();
        WeaponBehaviorRegistry.Register("GENSOUNOTABIBITO-BROKEN_SWORD", new BrokenSwordBehavior());
        WeaponBehaviorRegistry.Register("GENSOUNOTABIBITO-LIGHT_SWORD", new LightSwordBehavior());
    }
}
