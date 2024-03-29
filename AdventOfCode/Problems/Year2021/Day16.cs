﻿using AdventOfCode.Functions;
using Garyon.Objects;

namespace AdventOfCode.Problems.Year2021;

public class Day16 : Problem<int, ulong>
{
    private Transmission transmission;

    public override int SolvePart1()
    {
        return transmission.RootPacket.VersionSum;
    }
    public override ulong SolvePart2()
    {
        return transmission.RootOperatorPacket?.CalculatedValue ?? 0;
    }

    protected override void LoadState()
    {
        transmission = Transmission.Parse(FileContents.Trim());
    }
    protected override void ResetState()
    {
        transmission = null;
    }

#nullable enable

    private enum PacketType
    {
        Sum = 0,
        Product = 1,
        Minimum = 2,
        Maximum = 3,
        Literal = 4,
        GreaterThan = 5,
        LessThan = 6,
        EqualTo = 7,
    }

    private enum OperatorPacketLengthType
    {
        TotalLength = 0,
        SubpacketCount = 1,
    }
    private static int OperatorPacketLengthBits(OperatorPacketLengthType type) => type switch
    {
        OperatorPacketLengthType.TotalLength => 15,
        OperatorPacketLengthType.SubpacketCount => 11,
    };

    private record struct PacketHeaders(int Version, PacketType Type);

    private abstract class TransmissionReader
    {
        private readonly TransmissionReader? parentReader;
        private int currentIndex;

        protected readonly Transmission Transmission;
        public int CurrentIndex => parentReader?.CurrentIndex ?? currentIndex;

        protected TransmissionReader(Transmission source, int startIndex)
        {
            Transmission = source;
            currentIndex = startIndex;
        }
        protected TransmissionReader(TransmissionReader parent)
        {
            parentReader = parent.parentReader ?? parent;
            Transmission = parent.Transmission;
        }

        public int ReadBits(int length)
        {
            if (parentReader is not null)
                return parentReader.ReadBits(length);

            int bits = Transmission.BitsFrom(CurrentIndex, length);
            currentIndex += length;
            return bits;
        }
    }
    private sealed class PacketParser : TransmissionReader
    {
        public PacketParser(Transmission source, int startIndex)
            : base(source, startIndex) { }
        public PacketParser(TransmissionReader reader)
            : base(reader) { }

        public Packet Parse()
        {
            int version = ReadBits(3);
            var packetType = (PacketType)ReadBits(3);
            var headers = new PacketHeaders(version, packetType);
            return PacketContentParser.ContentParserForType(packetType, this).ConstructPacket(headers);
        }
    }
    private abstract class PacketContentParser : TransmissionReader
    {
        public PacketContentParser(TransmissionReader reader)
            : base(reader) { }

        public abstract Packet ConstructPacket(PacketHeaders headers);

        public static PacketContentParser ContentParserForType(PacketType type, TransmissionReader parent)
        {
            return type switch
            {
                PacketType.Literal => new LiteralPacketContentParser(parent),
                _ => new OperatorPacketContentParser(parent),
            };
        }
    }

    private sealed class LiteralPacketContentParser : PacketContentParser
    {
        public LiteralPacketContentParser(TransmissionReader parent)
            : base(parent) { }

        public override Packet ConstructPacket(PacketHeaders headers)
        {
            ulong value = 0;
            while (true)
            {
                var chunk = new LiteralValueChunk(ReadBits(5));
                value <<= 4;
                value |= chunk.UnsignedValueChunk;

                if (!chunk.HasNext)
                    break;
            }
            return new LiteralPacket(headers, value);
        }

        private record struct LiteralValueChunk(int ParsedBits)
        {
            public bool HasNext => (ParsedBits & 0b10000) != 0;
            public uint UnsignedValueChunk => (uint)ParsedBits & 0b1111U;
        }
    }
    private sealed class OperatorPacketContentParser : PacketContentParser
    {
        public OperatorPacketContentParser(TransmissionReader parent)
            : base(parent) { }

        public override Packet ConstructPacket(PacketHeaders headers)
        {
            var lengthType = (OperatorPacketLengthType)ReadBits(1);
            var lengthBits = OperatorPacketLengthBits(lengthType);
            int length = ReadBits(lengthBits);
            var packets = SubpacketParser.CreateFromType(lengthType, this).ParsePackets(length);
            return OperatorPacket.Create(headers, packets);
        }

        private abstract class SubpacketParser : TransmissionReader
        {
            protected SubpacketParser(TransmissionReader parent)
                : base(parent) { }

            public abstract Packet[] ParsePackets(int length);

            protected Packet ReadParseSubpacket() => new PacketParser(this).Parse();

            public static SubpacketParser CreateFromType(OperatorPacketLengthType type, TransmissionReader parent)
            {
                return type switch
                {
                    OperatorPacketLengthType.TotalLength => new TotalLengthSubpacketParser(parent),
                    OperatorPacketLengthType.SubpacketCount => new SubpacketCountSubpacketParser(parent),
                };
            }
        }
        private sealed class TotalLengthSubpacketParser : SubpacketParser
        {
            public TotalLengthSubpacketParser(TransmissionReader parent)
                : base(parent) { }

            public override Packet[] ParsePackets(int length)
            {
                int parseEndIndex = CurrentIndex + length;
                var packets = new List<Packet>();
                while (CurrentIndex < parseEndIndex)
                    packets.Add(ReadParseSubpacket());
                return packets.ToArray();
            }
        }
        private sealed class SubpacketCountSubpacketParser : SubpacketParser
        {
            public SubpacketCountSubpacketParser(TransmissionReader parent)
                : base(parent) { }

