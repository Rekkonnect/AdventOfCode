using AdventOfCode.Utilities;
using Garyon.Reflection;

namespace AdventOfCode.Problems;

public static class ComputerOperatorInformation
{
    private static readonly FlexibleDictionary<ComputerOperator, MnemonableInstructionInformationAttribute> operatorAttributes;
    private static readonly FlexibleDictionary<string, ComputerOperator> mnemonics = new();

    static ComputerOperatorInformation()
    {
        operatorAttributes = new(EnumReflectionHelpers.GetEnumFieldDictionary<ComputerOperator, MnemonableInstructionInformationAttribute>());

        foreach (var op in operatorAttributes)
        {
            mnemonics.Add(op.Value.Mnemonic, op.Key);
        }
    }

    public static ComputerOperator ParseMnemonic(string s) => mnemonics[s];

    public static string GetMnemonic(this ComputerOperator op) => operatorAttributes[op].Mnemonic;
    public static int GetArgumentCount(this ComputerOperator op) => operatorAttributes[op].ArgumentCount;
    public static bool HasFunctionalityType(this ComputerOperator op, OperatorFunctionalityTypes type)
    {
        return operatorAttributes[op].FunctionalityTypes.HasFlag(type);
    }

    public static bool IsJump(this ComputerOperator op) => op.HasFunctionalityType(OperatorFunctionalityTypes.Jump);
    public static bool IsValueAdjustment(this ComputerOperator op) => op.HasFunctionalityType(OperatorFunctionalityTypes.ValueAdjustment);
}
