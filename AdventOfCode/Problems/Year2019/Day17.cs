using AdventOfCode.Problems.Year2019.Utilities;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2019;

public class Day17 : Problem<int>
{
    [PartSolution(PartSolutionStatus.Refactoring)]
    public override int SolvePart1() => General(Part1GeneralFunction, Part1Returner);
    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart2() => General(Part2GeneralFunction, Part2Returner);

    private void Part1GeneralFunction(IntcodeComputer computer) { }
    private void Part2GeneralFunction(IntcodeComputer computer)
    {
        computer.SetStaticMemoryAt(0, 2);
    }

    private int Part1Returner(SpaceGrid grid, int dust)
    {
        int result = 0;

        for (int x = 0; x < grid.Width; x++)
            for (int y = 0; y < grid.Height; y++)
                if (grid[x, y] == ElementType.Intersection)
                    result += x * y;

        return result;
    }
    private int Part2Returner(SpaceGrid grid, int dust) => dust;

    private int General(GeneralFunction beforeOperation, Returner returner)
    {
        var builder = new StringBuilder();
        var commands = new CommandList();
        string inputString = null;
        int currentInputIndex = 0;

        SpaceGrid grid = null;

        var computer = new IntcodeComputer(FileContents);
        computer.InputRequested += InputRequested;
        computer.OutputWritten += OutputWritten;
        beforeOperation(computer);
        int dust = (int)computer.RunToHalt();

        InitializeGrid();

        return returner(grid, dust);

        long InputRequested()
        {
            InitializeGrid();
            char c = inputString[currentInputIndex];
            currentInputIndex++;
            return c;
        }
        void OutputWritten(long output)
        {
            char charput = (char)(int)output;
            if (charput == (char)10 || ParseElementType(charput) != ElementType.Invalid)
                builder.Append(charput);
        }

        void InitializeGrid()
        {
            if (grid == null)
            {
                grid = SpaceGrid.Parse(builder.ToString());
                grid.PrintGrid();
                AnalyzePath();
            }
        }
        void AnalyzePath()
        {
            bool reachedEnd = false;
            while (!reachedEnd)
            {
                Command command = null;
                var direction = new DirectionalLocation(grid.CurrentRobotDirection, false, true);
                var velocity = direction.LocationOffset;

                int movement = 0;
                bool canMoveForward;
                do
                {
                    movement++;
                    canMoveForward = CanMoveTo(grid.CurrentRobotLocation + movement * velocity);
                }
                while (canMoveForward);
                movement--;

                if (movement == 0)
                {
                    var left = direction;
                    left.TurnLeft();
                    var right = direction;
                    right.TurnRight();

                    if (CanMoveTo(grid.CurrentRobotLocation + left.LocationOffset))
                        SetCommand(new TurnLeftCommand(), left);
                    else if (CanMoveTo(grid.CurrentRobotLocation + right.LocationOffset))
                        SetCommand(new TurnRightCommand(), right);
                }
                else
                {
                    command = new MoveForwardCommand(movement);
                    grid.CurrentRobotLocation += movement * velocity;
                }

                if (!(reachedEnd = command == null))
                    commands.Add(command);

                bool CanMoveTo(Location2D newLocation) => grid.IsValidLocation(newLocation) && grid[newLocation] == ElementType.Scaffold;
                void SetCommand(Command newCommand, DirectionalLocation newDirection)
                {
                    command = newCommand;
                    direction = newDirection;
                    grid.CurrentRobotDirection = newDirection.Direction;
                }
            }

            inputString = commands.GetMainFunction().GetInputString();
        }
    }

    private static ElementType ParseElementType(char c)
    {
        return c switch
        {
            '^' => ElementType.VacuumRobot,
            '>' => ElementType.VacuumRobot,
            'v' => ElementType.VacuumRobot,
            '<' => ElementType.VacuumRobot,
            'X' => ElementType.VacuumRobot,
            '#' => ElementType.Scaffold,
            '.' => ElementType.Void,
            'O' => ElementType.Intersection,
            _ => ElementType.Invalid,
        };
    }

    private enum ElementType : byte
    {
        Void,
        Scaffold,
        Intersection,
        VacuumRobot,
        Invalid,
    }

    private delegate void GeneralFunction(IntcodeComputer computer);
    private delegate int Returner(SpaceGrid grid, int dust);

    // THE best usage of OOP provided to you by the one and only best developer in the entire planet while having just turned 19
    // // I WILL "regret" this comment
    private abstract class Command : IEquatable<Command>
    {
        public abstract Location2D ChangeLocation(Location2D location, Direction direction);
        public abstract Direction ChangeDirection(Direction direction);

        public virtual bool Equals(Command other) => GetType() == other.GetType();
    }
    private abstract class DirectionRotationCommand : Command
    {
        public sealed override Location2D ChangeLocation(Location2D location, Direction direction) => location;
    }
    private sealed class TurnLeftCommand : DirectionRotationCommand
    {
        public override Direction ChangeDirection(Direction direction) => new DirectionalLocation(direction).TurnLeft();

        public override string ToString() => "L";
    }
    private sealed class TurnRightCommand : DirectionRotationCommand
    {
        public override Direction ChangeDirection(Direction direction) => new DirectionalLocation(direction).TurnRight();

        public override string ToString() => "R";
    }
    private sealed class MoveForwardCommand : Command
    {
        public readonly int Movement;

        public MoveForwardCommand(int movement) => Movement = movement;

