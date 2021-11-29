namespace AdventOfCode.Utilities;

public class FlexibleInitializableValueList<T> : FlexibleList<T>
    where T : new()
{
    protected override T GetDefaultInitializedValue() => new();
}
