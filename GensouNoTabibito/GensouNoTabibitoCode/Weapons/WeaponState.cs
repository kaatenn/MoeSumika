namespace GensouNoTabibito.GensouNoTabibitoCode.Weapons;

public sealed class WeaponState
{
    private WeaponState(string id, WeaponKind kind, int level, int upgradeCount)
    {
        Id = id;
        Kind = kind;
        Level = level;
        UpgradeCount = upgradeCount;
    }

    public string Id { get; }
    public WeaponKind Kind { get; }
    public int Level { get; private set; }
    public int UpgradeCount { get; private set; }

    public static WeaponState CreateBrokenSword()
    {
        return new WeaponState("GENSOUNOTABIBITO-BROKEN_SWORD", WeaponKind.Sword, 1, 0);
    }

    public static WeaponState CreateLightSword()
    {
        return new WeaponState("GENSOUNOTABIBITO-LIGHT_SWORD", WeaponKind.Sword, 1, 0);
    }

    public static WeaponState Create(string id, WeaponKind kind, int level, int upgradeCount)
    {
        return new WeaponState(id, kind, level, upgradeCount);
    }

    public void Upgrade()
    {
        ++UpgradeCount;
        ++Level;
    }
}