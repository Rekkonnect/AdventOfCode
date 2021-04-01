using AdventOfCode.Utilities.TwoDimensions;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day12 : Problem<int>
    {
        private ShipCommand[] commands;

        public override int SolvePart1()
        {
            return RunCommands<ShipState>();
        }
        public override int SolvePart2()
        {
            return RunCommands<ShipWaypointState>();
        }

        private int RunCommands<T>()
            where T : ShipState, new()
        {
            var state = new T();

            foreach (var c in commands)
                state.ApplyCommand(c);

            return state.ShipLocation.Location.ManhattanDistanceFromCenter;
        }

        protected override void ResetState()
        {
            commands = null;
        }
        protected override void LoadState()
        {
            commands = FileLines.Select(ShipCommand.Parse).ToArray();
        }

        private class ShipWaypointState : ShipState
        {
            private Location2D waypointOffset;

            public Location2D WaypointOffset => waypointOffset;

            public ShipWaypointState()
                : base()
            {
                // Do it like that to avoid issues with hard-coding
                waypointOffset.Forward(Direction.East, 10);
                waypointOffset.Forward(Direction.North, 1);
            }

            public override void ApplyCommand(ShipCommand command)
            {
                switch (command.Type)
                {
                    case ShipCommandType.North:
                    case ShipCommandType.South:
                    case ShipCommandType.East:
                    case ShipCommandType.West:
                        waypointOffset += new DirectionalLocation((Direction)command.Type).LocationOffset * command.Value;
                        break;
                    case ShipCommandType.Left:
                        waypointOffset.TurnLeftAroundCenter(command.Value / 90);
                        break;
                    case ShipCommandType.Right:
                        waypointOffset.TurnRightAroundCenter(command.Value / 90);
                        break;
                    case ShipCommandType.Forward:
                        ShipLocation.Location += waypointOffset * command.Value;
                        break;
                }
            }

            public override string ToString() => $"Ship: {ShipLocation} - Waypoint Offset: {WaypointOffset}";
        }

        private class ShipState
        {
            public LocationWithDirection ShipLocation { get; }

            public ShipState()
            {
                ShipLocation = new(Direction.East);
            }

            public virtual void ApplyCommand(ShipCommand command)
            {
                switch (command.Type)
                {
                    case ShipCommandType.North:
                    case ShipCommandType.South:
                    case ShipCommandType.East:
                    case ShipCommandType.West:
                        AdjustShipLocation((Direction)command.Type, command.Value);
                        break;
                    case ShipCommandType.Left:
                        ShipLocation.TurnLeft(command.Value / 90);
                        break;
                    case ShipCommandType.Right:
                        ShipLocation.TurnRight(command.Value / 90);
                        break;
                    case ShipCommandType.Forward:
                        AdjustShipLocation(ShipLocation.FacedDirection.Direction, command.Value);
                        break;
                }
            }

            protected void AdjustShipLocation(Direction direction, int value)
            {
                ShipLocation.Location += new DirectionalLocation(direction).LocationOffset * value;
            }

            public override string ToString() => $"{ShipLocation}";
        }
        private struct ShipCommand
        {
            public ShipCommandType Type { get; init; }
            public int Value { get; init; }

            public static ShipCommand Parse(string command)
            {
                return new()
                {
                    Type = ParseCommandType(command[0]),
                    Value = int.Parse(command[1..])
                };
            }
            private static ShipCommandType ParseCommandType(char c)
            {
                return c switch
                {
                    'N' => ShipCommandType.North,
                    'S' => ShipCommandType.South,
                    'E' => ShipCommandType.East,
                    'W' => ShipCommandType.West,
                    'L' => ShipCommandType.Left,
                    'R' => ShipCommandType.Right,
                    'F' => ShipCommandType.Forward,
                };
            }

            public override string ToString() => $"{Type} {Value}";
        }
        private enum ShipCommandType
        {
            // Use Direction's values to allow conversion between the enums without issues
            North = Direction.North,
            South = Direction.South,
            East = Direction.East,
            West = Direction.West,
            Left = 4,
            Right = 5,
            Forward = 6
        }
    }
}
