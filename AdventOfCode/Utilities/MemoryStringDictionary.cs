namespace AdventOfCode.Utilities;

#nullable enable

public sealed class MemoryStringDictionary<TValue>
    : Dictionary<MemoryString, TValue>
{
    public MemoryStringDictionary()
        : base() { }
    public MemoryStringDictionary(IEqualityComparer<MemoryString> comparer)
        : base(comparer) { }

    public TValue? ValueOrDefault(string key)
    {
        return this.ValueOrDefault(key.AsMemory());
    }

    public TValue this[string key]
    {
        get => this[key.AsMemory()];
        set => this[key.AsMemory()] = value;
    }
}

public static class MemoryStringDictionaryExtensions
{
    public static MemoryStringDictionary<TSource> ToMemoryStringDictionary<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, MemoryString> keySelector,
        IEqualityComparer<MemoryString> comparer)
    {
        var result = new MemoryStringDictionary<TSource>(comparer);

        foreach (var value in source)
        {
            result.Add(keySelector(value), value);
        }

        return result;
    }
}
