namespace AdventOfCode.Problems;

public abstract class FinalDay<T> : Problem<T, string>
{
    public sealed override string SolvePart2()
    {
        return $"Congratulations on completing all of AoC {Year}!";
    }
}
