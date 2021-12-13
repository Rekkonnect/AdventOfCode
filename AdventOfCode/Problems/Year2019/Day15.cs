using AdventOfCode.Problems.Year2019.Utilities;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2019;

public class Day15 : Problem<int>
{
    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart1() => General(Part1Returner);
    public override int SolvePart2() => General(Part2Returner);

    private int Part1Returner(ShipSection grid, Location2D startingLocation)
    {
        return grid.GetShortestPath(startingLocation, grid.GetUniqueElementLocation(ElementType.OxygenSystem)).Count;
    }
    private int Part2Returner(ShipSection grid, Location2D startingLocation)
    {
        return 0;
    }

    private static readonly Dictionary<Direction, int> movementDirectionCodes = new()
    {
        { Direction.Up, 1 },
        { Direction.Down, 2 },
        { Direction.Right, 3 },
        { Direction.Left, 4 },
    };

    private int General(Returner returner)
    {
        const int gridSize = 1000;
        var grid = new ShipSection(gridSize);

        var computer = new IntcodeComputer(FileContents);

        ElementType lastOutput;

        var currentLocation = new Location2D(gridSize / 2);
        var startingLocation = currentLocation;
        var currentDirections = new List<Direction>();
        var currentDirection = Direction.Right;

        do
        {
            var newDirection = currentDirection;
            int movement = movementDirectionCodes[newDirection];
            lastOutput = (ElementType)(int)computer.RunUntilOutput(null, movement);
            grid[currentLocation] = lastOutput;

            if (lastOutput > 0)
                currentLocation += DirectionalLocation.GetLocationOffset(newDirection);
        }
        while (lastOutput != ElementType.OxygenSystem);


        // Determine smallest movement

        return returner(grid, startingLocation);
    }

    public enum ElementType : byte
    {
        Wall,
        Empty,
        OxygenSystem,
        Undiscovered = 1 << 2,
    }

    private delegate int Returner(ShipSection grid, Location2D startingLocation);

    public sealed class ShipSection : PrintableGrid2D<ElementType>
    {
        public ShipSection(int both) : base(both) { }
        public ShipSection(int width, int height) : base(width, height) { }

        protected override Dictionary<ElementType, char> GetPrintableCharacters()
        {
            return new Dictionary<ElementType, char>
                {
                    { ElementType.Wall , '.' },
                    { ElementType.Empty , '#' },
                    { ElementType.OxygenSystem , 'O' },
                    { ElementType.Undiscovered , ' ' },
                };
        }
    }
}
