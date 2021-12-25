using AdventOfCode.Utilities;
using System.Diagnostics;

namespace AdventOfCode.Problems.Year2021;

public partial class Day24 : Problem<ulong>
{
    private readonly Computer computer = new();
    private Instruction[] instructions;
    private DigitFinder digitFinder;

    public override ulong SolvePart1() => SolvePart(Extremum.Maximum);
    public override ulong SolvePart2() => SolvePart(Extremum.Minimum);

    private ulong SolvePart(Extremum extremum)
    {
        return GetResultDigits(digitFinder.ProduceExtremumDigits(extremum));
    }

    private const int digitCount = 14;
    private const int pairCount = digitCount / 2;

    private static ulong GetResultDigits(int[] digits)
    {
        ulong result = 0;
        // Cheatest solution
        for (int i = 0; i < digitCount; i++)
        {
            result *= 10;
            result += (ulong)digits[i];
        }

        return result;
    }

    protected override void LoadState()
    {
        instructions = ParsedFileLines(Instruction.Parse);
        var blocks = InstructionBlock.ParseBlocks(NormalizedFileContents);
        digitFinder = new(blocks);
    }
    protected override void ResetState()
    {
        instructions = null;
    }

#nullable enable

    // Credits for the shorter workaround:
    // https://github.com/dphilipson/advent-of-code-2021/blob/master/src/days/day24.rs

    // input[IndexA] = input[IndexB] + Offset
    // It is guaranteed that IndexA < IndexB
    private sealed record InputPair(int IndexA, int IndexB, int Offset)
    {
        public void AssignMaxDigits(int[] digits)
        {
            digits[IndexA] = Math.Min(9, 9 - Offset);
            AssignIndexB(digits);
        }
        public void AssignMinDigits(int[] digits)
        {
            digits[IndexA] = Math.Max(1, 1 - Offset);
            AssignIndexB(digits);
        }

        private void AssignIndexB(int[] digits)
        {
            digits[IndexB] = digits[IndexA] + Offset;
        }

        public void AssignExtremumDigits(int[] digits, Extremum extremum)
        {
            switch (extremum)
            {
                case Extremum.Minimum:
                    AssignMinDigits(digits);
                    break;
                case Extremum.Maximum:
                    AssignMaxDigits(digits);
                    break;
            }
        }
    }

    private class DigitFinder
    {
        private readonly InputPair[] pairs;

        public DigitFinder(InstructionBlock[] instructionBlocks)
        {
            pairs = PairProducer.Instance.ProducePairs(instructionBlocks);
        }

        public int[] ProduceExtremumDigits(Extremum extremum)
        {
            var result = new int[digitCount];
            foreach (var pair in pairs)
                pair.AssignExtremumDigits(result, extremum);
            return result;
        }
    }

    private class PairProducer
    {
        public static PairProducer Instance { get; } = new();

        private PairProducer() { }

        public InputPair[] ProducePairs(InstructionBlock[] blocks)
        {
            var stack = new Stack<StackedInfo>();
            var pairs = new List<InputPair>();

            for (int blockIndex = 0; blockIndex < blocks.Length; blockIndex++)
            {
                var block = blocks[blockIndex];
                if (block.Check > 0)
                {
                    stack.Push(new(blockIndex, block.Offset));
                }
                else
                {
                    var popped = stack.Pop();
                    var pair = new InputPair(popped.Index, blockIndex, popped.Offset + block.Check);
                    pairs.Add(pair);
                }
            }

            Debug.Assert(stack.Count is 0);

            return pairs.ToArray();
        }

        private record struct StackedInfo(int Index, int Offset);
    }

    private record InstructionBlock(int ZDiv, int Check, int Offset)
    {
        private static readonly Regex blockPattern = new(
@"inp w
mul x 0
add x z
mod x 26
div z (?'zDiv'-?\d*)
add x (?'check'-?\d*)
eql x w
eql x 0
mul y 0
add y 25
mul y x
add y 1
mul z y
mul y 0
add y w
add y (?'offset'-?\d*)
mul y x
add z y"
.NormalizeLineEndings());

