using AdventOfCode.Functions;
using AdventOfCode.Utilities.FourDimensions;
using AdventOfCode.Utilities.ThreeDimensions;
using AdventOfCSharp.Utilities;
using Garyon.Exceptions;
using static System.Convert;

namespace AdventOfCode.Utilities.TwoDimensions;

public class Grid2D<T> : BaseGrid<T, Location2D>, IEquatable<Grid2D<T>>
{
    protected T[,] Values;

    public readonly int Width, Height;

    // TODO: Abstract in BaseGrid
    public int TotalElements => Width * Height;

    public override Location2D Dimensions => (Width, Height);
    public override Location2D Center => Dimensions / 2;

    public Location2D EndLocation => Dimensions - (1, 1);

    public override FlexibleListDictionary<T, Location2D> ElementDictionary
    {
        get
        {
            var result = new FlexibleListDictionary<T, Location2D>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    result[Values[x, y]].Add((x, y));
            return result;
        }
    }

    #region Dimension Transformations
    public Grid3D<T> As3D
    {
        get
        {
            var result = new Grid3D<T>(Width, Height, 1);

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    result[x, y, 0] = Values[x, y];

            return result;
        }
    }
    public Grid4D<T> As4D
    {
        get
        {
            var result = new Grid4D<T>(Width, Height, 1, 1);

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    result[x, y, 0, 0] = Values[x, y];

            return result;
        }
    }
    #endregion

    #region Constructors
    protected Grid2D(Location2D dimensions, T defaultValue, bool initializeValueCounters)
        : this(dimensions.X, dimensions.Y, defaultValue, initializeValueCounters) { }
    protected Grid2D(int both, bool initializeValueCounters)
        : this(both, both, default, initializeValueCounters) { }
    protected Grid2D(int both, T defaultValue, bool initializeValueCounters)
        : this(both, both, defaultValue, initializeValueCounters) { }
    protected Grid2D(int width, int height, T defaultValue, bool initializeValueCounters)
    {
        Values = new T[Width = width, Height = height];
        if (!defaultValue.Equals(default(T)))
        {
            Values.AsSpan().Fill(defaultValue);
        }

        if (initializeValueCounters)
            ValueCounters = new(Values.Cast<T>());
    }
    protected Grid2D(int width, int height, T defaultValue, NextValueCounterDictionary<T> valueCounters)
        : this(width, height, defaultValue, false)
    {
        ValueCounters = new(valueCounters);
    }

    public Grid2D(int both)
        : this(both, both, default) { }
    public Grid2D(int both, T defaultValue)
        : this(both, both, defaultValue) { }
    public Grid2D(Location2D dimensions)
        : this(dimensions, default) { }
    public Grid2D(Location2D dimensions, T defaultValue)
        : this(dimensions.X, dimensions.Y, defaultValue) { }
    public Grid2D(int width, int height)
        : this(width, height, default) { }
    public Grid2D(int width, int height, T defaultValue)
        : this(width, height, defaultValue, true) { }
    public Grid2D(Grid2D<T> other)
        : this(other.Width, other.Height, default, false)
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                Values[x, y] = other.Values[x, y];

