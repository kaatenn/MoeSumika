using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MoeSumika.MoeSumikaCode.Weapons;
using MoeSumika.MoeSumikaCode.Weapons.Behaviors.Sword;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace MoeSumika.MoeSumikaCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "MoeSumika"; //Used for resource filepath
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; } =
        new(ModId, LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);

        harmony.PatchAll();
        WeaponBehaviorRegistry.Register("MOESUMIKA-BROKEN_SWORD", new BrokenSwordBehavior());
    }
}