using AdventOfCode.Functions;
using AdventOfCSharp.Utilities;
using System.Collections.Immutable;
using System.Data;

namespace AdventOfCode.Problems.Year2022;

public class Day6 : Problem<int>
{
    private Datastream stream;

    public override int SolvePart1()
    {
        return stream.GetDataStartIndex(4);
    }
    public override int SolvePart2()
    {
        return stream.GetDataStartIndex(14);
    }

    protected override void LoadState()
    {
        stream = new(NormalizedFileContents);
    }
    protected override void ResetState()
    {
        stream = null;
    }

    private sealed class Datastream
    {
        private readonly string stream;

        public Datastream(string data)
        {
            stream = data;
        }

        public int GetDataStartIndex(int markerLength)
        {
            var charCounter = new LookupTable<int>('a', 'z');
            int duplicates = 0;
            for (int i = 0; i < markerLength; i++)
            {
                char c = stream[i];
                int previous = charCounter[c];
                if (previous > 0)
                {
                    duplicates++;
                }
                charCounter[c]++;
            }

            for (int i = markerLength; i < stream.Length; i++)
            {
                char c = stream[i];
                int previous = charCounter[c];
                if (previous > 0)
                {
                    duplicates++;
                }

                if (duplicates is 0)
                {
                    return i;
                }

                charCounter[c]++;

                int dumpedIndex = i - markerLength;
                char dumpedChar = stream[dumpedIndex];
                int previousDumped = charCounter[dumpedChar];
                if (previousDumped > 1)
                {
                    duplicates--;
                }

                charCounter[dumpedChar]--;
            }

            return -1;
        }
    }
}
