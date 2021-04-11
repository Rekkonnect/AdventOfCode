using AdventOfCode.Functions;
using AdventOfCode.Utilities.TwoDimensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2016
{
    public class Day2 : Problem<string>
    {
        private static readonly DiamondKeypadGrid diamondKeypad = new();

        private InstructionString[] instructionStrings;

        public override string SolvePart1()
        {
            return GetCode(instructionString => instructionString.RunSquareKeypad(), GetSquareKeypadChar);
        }
        public override string SolvePart2()
        {
            return GetCode(instructionString => instructionString.RunDiamondKeypad(), location => diamondKeypad[location]);
        }

        protected override void ResetState()
        {
            instructionStrings = null;
        }
        protected override void LoadState()
        {
            instructionStrings = ParsedFileLines(InstructionString.Parse);
        }

        private string GetCode(Func<InstructionString, Location2D> keypadRunner, Func<Location2D, char> keypadCharGetter)
        {
            var endingLocations = instructionStrings.Select(keypadRunner).ToArray();
            char[] code = new char[endingLocations.Length];
            for (int i = 0; i < code.Length; i++)
                code[i] = keypadCharGetter(endingLocations[i]);
            return new string(code);
        }

        private char GetSquareKeypadChar(Location2D location) => (char)(location.Y * 3 + location.X + 1 + '0');

        private class InstructionString
        {
            private IEnumerable<Direction> instructions;

            public InstructionString(IEnumerable<Direction> directions)
            {
                instructions = directions;
            }

            public Location2D RunSquareKeypad()
            {
                return RunKeypad((1, 1), WithinBounds);

                static bool WithinBounds(Location2D location)
                {
                    var (x, y) = location;
                    return x >= 0 && x < 3 && y >= 0 && y < 3;
                }
            }
            public Location2D RunDiamondKeypad()
            {
                return RunKeypad(diamondKeypad.Center, diamondKeypad.WithinDiamond);
            }

            private Location2D RunKeypad(Location2D startingLocation, Predicate<Location2D> withinBounds)
            {
                var current = startingLocation;

                foreach (var direction in instructions)
                {
                    var next = current + DirectionalLocation.GetLocationOffset(direction, invertY: true);
                    if (!withinBounds(next))
                        continue;

                    current = next;
                }

                return current;
            }

            public static InstructionString Parse(string raw)
            {
                return new(raw.Select(ParseDirection));
            }
            private static Direction ParseDirection(char c)
            {
                return c switch
                {
                    'U' => Direction.Up,
                    'D' => Direction.Down,
                    'L' => Direction.Left,
                    'R' => Direction.Right,
                };
            }
        }

        private class DiamondKeypadGrid : Grid2D<char>
        {
            public DiamondKeypadGrid()
                : base(5, 5)
            {
                int currentValue = 1;

                SetValues(0, 2..3, ref currentValue);
                SetValues(1, 1..4, ref currentValue);
                SetValues(2, 0..5, ref currentValue);
                SetValues(3, 1..4, ref currentValue);
                SetValues(4, 2..3, ref currentValue);
            }

            private void SetValues(int row, Range range, ref int currentValue)
            {
                int start = range.Start.Value;
                int end = range.End.Value;
                for (int x = start; x < end; x++)
                {
                    this[x, row] = currentValue.ToHexChar();
                    currentValue++;
                }
            }

            public bool WithinDiamond(Location2D location)
            {
                if (!IsValidLocation(location))
                    return false;

                return this[location] != default;
            }
        }
    }
}