        public override Location2D ChangeLocation(Location2D location, Direction direction) => location + new DirectionalLocation(direction).LocationOffset * Movement;
        public override Direction ChangeDirection(Direction direction) => direction;

        public override string ToString() => $"{Movement}";

        public override bool Equals(Command other) => base.Equals(other) && Movement == (other as MoveForwardCommand).Movement;
    }

    private class MainFunction
    {
        public readonly CommandFunction A;
        public readonly CommandFunction B;
        public readonly CommandFunction C;

        public List<CommandFunction> Functions;

        public MainFunction(CommandList a, CommandList b, CommandList c)
        {
            A = new CommandFunction("A", a);
            B = new CommandFunction("B", b);
            C = new CommandFunction("C", c);
        }

        public void AddFunction(CommandFunction f) => Functions.Add(f);

        public string GetInputString() => $"{this}\n{A}\n{B}\n{C}\n".Replace('\n', (char)10);

        public override string ToString() => Functions.Select(f => f.Name).Aggregate((a, b) => $"{a},{b}");

        public CommandFunction this[int index]
        {
            get
            {
                return index switch
                {
                    1 => A,
                    2 => B,
                    3 => C,
                };
            }
        }
    }
    private class CommandFunction
    {
        public readonly string Name;
        public readonly CommandList Commands;

        public CommandFunction(string name, CommandList commands) => (Name, Commands) = (name, commands);
    }
    private class CommandList : List<Command>
    {
        public CommandList()
            : base(10) { }

        public void AddWithoutExceedingLimit(Command command)
        {
            Add(command);
            if (ToString().Length > 20)
                Remove(command);
        }

        public CommandList SubList(int startIndex, int count)
        {
            var result = new CommandList();
            for (int i = startIndex; i < startIndex + count; i++)
                result.Add(this[i]);
            return result;
        }

        public MainFunction GetMainFunction()
        {
            var lists = new List<CommandList>(3);
            var functionCalls = new int[Count];

            bool[] covered = new bool[Count];
            var commonCommandsStartIndices = new List<int>(Count);

            // TODO: Create a custom class to support different cases and analyze them separately; I'm fucking done

            for (int i = 0; i < Count; i += 2)
            {
                if (covered[i])
                    continue;

                int currentLength = 0;

                for (int j = 0; j < Count; j++)
                    if (!covered[j])
                        commonCommandsStartIndices.Add(j);

                while (true)
                {
                    currentLength += 2;
                    commonCommandsStartIndices.Remove(Count - currentLength);
                    commonCommandsStartIndices.Remove(Count - currentLength + 1);

                    var removedCommandsStartIndices = new List<int>(commonCommandsStartIndices);

                    for (int j = 0; j < removedCommandsStartIndices.Count - 1; j++)
                        if (removedCommandsStartIndices[j + 1] - removedCommandsStartIndices[j] < currentLength)
                        {
                            removedCommandsStartIndices.RemoveAt(j + 1);
                            j--;
                        }

                    if (i + currentLength < Count)
                        for (int j = removedCommandsStartIndices.Count - 1; j >= 0; j -= 2)
                            if (!this[i + currentLength - 1].Equals(this[removedCommandsStartIndices[j] + currentLength - 1]) ||
                                !this[i + currentLength].Equals(this[removedCommandsStartIndices[j] + currentLength]))
                                removedCommandsStartIndices.RemoveAt(j);

                    if (removedCommandsStartIndices.Count == 1 || currentLength == 6 || i + currentLength >= Count)
                    {
                        if (currentLength < 6)
                            currentLength -= 2;

                        if (currentLength < 2)
                            break;

                        lists.Add(SubList(i, currentLength));
                        foreach (var j in commonCommandsStartIndices)
                        {
                            functionCalls[j] = lists.Count;
                            for (int k = 0; k < currentLength; k++)
                                covered[j + k] = true;
                        }

                        break;
                    }
                    else
                        commonCommandsStartIndices = removedCommandsStartIndices;
                }

                commonCommandsStartIndices.Clear();
            }

            var resultingFunctions = functionCalls.ToList();
            resultingFunctions.RemoveAll(i => i == 0);

            var main = new MainFunction(lists[0], lists[1], lists[2]);
            main.Functions = resultingFunctions.Select(i => main[i]).ToList();
            return main;
        }

        public override string ToString() => this.Select(c => c.ToString()).Aggregate((a, b) => $"{a},{b}");
    }

    private sealed class SpaceGrid : PrintableGrid2D<ElementType>
    {
        public Location2D CurrentRobotLocation { get; set; }
        public Direction CurrentRobotDirection { get; set; }

        public SpaceGrid(int both)
            : base(both) { }
        public SpaceGrid(int width, int height)
            : base(width, height) { }

        public override char GetPrintableCharacter(ElementType value)
        {
            return value switch
            {
                ElementType.Void => '.',
                ElementType.Scaffold => '#',
                ElementType.Intersection => 'O',
                ElementType.VacuumRobot => '^',
            };
        }

        public static SpaceGrid Parse(string s)
        {
            var lines = s.GetLines();
            var k = lines.ToList();
            k.RemoveAll(l => l == "");
            lines = k.ToArray();
            var grid = new SpaceGrid(lines[0].Length, lines.Length);
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if ((grid[x, y] = ParseElementType(lines[y][x])) == ElementType.VacuumRobot)
                    {
                        grid.CurrentRobotLocation = (x, y);
                        grid.CurrentRobotDirection = CommonParsing.ParseDirectionArrow(lines[y][x]);
                    }
                }
            }

            return grid;
        }
    }
}
