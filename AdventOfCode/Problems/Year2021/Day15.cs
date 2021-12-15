using AdventOfCode.Utilities.TwoDimensions;
using System.Diagnostics;

namespace AdventOfCode.Problems.Year2021;

public class Day15 : Problem<int>
{
    private Cave cave;

    public override int SolvePart1()
    {
        return SolvePart(1);
    }
    public override int SolvePart2()
    {
        return SolvePart(5);
    }

    private int SolvePart(int tiles)
    {
        return cave.GetOptimalPath(tiles);
    }

    protected override void LoadState()
    {
        cave = Cave.Parse(FileLines);
    }
    protected override void ResetState()
    {
        cave = null;
    }

    private sealed class Cave : SquareGrid2D<int>
    {
        private Cave(int both)
            : base(both) { }

        public int GetOptimalPath(int tiles)
        {
            return GetOptimalPathPriorityQueue(tiles);
        }

        // PriorityQueue is a life-saving data structure
        private int GetOptimalPathPriorityQueue(int tiles)
        {
            int elongatedSize = Size * tiles;
            int highestRisk = elongatedSize * 2 * 9;
            int best = highestRisk;
            var distanceGrid = new SquareGrid2D<int>(elongatedSize, highestRisk);
            var end = distanceGrid.EndLocation;

            distanceGrid[Location2D.Zero] = 0;

            var priorityQueue = new PriorityQueue<Location2D, int>();
            priorityQueue.Enqueue(Location2D.Zero, 0);

            while (priorityQueue.Count > 0)
            {
                // Will always be dequeued
                priorityQueue.TryDequeue(out var location, out int distance);

                if (location == end)
                {
                    best = Math.Min(best, distance);
                    continue;
                }

                var currentDirection = new DirectionalLocation(Direction.Right);
                for (int i = 0; i < 4; i++, currentDirection.TurnRight())
                {
                    var nextLocation = location + currentDirection.LocationOffset;
                    if (!IsValidLocation(nextLocation, tiles))
                        continue;

                    int nextDistance = distance + this[nextLocation];
                    if (nextDistance >= distanceGrid[nextLocation])
                        continue;

                    distanceGrid[nextLocation] = nextDistance;
                    priorityQueue.Enqueue(nextLocation, nextDistance);
                }
            }

            return best;
        }

        // Overshoots the best path; doesn't evaluate going up or left
        // A fix for this involves evaluating certain previously evaluated cells
        // until everything appears stable; this kind of multiple iteration requires
        // different heuristics that I don't care about
        private int GetOptimalPathDownRightBFS(int tiles)
        {
            const int initialValue = int.MaxValue;
            int elongatedSize = Size * tiles;
            var distanceGrid = new SquareGrid2D<int>(elongatedSize, initialValue);

            distanceGrid[Location2D.Zero] = 0;

            for (int i = 1; i < elongatedSize * 2; i++)
            {
                int jStart = Math.Max(0, i - elongatedSize + 1);
                int jEnd = Math.Min(i, elongatedSize - 1);

                for (int j = jStart; j <= jEnd; j++)
                {
                    int x = j;
                    int y = i - j;

                    // (x, y) should always be within the grid
                    Debug.Assert(IsValidLocation(x, y, tiles));

                    int bestPrevious = initialValue;
                    if (x > 0)
                    {
                        int previous = distanceGrid[x - 1, y];
                        bestPrevious = Math.Min(bestPrevious, previous);
                    }
                    if (y > 0)
                    {
                        int previous = distanceGrid[x, y - 1];
                        bestPrevious = Math.Min(bestPrevious, previous);
                    }

                    int current = this[x, y];
                    distanceGrid[x, y] = bestPrevious + current;
                }
            }

            return distanceGrid[Dimensions * tiles - (1, 1)];
        }

        private bool IsValidLocation(Location2D location, int tiles) => IsValidLocation(location.X, location.Y, tiles);
        private bool IsValidLocation(int x, int y, int tiles)
        {
            int elongatedSize = Size * tiles;
            return x >= 0 && y >= 0 && x < elongatedSize && y < elongatedSize;
        }

        public override int this[int x, int y]
        {
            get
            {
                int xTile = Math.DivRem(x, Size, out int x0);
                int yTile = Math.DivRem(y, Size, out int y0);
                int riskIncrease = xTile + yTile;
                int risk = Values[x0, y0] + riskIncrease;
                if (risk > 9)
                    return risk - 9;
                return risk;
            }
        }

        // I'm starting to feel that parsing grids is far too common a task
        // Maybe I should have this abstracted somewhere?
        public static Cave Parse(string[] levels)
        {
            int size = levels.Length;

            var result = new Cave(size);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    result[x, y] = levels[y][x].GetNumericValueInteger();
                }
            }

            return result;
        }
    }
}
