using System.Numerics;

namespace AdventOfCode.Utilities.TwoDimensions;

public struct Rectangle
{
    public int Left { get; set; }
    public int Right { get; set; }
    public int Top { get; set; }
    public int Bottom { get; set; }

    public Location2D TopLeft
    {
        get => (Left, Top);
        set => (Left, Top) = value;
    }
    public Location2D TopRight
    {
        get => (Right, Top);
        set => (Right, Top) = value;
    }
    public Location2D BottomLeft
    {
        get => (Left, Bottom);
        set => (Left, Bottom) = value;
    }
    public Location2D BottomRight
    {
        get => (Right, Bottom);
        set => (Right, Bottom) = value;
    }

    public int MinX => Math.Min(Left, Right);
    public int MaxX => Math.Max(Left, Right);

    public int MinY => Math.Min(Bottom, Top);
    public int MaxY => Math.Max(Bottom, Top);

    public Location2D Min => (MinX, MinY);
    public Location2D Max => (MaxX, MaxY);

    public int Width => Math.Abs(Left - Right) + 1;
    public int Height => Math.Abs(Top - Bottom) + 1;

    public Location2D Dimensions => new(Width, Height);

    public int Area => Width * Height;

    public Rectangle(int left, int right, int bottom, int top)
    {
        Left = left;
        Right = right;
        Bottom = bottom;
        Top = top;
    }
    public Rectangle(Location2D topLeft, Location2D topRight, Location2D bottomLeft, Location2D bottomRight)
        : this()
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
    }

    public bool IsWithinX(int x) => MathFunctions.BetweenInclusive(x, Left, Right);
    public bool IsWithinY(int y) => MathFunctions.BetweenInclusive(y, Top, Bottom);
    public bool IsWithin(Location2D point) => IsWithinX(point.X) && IsWithinY(point.Y);

    public static Rectangle FromRectangles(Rectangle a, Rectangle b)
    {
        // I fucking hate this
        var minA = a.Min;
        var maxA = a.Max;
        var minB = b.Min;
        var maxB = b.Max;

        int minX = Math.Min(a.MinX, b.MinX);
        int maxX = Math.Max(a.MaxX, b.MaxX);
        int minY = Math.Min(a.MinY, b.MinY);
        int maxY = Math.Max(a.MaxY, b.MaxY);

        return new(minX, maxX, minY, maxY);
    }

    public static Rectangle FromBounds(Location2D a, Location2D b)
    {
        MinMax(a.X, b.X, out int minX, out int maxX);
        MinMax(a.Y, b.Y, out int minY, out int maxY);
        return new(minX, maxX, minY, maxY);
    }
    private static void MinMax<T>(T a, T b, out T min, out T max)
        where T : struct, INumber<T>
    {
        if (a < b)
        {
            min = a;
            max = b;
        }
        else
        {
            min = b;
            max = a;
        }
    }
}
