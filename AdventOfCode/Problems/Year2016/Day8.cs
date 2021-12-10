using AdventOfCode.Utilities.TwoDimensions;
using AdventOfCSharp;
using AdventOfCSharp.Extensions;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2016;

public class Day8 : Problem<int, IGlyphGrid>
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

    private class Screen : PrintableGrid2D<PixelState>, IGlyphGrid
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

        protected override Dictionary<PixelState, char> GetPrintableCharacters()
        {
            return new()
            {
                [PixelState.Off] = '.',
                [PixelState.On] = '#',
            };
        }
    }
    private enum PixelState
    {
        Off,
        On,
    }

    private record PixelAdjustmentInstruction(PixelAdjustmentOperation Operation, int A, int B)
    {
        private static readonly Regex rectanglePattern = new(@"rect (?'a'\d*)x(?'b'\d*)", RegexOptions.Compiled);
        private static readonly Regex rotateRowPattern = new(@"rotate row y=(?'a'\d*) by (?'b'\d*)", RegexOptions.Compiled);
        private static readonly Regex rotateColumnPattern = new(@"rotate column x=(?'a'\d*) by (?'b'\d*)", RegexOptions.Compiled);

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
