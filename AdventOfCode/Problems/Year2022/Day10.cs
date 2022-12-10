using AdventOfCode.Functions;
using AdventOfCode.Utilities.TwoDimensions;
using AdventOfCSharp.Utilities;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2022;

public class Day10 : Problem<int, IGlyphGrid>
{
    private ClockProgram program;

    public override int SolvePart1()
    {
        var clockCycles = new[] { 20, 60, 100, 140, 180, 220 };
        var computer = new SignalStrengthClockComputer(clockCycles);
        computer.ExecuteProgram(program);
        var signalStrengths = computer.RecordedSignalStrengths;
        return signalStrengths.Sum(x => x.Strength);
    }
    public override IGlyphGrid SolvePart2()
    {
        var computer = new CRTScreenClockComputer();
        computer.ExecuteProgram(program);
        return computer.DrawnScreen;
    }

    protected override void LoadState()
    {
        var instructions = ParsedFileLines(Instruction.Parse);
        program = new(instructions.ToImmutableArray());
    }

    private class CRTScreenClockComputer : ClockComputerBase
    {
        private const int width = 40;
        private const int height = 6;

        private readonly CRTScreen screen;

        public IGlyphGrid DrawnScreen => screen;

        public CRTScreenClockComputer()
        {
            screen = new((width, height));
        }

        protected override void PrepareInstructionExecution(int nextClockCycle)
        {
            while (Clock <= nextClockCycle)
            {
                DrawPixel();
                Clock++;
            }
        }

        private void DrawPixel()
        {
            const int length = width * height;
            if (Clock >= length)
                return;

            var location = CurrentLocation();
            screen[location] = GetSpriteColor(location);
        }

        private Location2D CurrentLocation()
        {
            var (row, column) = Math.DivRem(Clock, width);
            return new(column, row);
        }

        private CRTPixel GetSpriteColor(Location2D location)
        {
            return GetSpriteColor(location.X);
        }
        private CRTPixel GetSpriteColor(int column)
        {
            int offset = RegisterX - column;
            offset = Math.Abs(offset);
            return (offset <= 1) switch
            {
                true => CRTPixel.Light,
                false => CRTPixel.Dark,
            };
        }
    }

    private class CRTScreen : PrintableGlyphGrid2D<CRTPixel>
    {
        public CRTScreen(Location2D dimensions)
            : base(dimensions) { }

        protected override bool IsDrawnPixel(CRTPixel value) => value is CRTPixel.Light;
    }
    private enum CRTPixel
    {
        Dark,
        Light,
    }

    private class SignalStrengthClockComputer : ClockComputerBase
    {
        private readonly SignalStengthCycleIterator cycleIterator;

        public IEnumerable<SignalStrength> RecordedSignalStrengths => cycleIterator.SignalStrengths;

        public SignalStrengthClockComputer(IEnumerable<int> interestingClockCycles)
        {
            cycleIterator = new(interestingClockCycles);
        }

        protected override void PrepareInstructionExecution(int nextClockCycle)
        {
            int nextIteratedCycle = cycleIterator.NextClockCycle();
            if (nextIteratedCycle > 0)
            {
                if (nextIteratedCycle <= nextClockCycle)
                {
                    RecordSignalStrength();
                }
            }
        }

        private void RecordSignalStrength()
        {
            cycleIterator.Register(RegisterX);
        }
    }

    private abstract class ClockComputerBase
    {
        public int Clock { get; protected set; }
        public int RegisterX { get; private set; }

        protected ClockComputerBase()
        {
            Clock = 0;
            RegisterX = 1;
        }

        public void ExecuteProgram(ClockProgram program)
        {
            foreach (var instruction in program.Instructions)
            {
                ExecuteInstruction(instruction);
            }
        }

        public void ExecuteInstruction(Instruction instruction)
        {
            int clockCycles = GetClockCycles(instruction);

            int nextClockCycle = Clock + clockCycles;
            PrepareInstructionExecution(nextClockCycle);

            PerformInstruction(instruction);

            Clock = nextClockCycle;
        }

        private void PerformInstruction(Instruction instruction)
        {
            switch (instruction.Operation)
            {
                case InstructionOperation.AddX:
                    RegisterX += instruction.Argument;
                    break;

                case InstructionOperation.Noop:
                    break;
            }
        }

        protected abstract void PrepareInstructionExecution(int nextClockCycle);

        private static int GetClockCycles(Instruction instruction)
        {
            return GetClockCycles(instruction.Operation);
        }
        private static int GetClockCycles(InstructionOperation operation)
        {
            return operation switch
            {
                InstructionOperation.AddX => 2,
                InstructionOperation.Noop => 1,
            };
        }

    }

    private class SignalStengthCycleIterator
    {
        private readonly ImmutableArray<int> clockCycles;
        private int currentIndex = 0;

        private readonly SignalStrength[] signalStrengths;

        public IEnumerable<SignalStrength> SignalStrengths => signalStrengths;

        public SignalStengthCycleIterator(IEnumerable<int> interestingClockCycles)
        {
            clockCycles = interestingClockCycles.ToImmutableArray();
            signalStrengths = new SignalStrength[clockCycles.Length];
        }

        public int NextClockCycle()
        {
            if (currentIndex >= clockCycles.Length)
                return -1;

            return clockCycles[currentIndex];
        }
        public void Register(int registerValue)
        {
            int cycle = NextClockCycle();
            signalStrengths[currentIndex] = new(cycle, registerValue);
            currentIndex++;
        }
    }

    private readonly record struct SignalStrength(int ClockCycle, int RegisterValue)
    {
        public int Strength => ClockCycle * RegisterValue;
    }

    private class ClockProgram
    {
        public ImmutableArray<Instruction> Instructions;

        public ClockProgram(ImmutableArray<Instruction> instructions)
        {
            Instructions = instructions;
        }
    }

    private readonly record struct Instruction(InstructionOperation Operation, int Argument)
    {
        public static Instruction Noop()
        {
            return new(InstructionOperation.Noop, 0);
        }

        public static Instruction Parse(string line)
        {
            switch (line)
            {
                case "noop":
                    return Noop();

                default:
                    int value = line.SubstringSpanAfter("addx ").ParseInt32();
                    return new(InstructionOperation.AddX, value);
            }
        }
    }

    private enum InstructionOperation
    {
        AddX,
        Noop,
    }
}
