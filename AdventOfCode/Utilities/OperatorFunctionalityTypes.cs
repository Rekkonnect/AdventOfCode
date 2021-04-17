using System;

namespace AdventOfCode.Utilities
{
    [Flags]
    public enum OperatorFunctionalityTypes
    {
        None = 0,
        Jump = 1,
        ValueAdjustment = 1 << 1,
    }
}
