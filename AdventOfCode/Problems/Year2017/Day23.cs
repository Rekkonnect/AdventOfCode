using AdventOfCode.Problems.Year2017.Utilities;

namespace AdventOfCode.Problems.Year2017;

[SolutionInfo(SolutionFlags.Part2WIP)]
public class Day23 : Problem<int, int>
{
    private ComputerInstruction[] instructions;

    public override int SolvePart1()
    {
        var computer = new DuetComputer(instructions);
        computer.RunProgram();
        return computer.GetInvocationCount(ComputerOperator.Multiply);
    }
    public override int SolvePart2()
    {
        // Requires the optimization thing that is "WIP"
        return -1;
    }

    protected override void LoadState()
    {
        instructions = ParsedFileLines(s => ComputerInstruction.Parse(s));
    }
    protected override void ResetState()
    {
        instructions = null;
    }
}
