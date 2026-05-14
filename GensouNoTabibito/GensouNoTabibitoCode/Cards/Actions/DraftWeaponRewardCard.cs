using GensouNoTabibito.GensouNoTabibitoCode.Localization;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace GensouNoTabibito.GensouNoTabibitoCode.Cards.Actions;

public abstract class DraftWeaponRewardCard : WeaponRewardActionCard
{
    private const string DraftedWeaponNameKey = "DraftedWeaponName";

    protected WeaponState? DraftedWeapon { get; private set; }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringDynamicVar(DraftedWeaponNameKey, () => WeaponLocalization.GetTitle(DraftedWeapon))
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        DraftedWeapon == null
            ? []
            : WeaponBehaviorRegistry.GetHoverTips(DraftedWeapon);

    public void SetDraftedWeapon(WeaponState weapon)
    {
        AssertMutable();
        DraftedWeapon = weapon;
    }

    protected WeaponState GetDraftedWeapon()
    {
        return DraftedWeapon ?? throw new InvalidOperationException("Draft weapon reward card has no weapon.");
    }
}
