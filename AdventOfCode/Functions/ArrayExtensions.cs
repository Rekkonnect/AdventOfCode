using System;

namespace AdventOfCode.Functions
{
    public static class ArrayExtensions
    {
        // Although highly inefficient and awfully ugly, this provides the user the ability to use System.Index for indexing multi-dimensional arrays
        public static T At<T>(this T[,] array, Index index0, Index index1) => array[index0.GetOffset(array, 0), index1.GetOffset(array, 1)];
        public static T At<T>(this T[,,] array, Index index0, Index index1, Index index2) => array[index0.GetOffset(array, 0), index1.GetOffset(array, 1), index2.GetOffset(array, 2)];
        public static T At<T>(this T[,,,] array, Index index0, Index index1, Index index2, Index index3) => array[index0.GetOffset(array, 0), index1.GetOffset(array, 1), index2.GetOffset(array, 2), index3.GetOffset(array, 3)];
    }
}
