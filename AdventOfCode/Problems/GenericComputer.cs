using AdventOfCode.Functions;
using Garyon.DataStructures;

namespace AdventOfCode.Problems
{
    public class GenericComputer
    {
        private ComputerProgram program;

        private int instructionIndex;

        protected bool HaltRequested;

        public ComputerInstruction[] Instructions
        {
            set => program.Instructions = value;
        }

        public event ExecutionOutputHandler OutputHandler;

        protected readonly FlexibleDictionary<char, long> Registers = new();

        public GenericComputer(ComputerInstruction[] instructions = null)
        {
            program = new(instructions);
        }

        public void LoadRunProgram(ComputerInstruction[] programInstructions)
        {
            Instructions = programInstructions;
            RunProgram();
        }

        public void ResetRegisters()
        {
            Registers.Clear();
        }

        public void RunProgram()
        {
            instructionIndex = 0;
            ResumeExecution();
        }
        public void ResumeExecution()
        {
            do
                ExecuteInstruction(program[instructionIndex]);
            while (!HaltRequested && instructionIndex < program.Length);
        }

        private void ExecuteInstruction(ComputerInstruction instruction)
        {
            var arg0 = ExtractArgumentInfo(instruction, 0);
            var arg1 = ExtractArgumentInfo(instruction, 1);

            int instructionOffset = 1;

            RunInstruction(instruction, arg0, arg1, ref instructionOffset);

            JumpToOffset(instructionOffset);
        }

        protected virtual void RunInstruction(ComputerInstruction instruction, ArgumentInfo arg0, ArgumentInfo arg1, ref int instructionOffset)
        {
            switch (instruction.Operator)
            {
                case ComputerOperator.Copy:
                    if (arg1.IsConstant)
                        break;
                    Registers[arg1.RegisterName] = arg0.Value;
                    break;

                // Yes, for some reason this is the opposite of cpy
                case ComputerOperator.Set:
                    if (arg0.IsConstant)
                        break;
                    Registers[arg0.RegisterName] = arg1.Value;
                    break;

                case ComputerOperator.Increase:
                    if (arg0.IsConstant)
                        break;
                    Registers[arg0.RegisterName]++;
                    break;

                case ComputerOperator.Decrease:
                    if (arg0.IsConstant)
                        break;
                    Registers[arg0.RegisterName]--;
                    break;

                case ComputerOperator.Halve:
                    if (arg0.IsConstant)
                        break;
                    Registers[arg0.RegisterName] /= 2;
                    break;

                case ComputerOperator.Triple:
                    if (arg0.IsConstant)
                        break;
                    Registers[arg0.RegisterName] *= 3;
                    break;

                case ComputerOperator.Add:
                    if (arg1.IsConstant)
                        break;
                    Registers[arg1.RegisterName] += arg0.Value;
                    break;

                case ComputerOperator.Subtract:
                    if (arg1.IsConstant)
                        break;
                    Registers[arg1.RegisterName] -= arg0.Value;
                    break;

                case ComputerOperator.Multiply:
                    if (arg1.IsConstant)
                        break;
                    Registers[arg1.RegisterName] *= arg0.Value;
                    break;

                case ComputerOperator.Jump:
                    instructionOffset = arg0.Value32;
                    break;

                case ComputerOperator.JumpIfNotZero:
                    if (arg0.Value != 0)
                        instructionOffset = arg1.Value32;
                    break;

                case ComputerOperator.JumpIfEven:
                    if (arg0.IsConstant)
                        break;
                    if (Registers[arg0.RegisterName] % 2 is 0)
                        instructionOffset = arg1.Value32;
                    break;

                case ComputerOperator.JumpIfOne:
                    if (arg0.IsConstant)
                        break;
                    if (Registers[arg0.RegisterName] is 1)
                        instructionOffset = arg1.Value32;
                    break;

                case ComputerOperator.JumpIfGreaterThanZero:
                    if (arg0.Value > 0)
                        instructionOffset = arg1.Value32;
                    break;

                case ComputerOperator.Toggle:
                    int toggledIndex = instructionIndex + arg0.Value32;
                    program.ToggleInstruction(toggledIndex);
                    break;

                case ComputerOperator.Output:
                    if (HaltRequested = !OutputHandler(arg0.Value))
                        return;
                    break;
            }
        }

        private ArgumentInfo ExtractArgumentInfo(ComputerInstruction instruction, int argumentIndex)
        {
            var arguments = instruction.Arguments;

            if (argumentIndex >= arguments.Length)
                return ArgumentInfo.InvalidArgument;

            bool isConstant = instruction.Arguments[argumentIndex].ExtractInt64AndFirstChar(out long value, out char registerName);

            if (!isConstant)
                value = Registers[registerName];

            return new(isConstant, value, registerName);
        }

        private void JumpToOffset(int offset) => instructionIndex += offset;

        public long GetRegisterValue(char name) => Registers[name];
        public void SetRegisterValue(char name, int value) => Registers[name] = value;

        public delegate bool ExecutionOutputHandler(long output);

        public struct ArgumentInfo
        {
            public static readonly ArgumentInfo InvalidArgument = new(false, default, default);

            public bool IsConstant { get; }
            public long Value { get; }
            public char RegisterName { get; }

            public int Value32 => (int)Value;

            public ArgumentInfo(bool isConstant, long value, char registerName)
            {
                IsConstant = isConstant;
                Value = value;
                RegisterName = registerName;
            }
        }
    }
}
