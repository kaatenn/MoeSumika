using BaseLib.Abstracts;
using BaseLib.Extensions;
using MoeSumika.MoeSumikaCode.Extensions;
using Godot;

namespace MoeSumika.MoeSumikaCode.Powers;

public abstract class MoeSumikaPower : CustomPowerModel
{
    //Loads from MoeSumika/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}