            public override Packet[] ParsePackets(int length)
            {
                var packets = new Packet[length];
                for (int i = 0; i < length; i++)
                    packets[i] = ReadParseSubpacket();
                return packets;
            }
        }
    }

    private abstract record Packet(PacketHeaders Headers)
    {
        public abstract int VersionSum { get; }
        public abstract ulong CalculatedValue { get; }
    }
    private sealed record LiteralPacket(PacketHeaders Headers, ulong Value)
        : Packet(Headers)
    {
        public override int VersionSum => Headers.Version;
        public override ulong CalculatedValue => Value;
    }
    private abstract record OperatorPacket(PacketHeaders Headers, Packet[] Subpackets)
        : Packet(Headers)
    {
        public sealed override int VersionSum => Headers.Version + Subpackets.Sum(packet => packet.VersionSum);

        public static OperatorPacket Create(PacketHeaders Headers, Packet[] Subpackets)
        {
            return Headers.Type switch
            {
                PacketType.Sum => new SumPacket(Headers, Subpackets),
                PacketType.Product => new ProductPacket(Headers, Subpackets),
                PacketType.Minimum => new MinimumPacket(Headers, Subpackets),
                PacketType.Maximum => new MaximumPacket(Headers, Subpackets),

                PacketType.GreaterThan => new GreaterThanPacket(Headers, Subpackets),
                PacketType.LessThan => new LessThanPacket(Headers, Subpackets),
                PacketType.EqualTo => new EqualToPacket(Headers, Subpackets),
            };
        }
    }

    private abstract record SubpacketBatchOperatorPacket(PacketHeaders Headers, Packet[] Subpackets)
        : OperatorPacket(Headers, Subpackets)
    {
        public IEnumerable<ulong> CalculatedValues => Subpackets.Select(packet => packet.CalculatedValue);
    }

    private sealed record SumPacket(PacketHeaders Headers, Packet[] Subpackets)
        : SubpacketBatchOperatorPacket(Headers, Subpackets)
    {
        public override ulong CalculatedValue => CalculatedValues.Sum();
    }
    private sealed record ProductPacket(PacketHeaders Headers, Packet[] Subpackets)
        : SubpacketBatchOperatorPacket(Headers, Subpackets)
    {
        public override ulong CalculatedValue => CalculatedValues.Product();
    }
    private sealed record MinimumPacket(PacketHeaders Headers, Packet[] Subpackets)
        : SubpacketBatchOperatorPacket(Headers, Subpackets)
    {
        public override ulong CalculatedValue => CalculatedValues.Min();
    }
    private sealed record MaximumPacket(PacketHeaders Headers, Packet[] Subpackets)
        : SubpacketBatchOperatorPacket(Headers, Subpackets)
    {
        public override ulong CalculatedValue => CalculatedValues.Max();
    }

    private abstract record ComparisonOperatorPacket(PacketHeaders Headers, Packet[] Subpackets)
        : OperatorPacket(Headers, Subpackets)
    {
        public Packet Left => Subpackets[0];
        public Packet Right => Subpackets[1];

        public sealed override ulong CalculatedValue => Convert.ToUInt64(Left.CalculatedValue.MatchesComparisonResult(Right.CalculatedValue, TargetComparison));
        protected abstract ComparisonResult TargetComparison { get; }
    }

    private sealed record GreaterThanPacket(PacketHeaders Headers, Packet[] Subpackets)
        : ComparisonOperatorPacket(Headers, Subpackets)
    {
        protected override ComparisonResult TargetComparison => ComparisonResult.Greater;
    }
    private sealed record LessThanPacket(PacketHeaders Headers, Packet[] Subpackets)
        : ComparisonOperatorPacket(Headers, Subpackets)
    {
        protected override ComparisonResult TargetComparison => ComparisonResult.Less;
    }
    private sealed record EqualToPacket(PacketHeaders Headers, Packet[] Subpackets)
        : ComparisonOperatorPacket(Headers, Subpackets)
    {
        protected override ComparisonResult TargetComparison => ComparisonResult.Equal;
    }

    // BitArray is not desirable for this purpose
    // I'd rather avoid having to write an alternative to that class
    private sealed class Transmission
    {
        private readonly byte[] bytes;

        public int Length => bytes.Length * 8;
        public Packet RootPacket { get; }
        public OperatorPacket? RootOperatorPacket => RootPacket as OperatorPacket;

        private Transmission(byte[] packetContents)
        {
            bytes = packetContents;
            RootPacket = new PacketParser(this, 0).Parse();
        }

        public int BitsFrom(int startIndex, int length)
        {
            // Grossly unoptimized, too lazy because fuck bithacks
            int result = 0;
            for (int i = 0; i < length; i++)
                result |= BitAt(startIndex + i) << (length - 1 - i);

            return result;
        }
        private int BitAt(int index) => Convert.ToInt32(MaskedBitAt(index) != 0);
        private int MaskedBitAt(int index) => bytes[index / 8] & (0b1000_0000 >> (index % 8));

        public static Transmission Parse(string hex)
        {
            int byteCount = (hex.Length + 1) / 2;
            var bytes = new byte[byteCount];
            for (int i = 0; i < bytes.Length; i++)
            {
                AdjustByte(ref bytes[i], hex[i * 2], 1);
                AdjustByte(ref bytes[i], hex[i * 2 + 1], 0);
            }
            return new(bytes);
        }
        private static void AdjustByte(ref byte b, char hexChar, int index)
        {
            b |= (byte)(FromHex(hexChar) << (index * 4));
        }
        private static int FromHex(char c) => c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
        };
    }
}
