using AdventOfCode.Functions;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2021;

public partial class Day17 : Problem<int>
{
    private Trajector trajector;

    public override int SolvePart1()
    {
        return trajector.HighestVelocityY();
    }
    public override int SolvePart2()
    {
        return trajector.ValidVelocityCounts();
    }

    protected override void LoadState()
    {
        trajector = Trajector.Parse(FileContents.Trim());
    }
    protected override void ResetState()
    {
        trajector = null;
    }

#nullable enable

    private sealed partial class Trajector
    {
        // While always x > 0 and y < 0, this regex pattern matches more cases
        // than the solver itself supports
        private static readonly Regex targetAreaPattern = TargetAreaRegex();

        private readonly Rectangle target;

        private Trajector(Rectangle targetArea)
        {
            target = targetArea;
        }

        public int HighestVelocityY()
        {
            return MathFunctions.Sum(Math.Abs(target.Bottom) - 1);
        }

        // Target pattern (unproven; heuristically concluded)
        /*
         * - for all y > 0, up until max y, find x < area x start that get you in
         * - for all area y start / 2 <= y <= 0, find x > area x start that get you in
         * - add the number of positions in the target area (width * height) that you can directly shoot the probe at
         */
        // However, brute force is preferred to avoid workarounds, debugging and alike

        public int ValidVelocityCounts()
        {
            int count = 0;

            int maxY = HighestVelocityY();
            for (int x = 1; x <= target.Right / 2 + 1; x++)
            {
                if (!IsValidInitialVelocityX(x))
                    continue;

                for (int y = target.Bottom; y <= maxY; y++)
                {
                    if (!IsValidInitialVelocity(new(x, y), out _))
                        continue;

                    count++;
                }
            }

            return count + target.Area;
        }

        private bool IsValidInitialVelocityX(int initialVelocityX)
        {
            int maxX = MathFunctions.Sum(initialVelocityX);
            if (target.IsWithinX(maxX))
                return true;

            if (maxX < target.Left)
                return false;

            // Iterate one by one
            int currentX = 0;
            int velocity = initialVelocityX;
            while (velocity > 0)
            {
                currentX += velocity;

                if (target.IsWithinX(currentX))
                    return true;

                velocity--;
            }

            return false;
        }
        private bool IsValidInitialVelocityY(int initialVelocityY)
        {
            int velocity = initialVelocityY;
            if (velocity > 0)
                velocity = -velocity - 1;

            if (velocity < target.Bottom)
                return false;

            int currentY = 0;
            while (true)
            {
                currentY += velocity;

                if (target.IsWithinY(currentY))
                    return true;

                if (currentY < target.Bottom)
                    return false;

                velocity--;
            }
        }
        private bool IsValidInitialVelocity(Location2D initial, out bool xStopped)
        {
            xStopped = false;
            bool validY = IsValidInitialVelocityY(initial.Y);
            if (!validY)
                return false;

            var current = Location2D.Zero;
            var velocity = initial;
            while (true)
            {
                if (target.IsWithin(current))
                    return true;

                if (current.X > target.Right || current.Y < target.Bottom)
                    return false;

                current += velocity;
                AdjustVelocity(ref velocity);
                if (xStopped = velocity.X is 0)
                    return validY;
            }
        }

        private static void AdjustVelocity(ref Location2D velocity)
        {
            AdjustVelocityX(ref velocity);
            AdjustVelocityY(ref velocity);
        }
        private static void AdjustVelocityX(ref Location2D velocity)
        {
            if (velocity.X < 0)
                velocity.X++;
            else if (velocity.X > 0)
                velocity.X--;
        }
        private static void AdjustVelocityY(ref Location2D velocity)
        {
            velocity.Y--;
        }

        public static Trajector Parse(string targetArea)
        {
            var groups = targetAreaPattern.Match(targetArea).Groups;
            int xStart = groups["xStart"].Value.ParseInt32();
            int xEnd = groups["xEnd"].Value.ParseInt32();
            int yStart = groups["yStart"].Value.ParseInt32();
            int yEnd = groups["yEnd"].Value.ParseInt32();
            return new(new(xStart, xEnd, yStart, yEnd));
        }

        [GeneratedRegex("target area: x=(?'xStart'-?\\d*)\\.\\.(?'xEnd'-?\\d*), y=(?'yStart'-?\\d*)\\.\\.(?'yEnd'-?\\d*)")]
        private static partial Regex TargetAreaRegex();
    }
}
