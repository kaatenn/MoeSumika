using BaseLib.Abstracts;
using GensouNoTabibito.GensouNoTabibitoCode.Extensions;
using Godot;

namespace GensouNoTabibito.GensouNoTabibitoCode.Character;

public class GensouNoTabibitoRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => GensouNoTabibito.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}