        public static InstructionBlock[] ParseBlocks(string raw)
        {
            return blockPattern.Matches(raw).Select(ParseBlockFromMatch).ToArray();
        }
        private static InstructionBlock ParseBlockFromMatch(Match match)
        {
            var groups = match.Groups;
            int zDiv = groups["zDiv"].Value.ParseInt32();
            int check = groups["check"].Value.ParseInt32();
            int offset = groups["offset"].Value.ParseInt32();
            return new(zDiv, check, offset);
        }
    }

}

#region If you wanna have some fun
public partial class Day24
{
    private ulong WreckComputer()
    {
        computer.Reset();
        computer.Instructions = instructions;

        var digits = new int[digitCount];
        int currentProvidedDigit = 0;

        int initialCursorTop = Console.CursorTop;

        computer.InputRequested += ProvideInput;
        computer.RunUntilInput();
        IterateAllDigits(0);

        return GetResultDigits(digits);

        void PrintResultDigits()
        {
            Console.CursorTop = initialCursorTop;
            Console.WriteLine(GetResultDigits(digits));
        }

        long ProvideInput()
        {
            return currentProvidedDigit;
        }
        bool IterateAllDigits(int index)
        {
            for (int i = 9; i > 0; i--)
            {
                if (IterateDigit(index, i))
                    return true;

                computer.RecoverLastState();
            }
            return false;
        }
        bool IterateDigit(int index, int digit)
        {
            currentProvidedDigit = digit;
            computer.RunNextInstruction();

            if (!computer.RunUntilInput())
                return false;

            digits[index] = digit;
            //PrintResultDigits();
            if (index is digitCount - 1)
                return computer.RegisterState.Z is 0;

            return IterateAllDigits(index + 1);
        }
    }

    private abstract record InstructionArgument
    {
        public abstract long GetActualValue(ref RegisterState registers);

        public abstract override string ToString();

        public static InstructionArgument ParseArgument(string raw)
        {
            if (raw[0].IsLetter())
                return RegisterInstructionArgument.ParseRegisterArgument(raw);

            return LiteralInstructionArgument.ParseLiteralArgument(raw);
        }
    }
    private sealed record LiteralInstructionArgument(int Value)
        : InstructionArgument
    {
        public override long GetActualValue(ref RegisterState registers) => Value;

        public override string ToString() => Value.ToString();

        public static LiteralInstructionArgument ParseLiteralArgument(string raw) => new(raw.ParseInt32());
    }
    private sealed record RegisterInstructionArgument(char Identifier)
        : InstructionArgument
    {
        public override long GetActualValue(ref RegisterState registers) => registers.GetRegisterValue(Identifier);
        public void SetValue(ref RegisterState registers, int value) => registers.SetRegisterValue(Identifier, value);

        public override string ToString() => Identifier.ToString();

        public static RegisterInstructionArgument ParseRegisterArgument(string raw) => new(raw[0]);
    }

    private abstract record Instruction(RegisterInstructionArgument AffectedRegister)
    {
        public abstract ComputerOperator Operator { get; }

