using AdventOfCode.Functions;
using System;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems;

public record ComputerInstruction(ComputerOperator Operator, string[] Arguments)
{
    private static readonly Regex statPattern = new(@"(?'operator'\w*) (?'arguments'.*)", RegexOptions.Compiled);

    public static readonly ComputerInstruction NoOperation = new(ComputerOperator.NoOperation, Array.Empty<string>());

    public bool Argument(int index, out int constantValue, out char register)
    {
        constantValue = default;
        register = default;

        if (index >= Arguments.Length)
            return false;

        return Arguments[index].ExtractInt32AndFirstChar(out constantValue, out register);
    }

    public static ComputerInstruction Parse(string s, string argumentSplitter = " ")
    {
        var groups = statPattern.Match(s).Groups;
        var op = ComputerOperatorInformation.ParseMnemonic(groups["operator"].Value);
        var arguments = groups["arguments"].Value.Split(argumentSplitter);
        return new(op, arguments);
    }

    public override string ToString()
    {
        return $"{Operator.GetMnemonic()} {string.Join(" ", Arguments)}";
    }
}
