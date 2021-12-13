using System.Text.Json;

namespace AdventOfCode.Functions;

public static class JsonElementExtensions
{
    public static IEnumerable<JsonElement> EnumerateElements(this JsonElement element)
    {
        yield return element;

        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var yieldedElement in element.EnumerateArray().SelectMany(e => e.EnumerateElements()))
                    yield return yieldedElement;
                break;
            case JsonValueKind.Object:
                foreach (var yieldedElement in element.EnumerateObject().SelectMany(e => e.Value.EnumerateElements()))
                    yield return yieldedElement;
                break;
        }
    }
}
