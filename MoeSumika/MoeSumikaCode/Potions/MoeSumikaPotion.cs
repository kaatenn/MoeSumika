using BaseLib.Abstracts;
using BaseLib.Utils;
using MoeSumika.MoeSumikaCode.Character;

namespace MoeSumika.MoeSumikaCode.Potions;

[Pool(typeof(MoeSumikaPotionPool))]
public abstract class MoeSumikaPotion : CustomPotionModel;