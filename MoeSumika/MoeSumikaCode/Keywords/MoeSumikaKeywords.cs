using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MoeSumika.MoeSumikaCode.Keywords;

public static class MoeSumikaKeywords
{
    [CustomEnum("SWORD_WEAPON")] [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword SwordWeapon;

    [CustomEnum("STAFF_WEAPON")] [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword StaffWeapon;

    [CustomEnum("BOW_WEAPON")] [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword BowWeapon;
}