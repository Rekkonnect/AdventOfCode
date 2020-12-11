using System.Collections;
using System.Collections.Generic;
using static System.Convert;

namespace AdventOfCode.Utilities.TwoDimensions
{
    public abstract class Grid<T> : IEnumerable<T>
    {
        protected T[,] Values;

        public readonly ValueCounterDictionary<T> ValueCounters;
        public readonly int Width, Height;

        public Location2D Dimensions => (Width, Height);
        public Location2D Center => Dimensions / 2;

        public FlexibleDictionary<T, List<Location2D>> ElementDictionary
        {
            get
            {
                var result = new FlexibleDictionary<T, List<Location2D>>();
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        result[Values[x, y]].Add((x, y));
                return result;
            }
        }

        protected Grid(int width, int height, T defaultValue, bool initializeValueCounters)
        {
            Values = new T[Width = width, Height = height];
            if (!defaultValue.Equals(default(T)))
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        Values[x, y] = defaultValue;

            if (initializeValueCounters)
                ValueCounters = new ValueCounterDictionary<T>(Values);
        }

        public Grid(int both)
            : this(both, both, default) { }
        public Grid(int both, T defaultValue)
            : this(both, both, defaultValue) { }
        public Grid(int width, int height)
            : this(width, height, default) { }
        public Grid(int width, int height, T defaultValue)
            : this(width, height, defaultValue, true) { }
        public Grid(Grid<T> other)
            : this(other.Width, other.Height, default, false)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Values[x, y] = other.Values[x, y];

            ValueCounters = new ValueCounterDictionary<T>(other.ValueCounters);
        }

        public Location2D GetUniqueElementLocation(T element)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (Values[x, y].Equals(element))
                        return (x, y);
            return (-1, -1);
        }
        public List<Location2D> GetLocationsOfElement(T element)
        {
            var result = new List<Location2D>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (Values[x, y].Equals(element))
                        result.Add((x, y));
            return result;
        }

        public void ReplaceWithIntersectionValues(T original, T intersection)
        {
            for (int x = 1; x < Width - 1; x++)
                for (int y = 1; y < Height - 1; y++)
                    if (Values[x, y].Equals(original) && GetAdjacentValues((x, y)) == 4)
                        Values[x, y] = intersection;
        }

        #region Adjacency
        public int GetAdjacentValues(Location2D location) => GetAdjacentValues(location.X, location.Y);
        public int GetAdjacentValues(Location2D location, T value) => GetAdjacentValues(location.X, location.Y, value);
        public int GetAdjacentValues(int x, int y) => GetAdjacentValues(x, y, Values[x, y]);
        public virtual int GetAdjacentValues(int x, int y, T value)
        {
            int result = 0;

            if (x - 1 >= 0)
                result += ToInt32(value.Equals(Values[x - 1, y]));
            if (x + 1 < Width)
                result += ToInt32(value.Equals(Values[x + 1, y]));
            if (y - 1 >= 0)
                result += ToInt32(value.Equals(Values[x, y - 1]));
            if (y + 1 < Height)
                result += ToInt32(value.Equals(Values[x, y + 1]));

            return result;
        }

        public int GetAdjacentValuesWithDiagonals(Location2D location) => GetAdjacentValuesWithDiagonals(location.X, location.Y);
        public int GetAdjacentValuesWithDiagonals(Location2D location, T value) => GetAdjacentValuesWithDiagonals(location.X, location.Y, value);
        public int GetAdjacentValuesWithDiagonals(int x, int y) => GetAdjacentValuesWithDiagonals(x, y, Values[x, y]);
        public virtual int GetAdjacentValuesWithDiagonals(int x, int y, T value)
        {
            int result = GetAdjacentValues(x, y, value);

            // If only there was a convenient way to ignore out of bounds exceptions
            if (x - 1 >= 0)
            {
                if (y - 1 >= 0)
                    result += ToInt32(value.Equals(Values[x - 1, y - 1]));
                if (y + 1 < Height)
                    result += ToInt32(value.Equals(Values[x - 1, y + 1]));
            }
            if (x + 1 < Width)
            {
                if (y - 1 >= 0)
                    result += ToInt32(value.Equals(Values[x + 1, y - 1]));
                if (y + 1 < Height)
                    result += ToInt32(value.Equals(Values[x + 1, y + 1]));
            }

            return result;
        }
        #endregion

        public int GetMedianXOfFirstRegion(int y, T regionValue)
        {
            int x0 = -1;
            int x1 = -1;

            for (int x = 0; x0 == -1 && x < Width; x++)
                if (Values[x, y].Equals(regionValue))
                    x0 = x;

            if (x0 == -1)
                return -1;

            for (int x = x0; x1 == -1 && x < Width; x++)
                if (!Values[x, y].Equals(regionValue))
                    x1 = x - 1;

            return (x0 + x1) / 2;
        }

        public List<Direction> GetShortestPath(Location2D start, Location2D end)
        {
            var d = new List<Direction>();
            List<Direction> resultingDirections = null;

            var grid = new int[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    grid[x, y] = int.MaxValue;

            AnalyzeGridDepth(start, 0);

            return resultingDirections;

            void AnalyzeGridDepth(Location2D location, int depth)
            {
                if (!IsValidLocation(location))
                    return;
                if (depth > grid[end.X, end.Y])
                    return;

                var (x1, y1) = location;
                if (!IsImpassableObject(Values[x1, y1]))
                {
                    if (grid[x1, y1] < depth)
                        return;

                    grid[x1, y1] = depth;

                    if (location == end)
                        resultingDirections = new List<Direction>(d);

                    var currentDirection = new DirectionalLocation(Direction.Right);
                    for (int i = 0; i < 4; i++)
                    {
                        d.Add(currentDirection.Direction);
                        AnalyzeGridDepth(location + currentDirection.LocationOffset, depth);
                        d.RemoveAt(d.Count - 1);

                        currentDirection.TurnRight();
                    }
                }
            }
        }

        public bool IsValidLocation(Location2D location) => location.IsNonNegative && location.X < Width && location.Y < Height;

        protected virtual bool IsImpassableObject(T element) => false;

        public IEnumerator<T> GetEnumerator()
        {
            // Is that how it should be done?
            foreach (T v in Values)
                yield return v;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual T this[int x, int y]
        {
            get => Values[x, y];
            set
            {
                ValueCounters.AdjustValue(Values[x, y], value);
                Values[x, y] = value;
            }
        }
        public T this[Location2D location]
        {
            get => this[location.X, location.Y];
            set => this[location.X, location.Y] = value;
        }
    }
}
