using AdventOfCode.Functions;
using AdventOfCode.Utilities.ThreeDimensions;
using Vector3D = AdventOfCode.Utilities.ThreeDimensions.Location3D;

namespace AdventOfCode.Problems.Year2019;

public class Day12 : Problem<int, long>
{
    public override int SolvePart1() => General(Part1BreakCondition, Part1PostRunner);
    public override long SolvePart2() => General(Part2BreakCondition, Part2PostRunner);

    private bool Part1BreakCondition(long steps, Moon[] moons, ref long xSteps, ref long ySteps, ref long zSteps)
    {
        return steps < 1000;
    }
    private bool Part2BreakCondition(long steps, Moon[] moons, ref long xSteps, ref long ySteps, ref long zSteps)
    {
        if (steps < 2)
            return true;

        bool hasInitialX = xSteps == 0;
        bool hasInitialY = ySteps == 0;
        bool hasInitialZ = zSteps == 0;
        foreach (var m in moons)
        {
            hasInitialX &= m.HasInitialX;
            hasInitialY &= m.HasInitialY;
            hasInitialZ &= m.HasInitialZ;
        }
        if (hasInitialX)
            xSteps = steps;
        if (hasInitialY)
            ySteps = steps;
        if (hasInitialZ)
            zSteps = steps;
        return xSteps == 0 || ySteps == 0 || zSteps == 0;
    }

    private int Part1PostRunner(long steps, Moon[] moons, long xSteps, long ySteps, long zSteps)
    {
        int totalEnergy = 0;
        foreach (var m in moons)
            totalEnergy += m.TotalEnergy;
        return totalEnergy;
    }
    private long Part2PostRunner(long steps, Moon[] moons, long xSteps, long ySteps, long zSteps)
    {
        return LCM(xSteps, ySteps, zSteps);
    }

    private T General<T>(BreakCondition condition, PostRunner<T> postRunner)
    {
        var lines = FileLines;
        var moons = new Moon[lines.Length];
        for (int i = 0; i < lines.Length; i++)
            moons[i] = Moon.Parse(lines[i]);

        long steps = 0;
        long xSteps = 0;
        long ySteps = 0;
        long zSteps = 0;
        for (; condition(steps, moons, ref xSteps, ref ySteps, ref zSteps); steps++)
        {
            for (int j = 0; j < moons.Length; j++)
                for (int k = j + 1; k < moons.Length; k++)
                    moons[j].ApplyGravity(moons[k]);

            for (int j = 0; j < moons.Length; j++)
                moons[j].ApplyVelocity();
        }

        return postRunner(steps, moons, xSteps, ySteps, zSteps);
    }

    private static long LCM(long a, long b) => a / MathFunctions.GCD(a, b) * b;
    private static long LCM(params long[] values)
    {
        long result = values[0];
        for (int i = 1; i < values.Length; i++)
            result = LCM(result, values[i]);
        return result;
    }

    public class Moon
    {
        private Vector3D InitialLocation;
        public Vector3D Location;
        public Vector3D Velocity;

        public bool HasInitialState => InitialLocation == Location && Velocity == Vector3D.Zero;
        public bool HasInitialX => InitialLocation.X == Location.X && Velocity.X == 0;
        public bool HasInitialY => InitialLocation.Y == Location.Y && Velocity.Y == 0;
        public bool HasInitialZ => InitialLocation.Z == Location.Z && Velocity.Z == 0;

        public int PotentialEnergy => Location.Absolute.ValueSum;
        public int KineticEnergy => Velocity.Absolute.ValueSum;
        public int TotalEnergy => PotentialEnergy * KineticEnergy;

        public Moon(Vector3D location) => InitialLocation = Location = location;

        public void ApplyGravity(Moon other)
        {
            var d = Location.SignedDifferenceFrom(other.Location);
            Velocity -= d;
            other.Velocity += d;
        }
        public void ApplyVelocity() => Location += Velocity;

        public static Moon Parse(string s)
        {
            var split = s[1..^1].Split(", ");
            return new Moon((GetDimension(split[0]), GetDimension(split[1]), GetDimension(split[2])));

            int GetDimension(string d) => int.Parse(d[2..]);
        }

        public override string ToString() => $"{Location} - {Velocity}";
    }

    private delegate bool BreakCondition(long steps, Moon[] moons, ref long xSteps, ref long ySteps, ref long zSteps);
    private delegate T PostRunner<T>(long steps, Moon[] moons, long xSteps, long ySteps, long zSteps);
}
