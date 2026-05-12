using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using GensouNoTabibito.GensouNoTabibitoCode.Extensions;
using GensouNoTabibito.GensouNoTabibitoCode.Localization;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons;

namespace GensouNoTabibito.GensouNoTabibitoCode.Relics;

public class WeaponBagRelic : GensouNoTabibitoRelic, IWeaponSlotSaveCarrier
{
    private const string DraftWeaponAlternativeId = "GENSOUNOTABIBITO-DRAFT_WEAPON";
    private const string UpgradeWeaponAlternativeId = "GENSOUNOTABIBITO-UPGRADE_WEAPON";

    private const string PrimaryWeaponNameKey = "PrimaryWeaponName";
    private const string PrimaryWeaponLevelTextKey = "PrimaryWeaponLevelText";
    private const string SecondaryWeaponNameKey = "SecondaryWeaponName";
    private const string SecondaryWeaponLevelTextKey = "SecondaryWeaponLevelText";


    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool ShouldReceiveCombatHooks => true;

    // TODO: Replace the placeholder relic image files with weapon_bag.png,
    // weapon_bag_outline.png, and big/weapon_bag.png.
    public override string PackedIconPath => "relic.png".RelicImagePath();
    protected override string PackedIconOutlinePath => "relic_outline.png".RelicImagePath();
    protected override string BigIconPath => "relic.png".BigRelicImagePath();

    [SavedProperty] public bool SavedHasWeapon { get; set; }

    [SavedProperty] public string SavedWeaponId { get; set; } = string.Empty;

    [SavedProperty] public int SavedWeaponKind { get; set; }

    [SavedProperty] public int SavedWeaponLevel { get; set; }

    [SavedProperty] public int SavedWeaponUpgradeCount { get; set; }

    [SavedProperty] public bool SavedHasSecondaryWeapon { get; set; }

    [SavedProperty] public string SavedSecondaryWeaponId { get; set; } = string.Empty;

    [SavedProperty] public int SavedSecondaryWeaponKind { get; set; }

    [SavedProperty] public int SavedSecondaryWeaponLevel { get; set; }

