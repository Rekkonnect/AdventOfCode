#nullable enable


namespace AdventOfCode.Problems.Year2021;

public class Day7 : Problem<int>
{
    private Crabs? crabs;

    public override int SolvePart1()
    {
        return crabs.GetMinLinearFuel();
    }
    public override int SolvePart2()
    {
        return crabs.GetMinExponentialFuel();
    }

    protected override void LoadState()
    {
        crabs = Crabs.Parse(FileContents);
    }
    protected override void ResetState()
    {
        crabs = null;
    }

    private class Crabs
    {
        private readonly int[] positions;

        private Crabs(int[] sortedPositions)
        {
            positions = sortedPositions;
        }

        public int GetMinLinearFuel() => GetLinearFuel(GetMinLinearFuelPosition());
        public int GetLinearFuel(int alignmentPosition)
        {
            return positions.Select(position => Math.Abs(position - alignmentPosition)).Sum();
        }
        private int GetMinLinearFuelPosition()
        {
            return positions[positions.Length / 2];
        }
        // This solution was constructed in accordance to the renowned paper
        // by some member in the community
        public int GetMinExponentialFuel()
        {
            int midPosition = GetMinExponentialFuelPosition();
            int min = int.MaxValue;
            for (int testOffset = -1; testOffset <= 1; testOffset++)
            {
                int fuel = GetExponentialFuel(midPosition + testOffset);
                if (fuel < min)
                    min = fuel;
            }
            return min;
        }
        public int GetExponentialFuel(int alignmentPosition)
        {
            return positions.Select(initial => GetExponentialFuelExpenditure(alignmentPosition, initial)).Sum();
        }
        private int GetMinExponentialFuelPosition()
        {
            return (int)Math.Round(positions.Average());
        }
        private static int GetExponentialFuelExpenditure(int alignmentPosition, int initialPosition)
        {
            int difference = alignmentPosition - initialPosition;
            int square = difference * difference;
            return (square + Math.Abs(difference)) / 2;
        }

        public static Crabs Parse(string rawPositions)
        {
            var positions = rawPositions.Split(',').Select(int.Parse).ToArray();
            return new(positions.Sort());
        }
    }
}
