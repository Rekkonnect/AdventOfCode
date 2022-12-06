using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AdventOfCode.Utilities;

public static class MDArraySpans
{
    // We take for granted that C# MD arrays are contiguous in memory

    public static Span<T> AsSpan<T>(this T[,] array)
    {
        ref byte arrayByteDataReference = ref MemoryMarshal.GetArrayDataReference(array);
        ref T arrayDataReference = ref Unsafe.As<byte, T>(ref arrayByteDataReference);
        return MemoryMarshal.CreateSpan(ref arrayDataReference, array.Length);
    }
    public static Span<T> AsSpan<T>(this T[,,] array)
    {
        ref byte arrayByteDataReference = ref MemoryMarshal.GetArrayDataReference(array);
        ref T arrayDataReference = ref Unsafe.As<byte, T>(ref arrayByteDataReference);
        return MemoryMarshal.CreateSpan(ref arrayDataReference, array.Length);
    }
    public static Span<T> AsSpan<T>(this T[,,,] array)
    {
        ref byte arrayByteDataReference = ref MemoryMarshal.GetArrayDataReference(array);
        ref T arrayDataReference = ref Unsafe.As<byte, T>(ref arrayByteDataReference);
        return MemoryMarshal.CreateSpan(ref arrayDataReference, array.Length);
    }

    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[,] array)
    {
        ref byte arrayByteDataReference = ref MemoryMarshal.GetArrayDataReference(array);
        ref T arrayDataReference = ref Unsafe.As<byte, T>(ref arrayByteDataReference);
        return MemoryMarshal.CreateReadOnlySpan(ref arrayDataReference, array.Length);
    }
    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[,,] array)
    {
        ref byte arrayByteDataReference = ref MemoryMarshal.GetArrayDataReference(array);
        ref T arrayDataReference = ref Unsafe.As<byte, T>(ref arrayByteDataReference);
        return MemoryMarshal.CreateReadOnlySpan(ref arrayDataReference, array.Length);
    }
    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[,,,] array)
    {
        ref byte arrayByteDataReference = ref MemoryMarshal.GetArrayDataReference(array);
        ref T arrayDataReference = ref Unsafe.As<byte, T>(ref arrayByteDataReference);
        return MemoryMarshal.CreateReadOnlySpan(ref arrayDataReference, array.Length);
    }
}
