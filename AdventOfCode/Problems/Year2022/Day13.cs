using Garyon.Objects.Enumerators;
using System.Diagnostics;

namespace AdventOfCode.Problems.Year2022;

public class Day13 : Problem<int>
{
    private ImmutableArray<PacketPair> packetPairs;

    public override int SolvePart1()
    {
        int indexSum = 0;

        foreach (var (index, packetPair) in packetPairs.WithIndex())
        {
            bool ordered = packetPair.IsOrdered();
            if (!ordered)
                continue;

            indexSum += index + 1;
        }
        return indexSum;
    }
    public override int SolvePart2()
    {
        var pairList = new PacketPairList(packetPairs);
        var packetList = pairList.GetAllPackets();

        var divisorPacket2 = Packet.Parse("[[2]]");
        var divisorPacket6 = Packet.Parse("[[6]]");

        packetList.Add(divisorPacket2);
        packetList.Add(divisorPacket6);

        packetList.Sort();

        int divisorPacket2Index = packetList.IndexOf(divisorPacket2) + 1;
        int divisorPacket6Index = packetList.IndexOf(divisorPacket6) + 1;
        return divisorPacket2Index * divisorPacket6Index;
    }

    protected override void LoadState()
    {
        var fileSpan = FileContents.AsSpan();
        var lines = fileSpan.EnumerateLines();

        var packets = new List<Packet>();
        foreach (var line in lines)
        {
            if (line.Length is 0)
                continue;

            packets.Add(Packet.Parse(line));
        }

        var pairBuilder = ImmutableArray.CreateBuilder<PacketPair>(packets.Count / 2);
        for (int i = 0; i < packets.Count; i += 2)
        {
            var left = packets[i];
            var right = packets[i + 1];
            var pair = new PacketPair(left, right);
            pairBuilder.Add(pair);
        }

        packetPairs = pairBuilder.ToImmutable();
    }

    private class PacketPairList
    {
        public ImmutableArray<PacketPair> PacketPairs { get; }

        public PacketPairList(ImmutableArray<PacketPair> packetPairs)
        {
            PacketPairs = packetPairs;
        }

        public List<Packet> GetAllPackets()
        {
            var result = new List<Packet>(PacketPairs.Length * 2);

            foreach (var packetPair in PacketPairs)
            {
                result.Add(packetPair.Left);
                result.Add(packetPair.Right);
            }

            return result;
        }
    }

    private record PacketPair(Packet Left, Packet Right)
    {
        public bool IsOrdered()
        {
            return Left.CompareTo(Right) < 0;
        }
    }

    private interface IPacketData : IComparable<IPacketData>, IEquatable<IPacketData>
    {
        public abstract IEnumerable<IPacketData> EnumerateNestedData();
    }

    private record Packet(IList<IPacketData> DataList)
        : PacketDataList(DataList)
    {
        private Packet(PacketDataList packetList)
            : this(packetList.DataList) { }

        public static Packet Parse(SpanString spanString)
        {
            var dataList = Parse(ref spanString);
            return new(dataList);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    private record PacketDataList(IList<IPacketData> DataList)
        : IPacketData
    {
        public IEnumerable<IPacketData> EnumerateNestedData() => DataList;

        public int CompareTo(IPacketData other)
        {
            switch (other)
            {
                case SinglePacketData single:
                    return -single.CompareTo(this);

                case PacketDataList otherList:
                    int countComparison = DataList.Count.CompareTo(otherList.DataList.Count);

                    int minCount = Math.Min(DataList.Count, otherList.DataList.Count);
                    for (int i = 0; i < minCount; i++)
                    {
                        int nestedComparison = DataList[i].CompareTo(otherList.DataList[i]);
                        if (nestedComparison is not 0)
                            return nestedComparison;
                    }

                    return countComparison;
            }

            return 0;
        }

        public static PacketDataList Parse(ref SpanString spanString)
        {
            var initialSpanString = spanString;

            // The data list parsing assumes that the initial character is the [
            char first = spanString[0];
            Debug.Assert(first is '[');

            spanString.AdvanceSliceRef(1);

            var data = new List<IPacketData>();

            while (true)
            {
                char currentChar = spanString[0];

                switch (currentChar)
                {
                    case '[':
                        var nestedList = Parse(ref spanString);
                        data.Add(nestedList);
                        break;

                    case ']':
                        spanString.AdvanceSliceRef(1);
                        goto finishParse;

                    case ',':
                        spanString.AdvanceSliceRef(1);
                        break;

                    default:
                        var single = SinglePacketData.Parse(ref spanString);
                        data.Add(single);
                        break;
                }
            }

        finishParse:
            return new(data);
        }

        public override string ToString()
        {
            return $"[{string.Join(',', DataList)}]";
        }

        public bool Equals(IPacketData other)
        {
            if (other is not PacketDataList packetList)
                return false;

            return DataList.SequenceEqual(packetList.DataList);
        }
    }
    private record SinglePacketData(int Value)
        : IPacketData
    {
        public int CompareTo(SinglePacketData singlePacketData)
        {
            return Value.CompareTo(singlePacketData.Value);
        }

        public int CompareTo(IPacketData right)
        {
            switch (right)
            {
                case SinglePacketData single:
                    return CompareTo(single);

                case null:
                    return 1;
            }

            var nextEnumerated = right.EnumerateNestedData();
            bool nextHasMoreElements = false;
            while (true)
            {
                if (nextEnumerated is null)
                {
                    // There is no element on the right hand side, but we do
                    return 1;
                }

                if (nextEnumerated is IEnumerable<SinglePacketData> single)
                {
                    int comparison = CompareTo(single.First());

                    if (comparison is 0)
                    {
                        if (nextHasMoreElements)
                            return -1;
                    }
                    return comparison;
                }

                int nextCount = nextEnumerated.Count();
                nextHasMoreElements |= nextCount > 1;

                nextEnumerated = nextEnumerated.FirstOrDefault()?.EnumerateNestedData();
            }
        }

        public IEnumerable<IPacketData> EnumerateNestedData() => new SingleElementCollection<SinglePacketData>(this);

        public static SinglePacketData Parse(ref SpanString spanString)
        {
            int value = spanString.ParseFirstInt32(0, out int next);
            spanString.AdvanceSliceRef(next);
            return new(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(IPacketData other)
        {
            return other is SinglePacketData single
                && single.Value == Value;
        }
    }
}
