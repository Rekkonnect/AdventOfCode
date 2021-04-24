using AdventOfCode.Utilities;
using Garyon.Extensions.ArrayExtensions.ArrayConverting;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;
using UltimateOrb;

namespace AdventOfCode.Problems.Year2017.Utilities
{
    public class KnotHasher
    {
        public const int ElementCount = 256;

        private readonly ConstructableArray<int> array;
        private int[] lengths;
        private int currentSkipSize;

        private KnotHasher(int[] inputLengths, int elementCount)
        {
            lengths = inputLengths;
            array = new(elementCount);
            Reset();
        }

        public void Reset()
        {
            currentSkipSize = 0;
            for (int i = 0; i < array.Length; i++)
                array[i] = i;
        }

        public void IterateOnce()
        {
            Iterate();
            array.ResetRotation();
        }

        public string GetKnotHashString()
        {
            return string.Concat(GetKnotHashBytes().Select(h => h.ToString("x2")));
        }
        public UInt128 GetKnotHash128()
        {
            var bytes = GetKnotHashBytes();
            UInt128 result = 0;
            for (int i = 0; i < 16; i++)
            {
                result <<= 8;
                result |= bytes[i];
            }
            return result;
        }
        public byte[] GetKnotHashBytes()
        {
            for (int i = 0; i < 64; i++)
                Iterate();

            array.ResetRotation();
            var resultHash = new int[16];
            for (int i = 0; i < array.Length; i++)
                resultHash[i / 16] ^= array[i];

            var result = new byte[16];
            for (int i = 0; i < 16; i++)
                result[i] = (byte)resultHash[i];
            return result;
        }

        private void Iterate()
        {
            foreach (var length in lengths)
            {
                array.ReverseOrder(0, length - 1);
                array.Rotate(-(length + currentSkipSize));
                currentSkipSize++;
            }
        }

        public int this[int index] => array[index];

        public static KnotHasher FromString(string chars, int elementCount = ElementCount)
        {
            return new(chars.Select(c => (int)c).Concat(new[] { 17, 31, 73, 47, 23 }).ToArray(), elementCount);
        }
        public static KnotHasher FromRawLengths(IEnumerable<int> lengths, int elementCount = ElementCount)
        {
            return new(lengths.ToArray(), elementCount);
        }
    }
}
