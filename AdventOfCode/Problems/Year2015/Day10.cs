namespace AdventOfCode.Problems.Year2015;

public class Day10 : Problem<int>
{
    private LookAndSaySequence startingSequence;

    public override int SolvePart1()
    {
        return startingSequence.Iterate(40).Length;
    }
    public override int SolvePart2()
    {
        return startingSequence.Iterate(50).Length;
    }

    protected override void ResetState()
    {
        startingSequence = null;
    }
    protected override void LoadState()
    {
        startingSequence = new(FileContents.Select(c => (byte)(c - '0')).ToArray());
    }

    private unsafe class LookAndSaySequence
    {
        private ArraySegment<byte> sequence;

        public int Length => sequence.Count;

        public LookAndSaySequence(ArraySegment<byte> bytes)
        {
            sequence = bytes;
        }

        public LookAndSaySequence Iterate(int times)
        {
            var resultingSequence = this;
            for (int i = 0; i < times; i++)
                resultingSequence = resultingSequence.GetNext();
            return resultingSequence;
        }

        public LookAndSaySequence GetNext()
        {
            byte last = sequence[0];
            byte consecutive = 1;

            var builder = new UnsafeArrayBuilder<byte>(Length * 2);

            for (int i = 1; i < Length; i++)
            {
                byte b = sequence[i];

                if (last == b)
                    consecutive++;
                else
                {
                    builder.Add(consecutive).Add(last);

                    last = b;
                    consecutive = 1;
                }
            }

            builder.Add(consecutive).Add(last);

            return new(builder.GetFinalizedSegment());
        }

        public override string ToString() => string.Concat(sequence);
    }

    private unsafe class UnsafeArrayBuilder<T>
    {
        private T[] ar;

        public int Length { get; private set; }

        public UnsafeArrayBuilder(int length)
        {
            ar = new T[length];
        }

        public UnsafeArrayBuilder<T> Add(T element)
        {
            ar[Length] = element;
            Length++;
            return this;
        }

        public ArraySegment<T> GetFinalizedSegment() => new(ar, 0, Length);

        public T this[int index] => ar[index];
    }
}
