using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;

namespace MoeSumika.MoeSumikaCode.Weapons;

public sealed class WeaponNetworkState
{
    public List<PlayerWeaponSnapshot> Players { get; } = [];

    public static WeaponNetworkState FromRun(IRunState runState)
    {
        var state = new WeaponNetworkState();

        foreach (var player in runState.Players)
            state.Players.Add(PlayerWeaponSnapshot.From(player));

        return state;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(Players.Count);
        foreach (var player in Players)
            player.Serialize(writer);
    }

    public static WeaponNetworkState Deserialize(PacketReader reader)
    {
        var state = new WeaponNetworkState();
        var count = reader.ReadInt();

        for (var i = 0; i < count; ++i)
            state.Players.Add(PlayerWeaponSnapshot.Deserialize(reader));

        return state;
    }

    public string ToDebugString()
    {
        return string.Join(Environment.NewLine, Players.Select(player => player.ToDebugString()));
    }
}

public sealed class PlayerWeaponSnapshot
{
    public ulong PlayerId { get; private init; }
    public WeaponSnapshot? PrimaryWeapon { get; private init; }
    public WeaponSnapshot? SecondaryWeapon { get; private init; }

    public static PlayerWeaponSnapshot From(Player player)
    {
        var slot = player.GetWeaponSlot();
        return new PlayerWeaponSnapshot
        {
            PlayerId = player.NetId,
            PrimaryWeapon = WeaponSnapshot.From(slot.PrimaryWeapon),
            SecondaryWeapon = WeaponSnapshot.From(slot.SecondaryWeapon)
        };
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteULong(PlayerId);
        WriteWeapon(writer, PrimaryWeapon);
        WriteWeapon(writer, SecondaryWeapon);
    }

    public static PlayerWeaponSnapshot Deserialize(PacketReader reader)
    {
        return new PlayerWeaponSnapshot
        {
            PlayerId = reader.ReadULong(),
            PrimaryWeapon = ReadWeapon(reader),
            SecondaryWeapon = ReadWeapon(reader)
        };
    }

    public string ToDebugString()
    {
        return
            $"Player {PlayerId} weapons: primary={PrimaryWeapon?.ToDebugString() ?? "none"} secondary={SecondaryWeapon?.ToDebugString() ?? "none"}";
    }

    private static void WriteWeapon(PacketWriter writer, WeaponSnapshot? weapon)
    {
        writer.WriteBool(weapon != null);
        if (weapon == null)
            return;

        writer.WriteString(weapon.Id);
        writer.WriteInt((int)weapon.Kind);
        writer.WriteInt(weapon.Level);
        writer.WriteInt(weapon.UpgradeCount);
    }

    private static WeaponSnapshot? ReadWeapon(PacketReader reader)
    {
        if (!reader.ReadBool())
            return null;

        return new WeaponSnapshot(
            reader.ReadString(),
            (WeaponKind)reader.ReadInt(),
            reader.ReadInt(),
            reader.ReadInt());
    }
}

public sealed record WeaponSnapshot(string Id, WeaponKind Kind, int Level, int UpgradeCount)
{
    public static WeaponSnapshot? From(WeaponState? weapon)
    {
        return weapon == null
            ? null
            : new WeaponSnapshot(weapon.Id, weapon.Kind, weapon.Level, weapon.UpgradeCount);
    }

    public string ToDebugString()
    {
        return $"{Id}/{Kind}/L{Level}/U{UpgradeCount}";
    }
}

public static class WeaponNetworkStates
{
    private static readonly ConditionalWeakTable<NetFullCombatState, WeaponNetworkState> States = new();

    public static void Set(NetFullCombatState state, WeaponNetworkState weaponState)
    {
        States.Remove(state);
        States.Add(state, weaponState);
    }

    public static WeaponNetworkState? Get(NetFullCombatState state)
    {
        return States.TryGetValue(state, out var weaponState) ? weaponState : null;
    }
}