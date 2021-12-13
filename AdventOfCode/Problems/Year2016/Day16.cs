namespace AdventOfCode.Problems.Year2016;

public class Day16 : Problem<string>
{
    private bool[] initialData;

    public override string SolvePart1()
    {
        return GetChecksum(272);
    }
    public override string SolvePart2()
    {
        return GetChecksum(35651584);
    }

    protected override void LoadState()
    {
        initialData = ParseBitData(FileContents);
    }
    protected override void ResetState()
    {
        initialData = null;
    }

    private string GetChecksum(int diskSize)
    {
        return new Disk(diskSize, initialData).GetChecksumString();
    }

    private static bool[] ParseBitData(string raw) => raw.Select(c => c is '1').ToArray();

    private class Disk
    {
        // Too lazy to SIMD optimize this
        private bool[] data;

        public int CurrentLength { get; private set; }
        public int DiskSize { get; }

        public int ChecksumLength { get; }
        public int ChecksumIterations { get; }

        public Disk(int size, bool[] initialData)
        {
            data = new bool[size];
            initialData.CopyTo(data, 0);
            CurrentLength = initialData.Length;
            DiskSize = size;

            ChecksumLength = DiskSize;
            ChecksumIterations = 0;
            do
            {
                int reduced = Math.DivRem(ChecksumLength, 2, out int remainder);
                if (remainder is 1)
                    break;

                ChecksumLength = reduced;
                ChecksumIterations++;
            }
            while (true);
        }

        public string GetChecksumString() => BitString(GetChecksum());
        public bool[] GetChecksum()
        {
            ExpandData();
            var result = new bool[ChecksumLength];

            var checksumIteration = new bool[DiskSize];
            data.CopyTo(checksumIteration, 0);

            int nextIterationLength = checksumIteration.Length / 2;
            for (int i = 0; i < ChecksumIterations; i++)
            {
                for (int checksumIndex = 0; checksumIndex < nextIterationLength; checksumIndex++)
                {
                    int originalIndex = checksumIndex * 2;
                    bool a = checksumIteration[originalIndex];
                    bool b = checksumIteration[originalIndex + 1];
                    bool checksumBit = !(a ^ b);
                    checksumIteration[checksumIndex] = checksumBit;
                }

                //Console.WriteLine($"{BitString(checksumIteration)}\n");
                nextIterationLength /= 2;
            }

            Array.Copy(checksumIteration, result, ChecksumLength);
            return result;
        }

        private void ExpandData()
        {
            while (CurrentLength < DiskSize)
            {
                IterateDataExpansion();
                CurrentLength = Math.Min(CurrentLength * 2 + 1, DiskSize);
            }
        }
        private void IterateDataExpansion()
        {
            int leftIndex = CurrentLength - 1;
            int rightIndex = CurrentLength + 1;
            for (int i = 0; i < CurrentLength; i++)
            {
                if (rightIndex >= DiskSize)
                    return;

                data[rightIndex] = !data[leftIndex];

                leftIndex--;
                rightIndex++;
            }

            //Console.WriteLine($"{BitString(data)}\n");
        }

        public static Disk Parse(int size, string raw) => new(size, raw.Select(c => c is '1').ToArray());

        private static string BitString(bool[] bits) => new(bits.Select(BitChar).ToArray());
        private static char BitChar(bool bit) => bit switch
        {
            true => '1',
            false => '0',
        };
    }
}
