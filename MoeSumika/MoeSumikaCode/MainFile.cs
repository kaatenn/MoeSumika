using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MoeSumika.MoeSumikaCode.Weapons.Effects;

namespace MoeSumika.MoeSumikaCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "MoeSumika"; //Used for resource filepath
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);

        harmony.PatchAll();
        WeaponEffects.Register("MOESUMIKA-BROKEN_SWORD", new BrokenSwordWeaponEffect());
    }
}
