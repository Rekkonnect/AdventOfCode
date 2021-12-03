using Garyon.Reflection;
using System;
using System.Reflection;

namespace AdventOfCode.Utilities;

public static class MemberInfoExtensions
{
    public static bool HasCustomAttribute<TAttribute>(this MemberInfo memberInfo)
        where TAttribute : Attribute
    {
        return memberInfo.HasCustomAttribute<TAttribute>(out _);
    }
}
