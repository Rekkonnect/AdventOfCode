using AdventOfCode.Functions;
using Garyon.DataStructures;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2016
{
    // Along with 2015/23, do something about abstracting this copy-pasted pile of shit
    public class Day12 : Problem<int>
    {
        private Computer computer = new();

        public override int SolvePart1()
        {
            computer.ResetRegisters();
            computer.RunProgram();
            return computer.GetRegisterValue('a');
        }
        public override int SolvePart2()
        {
            computer.ResetRegisters();
            computer.SetRegisterValue('c', 1);
            computer.RunProgram();
            return computer.GetRegisterValue('a');
        }

        protected override void LoadState()
        {
            computer.Instructions = ParsedFileLines(Instruction.Parse);
        }

        private class Computer
        {
            private Instruction[] instructions;
            private int instructionIndex;

            public Instruction[] Instructions
            {
                set => instructions = value;
            }

            private FlexibleDictionary<char, int> registers = new();

            public Computer(Instruction[] programInstructions = null) => instructions = programInstructions;

            public void LoadRunProgram(Instruction[] programInstructions)
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
                instructionIndex = 0;
                do
                    ExecuteInstruction(instructions[instructionIndex]);
                while (instructionIndex < instructions.Length);
            }
            private void ExecuteInstruction(Instruction instruction)
            {
                ExtractValue(instruction, 0, out int value0, out char register0);
                ExtractValue(instruction, 1, out int value1, out char register1);

                int instructionOffset = 1;

                switch (instruction.Operator)
                {
                    case Operator.Copy:
                        registers[register1] = value0;
                        break;
                    case Operator.Increase:
                        registers[register0]++;
                        break;
                    case Operator.Decrease:
                        registers[register0]--;
                        break;
                    case Operator.JumpIfNotZero:
                        if (value0 != 0)
                            instructionOffset = value1;
                        break;
                }

                JumpToOffset(instructionOffset);
            }

            private void ExtractValue(Instruction instruction, int argumentIndex, out int value, out char registerName)
            {
                var arguments = instruction.Arguments;

                value = default;
                registerName = default;

                if (argumentIndex >= arguments.Length)
                    return;

                bool isConstant = instruction.Arguments[argumentIndex].ExtractInt32AndFirstChar(out value, out registerName);

                if (!isConstant)
                    value = registers[registerName];
            }

            private void JumpToOffset(int offset) => instructionIndex += offset;

            public int GetRegisterValue(char name) => registers[name];
            public void SetRegisterValue(char name, int value) => registers[name] = value;
        }

        private enum Operator
        {
            Copy,
            Increase,
            Decrease,
            JumpIfNotZero,
        }

        private record Instruction(Operator Operator, string[] Arguments)
        {
            private static readonly Regex statPattern = new(@"(?'operator'\w*) (?'arguments'.*)", RegexOptions.Compiled);

            public static Instruction Parse(string s)
            {
                var groups = statPattern.Match(s).Groups;
                var op = ParseOperator(groups["operator"].Value);
                var arguments = groups["arguments"].Value.Split(" ");
                return new(op, arguments);
            }

            private static Operator ParseOperator(string raw) => raw switch
            {
                "cpy" => Operator.Copy,
                "inc" => Operator.Increase,
                "dec" => Operator.Decrease,
                "jnz" => Operator.JumpIfNotZero,
            };
        }
    }
}