        public static Instruction Parse(string raw)
        {
            var split = raw.Split(' ');
            var instructionOperator = ParseOperator(split[0]);
            return ParseInstruction(split, instructionOperator);
        }
        private static ComputerOperator ParseOperator(string mnemonic)
        {
            return mnemonic switch
            {
                "inp" => ComputerOperator.Input,
                "add" => ComputerOperator.Add,
                "mul" => ComputerOperator.Multiply,
                "div" => ComputerOperator.Divide,
                "mod" => ComputerOperator.Modulo,
                "eql" => ComputerOperator.Equality,
            };
        }
        private static Instruction ParseInstruction(string[] splitRaw, ComputerOperator instructionOperator)
        {
            // Parsing always feels hacky
            var affectedRegister = RegisterInstructionArgument.ParseRegisterArgument(splitRaw[1]);
            if (instructionOperator is ComputerOperator.Input)
                return new InputInstruction(affectedRegister);

            Debug.Assert(splitRaw.Length >= 3);
            var valueArgument = InstructionArgument.ParseArgument(splitRaw[2]);
            return instructionOperator switch
            {
                ComputerOperator.Add => new AddInstruction(affectedRegister, valueArgument),
                ComputerOperator.Multiply => new MultiplyInstruction(affectedRegister, valueArgument),
                ComputerOperator.Divide => new DivideInstruction(affectedRegister, valueArgument),
                ComputerOperator.Modulo => new ModuloInstruction(affectedRegister, valueArgument),
                ComputerOperator.Equality => new EqualityInstruction(affectedRegister, valueArgument),
            };
        }
    }
    private sealed record InputInstruction(RegisterInstructionArgument AffectedRegister)
        : Instruction(AffectedRegister)
    {
        public override ComputerOperator Operator => ComputerOperator.Input;

        public void SetResult(ref RegisterState registers, long value)
        {
            registers.SetRegisterValue(AffectedRegister.Identifier, value);
        }
    }

    private abstract record OperationInstruction(RegisterInstructionArgument AffectedRegister, InstructionArgument ValueArgument)
        : Instruction(AffectedRegister)
    {
        public bool SetResult(ref RegisterState registers)
        {
            long left = AffectedRegister.GetActualValue(ref registers);
            long right = ValueArgument.GetActualValue(ref registers);
            bool valid = CalculateResultingValue(left, right, out long result);

            if (valid)
            {
                registers.SetRegisterValue(AffectedRegister.Identifier, result);
            }

            return valid;
        }
        protected abstract bool CalculateResultingValue(long left, long right, out long result);
    }
    private abstract record AlwaysValidOperationInstruction(RegisterInstructionArgument AffectedRegister, InstructionArgument ValueArgument)
        : OperationInstruction(AffectedRegister, ValueArgument)
    {
        protected sealed override bool CalculateResultingValue(long left, long right, out long result)
        {
            result = CalculateResultingValue(left, right);
            return true;
        }
        protected abstract long CalculateResultingValue(long left, long right);
    }

    private sealed record AddInstruction(RegisterInstructionArgument AffectedRegister, InstructionArgument ValueArgument)
        : AlwaysValidOperationInstruction(AffectedRegister, ValueArgument)
    {
        public override ComputerOperator Operator => ComputerOperator.Add;

        protected override long CalculateResultingValue(long left, long right) => left + right;
    }
    private sealed record MultiplyInstruction(RegisterInstructionArgument AffectedRegister, InstructionArgument ValueArgument)
        : AlwaysValidOperationInstruction(AffectedRegister, ValueArgument)
    {
        public override ComputerOperator Operator => ComputerOperator.Multiply;

        protected override long CalculateResultingValue(long left, long right) => left * right;
    }
    private sealed record DivideInstruction(RegisterInstructionArgument AffectedRegister, InstructionArgument ValueArgument)
        : OperationInstruction(AffectedRegister, ValueArgument)
    {
        public override ComputerOperator Operator => ComputerOperator.Divide;

        protected override bool CalculateResultingValue(long left, long right, out long result)
        {
            result = default;
            if (right == 0)
                return false;

            result = left / right;
            return true;
        }
    }
    private sealed record ModuloInstruction(RegisterInstructionArgument AffectedRegister, InstructionArgument ValueArgument)
        : OperationInstruction(AffectedRegister, ValueArgument)
    {
        public override ComputerOperator Operator => ComputerOperator.Modulo;

        protected override bool CalculateResultingValue(long left, long right, out long result)
        {
            result = default;
            if (left < 0 || right <= 0)
                return false;

            result = left % right;
            return true;
        }
    }
    private sealed record EqualityInstruction(RegisterInstructionArgument AffectedRegister, InstructionArgument ValueArgument)
        : AlwaysValidOperationInstruction(AffectedRegister, ValueArgument)
    {
        public override ComputerOperator Operator => ComputerOperator.Equality;

        protected override long CalculateResultingValue(long left, long right) => Convert.ToInt64(left == right);
    }

