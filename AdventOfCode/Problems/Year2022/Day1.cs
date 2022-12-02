using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2022;

public class Day1 : Problem<int>
{
    private ElfCarriage[] elfCarriages;

    public override int SolvePart1() => elfCarriages.Max(e => e.TotalCalories);
    public override int SolvePart2()
    {
        return elfCarriages
            .Select(e => e.TotalCalories)
            .OrderByDescending(e => e)
            .Take(3)
            .Sum();
    }

    protected override void LoadState()
    {
        var sections = NormalizedFileContents.TrimEnd().Split("\n\n");
        elfCarriages = sections.Select(ElfCarriage.ParseSection).ToArray();
    }
    protected override void ResetState()
    {
        elfCarriages = null;
    }

    private sealed record ElfCarriage(ImmutableArray<int> SnacksCalories)
    {
        public int TotalCalories { get; } = SnacksCalories.Sum();

        public static ElfCarriage ParseSection(string section)
        {
            var snacksCalories = section.Split("\n").Select(int.Parse);
            return new(snacksCalories.ToImmutableArray());
        }
    }
}
