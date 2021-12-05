using AdventOfCSharp;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2020;

public class Day5 : Problem<int>
{
    private IEnumerable<SeatCode> seats;

    public override int SolvePart1()
    {
        return seats.Max(s => s.SeatID);
    }
    public override int SolvePart2()
    {
        var (min, max) = seats.MinMax(s => s.SeatID);

        bool[] occupiedSeats = new bool[max + 1];
        foreach (var s in seats)
            occupiedSeats[s.SeatID] = true;

        for (int i = min; i < max; i++)
            if (!occupiedSeats[i])
                return i;

        return -1;
    }

    protected override void LoadState()
    {
        seats = ParsedFileLines(SeatCode.Parse);
    }
    protected override void ResetState()
    {
        seats = null;
    }

    private static string SeatCodeToBinary(string code) => code.Replace('F', '0').Replace('B', '1').Replace('L', '0').Replace('R', '1');

    private static int ParseFromBinaryRepresentation(string binary)
    {
        int result = 0;
        for (int i = 0; i < binary.Length; i++)
            if (binary[^(i + 1)] == '1')
                result |= 1 << i;
        return result;
    }

    private class SeatCode
    {
        public int SeatID { get; }

        public int Row => SeatID >> 3;
        public int Column => SeatID & 0b111;

        public SeatCode(string code)
        {
            SeatID = ParseFromBinaryRepresentation(SeatCodeToBinary(code));
        }

        public static SeatCode Parse(string code) => new SeatCode(code);
    }
}
