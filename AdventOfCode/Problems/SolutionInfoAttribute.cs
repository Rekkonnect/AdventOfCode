using System;

namespace AdventOfCode.Problems;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SolutionInfoAttribute : Attribute
{
    public SolutionFlags SolutionFlags { get; }

    public SolutionInfoAttribute(SolutionFlags flags)
    {
        SolutionFlags = flags;
    }
}
