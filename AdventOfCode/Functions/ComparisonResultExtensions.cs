using Garyon.Objects;

namespace AdventOfCode.Functions;

public static class ComparisonResultExtensions
{
    public static T Best<T>(this ComparisonResult result, T x, T y)
        where T : IComparable<T>
    {
        if (x.MatchesComparisonResult(y, result))
            return x;

        return y;
    }
}
