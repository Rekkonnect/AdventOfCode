using AdventOfCode.Problems.Year2019.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace AdventOfCode.Problems.Year2019;

public class Day23 : Problem<long>
{
    [PartSolution(PartSolutionStatus.WIP)]
    public override long SolvePart1() => RunPart(Part1GeneralRunner);
    public override long SolvePart2() => RunPart(Part2GeneralRunner);

    private long Part1GeneralRunner(ComputerNetwork network)
    {
        network.PacketEnqueued += PacketEnqueued;

        long result = -1;

        network.RunNetwork();

        while (result == -1)
            Thread.Sleep(15);

        return result;

        void PacketEnqueued(Packet packet)
        {
            Console.WriteLine(packet);
            if (packet.Address == 255)
                result = packet.Y;
        }
    }

    private long Part2GeneralRunner(ComputerNetwork network)
    {
        return 0;
    }

    private T RunPart<T>(GeneralRunner<T> runner)
    {
        var program = FileContents;

        var network = new ComputerNetwork(program, 50);

        return runner(network);
    }

    private delegate T GeneralRunner<T>(ComputerNetwork network);

    private class ComputerNetwork
    {
        private readonly IntcodeComputer[] computers;
        private readonly Dictionary<int, Queue<Packet>> packetQueues = new Dictionary<int, Queue<Packet>>();
        private readonly Dictionary<int, Packet> incompletePackets = new Dictionary<int, Packet>();

        public readonly int ComputerCount;

        public event Action<Packet> PacketEnqueued;

        public ComputerNetwork(string program, int computerCount)
        {
            computers = new IntcodeComputer[ComputerCount = computerCount];
            for (int i = 0; i < computerCount; i++)
            {
                packetQueues.Add(i, new Queue<Packet>());
                incompletePackets.Add(i, null);

                int address = i; // copy the variable to avoid confusion in delegates
                var c = computers[i] = new IntcodeComputer(program);
                c.BufferInput(i);
                c.InputRequested += () => InputRequested(address);
                c.OutputWritten += o => OutputWritten(address, o);
            }
        }

        public void RunNetwork()
        {
            Task.Run(KeepNetworkAlive);
        }

        private long InputRequested(int address)
        {
            return -1;

            packetQueues.TryGetValue(address, out var q);
            if (q == null || q.Count == 0)
                return -1;
            long result = q.Peek().ProduceInput(out bool shouldDequeuePacket);
            if (shouldDequeuePacket)
                q.Dequeue();
            return result;
        }
        private void OutputWritten(int address, long output)
        {
            incompletePackets.TryAdd(address, null);
            var packet = incompletePackets[address];
            if (packet == null)
                incompletePackets[address] = packet = new Packet();
            if (packet.RecordOutput((long)output))
            {
                //packetQueues.TryAdd(packet.Address, new Queue<Packet>());
                //packetQueues[packet.Address].Enqueue(packet);

                if (packet.Address < ComputerCount)
                    computers[packet.Address].BufferInput(packet.X, packet.Y);

                incompletePackets[address] = null;
                PacketEnqueued?.Invoke(packet);
            }
        }
        private async Task KeepNetworkAlive()
        {
            bool[] halted = new bool[ComputerCount];
            int index = -1;
            int nextAddress = 0;
            PacketEnqueued += RegisterProducedPacket;

            while (!AreAllHalted())
            {
                index++;
                index %= ComputerCount;

                if (halted[index])
                    continue;

                await RunComputer(index);
                computers[index].BufferInput(-1);
                halted[index] = computers[index].IsHalted;
            }

            bool AreAllHalted()
            {
                for (int i = 0; i < ComputerCount; i++)
                    if (!halted[i])
                        return false;
                return true;
            }
            void RegisterProducedPacket(Packet p)
            {
                nextAddress = p.Address;
            }
        }
        private async Task RunComputer(int index)
        {
            try
            {
                await computers[index].RunUntilRequestedInputAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }

    private class Packet
    {
        private int inputIndex;
        private int outputIndex;

        public int Address;
        public long X, Y;

        public long ProduceInput(out bool shouldDequeuePacket)
        {
            shouldDequeuePacket = inputIndex == 1;
            return RegisterInput();
        }
        public bool RecordOutput(long value)
        {
            RegisterOutput(value);
            return outputIndex == 3;
        }

        private long RegisterInput() => inputIndex++ switch
        {
            0 => X,
            1 => Y,
        };
        private long RegisterOutput(long value) => outputIndex++ switch
        {
            0 => Address = (int)value,
            1 => X = value,
            2 => Y = value,
        };

        public override string ToString() => $"{Address} - ({X}, {Y})";
    }
}
