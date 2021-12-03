using Garyon.Extensions;
using Garyon.Objects;
using System;

namespace AdventOfCode.Utilities;

public record ValueComparison<T>(T Value, ComparisonKinds ComparisonKinds)
    where T : IComparable<T>
{
    public bool MatchesComparison(T other) => Value.SatisfiesComparison(other, ComparisonKinds);

    public static implicit operator ValueComparison<T>(T value) => new(value, ComparisonKinds.Equal);
}
