using AdventOfCode.Functions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AdventOfCode.Problems.Year2018;

public partial class Day16 : Problem<int>
{
    private MonitoredOperationResult[] monitoredResults;
    private Operation[] operations;

    public override int SolvePart1()
    {
        return monitoredResults.Count(r => r.CandidateOpcodeCount >= 3);
    }
    public override int SolvePart2()
    {
        var device = new Device(operations, monitoredResults);
        device.RunProgram();
        return device.CurrentRegisters[0];
    }

    protected override void LoadState()
    {
        var sections = NormalizedFileContents.Split("\n\n\n\n");
        monitoredResults = sections[0].Split("\n\n").Select(raw => MonitoredOperationResult.Parse(raw)).ToArray();
        operations = sections[1].Split("\n").Select(Operation.Parse).ToArray();
    }
    protected override void ResetState()
    {
        monitoredResults = null;
        operations = null;
    }

    private class Device
    {
        // Please for the fucking love of C# improve enum types

        private Operation[] operations;

        private Opcode[] opcodeNumbers = new Opcode[(int)Opcode.OpcodeCount];
        private OpcodeCandidates[] opcodeCandidates = new OpcodeCandidates[(int)Opcode.OpcodeCount];

        public RegisterState CurrentRegisters { get; private set; }

        public Device(Operation[] programOperations, IEnumerable<MonitoredOperationResult> monitoredOperationResults)
        {
            operations = programOperations;
            AnalyzeMonitoredOperationResults(monitoredOperationResults);
        }

        public void RunProgram()
        {
            foreach (var operation in operations)
                CurrentRegisters = CurrentRegisters.PerformOperation(operation, opcodeNumbers[operation.Opcode]);
        }

        private void AnalyzeMonitoredOperationResults(IEnumerable<MonitoredOperationResult> results)
        {
            opcodeCandidates.Fill(OpcodeCandidates.AllCandidateOpcodes);

            foreach (var result in results)
            {
                var startingCandidates = opcodeCandidates[result.Operation.Opcode];
                opcodeCandidates[result.Operation.Opcode] = result.GetOpcodeCandidates(startingCandidates);
            }

            var remainingIndices = new HashSet<int>(Enumerable.Range(0, (int)Opcode.OpcodeCount));
            while (remainingIndices.Any())
            {
                var remainingIndicesArray = remainingIndices.ToArray();

                foreach (int index in remainingIndicesArray)
                {
                    if (!opcodeCandidates[index].TryGetSingleOpcodeCandidate(out var opcode))
                        continue;

                    opcodeNumbers[index] = opcode;
                    foreach (var other in remainingIndices)
                        opcodeCandidates[other].ExcludeOpcode(opcode);

                    remainingIndices.Remove(index);
                }
            }
        }
    }

    private partial record MonitoredOperationResult(RegisterState InitialState, Operation Operation, RegisterState ResultingState)
    {
        private static readonly Regex resultPattern = ResultRegex();

        public OpcodeCandidates OpcodeCandidates => InitialState.GetCandidates(Operation, ResultingState);
        public int CandidateOpcodeCount => OpcodeCandidates.CandidateOpcodeCount;

        public OpcodeCandidates GetOpcodeCandidates(OpcodeCandidates knownCandidates) => InitialState.GetCandidates(Operation, ResultingState, knownCandidates);

        public static MonitoredOperationResult Parse(string raw)
        {
            var groups = resultPattern.Match(raw).Groups;

            var initial = ParseState(groups["initial"].Value);
            var operation = Operation.Parse(groups["operation"].Value);
            var result = ParseState(groups["result"].Value);

            return new(initial, operation, result);
        }

        private static RegisterState ParseState(string raw)
        {
            return new(raw.ParseInt32Array(", "));
        }

