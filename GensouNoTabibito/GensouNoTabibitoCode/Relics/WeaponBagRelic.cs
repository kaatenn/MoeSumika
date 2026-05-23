using System.Reflection;
using System.Runtime.CompilerServices;
using GensouNoTabibito.GensouNoTabibitoCode.Cards.Actions;
using GensouNoTabibito.GensouNoTabibitoCode.Extensions;
using GensouNoTabibito.GensouNoTabibitoCode.Localization;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons;
using GensouNoTabibito.GensouNoTabibitoCode.Weapons.Behaviors;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace GensouNoTabibito.GensouNoTabibitoCode.Relics;

public class WeaponBagRelic : GensouNoTabibitoRelic, IWeaponSlotSaveCarrier, IWeaponRewardOwner
{
    private const int MaxCardRewardAlternatives = 2;

    private const string WeaponRewardAlternativeId = "GENSOUNOTABIBITO-WEAPON_REWARD";

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
    protected virtual string DescriptionKey => "GENSOUNOTABIBITO-WEAPON_BAG_RELIC.description";

    [SavedProperty] public bool GensouNoTabibito_SavedHasWeapon { get; set; }

    [SavedProperty] public string GensouNoTabibito_SavedWeaponId { get; set; } = string.Empty;

    [SavedProperty] public int GensouNoTabibito_SavedWeaponKind { get; set; }

    [SavedProperty] public int GensouNoTabibito_SavedWeaponLevel { get; set; }

    [SavedProperty] public int GensouNoTabibito_SavedWeaponUpgradeCount { get; set; }

    [SavedProperty] public bool GensouNoTabibito_SavedHasSecondaryWeapon { get; set; }

    [SavedProperty] public string GensouNoTabibito_SavedSecondaryWeaponId { get; set; } = string.Empty;

    [SavedProperty] public int GensouNoTabibito_SavedSecondaryWeaponKind { get; set; }

    [SavedProperty] public int GensouNoTabibito_SavedSecondaryWeaponLevel { get; set; }

