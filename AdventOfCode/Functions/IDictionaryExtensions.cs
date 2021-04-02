using Garyon.Functions;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Functions
{
    public static class IDictionaryExtensions
    {
        public static IDictionary<TKey, bool> ToAvailabilityDictionary<TKey>(this IEnumerable<TKey> source) => source.ToDictionary(Selectors.SelfObjectReturner, _ => true);
    }
}
