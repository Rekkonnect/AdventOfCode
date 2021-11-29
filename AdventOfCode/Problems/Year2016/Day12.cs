namespace AdventOfCode.Problems.Year2016;

public class Day12 : Problem<int>
{
    private GenericComputer computer = new();

    public override int SolvePart1()
    {
        computer.Reset();
        computer.RunProgram();
        return (int)computer.GetRegisterValue('a');
    }
    public override int SolvePart2()
    {
        computer.Reset();
        computer.SetRegisterValue('c', 1);
        computer.RunProgram();
        return (int)computer.GetRegisterValue('a');
    }

    protected override void LoadState()
    {
        computer.Instructions = ParsedFileLines(s => ComputerInstruction.Parse(s));
    }
}
