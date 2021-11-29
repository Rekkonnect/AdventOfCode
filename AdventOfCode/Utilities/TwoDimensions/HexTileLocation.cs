using System;
using System.Collections.Generic;

namespace AdventOfCode.Utilities.TwoDimensions;

public struct HexTileLocation : IEquatable<HexTileLocation>
{
    private Location2D location;

    public Location2D Location => location;

    public int StepsFromCenter => Location.ManhattanDistanceFromCenter / 2;

    public HexTileLocation(IEnumerable<VerticalHexSide> directions)
        : this()
    {
        foreach (var s in directions)
            ApplyDirection(s);
    }
    public HexTileLocation(IEnumerable<HorizontalHexSide> directions)
        : this()
    {
        foreach (var s in directions)
            ApplyDirection(s);
    }

    public void ApplyDirection<T>(T side)
        where T : struct, Enum
    {
        switch (side)
        {
            case VerticalHexSide verticalSide:
                ApplyDirection(verticalSide);
                break;
            case HorizontalHexSide horizontalSide:
                ApplyDirection(horizontalSide);
                break;
        }
    }
    public void ApplyDirection(VerticalHexSide side)
    {
        location += side.GetOffset();
    }
    public void ApplyDirection(HorizontalHexSide side)
    {
        location += side.GetOffset();
    }

    public bool Equals(HexTileLocation other) => location == other.location;
    public override bool Equals(object obj) => obj is HexTileLocation location && Equals(location);
    public override int GetHashCode() => location.GetHashCode();
    public override string ToString() => location.ToString();

    public static HexTileLocation WithFurthestFromCenter<T>(IEnumerable<T> directions, out HexTileLocation furthest)
        where T : struct, Enum
    {
        var result = new HexTileLocation();
        furthest = default;

        foreach (var d in directions)
        {
            result.ApplyDirection(d);

            if (result.StepsFromCenter > furthest.StepsFromCenter)
                furthest = result;
        }

        return result;
    }
}
