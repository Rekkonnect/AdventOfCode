namespace AdventOfCode.Functions;

public static class Initialization
{
    public static bool TryInitializeUpcast<TDerived, TBase>(ref TDerived field, TBase value)
        where TDerived : class, TBase
        where TBase : class
    {
        if (field is not null)
            return false;

        return TrySetUpcast(ref field, value);
    }
    public static bool TrySetUpcast<TDerived, TBase>(ref TDerived field, TBase value)
        where TDerived : class, TBase
        where TBase : class
    {
        if (value is not TDerived derived)
            return false;

        field = derived;
        return true;
    }
}
