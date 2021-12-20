using Garyon.Functions;

namespace AdventOfCode.Functions;

#nullable enable

public static class IDictionaryExtensions
{
    public static IDictionary<TKey, bool> ToAvailabilityDictionary<TKey>(this IEnumerable<TKey> source)
        where TKey : notnull
    {
        return source.ToDictionary(Selectors.SelfObjectReturner, _ => true);
    }
    public static IDictionary<TKey, TValue?> ToDefaultValueDictionary<TKey, TValue>(this IEnumerable<TKey> source)
        where TKey : notnull
    {
        return source.ToDictionary(Selectors.SelfObjectReturner, _ => default(TValue));
    }
}
