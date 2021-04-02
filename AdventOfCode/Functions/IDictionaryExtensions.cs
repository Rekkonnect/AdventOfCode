using Garyon.Functions;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Functions
{
    public static class IDictionaryExtensions
    {
        public static IDictionary<TKey, bool> ToAvailabilityDictionary<TKey>(this IEnumerable<TKey> source) => source.ToDictionary(Selectors.SelfObjectReturner, _ => true);
        public static IDictionary<TKey, TValue> ToDefaultValueDictionary<TKey, TValue>(this IEnumerable<TKey> source) => source.ToDictionary(Selectors.SelfObjectReturner, _ => default(TValue));
    }
}