        ValueCounters = new(other.ValueCounters);
    }
    public Grid2D(Grid2D<T> other, Location2D dimensions, Location2D offset)
        : this(dimensions, default, false)
    {
        for (int x = 0; x < other.Width; x++)
            for (int y = 0; y < other.Height; y++)
                this[offset + (x, y)] = other.Values[x, y];

        ValueCounters = new(other.ValueCounters);
        ValueCounters.Add(default, dimensions.ValueProduct - other.Dimensions.ValueProduct);
    }
    #endregion

    protected virtual Grid2D<T> InitializeClone() => new(Height, Width, default, ValueCounters);

    public T AccessibleValueOrDefault(int x, int y) => IsValidLocation(x, y) ? Values[x, y] : default;

    public T[] GetXLine(int y)
    {
        var result = new T[Width];
        for (int x = 0; x < Width; x++)
            result[x] = Values[x, y];
        return result;
    }
    public T[] GetYLine(int x)
    {
        var result = new T[Height];
        for (int y = 0; y < Height; y++)
            result[y] = Values[x, y];
        return result;
    }
    public T[] GetXLine(Index y) => GetXLine(y.GetOffset(Height));
    public T[] GetYLine(Index x) => GetYLine(x.GetOffset(Width));

    public void SetXLine(int y, T[] values)
    {
        if (values.Length != Width)
            ThrowHelper.Throw<ArgumentException>("The values must match the grid's width.");

        for (int x = 0; x < Width; x++)
            this[x, y] = values[x];
    }
    public void SetYLine(int x, T[] values)
    {
        if (values.Length != Height)
            ThrowHelper.Throw<ArgumentException>("The values must match the grid's height.");

        for (int y = 0; y < Height; y++)
            this[x, y] = values[y];
    }
    public void SetXLine(Index y, T[] values) => SetXLine(y.GetOffset(Width), values);
    public void SetYLine(Index x, T[] values) => SetYLine(x.GetOffset(Height), values);

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

    #region Transformations
    public virtual Grid2D<T> FlipHorizontally()
    {
        var result = InitializeClone();
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                result[^(x + 1), y] = Values[x, y];
        return result;
    }
    public virtual Grid2D<T> FlipVertically()
    {
        var result = InitializeClone();
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                result[x, ^(y + 1)] = Values[x, y];
        return result;
    }

    public virtual Grid2D<T> RotateClockwise(int turns = 1)
    {
        turns %= 4;

        if (turns == 0)
            return this;

        if (turns < 0)
            return RotateCounterClockwise(-turns);

        var result = InitializeClone();

        switch (turns)
        {
            case 1:
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        result[^(y + 1), x] = Values[x, y];
                return result;

            case 2:
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        result[^(x + 1), ^(y + 1)] = Values[x, y];
                return result;

            case 3:
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        result[y, ^(x + 1)] = Values[x, y];
                return result;
        }

        return result;
    }
    public virtual Grid2D<T> RotateCounterClockwise(int turns = 1)
    {
        return RotateClockwise(4 - turns % 4);
    }
    #endregion

    #region Adjacency
    public AdjacentValueSlots<T> GetAdjacentValueSlots(Location2D location) => GetAdjacentValueSlots(location.X, location.Y);
    public AdjacentValueSlots<T> GetAdjacentValueSlots(int x, int y)
    {
        var result = new AdjacentValueSlots<T>();
        GetAdjacentValueSlots(x, y, result);
        return result;
    }
    public void GetAdjacentValueSlots(int x, int y, AdjacentValueSlots<T> slots)
    {
        if (x - 1 >= 0)
            slots.Left = Values[x - 1, y];
        if (x + 1 < Width)
            slots.Right = Values[x + 1, y];
        if (y - 1 >= 0)
            slots.Bottom = Values[x, y - 1];
        if (y + 1 < Height)
            slots.Top = Values[x, y + 1];
    }

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

    public bool HasAdjacentValues(Location2D location, T value) => HasAdjacentValues(location.X, location.Y, value);
    public bool HasAdjacentValues(int x, int y, T value)
    {
        if (x - 1 >= 0)
        {
            if (value.Equals(Values[x - 1, y]))
                return true;
        }
        if (x + 1 < Width)
        {
            if (value.Equals(Values[x + 1, y]))
                return true;
        }
        if (y - 1 >= 0)
        {
            if (value.Equals(Values[x, y - 1]))
                return true;
        }
        if (y + 1 < Height)
        {
            if (value.Equals(Values[x, y + 1]))
                return true;
        }

        return false;
    }

    public int GetNeighborValues(Location2D location) => GetNeighborValues(location.X, location.Y);
    public int GetNeighborValues(Location2D location, T value) => GetNeighborValues(location.X, location.Y, value);
    public int GetNeighborValues(int x, int y) => GetNeighborValues(x, y, Values[x, y]);
    public virtual int GetNeighborValues(int x, int y, T value)
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

    public bool HasNeighborValues(Location2D location, T value) => HasNeighborValues(location.X, location.Y, value);
    public bool HasNeighborValues(int x, int y, T value)
    {
        if (HasAdjacentValues(x, y, value))
            return true;

        // If only there was a convenient way to ignore out of bounds exceptions
        if (x - 1 >= 0)
        {
            if (y - 1 >= 0)
            {
                if (value.Equals(Values[x - 1, y - 1]))
                    return true;
            }
            if (y + 1 < Height)
            {
                if (value.Equals(Values[x - 1, y + 1]))
                    return true;
            }
        }
        if (x + 1 < Width)
        {
            if (y - 1 >= 0)
            {
                if (value.Equals(Values[x + 1, y - 1]))
                    return true;
            }
            if (y + 1 < Height)
            {
                if (value.Equals(Values[x + 1, y + 1]))
                    return true;
            }
        }

        return false;
    }
    #endregion

    public ref T GetUnsafeRef(int x, int y)
    {
        return ref Values[x, y];
    }

    public int[,] GetRegionMap(T value, out int regionCount)
    {
        var map = new int[Width, Height];

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Width; y++)
                map[x, y] = Values[x, y].Equals(value) ? -1 : 0;

        int currentRegionIndex = 1;

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Width; y++)
                if (EnumerateRegion(x, y))
                    currentRegionIndex++;

        regionCount = currentRegionIndex - 1;
        return map;

        bool EnumerateRegion(int x0, int y0)
        {
            if (!IsValidLocation(x0, y0))
                return false;

            if (map[x0, y0] > -1)
                return false;

            map[x0, y0] = currentRegionIndex;
            EnumerateRegion(x0 - 1, y0);
            EnumerateRegion(x0 + 1, y0);
            EnumerateRegion(x0, y0 - 1);
            EnumerateRegion(x0, y0 + 1);
            return true;
        }
    }

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

    public List<Direction> GetShortestPath(Location2D start, Location2D end) => GetShortestPath(start, end, out _);
    public List<Direction> GetShortestPath(Location2D start, Location2D end, out int[,] distanceGrid)
    {
        var d = new List<Direction>();
        List<Direction> resultingDirections = null;

        var grid = new int[Width, Height];
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                grid[x, y] = int.MaxValue;

        AnalyzeGridDepth(start, 0);

        distanceGrid = grid;
        return resultingDirections;

        void AnalyzeGridDepth(Location2D location, int depth)
        {
            if (!IsValidLocation(location))
                return;

            if (depth >= grid[end.X, end.Y])
                return;

            var (x1, y1) = location;
            if (IsImpassableObject(this[location]))
                return;

            if (grid[x1, y1] <= depth)
                return;

            grid[x1, y1] = depth;

            if (location == end)
                resultingDirections = new List<Direction>(d);

            var currentDirection = new DirectionalLocation(Direction.Right);
            for (int i = 0; i < 4; i++)
            {
                d.Add(currentDirection.Direction);
                AnalyzeGridDepth(location + currentDirection.LocationOffset, depth + 1);
                d.RemoveAt(d.Count - 1);

                currentDirection.TurnRight();
            }
        }
    }

    public IEnumerable<Location2D> EnumerateWholeGridLocations() => Location2D.EnumerateRectangleLocations(Location2D.Zero, EndLocation);

    public bool TryGetValue(int x, int y, out T value)
    {
        bool valid = IsValidLocation(x, y);
        value = valid ? this[x, y] : default;
        return valid;
    }
    public bool TryGetValue(Location2D location, out T value)
    {
        return TryGetValue(location.X, location.Y, out value);
    }

    public bool IsValidX(int x) => x >= 0 && x < Width;
    public bool IsValidY(int y) => y >= 0 && y < Height;
    public bool IsValidLocation(int x, int y) => IsValidLocation((x, y));
    public bool IsValidLocation(Location2D location) => location.IsNonNegative && location.X < Width && location.Y < Height;

    protected virtual bool IsImpassableObject(T element) => false;

    public override IEnumerator<T> GetEnumerator() => Values.Cast<T>().GetEnumerator();

    public bool Equals(Grid2D<T> other)
    {
        if (!ValueCounters.Equals(other.ValueCounters))
            return false;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (!Values[x, y].Equals(other.Values[x, y]))
                    return false;
            }
        }

        return true;
    }
    public override bool Equals(object obj)
    {
        return obj is Grid2D<T> other && Equals(other);
    }
    public override int GetHashCode()
    {
        // Do not take too much time to generate the hash code
        return ValueCounters.GetHashCode();
    }

    public virtual T this[Range x, Range y]
    {
        set
        {
            x.GetStartAndEnd(Width, out int startX, out int endX);
            y.GetStartAndEnd(Height, out int startY, out int endY);
            this[startX, startY, endX, endY] = value;
        }
    }
    public virtual T this[int startX, int startY, int endX, int endY]
    {
        set
        {
            for (int x = startX; x < endX; x++)
                for (int y = startY; y < endY; y++)
                    this[x, y] = value;
        }
    }

    public virtual T this[int x, int y]
    {
        get => Values[x, y];
        set
        {
            ValueCounters?.AdjustCounters(Values[x, y], value);
            Values[x, y] = value;
        }
    }
    public virtual T this[Index x, Index y]
    {
        get => this[x.GetOffset(Width), y.GetOffset(Height)];
        set => this[x.GetOffset(Width), y.GetOffset(Height)] = value;
    }
    public override T this[Location2D location]
    {
        get => this[location.X, location.Y];
        set => this[location.X, location.Y] = value;
    }
}
