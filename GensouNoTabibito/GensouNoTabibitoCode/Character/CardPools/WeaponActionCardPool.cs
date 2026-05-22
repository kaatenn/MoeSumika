using BaseLib.Abstracts;
using Godot;

namespace GensouNoTabibito.GensouNoTabibitoCode.Character.CardPools;

public class WeaponActionCardPool : CustomCardPoolModel
{
    public override string Title => "Weapon Action Cards";

    public override bool IsShared => true;

    public override Color ShaderColor => GensouNoTabibitoCardColors.CardBackPink;

    public override Color DeckEntryCardColor => GensouNoTabibitoCardColors.CardBackPink;

    public override bool IsColorless => true;
}