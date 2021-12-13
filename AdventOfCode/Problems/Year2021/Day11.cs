#nullable enable

using System.Runtime.CompilerServices;

namespace AdventOfCode.Problems.Year2021;

public class Day11 : Problem<int>
{
    private OctopiGrid? octopi;

    public override int SolvePart1()
    {
        var cloned = octopi!.Clone();
        cloned.IterateSteps(100);
        return cloned.Flashes;
    }
    public override int SolvePart2()
    {
        var cloned = octopi!.Clone();
        return cloned.IterateUntilAllFlash();
    }

    protected override void LoadState()
    {
        octopi = OctopiGrid.Parse(FileLines);
    }
    protected override void ResetState()
    {
        octopi = null;
    }

    private class OctopiGrid
    {
        private readonly int[,] octopi;

        public int Flashes { get; private set; }

        private OctopiGrid(int[,] energyLevels)
        {
            octopi = energyLevels;
        }

        public OctopiGrid Clone() => new(octopi.Clone() as int[,]);

        public int IterateUntilAllFlash()
        {
            int step = 0;
            while (true)
            {
                Iterate();
                step++;

                if (HaveAllRecentlyFlashed())
                    return step;
            }
        }
        private unsafe bool HaveAllRecentlyFlashed()
        {
            fixed (int* energyLevelsPointer = octopi)
            {
                for (int i = 0; i < octopi.Length; i++)
                {
                    if (energyLevelsPointer[i] != 0)
                        return false;
                }
            }
            return true;
        }

        public void IterateSteps(int days)
        {
            for (int i = 0; i < days; i++)
                Iterate();
        }
        public unsafe void Iterate()
        {
            for (int x = 0; x < octopi.GetLength(0); x++)
            {
                for (int y = 0; y < octopi.GetLength(1); y++)
                {
                    IncreaseEnergyLevel(x, y);
                }
            }

            fixed (int* energyLevelsPointer = octopi)
            {
                for (int i = 0; i < octopi.Length; i++)
                {
                    int* energyLevelCell = energyLevelsPointer + i;
                    if (*energyLevelCell >= 10)
                        *energyLevelCell = 0;
                }
            }
        }

        private void IncreaseEnergyLevel(int x, int y)
        {
            ref int energyLevel = ref octopi[x, y];

            if (energyLevel >= 10)
                return;

            energyLevel++;

            if (energyLevel == 10)
                Flash(x, y);
        }
        private void Flash(int x, int y)
        {
            Flashes++;

            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                int nextX = x + xOffset;
                if (!IsValidDimensionIndex(nextX, 0))
                    continue;

                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    int nextY = y + yOffset;
                    if (!IsValidDimensionIndex(nextY, 1))
                        continue;

                    IncreaseEnergyLevel(nextX, nextY);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidDimensionIndex(int index, int dimension) => index >= 0 && index < octopi.GetLength(dimension);

        public static OctopiGrid Parse(string[] lines)
        {
            int height = lines.Length;
            int width = lines[0].Length;

            var grid = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = lines[y][x].GetNumericValueInteger();
                }
            }

            return new(grid);
        }
    }
}
