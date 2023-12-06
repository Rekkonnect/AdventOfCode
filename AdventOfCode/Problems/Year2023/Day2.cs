namespace AdventOfCode.Problems.Year2023;

public class Day2 : Problem<int>
{
    private Game[] _games;

    public override int SolvePart1()
    {
        var maxCubes = new CubeSet(12, 13, 14);
        return _games.Where(g => g.IsPossible(maxCubes))
            .Select(g => g.ID)
            .Sum();
    }
    public override int SolvePart2()
    {
        return _games
            .Select(g => g.MinRequiredSet().Power)
            .Sum();
    }

    protected override void LoadState()
    {
        _games = ParsedFileLines(ParseGame);
    }
    protected override void ResetState()
    {
        _games = null;
    }

    private static Game ParseGame(string line)
    {
        var span = line.AsSpan();
        span.SplitOnce(": ", out var gameDeclaration, out var roundDeclarations);
        const string gamePrefix = "Game ";
        int id = gameDeclaration[gamePrefix.Length..].ParseInt32();
        var sets = roundDeclarations.SplitSelect("; ", ParseCubeSet)
            .ToArrayOrExisting();
        return new(id, sets);
    }
    private static CubeSet ParseCubeSet(SpanString span)
    {
        int red = 0;
        int green = 0;
        int blue = 0;

        SpanString first = default;
        SpanString second = default;
        SpanString third = default;

        bool split1 = span.SplitOnce(", ", out var left, out var right);
        first = left;
        if (split1)
        {
            bool split2 = right.SplitOnce(", ", out left, out right);
            second = left;
            if (split2)
            {
                third = right;
            }
        }

        SetColors(first);
        SetColors(second);
        SetColors(third);

        return new(red, green, blue);

        void SetColors(SpanString span)
        {
            if (span == default)
                return;

            span.SplitOnce(' ', out var countSpan, out var colorSpan);
            ref int colorRef = ref red;

            switch (colorSpan)
            {
                case "red":
                    colorRef = ref red;
                    break;
                case "green":
                    colorRef = ref green;
                    break;
                case "blue":
                    colorRef = ref blue;
                    break;
            }

            colorRef = countSpan.ParseInt32();
        }
    }

    private record class Game(int ID, CubeSet[] CubeSets)
    {
        public CubeSet MinRequiredSet()
        {
            int red = 0;
            int green = 0;
            int blue = 0;

            foreach (var set in CubeSets)
            {
                red = Math.Max(red, set.Red);
                green = Math.Max(green, set.Green);
                blue = Math.Max(blue, set.Blue);
            }

            return new(red, green, blue);
        }

        public bool IsPossible(CubeSet maxCubes)
        {
            foreach (var set in CubeSets)
            {
                if (!maxCubes.FullyContains(set))
                    return false;
            }
            return true;
        }
    }

    private readonly record struct CubeSet(int Red, int Green, int Blue)
    {
        public int Power => Red * Green * Blue;

        public bool FullyContains(CubeSet other)
        {
            return Red >= other.Red
                && Green >= other.Green
                && Blue >= other.Blue;
        }
    }
}