    [SavedProperty] public int SavedSecondaryWeaponUpgradeCount { get; set; }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringDynamicVar(PrimaryWeaponNameKey, () => WeaponLocalization.GetTitle(GetPrimaryWeaponForDescription())),
        new StringDynamicVar(PrimaryWeaponLevelTextKey, () => GetWeaponLevelText(GetPrimaryWeaponForDescription())),
        new StringDynamicVar(SecondaryWeaponNameKey,
            () => WeaponLocalization.GetTitle(GetSecondaryWeaponForDescription())),
        new StringDynamicVar(SecondaryWeaponLevelTextKey, () => GetWeaponLevelText(GetSecondaryWeaponForDescription()))
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var weapon in GetWeaponsForDescription())
            foreach (var tip in WeaponBehaviorRegistry.GetHoverTips(weapon))
                yield return tip;
        }
    }

    public void SyncSavedWeaponFromPlayerSlot(Player player)
    {
        var slot = player.GetWeaponSlot();
        var weapon = slot.PrimaryWeapon;
        if (weapon == null)
        {
            SavedHasWeapon = false;
            SavedWeaponId = string.Empty;
            SavedWeaponKind = 0;
            SavedWeaponLevel = 0;
            SavedWeaponUpgradeCount = 0;
            ClearSavedSecondaryWeapon();
            return;
        }

        SavedHasWeapon = true;
        SavedWeaponId = weapon.Id;
        SavedWeaponKind = (int)weapon.Kind;
        SavedWeaponLevel = weapon.Level;
        SavedWeaponUpgradeCount = weapon.UpgradeCount;

        var secondaryWeapon = slot.SecondaryWeapon;
        if (secondaryWeapon == null)
        {
            ClearSavedSecondaryWeapon();
            return;
        }

        SavedHasSecondaryWeapon = true;
        SavedSecondaryWeaponId = secondaryWeapon.Id;
        SavedSecondaryWeaponKind = (int)secondaryWeapon.Kind;
        SavedSecondaryWeaponLevel = secondaryWeapon.Level;
        SavedSecondaryWeaponUpgradeCount = secondaryWeapon.UpgradeCount;
    }

    public void RestorePlayerSlotFromSavedWeapon(Player player)
    {
        var slot = player.GetWeaponSlot();
        if (!SavedHasWeapon)
        {
            slot.EnsureWeaponEquipped(WeaponState.CreateBrokenSword());
            return;
        }

        var primaryWeapon = WeaponState.Create(
            SavedWeaponId,
            (WeaponKind)SavedWeaponKind,
            SavedWeaponLevel,
            SavedWeaponUpgradeCount);

        slot.ReplaceWeapon(primaryWeapon);

        if (!SavedHasSecondaryWeapon)
            return;

        slot.ReplaceSecondaryWeapon(WeaponState.Create(
            SavedSecondaryWeaponId,
            (WeaponKind)SavedSecondaryWeaponKind,
            SavedSecondaryWeaponLevel,
            SavedSecondaryWeaponUpgradeCount));
    }

    public override async Task AfterObtained()
    {
        EnsureWeaponEquipped();
        await base.AfterObtained();
    }

    public override Task BeforeCombatStart()
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorRegistry.BeforeCombatStart(slot, Owner);
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        var slot = EnsureWeaponSlotEquipped();
        await WeaponBehaviorRegistry.AfterRoomEntered(slot, Owner, room);
    }

    public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorRegistry.AfterSideTurnStart(slot, Owner, side, combatState);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorRegistry.BeforeCardPlayed(slot, Owner, cardPlay);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorRegistry.AfterCardPlayed(slot, Owner, choiceContext, cardPlay);
    }

    public override Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorRegistry.BeforeTurnEnd(slot, Owner, choiceContext, side);
    }

    public override bool TryModifyCardRewardAlternatives(
        Player player,
        CardReward cardReward,
        List<CardRewardAlternative> alternatives)
    {
        if (!ReferenceEquals(player, Owner))
            return false;

        EnsureWeaponEquipped();

        alternatives.Add(new CardRewardAlternative(
            UpgradeWeaponAlternativeId,
            UpgradeWeapon,
            PostAlternateCardRewardAction.EndSelectionAndCompleteReward));

        return true;
    }

    private Task DraftWeapon()
    {
        var slot = GetWeaponSlot();
        slot.EnsureWeaponEquipped(WeaponState.CreateBrokenSword());

        // TODO: Generate weapon choices, show a keep/discard selection UI, and call slot.EquipWeapon
        // if the player keeps one. For now, this is intentionally a no-op placeholder.
        return Task.CompletedTask;
    }

    private Task UpgradeWeapon()
    {
        var slot = GetWeaponSlot();
        slot.EnsureWeaponEquipped(WeaponState.CreateBrokenSword());
        slot.UpgradeCurrentWeapon();
        SyncSavedWeaponFromPlayerSlot(Owner);
        return Task.CompletedTask;
    }

    private WeaponState EnsureWeaponEquipped()
    {
        return EnsureWeaponSlotEquipped().PrimaryWeapon!;
    }

    private WeaponSlotState EnsureWeaponSlotEquipped()
    {
        var slot = GetWeaponSlot();
        slot.EnsureWeaponEquipped(WeaponState.CreateBrokenSword());
        return slot;
    }

    private WeaponSlotState GetWeaponSlot()
    {
        return Owner.GetWeaponSlot();
    }

    private WeaponState? GetPrimaryWeaponForDescription()
    {
        var slot = TryGetWeaponSlotForDescription();
        if (slot?.PrimaryWeapon != null)
            return slot.PrimaryWeapon;

        return SavedHasWeapon
            ? WeaponState.Create(
                SavedWeaponId,
                (WeaponKind)SavedWeaponKind,
                SavedWeaponLevel,
                SavedWeaponUpgradeCount)
            : WeaponState.CreateBrokenSword();
    }

    private WeaponState? GetSecondaryWeaponForDescription()
    {
        var slot = TryGetWeaponSlotForDescription();
        if (slot?.SecondaryWeapon != null)
            return slot.SecondaryWeapon;

        return SavedHasSecondaryWeapon
            ? WeaponState.Create(
                SavedSecondaryWeaponId,
                (WeaponKind)SavedSecondaryWeaponKind,
                SavedSecondaryWeaponLevel,
                SavedSecondaryWeaponUpgradeCount)
            : null;
    }

    private WeaponSlotState? TryGetWeaponSlotForDescription()
    {
        if (!IsMutable)
            return null;

        return Owner?.GetWeaponSlot();
    }

    private IEnumerable<WeaponState> GetWeaponsForDescription()
    {
        var primaryWeapon = GetPrimaryWeaponForDescription();
        if (primaryWeapon != null)
            yield return primaryWeapon;

        var secondaryWeapon = GetSecondaryWeaponForDescription();
        if (secondaryWeapon != null)
            yield return secondaryWeapon;
    }

    private static string GetWeaponLevelText(WeaponState? weapon)
    {
        return weapon == null
            ? string.Empty
            : $" Lv. {weapon.Level}";
    }

    private void ClearSavedSecondaryWeapon()
    {
        SavedHasSecondaryWeapon = false;
        SavedSecondaryWeaponId = string.Empty;
        SavedSecondaryWeaponKind = 0;
        SavedSecondaryWeaponLevel = 0;
        SavedSecondaryWeaponUpgradeCount = 0;
    }
}