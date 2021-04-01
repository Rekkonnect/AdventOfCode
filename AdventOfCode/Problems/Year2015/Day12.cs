using AdventOfCode.Functions;
using System.Linq;
using System.Text.Json;

namespace AdventOfCode.Problems.Year2015
{
    public class Day12 : Problem<int>
    {
        private JsonDocument document;

        public override int SolvePart1()
        {
            return document.EnumerateAllElements().Where(e => e.ValueKind == JsonValueKind.Number).Select(e => e.GetInt32()).Sum();
        }
        public override int SolvePart2()
        {
            int count = 0;
            EnumerateElements(document.RootElement);
            return count;

            bool EnumerateElements(JsonElement element)
            {
                int previousCount = count;
                switch (element.ValueKind)
                {
                    case JsonValueKind.Array:
                        foreach (var e in element.EnumerateArray())
                            EnumerateElements(e);
                        break;
                    case JsonValueKind.Object:
                        foreach (var e in element.EnumerateObject())
                            if (EnumerateElements(e.Value))
                            {
                                // Reset count after ignoring structure
                                count = previousCount;
                                return false;
                            }
                        break;
                    case JsonValueKind.Number:
                        count += element.GetInt32();
                        break;
                    case JsonValueKind.String:
                        if (element.GetString() is "red")
                            return true;
                        break;
                }
                return false;
            }
        }

        protected override void ResetState()
        {
            document = null;
        }
        protected override void LoadState()
        {
            if (document != null)
                return;

            document = JsonDocument.Parse(FileContents);
        }
    }
}
