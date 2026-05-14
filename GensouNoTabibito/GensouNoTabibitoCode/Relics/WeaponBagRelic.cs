using System.Reflection;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using GensouNoTabibito.GensouNoTabibitoCode.Cards;
using GensouNoTabibito.GensouNoTabibitoCode.Cards.Actions;
using GensouNoTabibito.GensouNoTabibitoCode.Extensions;
using GensouNoTabibito.GensouNoTabibitoCode.Localization;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons;

namespace GensouNoTabibito.GensouNoTabibitoCode.Relics;

public class WeaponBagRelic : GensouNoTabibitoRelic, IWeaponSlotSaveCarrier
{
    private const int MaxCardRewardAlternatives = 2;

    private const string WeaponRewardAlternativeId = "GENSOUNOTABIBITO-WEAPON_REWARD";
    private const string DraftWeaponAlternativeId = "GENSOUNOTABIBITO-DRAFT_WEAPON";
    private const string UpgradeWeaponAlternativeId = "GENSOUNOTABIBITO-UPGRADE_WEAPON";

    private const string PrimaryWeaponNameKey = "PrimaryWeaponName";
    private const string PrimaryWeaponLevelTextKey = "PrimaryWeaponLevelText";
    private const string SecondaryWeaponNameKey = "SecondaryWeaponName";
    private const string SecondaryWeaponLevelTextKey = "SecondaryWeaponLevelText";

    private static readonly object WeaponRewardReplacementMarker = new();
    private static readonly ConditionalWeakTable<CardReward, object> ReplacedWeaponRewards = new();

