using System;

namespace AdventOfCode.Problems;

// TODO: Create analyzer verifying that this attribute is applied to valid methods
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PartSolutionAttribute : Attribute
{
    public PartSolutionStatus Status { get; }

    public PartSolutionAttribute(PartSolutionStatus status)
    {
        Status = status;
    }
}
