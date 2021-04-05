using System.Collections.Generic;

namespace AdventOfCode.Functions
{
    public static class KeyValuePairExtensions
    {
        public static KeyValuePair<TKey, TValue> WithKey<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, TKey key) => new(key, kvp.Value);
        public static KeyValuePair<TKey, TValue> WithValue<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, TValue value) => new(kvp.Key, value);
    }
}