    private static readonly FieldInfo CardRewardCardsField =
        typeof(CardReward).GetField("_cards", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException(typeof(CardReward).FullName, "_cards");

    private static readonly FieldInfo CardRewardCurrentlyShownScreenField =
        typeof(CardReward).GetField("_currentlyShownScreen", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException(typeof(CardReward).FullName, "_currentlyShownScreen");

    private static readonly Func<WeaponState>[] WeaponDraftPool =
    [
        WeaponState.CreateBrokenSword,
        WeaponState.CreateLightSword
    ];

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

    public HoverTip CreateCurrentHoverTip()
    {
        var primaryWeapon = GetPrimaryWeaponForDescription();
        var secondaryWeapon = GetSecondaryWeaponForDescription();
        var description = new LocString("relics", "GENSOUNOTABIBITO-WEAPON_BAG_RELIC.description");

        description.Add(PrimaryWeaponNameKey, WeaponLocalization.GetTitle(primaryWeapon));
        description.Add(PrimaryWeaponLevelTextKey, GetWeaponLevelText(primaryWeapon));
        description.Add(SecondaryWeaponNameKey, WeaponLocalization.GetTitle(secondaryWeapon));
        description.Add(SecondaryWeaponLevelTextKey, GetWeaponLevelText(secondaryWeapon));

        return new HoverTip(Title, description);
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

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (cardSource?.Owner == null || !ReferenceEquals(cardSource.Owner.Creature, Owner.Creature))
            return 0m;

        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorRegistry.ModifyDamageAdditive(slot, target, amount, props, dealer, cardSource);
    }

    public override bool TryModifyCardRewardAlternatives(
        Player player,
        CardReward cardReward,
        List<CardRewardAlternative> alternatives)
    {
        if (!ReferenceEquals(player, Owner))
            return false;

        if (alternatives.Count >= MaxCardRewardAlternatives)
            return false;

        if (ReplacedWeaponRewards.TryGetValue(cardReward, out _))
            return false;

        EnsureWeaponSlotEquipped();
        alternatives.Add(new CardRewardAlternative(
            WeaponRewardAlternativeId,
            () => ReplaceCardRewardWithWeaponActions(cardReward),
            0));

        return true;
    }

    public void DraftWeaponFromReward()
    {
        var slot = GetWeaponSlot();
        slot.EquipWeapon(CreateRandomWeapon());

        SyncSavedWeaponFromPlayerSlot(Owner);
        InvokeDisplayAmountChanged();
    }

    public void DraftPrimaryWeaponFromReward()
    {
        var slot = GetWeaponSlot();
        slot.ReplaceWeapon(CreateRandomWeapon());

        SyncSavedWeaponFromPlayerSlot(Owner);
        InvokeDisplayAmountChanged();
    }

    public void DraftSecondaryWeaponFromReward()
    {
        var slot = GetWeaponSlot();
        if (!slot.CanHoldSecondaryWeapon)
            return;

        slot.ReplaceSecondaryWeapon(CreateRandomWeapon());

        SyncSavedWeaponFromPlayerSlot(Owner);
        InvokeDisplayAmountChanged();
    }

    public void UpgradeWeaponFromReward()
    {
        var slot = GetWeaponSlot();
        slot.EnsureWeaponEquipped(WeaponState.CreateBrokenSword());
        if (!slot.UpgradeCurrentWeapon())
            return;

        SyncSavedWeaponFromPlayerSlot(Owner);
        InvokeDisplayAmountChanged();
    }

    public void UpgradePrimaryWeaponFromReward()
    {
        UpgradeWeaponFromReward();
    }

    public void UpgradeSecondaryWeaponFromReward()
    {
        var slot = GetWeaponSlot();
        if (!slot.UpgradeSecondaryWeapon())
            return;

        SyncSavedWeaponFromPlayerSlot(Owner);
        InvokeDisplayAmountChanged();
    }

    public override bool ShouldAddToDeck(CardModel card)
    {
        return card is not WeaponRewardActionCard || !ReferenceEquals(card.Owner, Owner);
    }

    public override Task AfterAddToDeckPrevented(CardModel card)
    {
        if (card is WeaponRewardActionCard weaponRewardAction && ReferenceEquals(card.Owner, Owner))
            weaponRewardAction.Resolve(this);

        return Task.CompletedTask;
    }

    private Task ReplaceCardRewardWithWeaponActions(CardReward sourceReward)
    {
        var cards = GetCardRewardCards(sourceReward);
        cards.Clear();
        cards.AddRange(CreateWeaponRewardCards().Select(card => new CardCreationResult(card)));

        sourceReward.CanReroll = false;
        ReplacedWeaponRewards.Remove(sourceReward);
        ReplacedWeaponRewards.Add(sourceReward, WeaponRewardReplacementMarker);

        if (CardRewardCurrentlyShownScreenField.GetValue(sourceReward) is NCardRewardSelectionScreen screen)
            screen.RefreshOptions(cards, Array.Empty<CardRewardAlternative>());

        return Task.CompletedTask;
    }

    private static List<CardCreationResult> GetCardRewardCards(CardReward cardReward)
    {
        return (List<CardCreationResult>)CardRewardCardsField.GetValue(cardReward)!;
    }

    private IEnumerable<CardModel> CreateWeaponRewardCards()
    {
        var slot = GetWeaponSlot();
        if (!slot.CanHoldSecondaryWeapon)
        {
            if (slot.CanUpgradeCurrentWeapon)
                yield return Owner.RunState.CreateCard<UpgradeWeaponReward>(Owner);

            yield return Owner.RunState.CreateCard<DraftWeaponReward>(Owner);
            yield break;
        }

        if (slot.CanUpgradeCurrentWeapon)
            yield return Owner.RunState.CreateCard<UpgradePrimaryWeaponReward>(Owner);

        if (slot.CanUpgradeSecondaryWeapon)
            yield return Owner.RunState.CreateCard<UpgradeSecondaryWeaponReward>(Owner);

        yield return Owner.RunState.CreateCard<DraftPrimaryWeaponReward>(Owner);
        yield return Owner.RunState.CreateCard<DraftSecondaryWeaponReward>(Owner);
    }

    private static WeaponState CreateRandomWeapon()
    {
        return WeaponDraftPool[Random.Shared.Next(WeaponDraftPool.Length)]();
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
