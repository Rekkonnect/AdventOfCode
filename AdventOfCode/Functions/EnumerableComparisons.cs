using Garyon.Objects;

namespace AdventOfCode.Functions;

public static class EnumerableComparisons
{
    public static T Min<T>(this IEnumerable<T> source, Comparison<T> comparison)
    {
        return Best(source, comparison, ComparisonResult.Less);
    }
    public static T Max<T>(this IEnumerable<T> source, Comparison<T> comparison)
    {
        return Best(source, comparison, ComparisonResult.Greater);
    }
    // I don't like this; not at all
    public static T Best<T>(this IEnumerable<T> source, Comparison<T> comparison, ComparisonResult targetComparisonResult)
    {
        bool hasFirst = false;
        T best = default;
        foreach (var value in source)
        {
            if (!hasFirst)
            {
                best = value;
                hasFirst = true;
            }
            else
            {
                int compared = comparison(value, best);
                if (compared.MatchesComparisonResult(0, targetComparisonResult))
                    best = value;
            }
        }

        if (!hasFirst)
            throw new ArgumentException("The source contained no elements");

        return best;
    }
}
