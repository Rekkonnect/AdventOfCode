using AdventOfCode.Utilities.TwoDimensions;
using Garyon.Extensions;

namespace AdventOfCode.Problems.Year2018;

[SolutionInfo(SolutionFlags.Part2Unoptimized)]
public class Day11 : Problem<Location2D, Day11.GridSquare>
{
    private FuelGrid grid;

    public override Location2D SolvePart1()
    {
        return grid.GetMostPowerfulSquare(3).TopLeftLocation;
    }
    public override GridSquare SolvePart2()
    {
        return grid.GetMostPowerfulSquareAnySize();
    }

    protected override void LoadState()
    {
        grid = new(FileContents.ParseInt32());
    }
    protected override void ResetState()
    {
        grid = null;
    }

    public struct GridSquare
    {
        public static readonly GridSquare LeastPowerful = new(Location2D.Zero, 0, int.MinValue);

        public Location2D TopLeftLocation { get; }
        public int Size { get; }
        public int Power { get; }

        public GridSquare(Location2D topLeft, int size, int power)
        {
            TopLeftLocation = topLeft;
            Size = size;
            Power = power;
        }

        public override string ToString()
        {
            return $"{TopLeftLocation.X},{TopLeftLocation.Y},{Size}";
        }
    }

    private class FuelGrid : Grid2D<int>
    {
        public const int GridSize = 300;

        private int serialNumber;

        public FuelGrid(int serialNumber)
            : base(GridSize)
        {
            this.serialNumber = serialNumber;
            FillGrid();
        }

        public GridSquare GetMostPowerfulSquareAnySize()
        {
            var mostPowerful = GridSquare.LeastPowerful;

            const int maxRemainingEvaluations = 2;
            int remainingEvaluations = maxRemainingEvaluations;

            for (int i = 3; i < 300 && remainingEvaluations > 0; i++)
            {
                var powerfulForSize = GetMostPowerfulSquare(i);

                if (powerfulForSize.Power > mostPowerful.Power)
                {
                    mostPowerful = powerfulForSize;
                    remainingEvaluations = maxRemainingEvaluations;
                }
                else
                    remainingEvaluations--;
            }

            return mostPowerful;
        }

        public GridSquare GetMostPowerfulSquare(int size)
        {
            int maxPower = int.MinValue;
            var maxLocation = Location2D.Zero;

            foreach (var location in Location2D.EnumerateRectangleLocations(Location2D.Zero, Dimensions - (size, size)))
            {
                int powerSum = 0;

                for (int x0 = 0; x0 < size; x0++)
                    for (int y0 = 0; y0 < size; y0++)
                    {
                        powerSum += this[location + (x0, y0)];
                    }

                if (powerSum <= maxPower)
                    continue;

                maxPower = powerSum;
                maxLocation = location;
            }

            return new(maxLocation + (1, 1), size, maxPower);
        }

        private void FillGrid()
        {
            foreach (var location in EnumerateWholeGridLocations())
            {
                int rackID = location.X + 1 + 10;
                int powerLevel = rackID * (location.Y + 1) + serialNumber;
                powerLevel *= rackID;
                powerLevel %= 1000;
                powerLevel /= 100;
                powerLevel -= 5;

                this[location] = powerLevel;
            }
        }
    }
}
