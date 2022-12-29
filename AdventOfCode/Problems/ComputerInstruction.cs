using AdventOfCode.Functions;

namespace AdventOfCode.Problems;

public partial record ComputerInstruction(ComputerOperator Operator, IReadOnlyList<string> Arguments)
{
    public static readonly ComputerInstruction NoOperation = new(ComputerOperator.NoOperation, Array.Empty<string>());

    public bool Argument(int index, out int constantValue, out char register)
    {
        constantValue = default;
        register = default;

        if (index >= Arguments.Count)
            return false;

        return Arguments[index].ExtractInt32AndFirstChar(out constantValue, out register);
    }

    public static ComputerInstruction Parse(string s, string argumentSplitter = " ")
    {
        var spanString = s.AsSpan();
        spanString.SplitOnce(' ', out var operatorSpan, out var argumentsSpan);
        var op = ComputerOperatorInformation.ParseMnemonic(operatorSpan.ToString());
        var arguments = argumentsSpan.SplitToStrings(argumentSplitter).ToArray();
        return new(op, arguments);
    }

    public override string ToString()
    {
        return $"{Operator.GetMnemonic()} {string.Join(" ", Arguments)}";
    }
}
