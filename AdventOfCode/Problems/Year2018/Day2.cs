namespace AdventOfCode.Problems.Year2018;

public class Day2 : Problem<int, string>
{
    private BoxCatalog boxes;

    public override int SolvePart1()
    {
        return boxes.Checksum;
    }
    public override string SolvePart2()
    {
        return boxes.GetMostCommonLetters();
    }

    protected override void LoadState()
    {
        boxes = new(ParsedFileLines(BoxID.From));
    }
    protected override void ResetState()
    {
        boxes = null;
    }

    private class BoxCatalog
    {
        private BoxID[] boxes;

        public int Checksum => boxes.Count(b => b.ContainsDoubleLetter) * boxes.Count(b => b.ContainsTripleLetter);

        public BoxCatalog(BoxID[] boxIDs)
        {
            boxes = boxIDs;
        }

        public string GetMostCommonLetters()
        {
            for (int i = 0; i < boxes.Length; i++)
            {
                var boxA = boxes[i].ID;

                for (int j = i + 1; j < boxes.Length; j++)
                {
                    var boxB = boxes[j].ID;

                    var compared = CompareStrings(boxA, boxB);
                    if (compared is null)
                        continue;

                    return compared;
                }
            }
            return null;
        }

        private static string CompareStrings(string a, string b)
        {
            int differentIndex = -1;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == b[i])
                    continue;

                if (differentIndex >= 0)
                    return null;

                differentIndex = i;
            }

            return a.Remove(differentIndex, 1);
        }
    }

    private record BoxID
    {
        private readonly ValueCounterDictionary<char> letters;
        private readonly HashSet<int> tupleLetters;

        public string ID { get; }

        public bool ContainsDoubleLetter => tupleLetters.Contains(2);
        public bool ContainsTripleLetter => tupleLetters.Contains(3);

        public BoxID(string id)
        {
            ID = id;
            letters = new(ID);
            tupleLetters = letters.Values.ToHashSet();
        }

        public static BoxID From(string raw) => new(raw);
    }
}
