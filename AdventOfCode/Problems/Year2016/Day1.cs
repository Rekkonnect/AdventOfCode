using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2016;

public class Day1 : Problem<int>
{
    private InstructionString instructionString;

    public override int SolvePart1()
    {
        return instructionString.EndingLocation.ManhattanDistanceFromCenter;
    }
    public override int SolvePart2()
    {
        return instructionString.FirstRevisitedLocation.ManhattanDistanceFromCenter;
    }

    protected override void ResetState()
    {
        instructionString = null;
    }
    protected override void LoadState()
    {
        instructionString = InstructionString.Parse(FileContents);
    }

    private class InstructionString
    {
        private IEnumerable<Instruction> instructions;

        public Location2D EndingLocation { get; private set; }
        public Location2D FirstRevisitedLocation { get; private set; }

        public InstructionString(IEnumerable<Instruction> locationInstructions)
        {
            instructions = locationInstructions;
            RunInstructions();
        }

        private void RunInstructions()
        {
            var current = Location2D.Zero;
            var currentDirection = new DirectionalLocation(Direction.Up);
            var visitedLocations = new HashSet<Location2D> { current };
            bool hasRevisited = false;

            foreach (var instruction in instructions)
            {
                switch (instruction.Direction)
                {
                    case Direction.Left:
                        currentDirection.TurnLeft();
                        break;
                    case Direction.Right:
                        currentDirection.TurnRight();
                        break;
                }
                for (int i = 1; i <= instruction.Forward; i++)
                {
                    current += currentDirection.LocationOffset;

                    if (hasRevisited)
                        continue;

                    hasRevisited = !visitedLocations.Add(current);
                    if (hasRevisited)
                        FirstRevisitedLocation = current;
                }
            }

            EndingLocation = current;
        }

        public static InstructionString Parse(string raw)
        {
            return new(raw.Split(", ").Select(Instruction.Parse));
        }
    }
    private struct Instruction
    {
        public Direction Direction { get; }
        public int Forward { get; }

        public Instruction(Direction direction, int forward) => (Direction, Forward) = (direction, forward);

        public static Instruction Parse(string raw)
        {
            var direction = ParseDirection(raw[0]);
            int forward = raw[1..].ParseInt32();
            return new(direction, forward);
        }
        private static Direction ParseDirection(char c)
        {
            return c switch
            {
                'R' => Direction.Right,
                'L' => Direction.Left,
            };
        }
    }
}
