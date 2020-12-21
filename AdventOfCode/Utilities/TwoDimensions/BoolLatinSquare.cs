using System;
using System.Collections.Generic;

namespace AdventOfCode.Utilities.TwoDimensions
{
    public class BoolLatinSquare : SquareGrid2D<bool>
    {
        public int Count { get; protected set; }

        public BoolLatinSquare(int size, bool defaultValue = false)
            : base(size, defaultValue)
        {
            if (defaultValue)
                Count = size * size;
        }

        public int GetCountInX(int x)
        {
            int count = 0;
            for (int y = 0; y < Size; y++)
                if (Values[x, y])
                    count++;
            return count;
        }
        public int GetCountInY(int y)
        {
            int count = 0;
            for (int x = 0; x < Size; x++)
                if (Values[x, y])
                    count++;
            return count;
        }

        public int GetFirstIndexInX(int x)
        {
            for (int y = 0; y < Size; y++)
                if (Values[x, y])
                    return y;
            return -1;
        }
        public int GetFirstIndexInY(int y)
        {
            for (int x = 0; x < Size; x++)
                if (Values[x, y])
                    return x;
            return -1;
        }

        public IEnumerable<int> GetIndicesInX(int x)
        {
            for (int y = 0; y < Size; y++)
                if (Values[x, y])
                    yield return y;
        }
        public IEnumerable<int> GetIndicesInY(int y)
        {
            for (int x = 0; x < Size; x++)
                if (Values[x, y])
                    yield return x;
        }

        public void PrintSquare()
        {
            Console.WriteLine();
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                    Console.Write(Values[x, y] ? '#' : '.');
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private void CleanCandidates()
        {
            RemoveIsolatedCandidates();
        }
        private void RemoveIsolatedCandidates()
        {
            for (int i = 0; i < Size; i++)
            {
                if (GetCountInX(i) == 1)
                {
                    int y0 = GetFirstIndexInX(i);
                    for (int x = 0; x < Size; x++)
                    {
                        if (x == i)
                            continue;

                        this[x, y0] = false;
                    }
                }

                if (GetCountInY(i) == 1)
                {
                    int x0 = GetFirstIndexInY(i);
                    for (int y = 0; y < Size; y++)
                    {
                        if (y == i)
                            continue;

                        this[x0, y] = false;
                    }
                }
            }
        }

        public override bool this[int x, int y]
        {
            set
            {
                if (Values[x, y] == value)
                    return;

                if (value)
                    Count++;
                else
                    Count--;

                base[x, y] = value;

                CleanCandidates();
            }
        }
    }
}
