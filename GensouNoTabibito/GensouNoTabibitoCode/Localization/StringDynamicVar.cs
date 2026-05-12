using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace GensouNoTabibito.GensouNoTabibitoCode.Localization;

public sealed class StringDynamicVar(string name, Func<string> getValue) : DynamicVar(name, 0M)
{
    public override string ToString()
    {
        return getValue();
    }
}