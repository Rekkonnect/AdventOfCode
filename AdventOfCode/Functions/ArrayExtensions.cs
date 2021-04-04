using Garyon.Exceptions;
using Garyon.Extensions;
using System;

namespace AdventOfCode.Functions
{
    public static class ArrayExtensions
    {
        /// <summary>Gets the element at the specified indices in the 2D array.</summary>
        /// <typeparam name="T">The type of elements contained in the array.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="index0">The index of dimension 0.</param>
        /// <param name="index1">The index of dimension 1.</param>
        /// <returns>The element at the provided indices in the array.</returns>
        public static T At<T>(this T[,] array, Index index0, Index index1) => array[index0.GetOffset(array, 0), index1.GetOffset(array, 1)];
        /// <summary>Gets the element at the specified indices in the 3D array.</summary>
        /// <typeparam name="T">The type of elements contained in the array.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="index0">The index of dimension 0.</param>
        /// <param name="index1">The index of dimension 1.</param>
        /// <param name="index2">The index of dimension 2.</param>
        /// <returns>The element at the provided indices in the array.</returns>
        public static T At<T>(this T[,,] array, Index index0, Index index1, Index index2) => array[index0.GetOffset(array, 0), index1.GetOffset(array, 1), index2.GetOffset(array, 2)];
        /// <summary>Gets the element at the specified indices in the 4D array.</summary>
        /// <typeparam name="T">The type of elements contained in the array.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="index0">The index of dimension 0.</param>
        /// <param name="index1">The index of dimension 1.</param>
        /// <param name="index2">The index of dimension 2.</param>
        /// <param name="index3">The index of dimension 3.</param>
        /// <returns>The element at the provided indices in the array.</returns>
        public static T At<T>(this T[,,,] array, Index index0, Index index1, Index index2, Index index3) => array[index0.GetOffset(array, 0), index1.GetOffset(array, 1), index2.GetOffset(array, 2), index3.GetOffset(array, 3)];

        /// <summary>Gets the element at the specified indices in the array.</summary>
        /// <param name="array">The array.</param>
        /// <param name="indices">The indices.</param>
        /// <returns>The element at the provided indices in the array.</returns>
        public static object At(this Array array, params Index[] indices) => At<object>(array, indices);
        /// <summary>Gets the element at the specified indices in the array.</summary>
        /// <typeparam name="T">The type of elements contained in the array.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="indices">The indices.</param>
        /// <returns>The element at the provided indices in the array.</returns>
        public static T At<T>(this Array array, params Index[] indices)
        {
            if (indices.Length != array.Rank)
                ThrowHelper.Throw<RankException>("The provided indices do not match the array's rank.");

            var offsets = new int[indices.Length];
            for (int i = 0; i < offsets.Length; i++)
                offsets[i] = indices[i].GetOffset(array, i);
            return (T)array.GetValue(offsets);
        }

        public static T[] Replace<T>(this T[] source, T[] replacement, int start, int length)
        {
            int end = start + length;
            var replaced = new T[source.Length - length + replacement.Length];

            Array.Copy(source, replaced, start);
            Array.Copy(replacement, 0, replaced, start, replacement.Length);
            Array.Copy(source, end, replaced, start + replacement.Length, source.Length - end);

            return replaced;
        }
        public static T[] Replace<T>(this T[] source, T replacement, int start, int length)
        {
            int end = start + length;
            var replaced = new T[source.Length - length + 1];

            Array.Copy(source, replaced, start);
            replaced[start] = replacement;
            Array.Copy(source, end, replaced, start + 1, source.Length - end);

            return replaced;
        }
    }
}
