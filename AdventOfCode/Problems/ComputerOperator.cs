using AdventOfCode.Utilities;

namespace AdventOfCode.Problems
{
    public enum ComputerOperator
    {
        [MnemonableInstructionInformation("nop", 0)]
        NoOperation,
        [MnemonableInstructionInformation("cpy", 2, FunctionalityTypes = OperatorFunctionalityTypes.ValueAdjustment)]
        Copy,
        [MnemonableInstructionInformation("inc", 1, FunctionalityTypes = OperatorFunctionalityTypes.ValueAdjustment)]
        Increase,
        [MnemonableInstructionInformation("dec", 1, FunctionalityTypes = OperatorFunctionalityTypes.ValueAdjustment)]
        Decrease,
        [MnemonableInstructionInformation("hlf", 1, FunctionalityTypes = OperatorFunctionalityTypes.ValueAdjustment)]
        Halve,
        [MnemonableInstructionInformation("tpl", 1, FunctionalityTypes = OperatorFunctionalityTypes.ValueAdjustment)]
        Triple,
        [MnemonableInstructionInformation("add", 2, FunctionalityTypes = OperatorFunctionalityTypes.ValueAdjustment)]
        Add,
        [MnemonableInstructionInformation("mul", 2, FunctionalityTypes = OperatorFunctionalityTypes.ValueAdjustment)]
        Multiply,
        [MnemonableInstructionInformation("jmp", 1, FunctionalityTypes = OperatorFunctionalityTypes.Jump)]
        Jump,
        [MnemonableInstructionInformation("jnz", 2, FunctionalityTypes = OperatorFunctionalityTypes.Jump)]
        JumpIfNotZero,
        [MnemonableInstructionInformation("jie", 2, FunctionalityTypes = OperatorFunctionalityTypes.Jump)]
        JumpIfEven,
        [MnemonableInstructionInformation("jio", 2, FunctionalityTypes = OperatorFunctionalityTypes.Jump)]
        JumpIfOne,
        [MnemonableInstructionInformation("tgl", 1)]
        Toggle,
    }
}
