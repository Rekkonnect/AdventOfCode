using AdventOfCode.Utilities.TwoDimensions;
using AdventOfCSharp.Extensions;

namespace AdventOfCode.Problems.Year2016;

public partial class Day8 : Problem<int, IGlyphGrid>
{
    private PixelAdjustmentInstruction[] instructions;
    private Screen screen;

    public override int SolvePart1()
    {
        return screen.ValueCounters[PixelState.On];
    }
    public override IGlyphGrid SolvePart2()
    {
        return screen;
    }

    protected override void ResetState()
    {
        instructions = null;
        screen = null;
    }
    protected override void LoadState()
    {
        instructions = ParsedFileLines(PixelAdjustmentInstruction.Parse);
        screen = new Screen();
        screen.ApplyInstructions(instructions);
    }

    private class Screen : PrintableGlyphGrid2D<PixelState>
    {
        public Screen()
            : base(50, 6) { }

        public void ApplyInstructions(IEnumerable<PixelAdjustmentInstruction> instructions)
        {
            foreach (var instruction in instructions)
                ApplyInstruction(instruction);
        }
        public void ApplyInstruction(PixelAdjustmentInstruction instruction)
        {
            switch (instruction.Operation)
            {
                case PixelAdjustmentOperation.Rectangle:
                    CreateRectangle(instruction.A, instruction.B);
                    break;
                case PixelAdjustmentOperation.RotateRow:
                    RotateRow(instruction.A, instruction.B);
                    break;
                case PixelAdjustmentOperation.RotateColumn:
                    RotateColumn(instruction.A, instruction.B);
                    break;
            }
        }

        private void CreateRectangle(int width, int height)
        {
            this[..width, ..height] = PixelState.On;
        }
        private void RotateRow(int row, int rotation)
        {
            SetXLine(row, GetXLine(row).RotateRight(rotation));
        }
        private void RotateColumn(int column, int rotation)
        {
            SetYLine(column, GetYLine(column).RotateRight(rotation));
        }

        protected override bool IsDrawnPixel(PixelState value) => value is PixelState.On;
    }
    private enum PixelState
    {
        Off,
        On,
    }

    private partial record PixelAdjustmentInstruction(PixelAdjustmentOperation Operation, int A, int B)
    {
        private static readonly Regex rectanglePattern = RectangleRegex();
        private static readonly Regex rotateRowPattern = RotateRowRegex();
        private static readonly Regex rotateColumnPattern = RotateColumnRegex();

        [GeneratedRegex("rect (?'a'\\d*)x(?'b'\\d*)", RegexOptions.Compiled)]
        private static partial Regex RectangleRegex();
        [GeneratedRegex("rotate row y=(?'a'\\d*) by (?'b'\\d*)", RegexOptions.Compiled)]
        private static partial Regex RotateRowRegex();
        [GeneratedRegex("rotate column x=(?'a'\\d*) by (?'b'\\d*)", RegexOptions.Compiled)]
        private static partial Regex RotateColumnRegex();

        public static PixelAdjustmentInstruction Parse(string raw)
        {
            var operation = GetInstructionMatch(raw, out var match);
            var groups = match.Groups;
            int a = groups["a"].Value.ParseInt32();
            int b = groups["b"].Value.ParseInt32();
            return new(operation, a, b);
        }

        private static PixelAdjustmentOperation GetInstructionMatch(string raw, out Match match)
        {
            match = rectanglePattern.Match(raw);
            if (match.Success)
                return PixelAdjustmentOperation.Rectangle;

            match = rotateRowPattern.Match(raw);
            if (match.Success)
                return PixelAdjustmentOperation.RotateRow;

            match = rotateColumnPattern.Match(raw);
            if (match.Success)
                return PixelAdjustmentOperation.RotateColumn;

            return default;
        }
    }

    private enum PixelAdjustmentOperation
    {
        Rectangle,
        RotateRow,
        RotateColumn
    }
}
