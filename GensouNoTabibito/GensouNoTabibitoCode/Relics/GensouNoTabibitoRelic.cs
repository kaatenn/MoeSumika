using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using GensouNoTabibito.GensouNoTabibitoCode.Character;
using GensouNoTabibito.GensouNoTabibitoCode.Extensions;
using Godot;

namespace GensouNoTabibito.GensouNoTabibitoCode.Relics;

[Pool(typeof(GensouNoTabibitoRelicPool))]
public abstract class GensouNoTabibitoRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}