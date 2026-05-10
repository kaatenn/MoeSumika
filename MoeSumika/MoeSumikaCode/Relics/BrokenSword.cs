using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;
using MoeSumika.MoeSumikaCode.Extensions;
using MoeSumika.MoeSumikaCode.Weapons;
using MoeSumika.MoeSumikaCode.Weapons.Effects;

namespace MoeSumika.MoeSumikaCode.Relics;

public class BrokenSword : MoeSumikaRelic, IWeaponSlotSaveCarrier
{
    private const string DraftWeaponAlternativeId = "MOESUMIKA-DRAFT_WEAPON";
    private const string UpgradeWeaponAlternativeId = "MOESUMIKA-UPGRADE_WEAPON";

    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool ShouldReceiveCombatHooks => true;

    // TODO: Replace the placeholder relic image files with broken_sword.png,
    // broken_sword_outline.png, and big/broken_sword.png.
    public override string PackedIconPath => "relic.png".RelicImagePath();
    protected override string PackedIconOutlinePath => "relic_outline.png".RelicImagePath();
    protected override string BigIconPath => "relic.png".BigRelicImagePath();

    [SavedProperty]
    public bool SavedHasWeapon { get; set; }

    [SavedProperty]
    public string SavedWeaponId { get; set; } = string.Empty;

    [SavedProperty]
    public int SavedWeaponKind { get; set; }

    [SavedProperty]
    public int SavedWeaponLevel { get; set; }

    [SavedProperty]
    public int SavedWeaponUpgradeCount { get; set; }

    [SavedProperty]
    public bool SavedHasSecondaryWeapon { get; set; }

    [SavedProperty]
    public string SavedSecondaryWeaponId { get; set; } = string.Empty;

    [SavedProperty]
    public int SavedSecondaryWeaponKind { get; set; }

    [SavedProperty]
    public int SavedSecondaryWeaponLevel { get; set; }

    [SavedProperty]
    public int SavedSecondaryWeaponUpgradeCount { get; set; }

    public override async Task AfterObtained()
    {
        EnsureWeaponEquipped();
        await base.AfterObtained();
    }

    public override Task BeforeCombatStart()
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponEffects.BeforeCombatStart(slot, Owner);
    }

    public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponEffects.AfterSideTurnStart(slot, Owner, side, combatState);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponEffects.BeforeCardPlayed(slot, cardPlay);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponEffects.AfterCardPlayed(slot, choiceContext, cardPlay);
    }

    public override Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponEffects.BeforeTurnEnd(slot, choiceContext, side);
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
            DraftWeaponAlternativeId,
            DraftWeapon,
            PostAlternateCardRewardAction.EndSelectionAndCompleteReward));

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

    private void ClearSavedSecondaryWeapon()
    {
        SavedHasSecondaryWeapon = false;
        SavedSecondaryWeaponId = string.Empty;
        SavedSecondaryWeaponKind = 0;
        SavedSecondaryWeaponLevel = 0;
        SavedSecondaryWeaponUpgradeCount = 0;
    }
}
