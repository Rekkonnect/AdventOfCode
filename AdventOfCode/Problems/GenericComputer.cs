using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using Garyon.DataStructures;
using Garyon.Extensions.ArrayExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;

namespace AdventOfCode.Problems
{
    public class GenericComputer
    {
        private ComputerInstruction[] instructions;
        private int instructionIndex;

        private List<ComputerInstruction> lastInstructions = new();

        public ComputerInstruction[] Instructions
        {
            set => instructions = value.CopyArray();
        }

        private FlexibleDictionary<char, int> registers = new();
        private FlexibleDictionary<char, int> registerAdjustments = new();

        public GenericComputer(ComputerInstruction[] programInstructions = null)
        {
            instructions = programInstructions;
        }

        public void LoadRunProgram(ComputerInstruction[] programInstructions)
        {
            instructions = programInstructions;
            RunProgram();
        }

        public void ResetRegisters()
        {
            registers.Clear();
        }

        public void RunProgram()
        {
            lastInstructions.Clear();
            instructionIndex = 0;

            do
                ExecuteInstruction(instructions[instructionIndex]);
            while (instructionIndex < instructions.Length);
        }
        private void ExecuteInstruction(ComputerInstruction instruction)
        {
            bool constant0 = ExtractValue(instruction, 0, out int value0, out char register0);
            bool constant1 = ExtractValue(instruction, 1, out int value1, out char register1);

            int instructionOffset = 1;

            switch (instruction.Operator)
            {
                case ComputerOperator.Copy:
                    if (constant1)
                        break;
                    registers[register1] = value0;
                    break;

                case ComputerOperator.Increase:
                    if (constant0)
                        break;
                    registers[register0]++;
                    break;

                case ComputerOperator.Decrease:
                    if (constant0)
                        break;
                    registers[register0]--;
                    break;

                case ComputerOperator.Halve:
                    if (constant0)
                        break;
                    registers[register0] /= 2;
                    break;

                case ComputerOperator.Triple:
                    if (constant0)
                        break;
                    registers[register0] *= 3;
                    break;

                case ComputerOperator.Jump:
                    instructionOffset = value0;
                    break;

                case ComputerOperator.JumpIfNotZero:
                    if (value0 != 0)
                        instructionOffset = value1;
                    break;

                case ComputerOperator.JumpIfEven:
                    if (constant0)
                        break;
                    if (registers[register0] % 2 is 0)
                        instructionOffset = value1;
                    break;

                case ComputerOperator.JumpIfOne:
                    if (constant0)
                        break;
                    if (registers[register0] is 1)
                        instructionOffset = value1;
                    break;

                case ComputerOperator.Toggle:
                    int toggledIndex = instructionIndex + value0;
                    if (toggledIndex < 0 || toggledIndex >= instructions.Length)
                        break;

                    var toggledInstruction = instructions[toggledIndex];
                    instructions[toggledIndex] = toggledInstruction with { Operator = ToggleOperator(toggledInstruction.Operator) };
                    break;
            }

            JumpToOffset(instructionOffset);
        }

        private static ComputerOperator ToggleOperator(ComputerOperator op)
        {
            return (op.GetArgumentCount(), op) switch
            {
                (2, ComputerOperator.JumpIfNotZero) => ComputerOperator.Copy,
                (2, _) => ComputerOperator.JumpIfNotZero,
                (1, ComputerOperator.Increase) => ComputerOperator.Decrease,
                (1, _) => ComputerOperator.Increase,
                _ => ComputerOperator.NoOperation,
            };
        }

        private bool ExtractValue(ComputerInstruction instruction, int argumentIndex, out int value, out char registerName)
        {
            var arguments = instruction.Arguments;

            value = default;
            registerName = default;

            if (argumentIndex >= arguments.Length)
                return false;

            bool isConstant = instruction.Arguments[argumentIndex].ExtractInt32AndFirstChar(out value, out registerName);

            if (!isConstant)
                value = registers[registerName];

            return isConstant;
        }

        private void JumpToOffset(int offset) => instructionIndex += offset;

        public int GetRegisterValue(char name) => registers[name];
        public void SetRegisterValue(char name, int value) => registers[name] = value;
    }
}
