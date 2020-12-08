using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2020.Utilities
{
    public enum ConsoleSimulatorOperation
    {
        [MnemonableInstructionInformation("acc", 1)]
        AccumulatorIncrement,
        [MnemonableInstructionInformation("jmp", 1)]
        Jump,
        [MnemonableInstructionInformation("nop", 1)]
        NoOperation,
    }
}
