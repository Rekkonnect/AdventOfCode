using Garyon.Objects;

namespace AdventOfCode.Utilities;

public record OpenRange<T>(T Value, ComparisonKinds ComparisonKinds)
    where T : IComparable<T>
{
    public bool MatchesComparison(T other) => other.SatisfiesComparison(Value, ComparisonKinds);

    public static implicit operator OpenRange<T>(T value) => new(value, ComparisonKinds.Equal);
}
