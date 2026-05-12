using BaseLib.Abstracts;
using BaseLib.Extensions;
using GensouNoTabibito.GensouNoTabibitoCode.Extensions;
using Godot;

namespace GensouNoTabibito.GensouNoTabibitoCode.Powers;

public abstract class GensouNoTabibitoPower : CustomPowerModel
{
    //Loads from GensouNoTabibito/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}