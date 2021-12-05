using AdventOfCode.Utilities.TwoDimensions;
using AdventOfCSharp;

namespace AdventOfCode.Problems.Year2015;

public class Day18 : Problem<int>
{
    private LightGrid defaultGrid;

    public override int SolvePart1()
    {
        return defaultGrid.RunSteps(100, false).TurnedOnLights;
    }
    public override int SolvePart2()
    {
        return defaultGrid.RunSteps(100, true).TurnedOnLights;
    }

    protected override void LoadState()
    {
        var lines = FileLines;
        defaultGrid = new();
        for (int y = 0; y < defaultGrid.Height; y++)
        {
            for (int x = 0; x < defaultGrid.Width; x++)
                defaultGrid[x, y] = ParseState(lines[y][x]);
        }
    }
    protected override void ResetState()
    {
        defaultGrid = null;
    }

    private static LightState ParseState(char c)
    {
        return c switch
        {
            '#' => LightState.On,
            _ => LightState.Off,
        };
    }

    private class LightGrid : Grid2D<LightState>
    {
        public int TurnedOnLights => ValueCounters[LightState.On];

        public LightGrid()
            : base(100, 100) { }

        public LightGrid RunSteps(int steps, bool stuckLights)
        {
            var current = this;
            for (int i = 0; i < steps; i++)
                current = current.RunStep(stuckLights);
            return current;
        }
        public LightGrid RunStep(bool stuckLights)
        {
            var result = new LightGrid();

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    int neighbors = GetNeighborValues(x, y, LightState.On);
                    var value = this[x, y];
                    result[x, y] = (value, neighbors) switch
                    {
                        (LightState.Off, 3) => LightState.On,
                        (LightState.On, 2 or 3) => LightState.On,
                        _ => LightState.Off,
                    };
                }

            if (stuckLights)
            {
                result[0, 0] = LightState.On;
                result[0, result.Height - 1] = LightState.On;
                result[result.Width - 1, result.Height - 1] = LightState.On;
                result[result.Width - 1, 0] = LightState.On;
            }

            return result;
        }
    }
    private enum LightState
    {
        Off,
        On,
    }
}
