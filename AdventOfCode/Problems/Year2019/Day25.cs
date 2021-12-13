using AdventOfCode.Problems.Year2019.Utilities;

namespace AdventOfCode.Problems.Year2019;

// TODO: Either automate the player, or make it interactive
public class Day25 : FinalDay<long>
{
    // Need a new state indicating interactivity?
    [PartSolution(PartSolutionStatus.Refactoring)]
    public override long SolvePart1() => RunPart();

    private long RunPart()
    {
        var program = FileContents;
        string lineOutput = "";
        long password = 0;

        Console.ForegroundColor = ConsoleColor.Yellow;

        var computer = new IntcodeComputer(program);
        computer.OutputWritten += OutputWritten;
        while (password == 0)
        {
            computer.RunToHalt();
            computer.Reset();
        }

        Console.ResetColor();

        return password;

        void OutputWritten(long output)
        {
            char charput = (char)(int)output;
            if (charput != 10)
                lineOutput += charput;
            else
            {
                if (lineOutput.StartsWith('"'))
                {
                    var split = lineOutput.Split(' ');
                    for (int i = 0; i < split.Length && password == 0; i++)
                        long.TryParse(split[i], out password);
                }
                lineOutput = "";
            }

            Console.Write(charput);
            if (lineOutput == "Command?")
            {
                Console.WriteLine();
                RequestInput();
            }
        }
        void RequestInput()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            string input = Console.ReadLine();

            Console.ForegroundColor = ConsoleColor.Yellow;

            for (int i = 0; i < input.Length; i++)
                computer.BufferInput(input[i]);
            computer.BufferInput(10);
        }
    }
}
