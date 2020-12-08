using AdventOfCode.Problems.Year2020.Utilities;

namespace AdventOfCode.Problems.Year2020
{
    public class Day8 : Problem2<int>
    {
        public override int RunPart1()
        {
            var computer = GetComputer();
            computer.RunUntilExecutedInstruction();
            return computer.Accumulator;
        }
        public override int RunPart2()
        {
            var computer = GetComputer();
            for (int i = 0; i < computer.TotalInstructions; i++)
            {
                var instruction = computer.Instructions[i];
                var operation = instruction.Operation;

                // C# 9 be like
                if (operation is not ConsoleSimulatorOperation.Jump and not ConsoleSimulatorOperation.NoOperation)
                    continue;

                // CS8509.0: It actually is exhaustive, based on the context above, but you don't know that lmao
                instruction.Operation = operation switch
                {
                    ConsoleSimulatorOperation.Jump => ConsoleSimulatorOperation.NoOperation,
                    ConsoleSimulatorOperation.NoOperation => ConsoleSimulatorOperation.Jump,
                };

                bool forciblyTerminated = computer.RunUntilExecutedInstruction();
                if (!forciblyTerminated)
                    return computer.Accumulator;

                // Reset the initial state of the computer
                instruction.Operation = operation;
                computer.Reset();
            }

            return -1;
        }

        private ConsoleSimulator GetComputer()
        {
            return new(FileLines, true);
        }
    }
}
