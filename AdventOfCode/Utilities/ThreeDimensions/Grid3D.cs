using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Utilities.ThreeDimensions;

public class Grid3D<T> : BaseGrid<T, Location3D>
{
    #region Statically store the neighbor offsets
    private static readonly Location3D[] neighborOffsets;

    static Grid3D()
    {
        var xOffsets = new Location3D[3];
        var yOffsets = new Location3D[3];
        var zOffsets = new Location3D[3];
        for (int i = -1; i <= 1; i++)
        {
            xOffsets[i + 1] = (i, 0, 0);
            yOffsets[i + 1] = (0, i, 0);
            zOffsets[i + 1] = (0, 0, i);
        }

        var offsets = new List<Location3D>(3 * 3 * 3);

        foreach (var xOffset in xOffsets)
            foreach (var yOffset in yOffsets)
                foreach (var zOffset in zOffsets)
                    offsets.Add(xOffset + yOffset + zOffset);

        neighborOffsets = offsets.ToArray();
    }
    #endregion

    protected T[,,] Values;

    public readonly int Width, Height, Depth;

    public override Location3D Dimensions => (Width, Height, Depth);
    public override Location3D Center => Dimensions / 2;

    public override FlexibleListDictionary<T, Location3D> ElementDictionary
    {
        get
        {
            var result = new FlexibleListDictionary<T, Location3D>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                        result[Values[x, y, z]].Add((x, y, z));
            return result;
        }
    }

    protected Grid3D(Location3D dimensions, T defaultValue, bool initializeValueCounters)
        : this(dimensions.X, dimensions.Y, dimensions.Z, defaultValue, initializeValueCounters) { }
    protected Grid3D(int width, int height, int depth, T defaultValue, bool initializeValueCounters)
    {
        Values = new T[Width = width, Height = height, Depth = depth];
        if (!defaultValue.Equals(default(T)))
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    for (int z = 0; z < depth; z++)
                        Values[x, y, z] = defaultValue;

        if (initializeValueCounters)
        {
            ValueCounters = new();
            ValueCounters.Add(defaultValue, width * height * depth);
        }
    }
    protected Grid3D(int width, int height, int depth, T defaultValue, ValueCounterDictionary<T> valueCounters)
        : this(width, height, depth, defaultValue, false)
    {
        ValueCounters = new(valueCounters);
    }

    public Grid3D(int all)
        : this(all, all, all, default) { }
    public Grid3D(int all, T defaultValue)
        : this(all, all, all, defaultValue) { }
    public Grid3D(int width, int height, int depth)
        : this(width, height, depth, default) { }
    public Grid3D(Location3D dimensions)
        : this(dimensions.X, dimensions.Y, dimensions.Z, default) { }
    public Grid3D(int width, int height, int depth, T defaultValue)
        : this(width, height, depth, defaultValue, true) { }
    public Grid3D(Grid3D<T> other)
        : this(other, other.Dimensions, Location3D.Zero) { }
    public Grid3D(Grid3D<T> other, Location3D dimensions, Location3D offset)
        : this(dimensions, default, false)
    {
        for (int x = 0; x < other.Width; x++)
            for (int y = 0; y < other.Height; y++)
                for (int z = 0; z < other.Depth; z++)
                    this[offset + (x, y, z)] = other.Values[x, y, z];

        ValueCounters = new(other.ValueCounters);
        ValueCounters.Add(default, dimensions.ValueProduct - other.Dimensions.ValueProduct);
    }

    #region Slicing
    public Grid2D<T> GetXYSlice(int z)
    {
        var result = new Grid2D<T>(Width, Height);
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                result[x, y] = this[x, y, z];
        return result;
    }
    public Grid2D<T> GetXZSlice(int y)
    {
        var result = new Grid2D<T>(Width, Depth);
        for (int x = 0; x < Width; x++)
            for (int z = 0; z < Depth; z++)
                result[x, z] = this[x, y, z];
        return result;
    }
    public Grid2D<T> GetYZSlice(int x)
    {
        var result = new Grid2D<T>(Height, Depth);
        for (int y = 0; y < Height; y++)
            for (int z = 0; z < Depth; z++)
                result[y, z] = this[x, y, z];
        return result;
    }

