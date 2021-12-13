using System.Threading.Tasks;

namespace AdventOfCode.Problems.Year2019.Utilities;

public class IntcodeComputer
{
    private readonly long[] staticMemory;
    private readonly long[] memory = new long[100000];

    private readonly List<long> buffer = new();
    private long lastOutput;

    private int bufferIndex = 0;
    private int relativeModeOffset = 0;
    private int executionPointer = 0;

    public VMState State { get; private set; } = VMState.Standby;

    public bool IsStandby => State == VMState.Standby;
    public bool IsRunning => State == VMState.Running;
    public bool IsPaused => State == VMState.Paused;
    public bool IsHalted => State == VMState.Halted;

    public event InputReader InputRequested;
    public event OutputReader OutputWritten;

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

        State = VMState.Running;

        while (true)
        {
            var opcode = (Opcode)(memory[executionPointer] % 100);
            if (opcode == Opcode.Halt)
                break;

            var parameterModes = new ParameterMode[OpcodeInformation.MaxParameterCount];
            int d = 100;
            for (int a = 0; a < OpcodeInformation.MaxParameterCount; a++, d *= 10)
                parameterModes[a] = (ParameterMode)(memory[executionPointer] / d % 10);

            bool pointerChanged = false;
            bool shouldReturn = false;

            ExecuteOperation();
            if (!pointerChanged)
                executionPointer += GetPointerIncrement();
            if (shouldReturn)
            {
                State = VMState.Paused;
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
            int GetPointerIncrement() => OpcodeInformation.ArgumentCount(opcode) + 1;
        }

        State = VMState.Halted;

        return lastOutput;
    }

    // Could the compiler not make something to automatically return Task.CompletedTask on fully synced asuyc methods?
    public Task RunAsync(bool pauseOnOutput = false, bool pauseOnInput = false, long[] m = null, params long[] inputBuffer)
    {
        Run(pauseOnOutput, pauseOnInput, m, inputBuffer);
        return Task.CompletedTask;
    }
    public Task RunUntilOutputAsync(long[] m = null, params long[] inputBuffer)
    {
        RunUntilOutput(m, inputBuffer);
        return Task.CompletedTask;
    }
    public Task RunUntilRequestedInputAsync(long[] m = null, params long[] inputBuffer)
    {
        RunUntilRequestedInput(m, inputBuffer);
        return Task.CompletedTask;
    }
    public Task RunToHaltAsync(long[] m = null, params long[] inputBuffer)
    {
        RunToHalt(m, inputBuffer);
        return Task.CompletedTask;
    }

    public void ResetEvents()
    {
        InputRequested = null;
        OutputWritten = null;
    }
    public void Reset()
    {
        buffer.Clear();
        bufferIndex = 0;
        relativeModeOffset = 0;
        executionPointer = 0;
        State = VMState.Standby;
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

    public class OpcodeInformation
    {
        public const int MaxParameterCount = 3;

        private static readonly Dictionary<Opcode, int> argumentCounts = new();

        static OpcodeInformation()
        {
            // What the fuck went wrong with reflection
            foreach (var v in typeof(Opcode).GetEnumValues())
                argumentCounts.Add((Opcode)v, (typeof(Opcode).GetMember(v.ToString()).First().GetCustomAttributes(typeof(ArgumentCountAttribute), false).First() as ArgumentCountAttribute).ArgumentCount);
        }

        public static int ArgumentCount(Opcode opcode) => argumentCounts[opcode];
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
