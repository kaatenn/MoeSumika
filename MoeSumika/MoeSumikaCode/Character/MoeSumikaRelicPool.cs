using BaseLib.Abstracts;
using MoeSumika.MoeSumikaCode.Extensions;
using Godot;

namespace MoeSumika.MoeSumikaCode.Character;

public class MoeSumikaRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => MoeSumika.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}