using System;

namespace AdventOfCode.Problems.Year2019.Utilities
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public class ArgumentCountAttribute : Attribute
    {
        public int ArgumentCount;

        public ArgumentCountAttribute(int argumentCount) => ArgumentCount = argumentCount;
    }
}
