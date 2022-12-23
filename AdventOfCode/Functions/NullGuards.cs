﻿namespace AdventOfCode.Functions;

#nullable enable

// Copy-pasted until abstracted away
public static class NullGuards
{
    public static bool AnyNull<T>(T value)
        where T : class?
    {
        return value is null;
    }
    public static bool AnyNull<T1, T2>(T1 value1, T2 value2)
        where T1 : class?
        where T2 : class?
    {
        return value1 is null
            || value2 is null;
    }
    public static bool AnyNull<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        where T1 : class?
        where T2 : class?
        where T3 : class?
    {
        return value1 is null
            || value2 is null
            || value3 is null;
    }
    public static bool AnyNull<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        where T1 : class?
        where T2 : class?
        where T3 : class?
        where T4 : class?
    {
        return value1 is null
            || value2 is null
            || value3 is null
            || value4 is null;
    }

    public static bool AnyNull<T>(T? value)
        where T : struct
    {
        return value is null;
    }
    public static bool AnyNull<T1, T2>(T1? value1, T2? value2)
        where T1 : struct
        where T2 : struct
    {
        return value1 is null
            || value2 is null;
    }
    public static bool AnyNull<T1, T2, T3>(T1? value1, T2? value2, T3? value3)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        return value1 is null
            || value2 is null
            || value3 is null;
    }
    public static bool AnyNull<T1, T2, T3, T4>(T1? value1, T2? value2, T3? value3, T4? value4)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        return value1 is null
            || value2 is null
            || value3 is null
            || value4 is null;
    }

    public static bool AnyNonNull<T>(T value)
        where T : class?
    {
        return value is not null;
    }
    public static bool AnyNonNull<T1, T2>(T1 value1, T2 value2)
        where T1 : class?
        where T2 : class?
    {
        return value1 is not null
            || value2 is not null;
    }
    public static bool AnyNonNull<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        where T1 : class?
        where T2 : class?
        where T3 : class?
    {
        return value1 is not null
            || value2 is not null
            || value3 is not null;
    }
    public static bool AnyNonNull<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        where T1 : class?
        where T2 : class?
        where T3 : class?
        where T4 : class?
    {
        return value1 is not null
            || value2 is not null
            || value3 is not null
            || value4 is not null;
    }

    public static bool AnyNonNull<T>(T? value)
        where T : struct
    {
        return value is not null;
    }
    public static bool AnyNonNull<T1, T2>(T1? value1, T2? value2)
        where T1 : struct
        where T2 : struct
    {
        return value1 is not null
            || value2 is not null;
    }
    public static bool AnyNonNull<T1, T2, T3>(T1? value1, T2? value2, T3? value3)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        return value1 is not null
            || value2 is not null
            || value3 is not null;
    }
    public static bool AnyNonNull<T1, T2, T3, T4>(T1? value1, T2? value2, T3? value3, T4? value4)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        return value1 is not null
            || value2 is not null
            || value3 is not null
            || value4 is not null;
    }

    public static bool NoneNull<T>(T value)
        where T : class?
    {
        return !AnyNull(value);
    }
    public static bool NoneNull<T1, T2>(T1 value1, T2 value2)
        where T1 : class?
        where T2 : class?
    {
        return !AnyNull(value1, value2);
    }
    public static bool NoneNull<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        where T1 : class?
        where T2 : class?
        where T3 : class?
    {
        return !AnyNull(value1, value2, value3);
    }
    public static bool NoneNull<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        where T1 : class?
        where T2 : class?
        where T3 : class?
        where T4 : class?
    {
        return !AnyNull(value1, value2, value3, value4);
    }

    public static bool NoneNull<T>(T? value)
        where T : struct
    {
        return !AnyNull(value);
    }
    public static bool NoneNull<T1, T2>(T1? value1, T2? value2)
        where T1 : struct
        where T2 : struct
    {
        return !AnyNull(value1, value2);
    }
    public static bool NoneNull<T1, T2, T3>(T1? value1, T2? value2, T3? value3)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        return !AnyNull(value1, value2, value3);
    }
    public static bool NoneNull<T1, T2, T3, T4>(T1? value1, T2? value2, T3? value3, T4? value4)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        return !AnyNull(value1, value2, value3, value4);
    }
}