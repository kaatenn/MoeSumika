using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace GensouNoTabibito.GensouNoTabibitoCode.Keywords;

public static class GensouNoTabibitoKeywords
{
    [CustomEnum("SWORD_WEAPON")] [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword SwordWeapon;

    [CustomEnum("STAFF_WEAPON")] [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword StaffWeapon;

    [CustomEnum("BOW_WEAPON")] [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword BowWeapon;

    [CustomEnum("SWORD_SKILL_REQUIREMENT")] [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword SwordSkillRequirement;
}