    private record struct ComputerState(RegisterState Registers, int InstructionPointer)
    {
    }

    private record struct RegisterState(long W, long X, long Y, long Z)
    {
        // Generally please allow ref for struct fields

        public long GetRegisterValue(char id) => id switch
        {
            'w' => W,
            'x' => X,
            'y' => Y,
            'z' => Z,
        };
        // Please remove the need for _ = 
        public void SetRegisterValue(char id, long value) => _ = id switch
        {
            'w' => W = value,
            'x' => X = value,
            'y' => Y = value,
            'z' => Z = value,
        };
    }

    // Not using the general computer because of the
    // uniqueness of this structure
    private class Computer
    {
        /*
         * For this solution, it is important to note that operations are performed
         * after all input instructions, meaning a crash can be easily detected after
         * each input and save tons of searching space
         */

        private readonly Stack<ComputerState> lastComputerStates = new();
        private Instruction[] instructions;

        // ref field not being supported on structs is a massive design bottleneck here
        private RegisterState registers;
        private int instructionPointer;

        private ComputerState currentState;

        public event Func<long> InputRequested;

        // This could be ref readonly if the language supported it
        public RegisterState RegisterState => registers;

        public IEnumerable<Instruction> Instructions
        {
            set => instructions = value.ToArray();
        }

        public bool RunUntilInput()
        {
            while (instructionPointer < instructions.Length)
            {
                var instruction = instructions[instructionPointer];
                if (instruction is InputInstruction)
                    return true;

                if (!ApplyInstruction(instruction))
                    return false;

                instructionPointer++;
            }
            return true;
        }
        public bool RunNextInstruction()
        {
            var instruction = instructions[instructionPointer];

            if (!ApplyInstruction(instruction))
                return false;

            instructionPointer++;
            return true;
        }
        public bool Run()
        {
            while (instructionPointer < instructions.Length)
            {
                var instruction = instructions[instructionPointer];
                if (!ApplyInstruction(instruction))
                    return false;

                instructionPointer++;
            }
            return true;
        }
        public bool RunInstructions(IEnumerable<Instruction> instructions)
        {
            Instructions = instructions.ToArray();
            return Run();
        }

        private void SetComputerState(ComputerState state)
        {
            registers = state.Registers;
            instructionPointer = state.InstructionPointer;
        }
        public void RecoverLastState()
        {
            if (lastComputerStates.Count is 0)
                return;

            SetComputerState(lastComputerStates.Pop());
        }

        private bool ApplyInstruction(Instruction instruction)
        {
            switch (instruction)
            {
                case InputInstruction inputInstruction:
                    ApplyInput(inputInstruction);
                    return true;

                case OperationInstruction operationInstruction:
                    return ApplyOperator(operationInstruction);
            }

            // Unsupported instructions crash the program
            return false;
        }

        private bool ApplyOperator(OperationInstruction operationInstruction)
        {
            return operationInstruction.SetResult(ref registers);
        }
        private void ApplyInput(InputInstruction inputInstruction)
        {
            long input = InputRequested();
            inputInstruction.SetResult(ref registers, input);
            StoreCurrentComputerState();
        }

        private ComputerState CurrentState()
        {
            return new(registers, instructionPointer);
        }
        private void StoreCurrentComputerState()
        {
            lastComputerStates.Push(CurrentState());
        }

        public void Reset()
        {
            lastComputerStates.Clear();
            registers = default;
            instructionPointer = 0;
        }
    }
}
#endregion