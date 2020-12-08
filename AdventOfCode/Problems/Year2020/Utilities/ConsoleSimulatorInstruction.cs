using AdventOfCode.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdventOfCode.Problems.Year2020.Utilities
{
    public class ConsoleSimulatorInstruction
    {
        private static Dictionary<string, ConsoleSimulatorOperation> supportedOperationMnemonics = new();
        private static Dictionary<ConsoleSimulatorOperation, MnemonableInstructionInformationAttribute> supportedOperationAttributes = new();

        static ConsoleSimulatorInstruction()
        {
            var enumType = typeof(ConsoleSimulatorOperation);
            foreach (ConsoleSimulatorOperation v in enumType.GetEnumValues())
            {
                var memberInfo = enumType.GetMember(v.ToString()).First();
                var attribute = memberInfo.GetCustomAttribute(typeof(MnemonableInstructionInformationAttribute), false) as MnemonableInstructionInformationAttribute;
                supportedOperationMnemonics.Add(attribute.Mnemonic, v);
                supportedOperationAttributes.Add(v, attribute);
            }
        }

        private int[] arguments;

        public ConsoleSimulatorOperation Operation { get; set; }

        public ConsoleSimulatorInstruction(ConsoleSimulatorOperation operation, int[] instructionArguments)
        {
            Operation = operation;
            arguments = instructionArguments;
        }

        public int ArgumentAt(int index) => arguments[index];

        public static ConsoleSimulatorInstruction Parse(string rawInstruction)
        {
            var split = rawInstruction.Split(' ');
            var operation = supportedOperationMnemonics[split[0]];
            var arguments = split.Skip(1).Select(s => int.Parse(s)).ToArray();
            return new(operation, arguments);
        }

        public override string ToString()
        {
            return $"{supportedOperationAttributes[Operation].Mnemonic} {arguments.Select(a => a.ToString()).Aggregate((a, b) => $"{a} {b}")}";
        }
    }
}