    // The above implementations are not using this function to improve performance
    // This function only serves the purpose of allowing the consumer to get a differently-calculated slice
    public Grid2D<T> GetSlice(Location2D slicedDimensions, Func<Location2D, Location3D> locationSlicer)
    {
        var result = new Grid2D<T>(slicedDimensions);
        for (int x = 0; x < slicedDimensions.X; x++)
            for (int y = 0; y < slicedDimensions.Y; y++)
                result[x, y] = this[locationSlicer((x, y))];
        return result;
    }
    #endregion

    public Location3D GetUniqueElementLocation(T element)
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                for (int z = 0; z < Depth; z++)
                    if (Values[x, y, z].Equals(element))
                        return (x, y, z);
        return (-1, -1, -1);
    }
    public List<Location3D> GetLocationsOfElement(T element)
    {
        var result = new List<Location3D>();
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                for (int z = 0; z < Depth; z++)
                    if (Values[x, y, z].Equals(element))
                        result.Add((x, y, z));
        return result;
    }

    public void ReplaceWithIntersectionValues(T original, T intersection)
    {
        for (int x = 1; x < Width - 1; x++)
            for (int y = 1; y < Height - 1; y++)
                for (int z = 1; z < Depth - 1; z++)
                    if (Values[x, y, z].Equals(original) && GetAdjacentValues((x, y, z)) == 4)
                        Values[x, y, z] = intersection;
    }

    #region Adjacency
    public int GetAdjacentValues(Location3D location) => GetAdjacentValues(location, this[location]);
    public virtual int GetAdjacentValues(Location3D location, T value)
    {
        var offsets = new List<Location3D>(9);
        for (int i = -1; i <= 1; i++)
        {
            // This looks pretty
            offsets.Add((i, 0, 0));
            offsets.Add((0, i, 0));
            offsets.Add((0, 0, i));
        }

        int result = 0;

        foreach (var offset in offsets)
        {
            if (offset.IsCenter)
                continue;

            var l = location + offset;

            if (!IsValidLocation(l))
                continue;

            if (this[l].Equals(value))
                result++;
        }

        return result;
    }
    public int GetAdjacentValues(int x, int y, int z) => GetAdjacentValues((x, y, z), Values[x, y, z]);
    public int GetAdjacentValues(int x, int y, int z, T value) => GetAdjacentValues((x, y, z), value);

    public int GetNeighborValues(Location3D location) => GetNeighborValues(location, this[location]);
    public virtual int GetNeighborValues(Location3D location, T value)
    {
        int result = 0;

        foreach (var offset in neighborOffsets)
        {
            if (offset.IsCenter)
                continue;

            var l = location + offset;

            if (!IsValidLocation(l))
                continue;

            if (this[l].Equals(value))
                result++;
        }

        return result;
    }
    public int GetNeighborValues(int x, int y, int z) => GetNeighborValues((x, y, z), Values[x, y, z]);
    public int GetNeighborValues(int x, int y, int z, T value) => GetNeighborValues((x, y, z), value);
    #endregion

    public bool IsValidLocation(Location3D location) => location.IsNonNegative && location.X < Width && location.Y < Height && location.Z < Depth;

    protected virtual bool IsImpassableObject(T element) => false;

    public override IEnumerator<T> GetEnumerator()
    {
        foreach (T v in Values)
            yield return v;
    }

    public virtual T this[int x, int y, int z]
    {
        get => Values[x, y, z];
        set
        {
            ValueCounters?.AdjustCounters(Values[x, y, z], value);
            Values[x, y, z] = value;
        }
    }
    public sealed override T this[Location3D location]
    {
        get => this[location.X, location.Y, location.Z];
        set => this[location.X, location.Y, location.Z] = value;
    }
}
