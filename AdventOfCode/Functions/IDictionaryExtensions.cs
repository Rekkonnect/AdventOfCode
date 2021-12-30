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

    // Copy-pasting is awful if you wanna juice out a bit of performance
    public static TValue GetOrAddValue<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TValue> valueFactory, out bool added)
        where TKey : notnull
    {
        bool existed = source.TryGetValue(key, out var existingValue);
        if (!existed)
        {
            var value = valueFactory();
            source.Add(key, value);
            existingValue = value;
        }
        added = !existed;
        return existingValue!;
    }
    public static TValue GetOrAddValue<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value, out bool added)
        where TKey : notnull
    {
        bool existed = source.TryGetValue(key, out var existingValue);
        if (!existed)
        {
            source.Add(key, value);
            existingValue = value;
        }
        added = !existed;
        return existingValue!;
    }
}
