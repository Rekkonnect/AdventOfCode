using Garyon.DataStructures;
using Garyon.Extensions;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2015
{
    public class Day23 : Problem<uint>
    {
        private Computer computer = new();
        private Instruction[] instructions;

        public override uint SolvePart1()
        {
            computer.LoadRunProgram(instructions);
            return computer.GetRegisterValue('b');
        }
        public override uint SolvePart2()
        {
            computer.ResetRegisters();
            computer.SetRegisterValue('a', 1);
            computer.RunProgram();
            return computer.GetRegisterValue('b');
        }

        protected override void LoadState()
        {
            instructions = FileLines.Select(Instruction.Parse).ToArray();
        }
        protected override void ResetState()
        {
            instructions = null;
        }

        private class Computer
        {
            private Instruction[] instructions;
            private int instructionIndex;

            private FlexibleDictionary<char, uint> registers = new();

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
                char register = instruction.Arguments[0][0];
                bool isOffsetFirst = instruction.Arguments[0].TryParseInt32(out int offset);
                if (!isOffsetFirst)
                {
                    if (instruction.Arguments.Length > 1)
                        offset = instruction.Arguments[1].ParseInt32();
                }

                switch (instruction.Operator)
                {
                    case Operator.Halve:
                        registers[register] /= 2;
                        goto default;
                    case Operator.Triple:
                        registers[register] *= 3;
                        goto default;
                    case Operator.Increase:
                        registers[register]++;
                        goto default;
                    case Operator.Jump:
                        JumpToOffset(offset);
                        break;
                    case Operator.JumpIfEven:
                        if (registers[register] % 2 == 0)
                            JumpToOffset(offset);
                        else
                            goto default;
                        break;
                    case Operator.JumpIfOne:
                        if (registers[register] == 1)
                            JumpToOffset(offset);
                        else
                            goto default;
                        break;
                    default:
                        JumpToOffset(1);
                        break;
                }
            }

            private void JumpToOffset(int offset) => instructionIndex += offset;

            public uint GetRegisterValue(char name) => registers[name];
            public void SetRegisterValue(char name, uint value) => registers[name] = value;
        }

        private enum Operator
        {
            Halve,
            Triple,
            Increase,
            Jump,
            JumpIfEven,
            JumpIfOne,
        }

        private record Instruction(Operator Operator, string[] Arguments)
        {
            private static readonly Regex statPattern = new(@"(?'operator'\w*) (?'arguments'.*)", RegexOptions.Compiled);

            public static Instruction Parse(string s)
            {
                var groups = statPattern.Match(s).Groups;
                var op = ParseOperator(groups["operator"].Value);
                var arguments = groups["arguments"].Value.Split(", ");
                return new(op, arguments);
            }

            private static Operator ParseOperator(string raw) => raw switch
            {
                "hlf" => Operator.Halve,
                "tpl" => Operator.Triple,
                "inc" => Operator.Increase,
                "jmp" => Operator.Jump,
                "jie" => Operator.JumpIfEven,
                "jio" => Operator.JumpIfOne,
            };
        }
    }
}
