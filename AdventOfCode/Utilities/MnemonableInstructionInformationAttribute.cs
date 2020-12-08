using System;

namespace AdventOfCode.Utilities
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class MnemonableInstructionInformationAttribute : Attribute
    {
        public string Mnemonic { get; }
        public int ArgumentCount { get; }

        public MnemonableInstructionInformationAttribute(string mnemonic, int argumentCount)
        {
            Mnemonic = mnemonic;
            ArgumentCount = argumentCount;
        }
    }
}