    [SavedProperty] public int GensouNoTabibito_SavedSecondaryWeaponUpgradeCount { get; set; }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringDynamicVar(PrimaryWeaponNameKey, () => GetWeaponName(GetPrimaryWeaponForDescription())),
        new StringDynamicVar(PrimaryWeaponLevelTextKey, () => GetWeaponLevelText(GetPrimaryWeaponForDescription())),
        new StringDynamicVar(SecondaryWeaponNameKey,
            () => GetWeaponName(GetSecondaryWeaponForDescription())),
        new StringDynamicVar(SecondaryWeaponLevelTextKey, () => GetWeaponLevelText(GetSecondaryWeaponForDescription()))
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var equippedWeapon in GetEquippedWeaponsForDescription())
            foreach (var tip in WeaponBehaviorHookBridge.GetHoverTips(equippedWeapon))
                yield return tip;
        }
    }

    public void DraftWeaponFromReward(WeaponState weapon)
    {
        var slot = GetWeaponSlot();
        slot.EquipWeapon(weapon);

        SyncSavedWeaponFromPlayerSlot(Owner);
        InvokeDisplayAmountChanged();
    }

    public void DraftPrimaryWeaponFromReward(WeaponState weapon)
    {
        var slot = GetWeaponSlot();
        slot.ReplaceWeapon(weapon);

        SyncSavedWeaponFromPlayerSlot(Owner);
        InvokeDisplayAmountChanged();
    }

    public void DraftSecondaryWeaponFromReward(WeaponState weapon)
    {
        var slot = GetWeaponSlot();
        if (!slot.CanHoldSecondaryWeapon)
            return;

        slot.ReplaceSecondaryWeapon(weapon);

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

    public void DiscardPrimaryWeaponFromReward()
    {
        var slot = GetWeaponSlot();
        if (!slot.DiscardPrimaryWeapon())
            return;

        SyncSavedWeaponFromPlayerSlot(Owner);
        InvokeDisplayAmountChanged();
    }

    public void DiscardSecondaryWeaponFromReward()
    {
        var slot = GetWeaponSlot();
        if (!slot.DiscardSecondaryWeapon())
            return;

        SyncSavedWeaponFromPlayerSlot(Owner);
        InvokeDisplayAmountChanged();
    }

    public void SyncSavedWeaponFromPlayerSlot(Player player)
    {
        var slot = player.GetWeaponSlot();
        var weapon = slot.PrimaryWeapon;
        if (weapon == null)
        {
            GensouNoTabibito_SavedHasWeapon = false;
            GensouNoTabibito_SavedWeaponId = string.Empty;
            GensouNoTabibito_SavedWeaponKind = 0;
            GensouNoTabibito_SavedWeaponLevel = 0;
            GensouNoTabibito_SavedWeaponUpgradeCount = 0;
            ClearSavedSecondaryWeapon();
            return;
        }

        GensouNoTabibito_SavedHasWeapon = true;
        GensouNoTabibito_SavedWeaponId = weapon.Id;
        GensouNoTabibito_SavedWeaponKind = (int)weapon.Kind;
        GensouNoTabibito_SavedWeaponLevel = weapon.Level;
        GensouNoTabibito_SavedWeaponUpgradeCount = weapon.UpgradeCount;

        var secondaryWeapon = slot.SecondaryWeapon;
        if (secondaryWeapon == null)
        {
            ClearSavedSecondaryWeapon();
            return;
        }

        GensouNoTabibito_SavedHasSecondaryWeapon = true;
        GensouNoTabibito_SavedSecondaryWeaponId = secondaryWeapon.Id;
        GensouNoTabibito_SavedSecondaryWeaponKind = (int)secondaryWeapon.Kind;
        GensouNoTabibito_SavedSecondaryWeaponLevel = secondaryWeapon.Level;
        GensouNoTabibito_SavedSecondaryWeaponUpgradeCount = secondaryWeapon.UpgradeCount;
    }

    public void RestorePlayerSlotFromSavedWeapon(Player player)
    {
        var slot = player.GetWeaponSlot();
        if (!GensouNoTabibito_SavedHasWeapon)
        {
            slot.EnsureWeaponEquipped(WeaponState.CreateBrokenSword());
            return;
        }

        var primaryWeapon = WeaponState.Create(
            GensouNoTabibito_SavedWeaponId,
            (WeaponKind)GensouNoTabibito_SavedWeaponKind,
            GensouNoTabibito_SavedWeaponLevel,
            GensouNoTabibito_SavedWeaponUpgradeCount);

        slot.ReplaceWeapon(primaryWeapon);

        if (!GensouNoTabibito_SavedHasSecondaryWeapon)
            return;

        slot.ReplaceSecondaryWeapon(WeaponState.Create(
            GensouNoTabibito_SavedSecondaryWeaponId,
            (WeaponKind)GensouNoTabibito_SavedSecondaryWeaponKind,
            GensouNoTabibito_SavedSecondaryWeaponLevel,
            GensouNoTabibito_SavedSecondaryWeaponUpgradeCount));
    }

    public HoverTip CreateCurrentHoverTip()
    {
        var primaryWeapon = GetPrimaryWeaponForDescription();
        var secondaryWeapon = GetSecondaryWeaponForDescription();
        var description = new LocString("relics", DescriptionKey);

        description.Add(PrimaryWeaponNameKey, GetWeaponName(primaryWeapon));
        description.Add(PrimaryWeaponLevelTextKey, GetWeaponLevelText(primaryWeapon));
        description.Add(SecondaryWeaponNameKey, GetWeaponName(secondaryWeapon));
        description.Add(SecondaryWeaponLevelTextKey, GetWeaponLevelText(secondaryWeapon));

        return new HoverTip(Title, description);
    }

    public override async Task AfterObtained()
    {
        EnsureWeaponEquipped();
        await base.AfterObtained();
    }

    public override Task BeforeCombatStart()
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorHookBridge.BeforeCombatStart(slot, Owner);
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        var slot = EnsureWeaponSlotEquipped();
        await WeaponBehaviorHookBridge.AfterRoomEntered(slot, Owner, room);
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorHookBridge.AfterSideTurnStart(slot, Owner, side, combatState);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorHookBridge.BeforeCardPlayed(slot, Owner, cardPlay);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorHookBridge.AfterCardPlayed(slot, Owner, choiceContext, cardPlay);
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        var slot = EnsureWeaponSlotEquipped();
        return WeaponBehaviorHookBridge.BeforeSideTurnEnd(slot, Owner, choiceContext, side);
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
        return WeaponBehaviorHookBridge.ModifyDamageAdditive(slot, target, amount, props, dealer, cardSource);
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

        var alternatives = CardRewardAlternative.Generate(sourceReward);
        if (CardRewardCurrentlyShownScreenField.GetValue(sourceReward) is NCardRewardSelectionScreen screen)
            screen.RefreshOptions(cards, alternatives);

        return Task.CompletedTask;
    }

    private static List<CardCreationResult> GetCardRewardCards(CardReward cardReward)
    {
        return (List<CardCreationResult>)CardRewardCardsField.GetValue(cardReward)!;
    }

    public IEnumerable<CardModel> CreateWeaponRewardCards()
    {
        var slot = GetWeaponSlot();
        if (!slot.CanHoldSecondaryWeapon)
        {
            if (slot.CanUpgradeCurrentWeapon)
                yield return Owner.RunState.CreateCard<UpgradeWeaponReward>(Owner);

            yield return CreateDraftWeaponRewardCard<DraftWeaponReward>();
            yield break;
        }

        if (slot.CanUpgradeCurrentWeapon)
            yield return Owner.RunState.CreateCard<UpgradePrimaryWeaponReward>(Owner);

        if (slot.CanUpgradeSecondaryWeapon)
            yield return Owner.RunState.CreateCard<UpgradeSecondaryWeaponReward>(Owner);

        if (slot.SecondaryWeapon != null)
        {
            yield return Owner.RunState.CreateCard<DiscardSecondaryWeaponReward>(Owner);
            yield return Owner.RunState.CreateCard<DiscardPrimaryWeaponReward>(Owner);
        }

        yield return CreateDraftWeaponRewardCard<DraftPrimaryWeaponReward>();
        yield return CreateDraftWeaponRewardCard<DraftSecondaryWeaponReward>();
    }

    private TCard CreateDraftWeaponRewardCard<TCard>()
        where TCard : DraftWeaponRewardCard
    {
        var card = Owner.RunState.CreateCard<TCard>(Owner);
        card.SetDraftedWeapon(CreateRandomWeapon());
        return card;
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

        return GensouNoTabibito_SavedHasWeapon
            ? WeaponState.Create(
                GensouNoTabibito_SavedWeaponId,
                (WeaponKind)GensouNoTabibito_SavedWeaponKind,
                GensouNoTabibito_SavedWeaponLevel,
                GensouNoTabibito_SavedWeaponUpgradeCount)
            : WeaponState.CreateBrokenSword();
    }

    private WeaponState? GetSecondaryWeaponForDescription()
    {
        var slot = TryGetWeaponSlotForDescription();
        if (slot?.SecondaryWeapon != null)
            return slot.SecondaryWeapon;

        return GensouNoTabibito_SavedHasSecondaryWeapon
            ? WeaponState.Create(
                GensouNoTabibito_SavedSecondaryWeaponId,
                (WeaponKind)GensouNoTabibito_SavedSecondaryWeaponKind,
                GensouNoTabibito_SavedSecondaryWeaponLevel,
                GensouNoTabibito_SavedSecondaryWeaponUpgradeCount)
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

    private IEnumerable<EquippedWeapon> GetEquippedWeaponsForDescription()
    {
        var primaryWeapon = GetPrimaryWeaponForDescription();
        if (primaryWeapon != null)
            yield return new EquippedWeapon(WeaponSlot.Primary, primaryWeapon);

        var secondaryWeapon = GetSecondaryWeaponForDescription();
        if (secondaryWeapon != null)
            yield return new EquippedWeapon(WeaponSlot.Secondary, secondaryWeapon);
    }

    private static string GetWeaponLevelText(WeaponState? weapon)
    {
        return weapon == null
            ? string.Empty
            : $" Lv. {weapon.Level}";
    }

    private static string GetWeaponName(WeaponState? weapon)
    {
        return new LocString("weapons", $"{weapon?.Id ?? "GENSOUNOTABIBITO-NONE"}.name").GetFormattedText();
    }

    private void ClearSavedSecondaryWeapon()
    {
        GensouNoTabibito_SavedHasSecondaryWeapon = false;
        GensouNoTabibito_SavedSecondaryWeaponId = string.Empty;
        GensouNoTabibito_SavedSecondaryWeaponKind = 0;
        GensouNoTabibito_SavedSecondaryWeaponLevel = 0;
        GensouNoTabibito_SavedSecondaryWeaponUpgradeCount = 0;
    }
}