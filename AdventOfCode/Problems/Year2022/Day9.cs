using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2022;

public class Day9 : Problem<int>
{
    private BridgeMovement bridgeMovement;

    public override int SolvePart1()
    {
        return SolvePart(2, 1);
    }
    public override int SolvePart2()
    {
        return SolvePart(10, 9);
    }

    private int SolvePart(int knotCount, int trackedTailIndex)
    {
        var gridCells = bridgeMovement.GetGridCells(knotCount, trackedTailIndex);
        return gridCells.ValueCounters[GridCell.TailTouched];
    }

    protected override void LoadState()
    {
        var instructions = ParsedFileLines(MovementInstruction.Parse).ToImmutableArray();
        var instructionSet = new MovementInstructionSet(instructions);
        bridgeMovement = new BridgeMovement(instructionSet);
    }

    private class BridgeMovement
    {
        private readonly MovementInstructionSet movementInstructions;

        public BridgeMovement(MovementInstructionSet movementInstructions)
        {
            this.movementInstructions = movementInstructions;
        }

        public Grid2D<GridCell> GetGridCells(int knotCount, int trackedKnotIndex)
        {
            var region = movementInstructions.GetLocationRectangle();
            var locationOffset = -region.BottomLeft;
            var dimensions = region.Dimensions + (4, 4);
            var grid = new Grid2D<GridCell>(dimensions);
            var startingLocation = locationOffset;

            var knots = new Knot[knotCount];
            for (int i = 0; i < knotCount; i++)
            {
                knots[i] = new Knot(startingLocation);
            }

            grid[startingLocation] = GridCell.TailTouched;
            var instructions = movementInstructions.Instructions;

            for (int i = 0; i < instructions.Length; i++)
            {
                var instruction = instructions[i];
                int steps = instruction.Steps;
                for (int s = 0; s < steps; s++)
                {
                    var globalHead = knots[0];
                    instruction.ApplyOnLocation(ref globalHead.Location, 1);

                    for (int k = 0; k < knotCount - 1; k++)
                    {
                        int tailIndex = k + 1;
                        var head = knots[k];
                        var tail = knots[tailIndex];

                        var currentHead = head.Location;

                        tail.FollowHead(currentHead);
                        if (tailIndex == trackedKnotIndex)
                        {
                            grid[tail.Location] = GridCell.TailTouched;
                        }
                    }
                }
            }

            return grid;
        }
    }

    private class Knot
    {
        public Location2D Location;

        public Knot(Location2D location)
        {
            Location = location;
        }

        public void FollowHead(Knot knot)
        {
            FollowHead(knot.Location);
        }

        public void FollowHead(Location2D headLocation)
        {
            if (TouchesHead(headLocation, out var headOffset))
            {
                return;
            }

            var movementOffset = headOffset;
            switch (movementOffset.X)
            {
                case > 1:
                    movementOffset.X = 1;
                    break;
                case < -1:
                    movementOffset.X = -1;
                    break;
            }
            switch (movementOffset.Y)
            {
                case > 1:
                    movementOffset.Y = 1;
                    break;
                case < -1:
                    movementOffset.Y = -1;
                    break;
            }

            Location += movementOffset;
        }

        public bool TouchesHead(Location2D headLocation, out Location2D headOffset)
        {
            headOffset = headLocation - Location;
            var locationDifference = headOffset.Absolute;
            return locationDifference.X <= 1
                && locationDifference.Y <= 1;
        }
    }

    private class MovementInstructionSet
    {
        public ImmutableArray<MovementInstruction> Instructions { get; }

        public MovementInstructionSet(ImmutableArray<MovementInstruction> instructions)
        {
            Instructions = instructions;
        }

        public Rectangle GetLocationRectangle()
        {
            var min = Location2D.Zero;
            var max = Location2D.Zero;

            var current = Location2D.Zero;
            for (int i = 0; i < Instructions.Length; i++)
            {
                var currentInstruction = Instructions[i];
                currentInstruction.ApplyOnLocation(ref current);

                min.X = Math.Min(current.X, min.X);
                min.Y = Math.Min(current.Y, min.Y);
                max.X = Math.Max(current.X, max.X);
                max.Y = Math.Max(current.Y, max.Y);
            }
            return Rectangle.FromBounds(min, max);
        }
    }

    private record struct MovementInstruction(Direction Direction, int Steps)
    {
        public Location2D ApplyOnLocation(Location2D current)
        {
            ApplyOnLocation(ref current);
            return current;
        }
        public void ApplyOnLocation(ref Location2D current)
        {
            int steps = Steps;
            ApplyOnLocation(ref current, steps);
        }
        public Location2D ApplyOnLocation(Location2D current, int steps)
        {
            ApplyOnLocation(ref current, steps);
            return current;
        }
        public void ApplyOnLocation(ref Location2D current, int steps)
        {
            switch (Direction)
            {
                case Direction.Up:
                    current.Y += steps;
                    break;

                case Direction.Down:
                    current.Y -= steps;
                    break;

                case Direction.Left:
                    current.X -= steps;
                    break;

                case Direction.Right:
                    current.X += steps;
                    break;
            }
        }

        public static MovementInstruction Parse(string line)
        {
            char directionChar = line[0];
            int steps = line.AsSpan()[2..].ParseInt32();
            var direction = Parse(directionChar);
            return new(direction, steps);
        }
        private static Direction Parse(char c)
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

    private enum GridCell
    {
        Untouched,
        HeadTouched,
        TailTouched,
    }
}
