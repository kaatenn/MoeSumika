using BaseLib.Abstracts;
using BaseLib.Utils;
using GensouNoTabibito.GensouNoTabibitoCode.Character;

namespace GensouNoTabibito.GensouNoTabibitoCode.Potions;

[Pool(typeof(GensouNoTabibitoPotionPool))]
public abstract class GensouNoTabibitoPotion : CustomPotionModel;