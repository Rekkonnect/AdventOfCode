using Garyon.Objects;

namespace AdventOfCode.Functions;

public static class TemporaryIComparableExtensions
{
    // Copied straight from the decompiler for a bit of fixing
    [Obsolete("Make sure to delete this when Garyon gets this bug fixed")]
    public static bool SatisfiesComparisonFixed<T>(this IComparable<T> value, T other, ComparisonKinds kinds)
    {
        return SatisfiesComparisonFixed(value.CompareTo(other), kinds);
    }

    private static bool SatisfiesComparisonFixed(int comparison, ComparisonKinds kinds)
    {
        switch (kinds)
        {
            case ComparisonKinds.Less:
                return comparison < 0;
            case ComparisonKinds.Equal:
                return comparison == 0;
            case ComparisonKinds.Greater:
                return comparison > 0;
            case ComparisonKinds.Different:
                return comparison != 0;
            case ComparisonKinds.LessOrEqual:
                return comparison <= 0;
            case ComparisonKinds.GreaterOrEqual:
                return comparison >= 0;
            case ComparisonKinds.All:
                return true;
            default:
                return false;
        }
    }
}
