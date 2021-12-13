using AdventOfCode.Utilities.TwoDimensions;
using static AdventOfCode.Functions.MathFunctions;
using static System.Math;

namespace AdventOfCode.Problems.Year2019;

public class Day10 : Problem<int>
{
    public override int SolvePart1() => General(Part1GeneralFunction);
    public override int SolvePart2() => General(Part2GeneralFunction);

    private int Part1GeneralFunction(AsteroidGrid asteroids, AsteroidGrid bestSolution)
    {
#if DEBUG
        bestSolution.PrintGrid();
#endif
        return bestSolution.AsteroidCount;
    }
    private int Part2GeneralFunction(AsteroidGrid asteroids, AsteroidGrid bestSolution)
    {
#if DEBUG
        asteroids.PrintGrid();
#endif
        if (asteroids.AsteroidCount < 200)
            return 0;

        asteroids.SetBestLocation(false);

        var sorted = new List<SlopedLocation>();
        for (int x = 0; x < asteroids.Width; x++)
            for (int y = 0; y < asteroids.Height; y++)
                if (asteroids[x, y])
                {
                    var location = new Location2D(x, y);
                    var degrees = AddDegrees(asteroids.BestLocation.GetSlopeDegrees(location), -90);

                    var slopedLocation = new SlopedLocation(location, degrees, asteroids.BestLocation);

                    sorted.Add(slopedLocation);
                }

        sorted.Sort();
        int consecutive = 1;
        for (int i = 1; i < sorted.Count; i++)
        {
            if (sorted[i].HasEqualAbsoluteAngle(sorted[i - 1]))
                sorted[i].AddFullCircleRotations(consecutive++);
            else
                consecutive = 1;
        }

        sorted.Sort();
        var l = sorted[199].Location;
        return l.X * 100 + l.Y;
    }

    private int General(GeneralFunction generalFunction)
    {
        var lines = FileLines;

        int height = lines.Length;
        int width = lines[0].Length;

        var asteroids = new AsteroidGrid(width, height);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                asteroids[x, y] = lines[y][x] == '#';

        var bestSolution = new AsteroidGrid(0, 0);
        Location2D bestLocation = (0, 0);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!asteroids[x, y])
                    continue;

                var currentlyVisibleAsteroids = new AsteroidGrid(asteroids);
                currentlyVisibleAsteroids[x, y] = false;

                for (int x0 = 0; x0 < width; x0++)
                    for (int y0 = 0; y0 < height; y0++)
                    {
                        if (!currentlyVisibleAsteroids[x0, y0])
                            continue;

                        int xDelta = x0 - x;
                        int yDelta = y0 - y;
                        SimplifyFraction(ref xDelta, ref yDelta);

                        int multiplier = 1;
                        int x1, y1;
                        bool foundFirst = false;
                        while (IsValidIndex(x1 = x + multiplier * xDelta, width) && IsValidIndex(y1 = y + multiplier * yDelta, height))
                        {
                            if (foundFirst && currentlyVisibleAsteroids[x1, y1])
                                currentlyVisibleAsteroids[x1, y1] = false;
                            foundFirst |= currentlyVisibleAsteroids[x1, y1];
                            multiplier++;
                        }
                    }

                if (currentlyVisibleAsteroids.AsteroidCount > bestSolution.AsteroidCount)
                {
                    bestSolution = currentlyVisibleAsteroids;
                    bestLocation = (x, y);
                }
            }

        bestSolution.BestLocation = asteroids.BestLocation = bestLocation;

        return generalFunction(asteroids, bestSolution);
    }

    private static bool IsValidIndex(int value, int upperBound) => value >= 0 && value < upperBound;

    private delegate int GeneralFunction(AsteroidGrid asteroids, AsteroidGrid bestSolution);

    private class SlopedLocation : IComparable<SlopedLocation>
    {
        private const double epsilon = 0.0000001;

        public Location2D Location { get; }
        public double Angle { get; private set; }
        public int ManhattanDistance { get; }

        public double AbsoluteAngle => Angle % FullCircleDegrees;

        public SlopedLocation(Location2D location, double angle, int manhattanDistance) => (Location, Angle, ManhattanDistance) = (location, angle, manhattanDistance);
        public SlopedLocation(Location2D location, double angle, Location2D other) : this(location, angle, location.ManhattanDistance(other)) { }

        public void AddFullCircleRotations(int count)
        {
            Angle += FullCircleDegrees * count;
        }

        public bool HasEqualAbsoluteAngle(SlopedLocation other) => Abs(AbsoluteAngle - other.AbsoluteAngle) < epsilon;

        public int CompareTo(SlopedLocation other)
        {
            int result = Angle.CompareTo(other.Angle);
            if (result == 0)
                return ManhattanDistance.CompareTo(other.ManhattanDistance);
            return result;
        }

        public override string ToString() => $"{Location} - {Angle}° - {ManhattanDistance}";
    }

    private sealed class AsteroidGrid : PrintableGrid2D<bool>
    {
        public Location2D BestLocation;

        public int AsteroidCount => ValueCounters[true];

        public AsteroidGrid(int width, int height) : base(width, height) { }
        public AsteroidGrid(AsteroidGrid other) : base(other) { }

        protected override Dictionary<bool, char> GetPrintableCharacters()
        {
            return new Dictionary<bool, char>
                {
                    { false, '.' },
                    { true, '#' },
                };
        }

        public void SetBestLocation(bool value) => this[BestLocation] = value;

        public override void PrintGrid()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    if ((x, y) == BestLocation)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write('#');
                        Console.ResetColor();
                        continue;
                    }
                    Console.Write(Values[x, y] ? '#' : '.');
                }
            Console.WriteLine();
        }
    }
}
