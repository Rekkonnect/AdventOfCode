#nullable enable

using System.IO;
using System.Runtime.CompilerServices;

namespace AdventOfCode;

public static class Input
{
    // TODO: Test this in the future to see how it plays out on other machines
    public static string GetBaseCodePath([CallerFilePath] string? filePath = null)
    {
        if (filePath is null)
            return "";

        return $@"{Directory.GetParent(filePath)}\";
    }
}
