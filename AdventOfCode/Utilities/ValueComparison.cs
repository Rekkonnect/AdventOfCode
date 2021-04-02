using Garyon.Extensions;
using System;

namespace AdventOfCode.Utilities
{
    public record ValueComparison<T>(T Value, ComparisonType ComparisonType)
        where T : IComparable<T>
    {
        public bool MatchesComparison(T value) => ComparisonType.Matches(value.GetComparisonResult(Value));

        public static implicit operator ValueComparison<T>(T value) => new(value, ComparisonType.Equal);
    }
}
