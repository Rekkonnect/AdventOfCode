namespace AdventOfCode.Functions;

public static class Initialization
{
    public static bool TryInitialize<TDerived, TBase>(ref TDerived field, TBase value)
        where TDerived : class, TBase
        where TBase : class
    {
        if (value is not TDerived derived)
            return false;

        if (field is not null)
            return false;

        field = derived;
        return true;
    }
}
