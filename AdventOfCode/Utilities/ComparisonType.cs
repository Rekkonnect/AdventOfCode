using Garyon.Objects;
using System;

namespace AdventOfCode.Utilities;

[Flags]
public enum ComparisonType
{
    /// <summary>Represents the default value of the enum. It should only be used in argument validation.</summary>
    None = 0,

    // The flags are implemented as such:
    // LEG
    // L: Less than
    // E: Equal to
    // G: Greater than

    Less = 0b100,
    Equal = 0b010,
    Greater = 0b001,

    LessOrEqual = Less | Equal,
    GreaterOrEqual = Greater | Equal,
    Different = Less | Greater,

    Any = Different | Equal,
}

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
