#nullable enable

using System.IO;
using System.Runtime.CompilerServices;

namespace AdventOfCSharp;

public static class ProblemFiles
{
    // TODO: Test this in the future to see how it plays out on other machines
    public static string GetBaseCodePath([CallerFilePath] string? filePath = null)
    {
        var entry = Assembly.GetEntryAssembly()!;
        return Path.GetDirectoryName(entry.Location)!;
    }
}
