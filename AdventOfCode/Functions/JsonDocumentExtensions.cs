using System.Text.Json;

namespace AdventOfCode.Functions;

public static class JsonDocumentExtensions
{
    public static IEnumerable<JsonElement> EnumerateAllElements(this JsonDocument document)
    {
        return document.RootElement.EnumerateElements();
    }
}
