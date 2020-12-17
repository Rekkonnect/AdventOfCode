using System;
using static System.Math;

namespace AdventOfCode.Utilities.FourDimensions
{
    public struct Location4D : ILocation<Location4D>, IHasX, IHasY, IHasZ, IHasW
    {
        public static Location4D Zero => (0, 0, 0, 0);

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int W { get; set; }

        public bool IsPositive => X > 0 && Y > 0 && Z > 0 && W > 0;
        public bool IsNonNegative => X >= 0 && Y >= 0 && Z >= 0 && W >= 0;
        public bool IsCenter => (X | Y | Z | W) == 0;
        public bool IsNonPositive => X <= 0 && Y <= 0 && Z <= 0 && W <= 0;
        public bool IsNegative => X < 0 && Y < 0 && Z < 0 && W < 0;

        public int ValueSum => X + Y + Z + W;
        public int ValueProduct => X * Y * Z * W;
        public int ManhattanDistanceFromCenter => Absolute.ValueSum;

        public Location4D Absolute => (Abs(X), Abs(Y), Abs(Z), Abs(W));
        public Location4D Invert => (-X, -Y, -Z, -W);

        public Location4D InvertX => (-X, Y, Z, W);
        public Location4D InvertY => (X, -Y, Z, W);
        public Location4D InvertZ => (X, Y, -Z, W);
        public Location4D InvertW => (X, Y, Z, -W);

        IHasX IHasX.InvertX => InvertX;
        IHasY IHasY.InvertY => InvertY;
        IHasZ IHasZ.InvertZ => InvertZ;
        IHasW IHasW.InvertW => InvertW;

        public Location4D(int all) => (X, Y, Z, W) = (all, all, all, all);
        public Location4D(int x, int y, int z, int w) => (X, Y, Z, W) = (x, y, z, w);
        public Location4D((int, int, int, int) point) => (X, Y, Z, W) = point;

        public int ManhattanDistance(Location4D other) => Abs(X - other.X) + Abs(Y - other.Y) + Abs(Z - other.Z) + Abs(W - other.W);

        public Location4D SignedDifferenceFrom(Location4D other)
        {
            var (x, y, z, w) = this - other;
            return (Sign(x), Sign(y), Sign(z), Sign(w));
        }

        public void Deconstruct(out int x, out int y, out int z, out int w)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }

        public static implicit operator Location4D((int, int, int, int) point) => new Location4D(point);
        public static implicit operator (int X, int Y, int Z, int W)(Location4D point) => (point.X, point.Y, point.Z, point.W);

        public static Location4D operator -(Location4D location) => location.Invert;
        public static Location4D operator +(Location4D left, Location4D right) => (left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        public static Location4D operator -(Location4D left, Location4D right) => left + -right;
        public static Location4D operator *(Location4D left, int right) => (left.X * right, left.Y * right, left.Z * right, left.W * right);
        public static Location4D operator *(int left, Location4D right) => (left * right.X, left * right.Y, left * right.Z, left * right.W);
        public static Location4D operator *(Location4D left, Location4D right) => (left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        public static Location4D operator /(Location4D left, int right) => (left.X / right, left.Y / right, left.Z / right, left.W / right);
        public static Location4D operator /(int left, Location4D right) => (left / right.X, left / right.Y, left / right.Z, left / right.W);
        public static bool operator ==(Location4D left, Location4D right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z && left.W == right.W;
        public static bool operator !=(Location4D left, Location4D right) => left.X != right.X || left.Y != right.Y || left.Z != right.Z || left.W != right.W;

        public bool Equals(Location4D other) => this == other;

        public override string ToString() => $"({X}, {Y}, {Z}, {W})";
        public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
        public override bool Equals(object obj) => obj is Location4D l && this == l;
    }
}
