using AdventOfCode.Functions;
using Garyon.Objects;
using static System.Math;

namespace AdventOfCode.Utilities.ThreeDimensions;

public struct Location3D : ILocation<Location3D>, IHasX, IHasY, IHasZ
{
    public static Location3D Zero => (0, 0, 0);

    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public bool IsPositive => X > 0 && Y > 0 && Z > 0;
    public bool IsNonNegative => X >= 0 && Y >= 0 && Z >= 0;
    public bool IsCenter => (X | Y | Z) == 0;
    public bool IsNonPositive => X <= 0 && Y <= 0 && Z <= 0;
    public bool IsNegative => X < 0 && Y < 0 && Z < 0;

    public int ValueSum => X + Y + Z;
    public int ValueProduct => X * Y * Z;
    public long ValueProduct64 => (long)X * Y * Z;
    public int ManhattanDistanceFromCenter => Absolute.ValueSum;

    public Location3D Absolute => (Abs(X), Abs(Y), Abs(Z));
    public Location3D Invert => (-X, -Y, -Z);

    public Location3D InvertX => (-X, Y, Z);
    public Location3D InvertY => (X, -Y, Z);
    public Location3D InvertZ => (X, Y, -Z);

    IHasX IHasX.InvertX => InvertX;
    IHasY IHasY.InvertY => InvertY;
    IHasZ IHasZ.InvertZ => InvertZ;

    public Location3D(int all) => (X, Y, Z) = (all, all, all);
    public Location3D(int x, int y, int z) => (X, Y, Z) = (x, y, z);
    public Location3D((int, int, int) point) => (X, Y, Z) = point;

    public int ManhattanDistance(Location3D other) => Abs(X - other.X) + Abs(Y - other.Y) + Abs(Z - other.Z);

    public Location3D SignedDifferenceFrom(Location3D other)
    {
        var (x, y, z) = this - other;
        return (Sign(x), Sign(y), Sign(z));
    }

    public bool SatisfiesComparisonPerCoordinate(Location3D other, ComparisonKinds kinds)
    {
        var difference = SignedDifferenceFrom(other);
        return difference.X.SatisfiesComparison(0, kinds)
            && difference.Y.SatisfiesComparison(0, kinds)
            && difference.Z.SatisfiesComparison(0, kinds);
    }
    public bool MatchesComparisonPerCoordinate(Location3D other, ComparisonResult result)
    {
        var difference = SignedDifferenceFrom(other);
        return difference.X == (int)result
            && difference.Y == (int)result
            && difference.Z == (int)result;
    }

    public Location3D Shuffle(Orientation orientation)
    {
        return (this * orientation.Signs).Shuffle(orientation.AxesOrder);
    }
    public Location3D Shuffle(AxesOrder axes)
    {
        return axes switch
        {
            AxesOrder.XYZ => this,
            AxesOrder.XZY => (X, Z, Y),
            AxesOrder.YXZ => (Y, X, Z),
            AxesOrder.YZX => (Y, Z, X),
            AxesOrder.ZXY => (Z, X, Y),
            AxesOrder.ZYX => (Z, Y, X),
        };
    }

    public void Deconstruct(out int x, out int y, out int z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    public static implicit operator Location3D((int, int, int) point) => new Location3D(point);
    public static implicit operator (int X, int Y, int Z)(Location3D point) => (point.X, point.Y, point.Z);

    public static Location3D operator -(Location3D location) => location.Invert;
    public static Location3D operator +(Location3D left, Location3D right) => (left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static Location3D operator -(Location3D left, Location3D right) => left + -right;
    public static Location3D operator *(Location3D left, int right) => (left.X * right, left.Y * right, left.Z * right);
    public static Location3D operator *(int left, Location3D right) => (left * right.X, left * right.Y, left * right.Z);
    public static Location3D operator *(Location3D left, Location3D right) => (left.X * right.X, left.Y * right.Y, left.Z * right.Z);
    public static Location3D operator /(Location3D left, int right) => (left.X / right, left.Y / right, left.Z / right);
    public static Location3D operator /(int left, Location3D right) => (left / right.X, left / right.Y, left / right.Z);
    public static bool operator ==(Location3D left, Location3D right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z;
    public static bool operator !=(Location3D left, Location3D right) => left.X != right.X || left.Y != right.Y || left.Z != right.Z;

    public bool Equals(Location3D other) => this == other;

    public override string ToString() => $"({X}, {Y}, {Z})";
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override bool Equals(object obj) => obj is Location3D l && this == l;

    public static Location3D Min(Location3D left, Location3D right)
    {
        return new(Math.Min(left.X, right.X), Math.Min(left.Y, right.Y), Math.Min(left.Z, right.Z));
    }
    public static Location3D Max(Location3D left, Location3D right)
    {
        return new(Math.Max(left.X, right.X), Math.Max(left.Y, right.Y), Math.Max(left.Z, right.Z));
    }
}
