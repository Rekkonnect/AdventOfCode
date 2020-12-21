using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Functions
{
    public static class IndexExtensions
    {
        public static int GetOffset<T>(this Index index, IEnumerable<T> collection) => index.GetOffset(collection.Count());
        public static int GetOffset<T>(this Index index, ICollection<T> collection) => index.GetOffset(collection.Count);

        public static int GetOffset<T>(this Index index, T[] array) => index.GetOffset(array.Length);
        public static int GetOffset<T>(this Index index, T[,] array, int dimension) => index.GetOffset(array.GetLength(dimension));
        public static int GetOffset<T>(this Index index, T[,,] array, int dimension) => index.GetOffset(array.GetLength(dimension));
        public static int GetOffset<T>(this Index index, T[,,,] array, int dimension) => index.GetOffset(array.GetLength(dimension));
    }
}
