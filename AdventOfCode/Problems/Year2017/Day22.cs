using AdventOfCode.Utilities.TwoDimensions;
using Garyon.DataStructures;

namespace AdventOfCode.Problems.Year2017;

public class Day22 : Problem<int>
{
    private InfectionGrid infectionGrid;

    public override int SolvePart1()
    {
        return PerformIteration(10000, false);
    }
    public override int SolvePart2()
    {
        return PerformIteration(10000000, true);
    }

    protected override void LoadState()
    {
        infectionGrid = InfectionGrid.Parse(FileLines);
    }
    protected override void ResetState()
    {
        infectionGrid = null;
    }

    private int PerformIteration(int iterations, bool evolved)
    {
        var grid = new InfectionGrid(infectionGrid);
        grid.Iterate(iterations, evolved);
        return grid.InfectionBursts;
    }

    private class InfectionGrid
    {
        private readonly FlexibleDictionary<Location2D, InfectionState> infectionStates = new();

        private DirectionalLocation virusDirection = new(Direction.Up, invertY: true);
        private Location2D virusLocation;

        public int InfectionBursts { get; private set; }

        public InfectionGrid()
        {
            infectionStates = new();
        }
        public InfectionGrid(InfectionGrid other)
        {
            infectionStates = new(other.infectionStates);
        }

        public void Iterate(int times, bool evolved)
        {
            for (int i = 0; i < times; i++)
                Iterate(evolved);
        }
        public void Iterate(bool evolved)
        {
            var originalState = infectionStates[virusLocation];
            var newState = IterateState(originalState, evolved);

            AdjustDirection(originalState);

            switch (newState)
            {
                case InfectionState.Clean:
                    infectionStates.Remove(virusLocation);
                    break;

                case InfectionState.Infected:
                    InfectionBursts++;
                    goto default;
                default:
                    infectionStates[virusLocation] = newState;
                    break;
            }

            virusLocation += virusDirection.LocationOffset;
        }

        private void AdjustDirection(InfectionState originalState)
        {
            switch (originalState)
            {
                case InfectionState.Clean:
                    virusDirection.TurnLeft();
                    break;

                case InfectionState.Weakened:
                    break;

                case InfectionState.Infected:
                    virusDirection.TurnRight();
                    break;

                case InfectionState.Flagged:
                    virusDirection.TurnAround();
                    break;
            }
        }

        private static InfectionState IterateState(InfectionState state, bool evolved)
        {
            return evolved switch
            {
                true => IterateStateEvolved(state),
                false => IterateStateOriginal(state),
            };
        }
        private static InfectionState IterateStateOriginal(InfectionState state)
        {
            return state switch
            {
                InfectionState.Clean => InfectionState.Infected,
                InfectionState.Infected => InfectionState.Clean,
            };
        }
        private static InfectionState IterateStateEvolved(InfectionState state)
        {
            return state switch
            {
                InfectionState.Clean => InfectionState.Weakened,
                InfectionState.Weakened => InfectionState.Infected,
                InfectionState.Infected => InfectionState.Flagged,
                InfectionState.Flagged => InfectionState.Clean,
            };

            return (InfectionState)((int)(state + 1) % (int)InfectionState.StateCount);
        }

        public static InfectionGrid Parse(string[] rows)
        {
            int height = rows.Length;
            int width = rows[0].Length;

            var center = new Location2D(width, height) / 2;

            var result = new InfectionGrid();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (rows[y][x] is '#')
                        result.infectionStates.Add((x, y) - center, InfectionState.Infected);
                }
            }

            return result;
        }
    }

    private enum InfectionState
    {
        Clean,
        Weakened,
        Infected,
        Flagged,

        StateCount
    }
}
