using AdventOfCode.Functions;

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

    public int Width => Math.Abs(Top - Bottom) + 1;
    public int Height => Math.Abs(Left - Right) + 1;

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
}