        [GeneratedRegex("Before: \\[(?'initial'[\\d, ]*)\\]\\n(?'operation'[\\d ]*)\\nAfter:  \\[(?'result'[\\d, ]*)\\]", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex ResultRegex();
    }

    private record Operation(int Opcode, int InputA, int InputB, int Output)
    {
        public static Operation Parse(string raw)
        {
            var split = raw.Split(' ');

            int opcode = split[0].ParseInt32();
            int inputA = split[1].ParseInt32();
            int inputB = split[2].ParseInt32();
            int output = split[3].ParseInt32();

            return new(opcode, inputA, inputB, output);
        }
    }

    private unsafe struct RegisterState
    {
        public const int RegisterCount = 4;

        private fixed int registers[RegisterCount];

        public RegisterState(int[] registerArray)
        {
            for (int i = 0; i < RegisterCount; i++)
                registers[i] = registerArray[i];
        }
        public RegisterState(RegisterState other)
        {
            // The good ol' C way
            fixed (int* r = registers)
                Unsafe.CopyBlock(r, other.registers, RegisterCount * sizeof(int));
        }

        public RegisterState WithRegister(int index, int value)
        {
            var result = new RegisterState(this);
            result.registers[index] = value;
            return result;
        }

        public OpcodeCandidates GetCandidates(Operation operation, RegisterState targetState)
        {
            return GetCandidates(operation, targetState, OpcodeCandidates.AllCandidateOpcodes);
        }
        public OpcodeCandidates GetCandidates(Operation operation, RegisterState targetState, OpcodeCandidates knownCandidates)
        {
            for (Opcode opcode = default; opcode < Opcode.OpcodeCount; opcode++)
            {
                if (!knownCandidates.IncludesOpcode(opcode))
                    continue;

                if (!MatchesTarget(operation, opcode, targetState))
                    knownCandidates.ExcludeOpcode(opcode);
            }

            return knownCandidates;
        }
        public bool MatchesTarget(Operation operation, Opcode opcode, RegisterState targetState)
        {
            return PerformOperation(operation, opcode) == targetState;
        }
        public RegisterState PerformOperation(Operation operation, Opcode opcode)
        {
            var analyzedOpcode = new AnalyzedOpcode(opcode);

            int value0 = operation.InputA;
            int value1 = operation.InputB;

            fixed (int* r = registers)
            {
                if (analyzedOpcode.Register0)
                    value0 = r[value0];
                if (analyzedOpcode.Register1)
                    value1 = r[value1];
            }

            int output = analyzedOpcode.Category switch
            {
                OperationCategory.Add => value0 + value1,
                OperationCategory.Multiply => value0 * value1,
                OperationCategory.BitwiseAND => value0 & value1,
                OperationCategory.BitwiseOR => value0 | value1,
                OperationCategory.Set => value0,
                OperationCategory.GreaterThan => Convert.ToInt32(value0 > value1),
                OperationCategory.Equal => Convert.ToInt32(value0 == value1),
            };

            return WithRegister(operation.Output, output);
        }

        public static bool operator ==(RegisterState left, RegisterState right) => left.Equals(right);
        public static bool operator !=(RegisterState left, RegisterState right) => !left.Equals(right);

        public int this[int register]
        {
            get => registers[register];
            set => registers[register] = value;
        }

        public bool Equals(RegisterState other)
        {
            for (int i = 0; i < RegisterCount; i++)
                if (registers[i] != other.registers[i])
                    return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is RegisterState state && Equals(state);
        }
        public override int GetHashCode()
        {
            var code = new HashCode();

            for (int i = 0; i < RegisterCount; i++)
                code.Add(registers[i]);

            return code.ToHashCode();
        }
        public override string ToString()
        {
            return $"{this[0]}, {this[1]}, {this[2]}, {this[3]}";
        }
    }

    private struct OpcodeCandidates
    {
        public static OpcodeCandidates AllCandidateOpcodes => new(ushort.MaxValue);

        public ushort CandidateOpcodes { get; private set; }
        public int CandidateOpcodeCount => BitOperations.PopCount(CandidateOpcodes);

        public Opcode SingleOpcodeCandidate => (Opcode)BitManipulations.DecodeIndex(CandidateOpcodes);

        public OpcodeCandidates(ushort candidateOpcodes)
        {
            CandidateOpcodes = candidateOpcodes;
        }

        public bool TryGetSingleOpcodeCandidate(out Opcode singleOpcodeCandidate)
        {
            return (singleOpcodeCandidate = SingleOpcodeCandidate) is not <= Opcode.Invalid and not >= Opcode.OpcodeCount;
        }

        public void IncludeOpcode(Opcode opcode)
        {
            CandidateOpcodes = (ushort)(CandidateOpcodes | GetOpcodeFlag(opcode));
        }
        public void ExcludeOpcode(Opcode opcode)
        {
            CandidateOpcodes = (ushort)(CandidateOpcodes & ~GetOpcodeFlag(opcode));
        }

        public bool IncludesOpcode(Opcode opcode) => (CandidateOpcodes & GetOpcodeFlag(opcode)) is not 0;

        public static OpcodeCandidates operator |(OpcodeCandidates left, OpcodeCandidates right) => new((ushort)(left.CandidateOpcodes | right.CandidateOpcodes));
        public static OpcodeCandidates operator &(OpcodeCandidates left, OpcodeCandidates right) => new((ushort)(left.CandidateOpcodes & right.CandidateOpcodes));
        public static OpcodeCandidates operator ^(OpcodeCandidates left, OpcodeCandidates right) => new((ushort)(left.CandidateOpcodes ^ right.CandidateOpcodes));

        private static uint GetOpcodeFlag(Opcode opcode) => 1U << (int)opcode;

        public override string ToString()
        {
            return CandidateOpcodes.GetBinaryRepresentation();
        }
    }

    private struct AnalyzedOpcode
    {
        public OperationCategory Category { get; }
        public OperationInputType InputType { get; }

        public bool Register0 => InputType.HasFlag(OperationInputType.Register0);
        public bool Register1 => InputType.HasFlag(OperationInputType.Register1);

        public AnalyzedOpcode(Opcode opcode)
        {
            Category = GetCategory(opcode);
            InputType = GetInputType(opcode);
        }

        public override string ToString()
        {
            return $"{Category} {InputType}";
        }

        private static OperationCategory GetCategory(Opcode opcode) => opcode switch
        {
            Opcode.AddImmediate or
            Opcode.AddRegister => OperationCategory.Add,

            Opcode.MultiplyImmediate or
            Opcode.MultiplyRegister => OperationCategory.Multiply,

            Opcode.BitwiseANDImmediate or
            Opcode.BitwiseANDRegister => OperationCategory.BitwiseAND,

            Opcode.BitwiseORImmediate or
            Opcode.BitwiseORRegister => OperationCategory.BitwiseOR,

            Opcode.SetImmediate or
            Opcode.SetRegister => OperationCategory.Set,

            Opcode.GreaterThanImmediateRegister or
            Opcode.GreaterThanRegisterImmediate or
            Opcode.GreaterThanRegisterRegister => OperationCategory.GreaterThan,

            Opcode.EqualImmediateRegister or
            Opcode.EqualRegisterImmediate or
            Opcode.EqualRegisterRegister => OperationCategory.Equal,
        };
        private static OperationInputType GetInputType(Opcode opcode) => opcode switch
        {
            Opcode.SetImmediate => OperationInputType.ImmediateImmediate,

            Opcode.AddRegister or
            Opcode.MultiplyRegister or
            Opcode.BitwiseANDRegister or
            Opcode.BitwiseORRegister or
            Opcode.GreaterThanRegisterRegister or
            Opcode.EqualRegisterRegister => OperationInputType.RegisterRegister,

            Opcode.GreaterThanImmediateRegister or
            Opcode.EqualImmediateRegister => OperationInputType.ImmediateRegister,

            Opcode.AddImmediate or
            Opcode.MultiplyImmediate or
            Opcode.BitwiseANDImmediate or
            Opcode.BitwiseORImmediate or
            Opcode.SetRegister or
            Opcode.GreaterThanRegisterImmediate or
            Opcode.EqualRegisterImmediate => OperationInputType.RegisterImmediate,
        };
    }

    private enum OperationCategory
    {
        Add,
        Multiply,
        BitwiseAND,
        BitwiseOR,
        Set,
        GreaterThan,
        Equal,
    }
    [Flags]
    private enum OperationInputType
    {
        Register0 = 1,
        Register1 = 1 << 1,

        ImmediateImmediate = 0,
        ImmediateRegister = Register1,
        RegisterImmediate = Register0,
        RegisterRegister = Register0 | Register1,
    }

    private enum Opcode
    {
        Invalid = -1,

        AddRegister,
        AddImmediate,

        MultiplyRegister,
        MultiplyImmediate,

        BitwiseANDRegister,
        BitwiseANDImmediate,

        BitwiseORRegister,
        BitwiseORImmediate,

        SetRegister,
        SetImmediate,

        GreaterThanImmediateRegister,
        GreaterThanRegisterImmediate,
        GreaterThanRegisterRegister,

        EqualImmediateRegister,
        EqualRegisterImmediate,
        EqualRegisterRegister,

        // Count field
        OpcodeCount,
    }
}
