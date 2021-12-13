using Garyon.Objects;

namespace AdventOfCode.Problems.Year2017;

public class Day8 : Problem<int>
{
    private Computer computer;

    public override int SolvePart1()
    {
        return computer.CurrentLargestRegisterValue;
    }
    public override int SolvePart2()
    {
        return computer.AllTimeLargestRegisterValue;
    }

    protected override void LoadState()
    {
        computer = new(ParsedFileLines(ConditionalInstruction.Parse));
        computer.RunProgram();
    }
    protected override void ResetState()
    {
        computer = null;
    }

    private class Computer
    {
        private readonly FlexibleDictionary<string, int> registers = new();

        private ConditionalInstruction[] instructions;
        private int instructionIndex;

        public Computer(ConditionalInstruction[] conditionalInstructions)
        {
            instructions = conditionalInstructions;
        }

        public int CurrentLargestRegisterValue => registers.Values.Max();
        public int AllTimeLargestRegisterValue { get; private set; }

        public void RunProgram()
        {
            instructionIndex = 0;

            do
                ExecuteInstruction(instructions[instructionIndex]);
            while (instructionIndex < instructions.Length);
        }
        private void ExecuteInstruction(ConditionalInstruction instruction)
        {
            int instructionOffset = 1;

            if (instruction.Condition.Valid(registers[instruction.Condition.LeftRegister]))
            {
                ExecuteValidatedInstruction(instruction);
            }

            instructionIndex += instructionOffset;
        }

        private void ExecuteValidatedInstruction(ConditionalInstruction instruction)
        {
            switch (instruction.Operator)
            {
                case ComputerOperator.Increase:
                case ComputerOperator.Decrease:
                    int multiplier = instruction.Operator is ComputerOperator.Decrease ? -1 : 1;
                    int result = registers[instruction.Register] += instruction.Adjustment * multiplier;

                    if (result > AllTimeLargestRegisterValue)
                        AllTimeLargestRegisterValue = result;

                    break;
            }
        }

        public void SetRegister(string s, int value) => registers[s] = value;
        public int GetRegister(string s) => registers[s];
    }

    private record Condition(string LeftRegister, ComparisonKinds Comparison, int RightValue)
    {
        private static readonly Regex conditionPattern = new(@"(?'register'\w*) (?'comparison'\W*) (?'value'[-\d]*)", RegexOptions.Compiled);

        public bool Valid(int registerValue) => Comparison.Matches(registerValue.GetComparisonResult(RightValue));

        public static Condition Parse(string raw)
        {
            var groups = conditionPattern.Match(raw).Groups;
            var register = groups["register"].Value;
            var comparison = ParseComparisonKinds(groups["comparison"].Value);
            int value = groups["value"].Value.ParseInt32();
            return new(register, comparison, value);
        }

        private static ComparisonKinds ParseComparisonKinds(string raw)
        {
            return raw switch
            {
                "<" => ComparisonKinds.Less,
                "<=" => ComparisonKinds.LessOrEqual,
                "==" => ComparisonKinds.Equal,
                ">=" => ComparisonKinds.GreaterOrEqual,
                ">" => ComparisonKinds.Greater,
                "!=" => ComparisonKinds.Different,
                _ => ComparisonKinds.None,
            };
        }
    }

    private record ConditionalInstruction(string Register, ComputerOperator Operator, int Adjustment, Condition Condition)
    {
        private static readonly Regex nodePattern = new(@"(?'register'\w*) (?'operator'\w*) (?'adjustment'[-\d]*) if (?'condition'.*)", RegexOptions.Compiled);

        public static ConditionalInstruction Parse(string raw)
        {
            var groups = nodePattern.Match(raw).Groups;
            var register = groups["register"].Value;
            var op = ComputerOperatorInformation.ParseMnemonic(groups["operator"].Value);
            int adjustment = groups["adjustment"].Value.ParseInt32();
            var condition = groups["condition"].Value;
            return new(register, op, adjustment, Condition.Parse(condition));
        }
    }
}
