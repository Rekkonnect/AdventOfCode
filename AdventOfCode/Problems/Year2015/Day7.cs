using AdventOfCSharp;
using Garyon.DataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2015;

public class Day7 : Problem<int>
{
    private Instruction[] instructions;

    public override int SolvePart1()
    {
        var system = new WireSystem(new(instructions));
        return system["a"];
    }
    public override int SolvePart2()
    {
        var instructionCollection = new InstructionCollection(instructions);
        var system = new WireSystem(instructionCollection);
        ushort value = system["a"];
        instructionCollection.SetAssignmentInstruction("b", Instruction.Assignment(value.ToString(), "b"));
        system.Reset();
        value = system["a"];
        return value;
    }

    protected override void ResetState()
    {
        instructions = null;
    }
    protected override void LoadState()
    {
        instructions = ParsedFileLines(Instruction.Parse);
    }

    private class WireSystem
    {
        private FlexibleDictionary<string, ushort?> wires = new();

        private InstructionCollection instructions;

        public WireSystem(InstructionCollection instructions)
        {
            this.instructions = instructions;
        }

        public void Reset() => wires.Clear();

        private void RunInstruction(Instruction instruction)
        {
            if (!int.TryParse(instruction.Argument0, out int value0))
                value0 = this[instruction.Argument0];

            int value1 = 0;
            if (instruction.Argument1 is not null)
            {
                if (!int.TryParse(instruction.Argument1, out value1))
                    value1 = this[instruction.Argument1];
            }

            int resultValue = instruction.Operator switch
            {
                Operator.NOP => value0,
                Operator.NOT => ~value0,
                Operator.AND => value0 & value1,
                Operator.OR => value0 | value1,
                Operator.XOR => value0 ^ value1,
                Operator.LSHIFT => value0 << value1,
                Operator.RSHIFT => value0 >> value1,
            };

            wires[instruction.AssignedWire] = (ushort)resultValue;
        }

        public ushort this[string wireName]
        {
            get
            {
                var value = wires[wireName];
                if (value is null)
                {
                    RunInstruction(instructions.GetAssignmentInstruction(wireName));
                    value = wires[wireName];
                }
                return value.Value;
            }
        }
    }

    private class InstructionCollection : IEnumerable<Instruction>
    {
        private IEnumerable<Instruction> instructions;

        private FlexibleDictionary<string, Instruction> assignmentInstructions = new();

        public InstructionCollection(IEnumerable<Instruction> instructions)
        {
            this.instructions = instructions;
            foreach (var instruction in instructions)
                assignmentInstructions[instruction.AssignedWire] = instruction;
        }

        public Instruction GetAssignmentInstruction(string assignedWire) => assignmentInstructions[assignedWire];
        public void SetAssignmentInstruction(string assignedWire, Instruction instruction) => assignmentInstructions[assignedWire] = instruction;

        public IEnumerator<Instruction> GetEnumerator() => instructions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => instructions.GetEnumerator();
    }

    private record Instruction(Operator Operator, string Argument0, string Argument1, string AssignedWire)
    {
        private static readonly Regex binaryOperatorPattern = new(@"([\d\w]*) (\w*) ([\d\w]*)", RegexOptions.Compiled);
        private static readonly Regex unaryOperatorPattern = new(@"(\w*) ([\d\w]*)", RegexOptions.Compiled);
        private static readonly Regex noOperatorPattern = new(@"([\d\w]*)", RegexOptions.Compiled);
        private static readonly Regex instructionPattern = new(@"([\d\w ]*) \-\> (\w*)", RegexOptions.Compiled);

        public static Instruction Assignment(string argument, string assignedWire) => new(Operator.NOP, argument, null, assignedWire);

        public static Instruction Parse(string s)
        {
            var instructionMatch = instructionPattern.Match(s);
            var leftHand = instructionMatch.Groups[1].Value;
            var assignedWire = instructionMatch.Groups[2].Value;

            Operator op = Operator.NOP;
            string arg0 = null;
            string arg1 = null;

            var binaryMatch = binaryOperatorPattern.Match(leftHand);
            if (binaryMatch.Success)
            {
                arg0 = binaryMatch.Groups[1].Value;
                op = ParseOperator(binaryMatch.Groups[2].Value);
                arg1 = binaryMatch.Groups[3].Value;
                goto end;
            }

            var unaryMatch = unaryOperatorPattern.Match(leftHand);
            if (unaryMatch.Success)
            {
                op = ParseOperator(unaryMatch.Groups[1].Value);
                arg0 = unaryMatch.Groups[2].Value;
                goto end;
            }

            var noOperatorMatch = noOperatorPattern.Match(leftHand);
            arg0 = noOperatorMatch.Groups[1].Value;

        end:
            return new(op, arg0, arg1, assignedWire);
        }

        private static Operator ParseOperator(string s)
        {
            return s switch
            {
                "NOT" => Operator.NOT,
                "AND" => Operator.AND,
                "OR" => Operator.OR,
                "XOR" => Operator.XOR,
                "LSHIFT" => Operator.LSHIFT,
                "RSHIFT" => Operator.RSHIFT,
            };
        }

        public override string ToString()
        {
            var result = "";
            if (Argument1 is not null)
                result += $"{Argument0} ";
            if (Operator is not Operator.NOP)
                result += $"{Operator} ";

            result += $"{Argument1 ?? Argument0} -> {AssignedWire}";
            return result;
        }
    }

    private enum Operator
    {
        NOP,
        AND,
        OR,
        XOR,
        NOT,
        LSHIFT,
        RSHIFT,
    }
}
