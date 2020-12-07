using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode.Problems.Year2019.Utilities
{
    public class IntcodeComputer
    {
        private static Dictionary<Opcode, int> argumentCounts = new Dictionary<Opcode, int>();

        public const int MaxParameterCount = 3;

        private long[] staticMemory;
        private long[] memory = new long[100000];

        private List<long> buffer = new List<long>();
        private long lastOutput;

        private int bufferIndex = 0;
        private int relativeModeOffset = 0;
        private int executionPointer = 0;

        private VMState state = VMState.Standby;

        public bool IsStandby => state == VMState.Standby;
        public bool IsRunning => state == VMState.Running;
        public bool IsPaused => state == VMState.Paused;
        public bool IsHalted => state == VMState.Halted;

        public event InputReader InputRequested;
        public event OutputReader OutputWritten;

        static IntcodeComputer()
        {
            // What the fuck went wrong with reflection
            foreach (var v in typeof(Opcode).GetEnumValues())
                argumentCounts.Add((Opcode)v, (typeof(Opcode).GetMember(v.ToString()).First().GetCustomAttributes(typeof(ArgumentCountAttribute), false).First() as ArgumentCountAttribute).ArgumentCount);
        }

        public IntcodeComputer() { }
        public IntcodeComputer(int[] m)
        {
            staticMemory = new long[m.Length];
            for (int i = 0; i < m.Length; i++)
                staticMemory[i] = m[i];
            InitializeMemory();
        }
        public IntcodeComputer(long[] m)
        {
            staticMemory = m;
            InitializeMemory();
        }
        public IntcodeComputer(string s)
        {
            var code = s.Split(',');
            staticMemory = new long[code.Length];
            for (int i = 0; i < code.Length; i++)
                staticMemory[i] = long.Parse(code[i]);
            InitializeMemory();
        }

        public long RunToHalt(long[] m = null, params long[] inputBuffer) => Run(false, false, m, inputBuffer);
        public long RunUntilOutput(long[] m = null, params long[] inputBuffer) => Run(true, false, m, inputBuffer);
        public long RunUntilRequestedInput(long[] m = null, params long[] inputBuffer) => Run(false, true, m, inputBuffer);
        public long Run(bool pauseOnOutput = false, bool pauseOnInput = false, long[] customMemory = null, params long[] inputBuffer)
        {
            buffer.AddRange(inputBuffer);

            if (!IsPaused)
                customMemory?.CopyTo(memory, 0);

            state = VMState.Running;

            while (true)
            {
                var opcode = (Opcode)(memory[executionPointer] % 100);
                if (opcode == Opcode.Halt)
                    break;

                var parameterModes = new ParameterMode[MaxParameterCount];
                int d = 100;
                for (int a = 0; a < MaxParameterCount; a++, d *= 10)
                    parameterModes[a] = (ParameterMode)(memory[executionPointer] / d % 10);

                bool pointerChanged = false;
                bool shouldReturn = false;

                ExecuteOperation();
                if (!pointerChanged)
                    executionPointer += GetPointerIncrement();
                if (shouldReturn)
                {
                    state = VMState.Paused;
                    return lastOutput;
                }

                void ExecuteOperation()
                {
                    var result = GetResult(pauseOnInput);
                    switch (opcode)
                    {
                        case Opcode.Add:
                        case Opcode.Multiply:
                        case Opcode.LessThan:
                        case Opcode.EqualTo:
                            WriteResult(2, result);
                            break;
                        case Opcode.Read:
                            if (shouldReturn = pauseOnInput)
                                return;
                            WriteResult(0, result);
                            break;
                        case Opcode.Write:
                            lastOutput = result;
                            shouldReturn = pauseOnOutput;
                            WriteOutput();
                            break;
                        case Opcode.JumpIfNotZero:
                        case Opcode.JumpIfZero:
                            if (pointerChanged = result > -1)
                                executionPointer = (int)result;
                            break;
                        case Opcode.SetRelativeOffset:
                            relativeModeOffset = (int)result;
                            break;
                    }
                }
                long GetResult(bool interceptInput = false)
                {
                    return opcode switch
                    {
                        Opcode.Add => GetArgument(0) + GetArgument(1),
                        Opcode.Multiply => GetArgument(0) * GetArgument(1),
                        Opcode.Read => ReadInput(interceptInput),
                        Opcode.Write => GetArgument(0),
                        Opcode.JumpIfNotZero => GetArgument(0) != 0 ? GetArgument(1) : -1,
                        Opcode.JumpIfZero => GetArgument(0) == 0 ? GetArgument(1) : -1,
                        Opcode.LessThan => GetArgument(0) < GetArgument(1) ? 1 : 0,
                        Opcode.EqualTo => GetArgument(0) == GetArgument(1) ? 1 : 0,
                        Opcode.SetRelativeOffset => relativeModeOffset + GetArgument(0),
                    };
                }
                long GetArgument(int index) => memory[GetAddressFromArgument(index)];
                void WriteResult(int index, long result) => memory[GetAddressFromArgument(index)] = result;
                int GetAddressFromArgument(int index)
                {
                    int offset = executionPointer + index + 1;
                    return parameterModes[index] switch
                    {
                        ParameterMode.Position => (int)memory[offset],
                        ParameterMode.Intermediate => offset,
                        ParameterMode.Relative => relativeModeOffset + (int)memory[offset],
                    };
                }
                int GetPointerIncrement() => argumentCounts[opcode] + 1;
            }

            state = VMState.Halted;

            return lastOutput;
        }
        public async Task RunAsync(bool pauseOnOutput = false, bool pauseOnInput = false, long[] m = null, params long[] inputBuffer) => await Task.Run(() => Run(pauseOnOutput, pauseOnInput, m, inputBuffer));
        public async Task RunUntilOutputAsync(long[] m = null, params long[] inputBuffer) => await Task.Run(() => RunUntilOutput(m, inputBuffer));
        public async Task RunUntilRequestedInputAsync(long[] m = null, params long[] inputBuffer) => await Task.Run(() => RunUntilRequestedInput(m, inputBuffer));
        public async Task RunToHaltAsync(long[] m = null, params long[] inputBuffer) => await Task.Run(() => RunToHalt(m, inputBuffer));

        public void Reset()
        {
            buffer.Clear();
            bufferIndex = 0;
            relativeModeOffset = 0;
            executionPointer = 0;
            state = VMState.Standby;
            InitializeMemory();
        }

        public void BufferInput(long input) => buffer.Add(input);
        public void BufferInput(params long[] input) => buffer.AddRange(input);

        public long GetMemoryAt(int address) => memory[address];
        public void SetMemoryAt(int address, long value) => memory[address] = value;
        public long GetStaticMemoryAt(int address) => staticMemory[address];
        public void SetStaticMemoryAt(int address, long value) => staticMemory[address] = value;

        private void InitializeMemory()
        {
            staticMemory.CopyTo(memory, 0);
        }

        private long ReadInput(bool interceptRequestedInput = false)
        {
            if (bufferIndex < buffer.Count)
            {
                bufferIndex++;
                return buffer[bufferIndex - 1];
            }
            if (interceptRequestedInput)
                return 0;
            return InputRequested?.Invoke() ?? default;
        }
        private void WriteOutput() => OutputWritten?.Invoke(lastOutput);

        public enum ParameterMode : byte
        {
            Position,
            Intermediate,
            Relative,
        }
        public enum Opcode : byte
        {
            [ArgumentCount(3)]
            Add = 1,
            [ArgumentCount(3)]
            Multiply = 2,
            [ArgumentCount(1)]
            Read = 3,
            [ArgumentCount(1)]
            Write = 4,
            [ArgumentCount(2)]
            JumpIfNotZero = 5,
            [ArgumentCount(2)]
            JumpIfZero = 6,
            [ArgumentCount(3)]
            LessThan = 7,
            [ArgumentCount(3)]
            EqualTo = 8,
            [ArgumentCount(1)]
            SetRelativeOffset = 9,

            [ArgumentCount(0)]
            Halt = 99,
        }
    }

    public delegate long InputReader();
    public delegate void OutputReader(long output);

    public enum VMState
    {
        Standby,
        Running,
        Paused,
        Halted,
    }
}
