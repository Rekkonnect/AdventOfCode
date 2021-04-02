using Garyon.Objects;

namespace AdventOfCode.Utilities
{
    public static class ComparisonTypeExtensions
    {
        public static ComparisonType GetComparisonType(this ComparisonResult result) => result switch
        {
            ComparisonResult.Equal => ComparisonType.Equal,
            ComparisonResult.Less => ComparisonType.Less,
            ComparisonResult.Greater => ComparisonType.Greater,
        };
        public static bool Matches(this ComparisonType type, ComparisonResult result) => type.HasFlag(result.GetComparisonType());
    }
}
