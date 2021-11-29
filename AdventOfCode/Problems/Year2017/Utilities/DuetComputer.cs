namespace AdventOfCode.Problems.Year2017.Utilities;

public class DuetComputer : GenericComputer
{
    private int lastSound;

    public event ExecutionOutputHandler SoundRecoveredHandler;

    public DuetComputer(ComputerInstruction[] instructions)
        : base(instructions) { }

    protected override void RunInstruction(ComputerInstruction instruction, ArgumentInfo arg0, ArgumentInfo arg1, ref int instructionOffset)
    {
        switch (instruction.Operator)
        {
            case ComputerOperator.Send:
                lastSound = arg0.Value32;
                break;

            case ComputerOperator.Add:
                Registers[arg0.RegisterName] += arg1.Value;
                break;

            case ComputerOperator.Subtract:
                Registers[arg0.RegisterName] -= arg1.Value;
                break;

            case ComputerOperator.Multiply:
                Registers[arg0.RegisterName] *= arg1.Value;
                break;

            case ComputerOperator.Modulo:
                Registers[arg0.RegisterName] %= arg1.Value;
                break;

            case ComputerOperator.Receive:
                if (lastSound is 0)
                    break;
                HaltRequested = SoundRecoveredHandler(lastSound);
                break;

            default:
                base.RunInstruction(instruction, arg0, arg1, ref instructionOffset);
                return;
        }
    }
}
