using AdventOfCode.Problems.Year2017.Utilities;
using AdventOfCSharp;
using System.Linq;

namespace AdventOfCode.Problems.Year2017;

public class Day10 : Problem<int, string>
{
    private string input;

    public override int SolvePart1()
    {
        var knotHash = KnotHasher.FromRawLengths(input.Split(',').Select(int.Parse));
        knotHash.IterateOnce();
        return knotHash[0] * knotHash[1];
    }
    public override string SolvePart2()
    {
        return KnotHasher.FromString(input).GetKnotHashString();
    }

    protected override void LoadState()
    {
        input = FileContents;
    }
    protected override void ResetState()
    {
        input = null;
    }
}
