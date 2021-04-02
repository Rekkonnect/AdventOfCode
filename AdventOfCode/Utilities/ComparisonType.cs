using System;

namespace AdventOfCode.Utilities
{
    [Flags]
    public enum ComparisonType
    {
        None = 0,
        Equal = 1,
        Less = 2,
        Greater = 4,
        LessOrEqual = Less | Equal,
        GreaterOrEqual = Greater | Equal,
        Different = Less | Greater,
        All = Less | Equal | Greater,
    }
}
