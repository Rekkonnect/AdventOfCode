using Garyon.Extensions.ArrayExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Utilities
{
    public class ConstructableArray<T> : IEnumerable<T>
    {
        // Rotations are handled in such a way that there is no
        // actual operation performed before fully constructing the array

        private int currentRotation;

        protected readonly T[] Array;

        public int Length => Array.Length;

        public ConstructableArray(T[] initial) => Array = initial.CopyArray();
        public ConstructableArray(int length) => Array = new T[length];

        public void SwapPosition(int x, int y)
        {
            T t = this[y];
            this[y] = this[x];
            this[x] = t;
        }
        public void SwapItem(T a, T b)
        {
            SwapPosition(IndexOf(a), IndexOf(b));
        }

        public void ReverseOrder(int start, int end)
        {
            while (start < end)
            {
                SwapPosition(start, end);

                start++;
                end--;
            }
        }

        public void Move(int from, int to)
        {
            if (from == to)
                return;

            T moved = this[from];

            if (from < to)
            {
                for (int i = from; i < to; i++)
                    this[i] = this[i + 1];
            }
            else
            {
                for (int i = from; i > to; i--)
                    this[i] = this[i - 1];
            }

            this[to] = moved;
        }

        public void ResetRotation() => currentRotation = 0;

        public void Rotate(int rotation)
        {
            currentRotation = (currentRotation + rotation + Length) % Length;
#if DEBUG_ROTATION
            Console.WriteLine($"Current Rotation {currentRotation}");
#endif
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Length; i++)
                if (this[i].Equals(item))
                    return i;
            return -1;
        }

        public T[] ConstructArray()
        {
            T[] result = new T[Length];

            for (int i = 0; i < Length; i++)
                result[i] = this[i];

            return result;
        }

        private int OffsetIndex(int index) => (index - currentRotation + Length) % Length;

        public T this[int index]
        {
            get => Array[OffsetIndex(index)];
            set => Array[OffsetIndex(index)] = value;
        }

        public IEnumerator<T> GetEnumerator() => Array.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
