using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using GensouNoTabibito.GensouNoTabibitoCode.Cards.Actions;
using GensouNoTabibito.GensouNoTabibitoCode.Extensions;
using GensouNoTabibito.GensouNoTabibitoCode.Relics;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace GensouNoTabibito.GensouNoTabibitoCode.Rewards;

public sealed class WeaponLibraryReward : CustomReward
{
    [CustomEnum] public static RewardType WeaponLibrary;

    private readonly List<CardCreationResult> _cards = [];
    private bool _isPopulated;

    public WeaponLibraryReward(Player player)
        : base(player)
    {
    }

    protected override RewardType RewardType => WeaponLibrary;

    public override LocString Description => new("gameplay_ui", "GENSOUNOTABIBITO-WEAPON_LIBRARY_REWARD");

    protected override string IconPath => "relic.png".RelicImagePath();

    public override bool IsPopulated => _isPopulated;

    public override CreateRewardFromSave<CustomReward> DeserializeMethod => CreateFromSerializable;

    public override void Populate()
    {
        _cards.Clear();

        var weaponLibrary = GetWeaponLibrary();
        foreach (var card in weaponLibrary.CreateWeaponRewardCards())
            _cards.Add(new CardCreationResult(card));

        _isPopulated = true;
    }

    protected override async Task<bool> OnSelect()
    {
        if (!IsPopulated)
            Populate();

        if (_cards.Count == 0)
            return false;

        var skipped = false;
        var alternatives = new[]
        {
            new CardRewardAlternative(
                "SKIP",
                () =>
                {
                    skipped = true;
                    return Task.CompletedTask;
                },
                PostAlternateCardRewardAction.EndSelectionAndDoNotCompleteReward)
        };

        var screen = NCardRewardSelectionScreen.ShowScreen(_cards, alternatives);
        if (screen == null)
            return false;

        var selectedIndex = await screen.OptionSelected();
        NOverlayStack.Instance?.Remove(screen);

        if (skipped)
            return true;

        if (selectedIndex == null || selectedIndex < 0 || selectedIndex >= _cards.Count)
            return false;

        if (_cards[selectedIndex.Value].Card is not WeaponRewardActionCard weaponRewardAction)
            return false;

        weaponRewardAction.Resolve(GetWeaponLibrary());
        return true;
    }

    public override SerializableReward ToSerializable()
    {
        return new SerializableReward
        {
            RewardType = RewardType
        };
    }

    public override void MarkContentAsSeen()
    {
    }

    private static CustomReward CreateFromSerializable(SerializableReward save, Player player)
    {
        return new WeaponLibraryReward(player);
    }

    private WeaponLibraryRelic GetWeaponLibrary()
    {
        return Player.Relics.OfType<WeaponLibraryRelic>().FirstOrDefault()
               ?? throw new InvalidOperationException("Weapon Library reward requires a Weapon Library relic.");
    }
}