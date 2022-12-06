using AdventOfCode.Utilities.ThreeDimensions;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Utilities.FourDimensions;

public class Grid4D<T> : BaseGrid<T, Location4D>
{
    #region Statically store the neighbor offsets
    private static readonly Location4D[] neighborOffsets;

    static Grid4D()
    {
        var xOffsets = new Location4D[3];
        var yOffsets = new Location4D[3];
        var zOffsets = new Location4D[3];
        var wOffsets = new Location4D[3];
        for (int i = -1; i <= 1; i++)
        {
            xOffsets[i + 1] = (i, 0, 0, 0);
            yOffsets[i + 1] = (0, i, 0, 0);
            zOffsets[i + 1] = (0, 0, i, 0);
            wOffsets[i + 1] = (0, 0, 0, i);
        }

        var offsets = new List<Location4D>(3 * 3 * 3 * 3);

        foreach (var xOffset in xOffsets)
            foreach (var yOffset in yOffsets)
                foreach (var zOffset in zOffsets)
                    foreach (var wOffset in wOffsets)
                        offsets.Add(xOffset + yOffset + zOffset + wOffset);

        neighborOffsets = offsets.ToArray();
    }
    #endregion

    protected T[,,,] Values;

    public readonly int Width, Height, Depth, DepthW;

    public override Location4D Dimensions => (Width, Height, Depth, DepthW);
    public override Location4D Center => Dimensions / 2;

    public override FlexibleListDictionary<T, Location4D> ElementDictionary
    {
        get
        {
            var result = new FlexibleListDictionary<T, Location4D>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                        for (int w = 0; w < DepthW; w++)
                            result[Values[x, y, z, w]].Add((x, y, z, w));
            return result;
        }
    }

    protected Grid4D(Location4D dimensions, T defaultValue, bool initializeValueCounters)
        : this(dimensions.X, dimensions.Y, dimensions.Z, dimensions.W, defaultValue, initializeValueCounters) { }
    protected Grid4D(int width, int height, int depth, int depthW, T defaultValue, bool initializeValueCounters)
    {
        Values = new T[Width = width, Height = height, Depth = depth, DepthW = depthW];
        if (!defaultValue.Equals(default(T)))
        {
            Values.AsSpan().Fill(defaultValue);
        }

        if (initializeValueCounters)
        {
            ValueCounters = new();
            ValueCounters.Add(defaultValue, width * height * depth * depthW);
        }
    }

    public Grid4D(int all)
        : this(all, all, all, all, default) { }
    public Grid4D(int all, T defaultValue)
        : this(all, all, all, all, defaultValue) { }
    public Grid4D(int width, int height, int depth, int depthW)
        : this(width, height, depth, depthW, default) { }
    public Grid4D(Location4D dimensions)
        : this(dimensions.X, dimensions.Y, dimensions.Z, default) { }
    public Grid4D(int width, int height, int depth, int depthW, T defaultValue)
        : this(width, height, depth, depthW, defaultValue, true) { }
    public Grid4D(Grid4D<T> other)
        : this(other, other.Dimensions, Location4D.Zero) { }
    public Grid4D(Grid4D<T> other, Location4D dimensions, Location4D offset)
        : this(dimensions, default, false)
    {
        for (int x = 0; x < other.Width; x++)
            for (int y = 0; y < other.Height; y++)
                for (int z = 0; z < other.Depth; z++)
                    for (int w = 0; w < other.DepthW; w++)
                        this[offset + (x, y, z, w)] = other.Values[x, y, z, w];

        ValueCounters = new(other.ValueCounters);
        ValueCounters.Add(default, dimensions.ValueProduct - other.Dimensions.ValueProduct);
    }

