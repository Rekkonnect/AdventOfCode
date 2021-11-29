using AdventOfCode.Problems.Year2017.Utilities;
using System.Collections.Generic;

namespace AdventOfCode.Problems.Year2017;

public class Day18 : Problem<long, int>
{
    private ComputerInstruction[] instructions;

    public override long SolvePart1()
    {
        long output = 0;

        var computer = new DuetComputer(instructions);
        computer.SoundRecoveredHandler += sound =>
        {
            output = sound;
            return true;
        };
        computer.RunProgram();
        return output;
    }
    public override int SolvePart2()
    {
        var program0 = new DuetProgram(instructions, 0);
        var program1 = new DuetProgram(instructions, 1);
        program0.LinkedProgram = program1;
        program1.LinkedProgram = program0;

        int valuesSent = 0;

        program1.ValueSent += _ =>
        {
            valuesSent++;
            return false;
        };

        do
        {
            program0.ResumeExecution();
            program1.ResumeExecution();
        }
        while (!program0.Deadlocked);
        return valuesSent;
    }

    protected override void LoadState()
    {
        instructions = ParsedFileLines(s => ComputerInstruction.Parse(s));
    }
    protected override void ResetState()
    {
        instructions = null;
    }

    private class DuetProgram : DuetComputer
    {
        private readonly Queue<long> messageQueue = new();

        public int ProgramID { get; }

        public DuetProgram LinkedProgram { get; set; }

        public bool Deadlocked => messageQueue.Count == 0 && LinkedProgram.messageQueue.Count == 0;

        public event ExecutionOutputHandler ValueSent;

        public DuetProgram(ComputerInstruction[] instructions, int programID)
            : base(instructions)
        {
            ProgramID = programID;
            Registers['p'] = programID;
        }

        protected override void RunInstruction(ComputerInstruction instruction, ArgumentInfo arg0, ArgumentInfo arg1, ref int instructionOffset)
        {
            switch (instruction.Operator)
            {
                case ComputerOperator.Send:
                    long message = arg0.Value;
                    LinkedProgram.messageQueue.Enqueue(message);
                    HaltRequested = ValueSent?.Invoke(message) ?? false;
                    break;

                case ComputerOperator.Receive:
                    bool received = messageQueue.TryDequeue(out long value);
                    if (!received)
                    {
                        // Reattempt receiving the value when continuing execution, halt until rerun
                        instructionOffset = 0;
                        HaltRequested = true;
                        break;
                    }

                    Registers[arg0.RegisterName] = value;
                    break;

                default:
                    base.RunInstruction(instruction, arg0, arg1, ref instructionOffset);
                    return;
            }
        }
    }
}
