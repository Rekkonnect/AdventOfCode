using System.Collections.Generic;

namespace AdventOfCode.Functions
{
    public static class ISetExtensions
    {
        public static SetElementToggleResult ToggleElement<T>(this ISet<T> source, T element)
        {
            if (source.Add(element))
                return SetElementToggleResult.Added;

            source.Remove(element);
            return SetElementToggleResult.Removed;
        }
    }
}