    #region Slicing
    // Too lazy to bloat with XY, XZ, XW, YZ, YW, ZW slicing in 4D
    public Grid2D<T> GetSlice(Location2D slicedDimensions, Func<Location2D, Location4D> locationSlicer)
    {
        var result = new Grid2D<T>(slicedDimensions);
        for (int x = 0; x < slicedDimensions.X; x++)
            for (int y = 0; y < slicedDimensions.Y; y++)
                result[x, y] = this[locationSlicer((x, y))];
        return result;
    }
    // Too lazy to bloat with XYZ, XYW, XZW, YZW slicing in 4D
    public Grid3D<T> GetSlice(Location3D slicedDimensions, Func<Location3D, Location4D> locationSlicer)
    {
        var result = new Grid3D<T>(slicedDimensions);
        for (int x = 0; x < slicedDimensions.X; x++)
            for (int y = 0; y < slicedDimensions.Y; y++)
                for (int z = 0; z < slicedDimensions.Z; z++)
                    result[x, y, z] = this[locationSlicer((x, y, z))];
        return result;
    }
    #endregion

    public Location4D GetUniqueElementLocation(T element)
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                for (int z = 0; z < Depth; z++)
                    for (int w = 0; w < DepthW; w++)
                        if (Values[x, y, z, w].Equals(element))
                            return (x, y, z, w);
        return (-1, -1, -1, -1);
    }
    public List<Location4D> GetLocationsOfElement(T element)
    {
        var result = new List<Location4D>();
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                for (int z = 0; z < Depth; z++)
                    for (int w = 0; w < DepthW; w++)
                        if (Values[x, y, z, w].Equals(element))
                            result.Add((x, y, z, w));
        return result;
    }

    public void ReplaceWithIntersectionValues(T original, T intersection)
    {
        for (int x = 1; x < Width - 1; x++)
            for (int y = 1; y < Height - 1; y++)
                for (int z = 1; z < Depth - 1; z++)
                    for (int w = 1; w < DepthW - 1; w++)
                        if (Values[x, y, z, w].Equals(original) && GetAdjacentValues((x, y, z, w)) == 8)
                            Values[x, y, z, w] = intersection;
    }

    #region Adjacency
    public int GetAdjacentValues(Location4D location) => GetAdjacentValues(location, this[location]);
    public virtual int GetAdjacentValues(Location4D location, T value)
    {
        var offsets = new List<Location4D>(12);
        for (int i = -1; i <= 1; i++)
        {
            // This looks pretty
            offsets.Add((i, 0, 0, 0));
            offsets.Add((0, i, 0, 0));
            offsets.Add((0, 0, i, 0));
            offsets.Add((0, 0, 0, i));
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
    public int GetAdjacentValues(int x, int y, int z, int w) => GetAdjacentValues((x, y, z, w), Values[x, y, z, w]);
    public int GetAdjacentValues(int x, int y, int z, int w, T value) => GetAdjacentValues((x, y, z, w), value);

    public int GetNeighborValues(Location4D location) => GetNeighborValues(location, this[location]);
    public virtual int GetNeighborValues(Location4D location, T value)
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
    public int GetNeighborValues(int x, int y, int z, int w) => GetNeighborValues((x, y, z, w), Values[x, y, z, w]);
    public int GetNeighborValues(int x, int y, int z, int w, T value) => GetNeighborValues((x, y, z, w), value);
    #endregion

    public bool IsValidLocation(Location4D location) => location.IsNonNegative && location.X < Width && location.Y < Height && location.Z < Depth && location.W < DepthW;

    protected virtual bool IsImpassableObject(T element) => false;

    public override IEnumerator<T> GetEnumerator()
    {
        foreach (T v in Values)
            yield return v;
    }

    public virtual T this[int x, int y, int z, int w]
    {
        get => Values[x, y, z, w];
        set
        {
            ValueCounters?.AdjustCounters(Values[x, y, z, w], value);
            Values[x, y, z, w] = value;
        }
    }
    public sealed override T this[Location4D location]
    {
        get => this[location.X, location.Y, location.Z, location.W];
        set => this[location.X, location.Y, location.Z, location.W] = value;
    }
}
