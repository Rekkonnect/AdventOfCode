using AdventOfCode.Problems.Year2019.Utilities;

namespace AdventOfCode.Problems.Year2019;

public abstract class SimpleIntcodeDay : Problem<long>
{
    private IntcodeComputer computer;

    public abstract int SingleArgumentPart1 { get; }
    public abstract int SingleArgumentPart2 { get; }

    public sealed override long SolvePart1() => SolvePart(SingleArgumentPart1);
    public sealed override long SolvePart2() => SolvePart(SingleArgumentPart2);

    protected long SolvePart(int argument)
    {
        computer.Reset();
        return computer.RunToHalt(null, argument);
    }

    protected sealed override void LoadState()
    {
        computer = new IntcodeComputer(FileContents);
    }
    protected sealed override void ResetState()
    {
        computer = null;
    }
}
