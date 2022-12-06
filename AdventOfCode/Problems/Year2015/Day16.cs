using AdventOfCode.Utilities;
using Garyon.Objects;

namespace AdventOfCode.Problems.Year2015;

public partial class Day16 : Problem<int>
{
    private static readonly GiftDictionary receivedGiftsPart1 = new()
    {
        ["children"] = 3,
        ["cats"] = 7,
        ["samoyeds"] = 2,
        ["pomeranians"] = 3,
        ["akitas"] = 0,
        ["vizslas"] = 0,
        ["goldfish"] = 5,
        ["trees"] = 3,
        ["cars"] = 2,
        ["perfumes"] = 1,
    };
    private static readonly GiftDictionary receivedGiftsPart2;

    static Day16()
    {
        receivedGiftsPart2 = new(receivedGiftsPart1);
        receivedGiftsPart2.ChangeComparisonKinds("cats", ComparisonKinds.Greater);
        receivedGiftsPart2.ChangeComparisonKinds("trees", ComparisonKinds.Greater);
        receivedGiftsPart2.ChangeComparisonKinds("pomeranians", ComparisonKinds.Less);
        receivedGiftsPart2.ChangeComparisonKinds("goldfish", ComparisonKinds.Less);
    }

    private FamilyGifts gifts;

    public override int SolvePart1()
    {
        return gifts.FindSueID(receivedGiftsPart1);
    }
    public override int SolvePart2()
    {
        return gifts.FindSueID(receivedGiftsPart2);
    }

    protected override void ResetState()
    {
        gifts = null;
    }
    protected override void LoadState()
    {
        gifts = new(ParsedFileLines(AuntGifts.Parse));
    }

    private class GiftDictionary : Dictionary<string, OpenInterval<int>>
    {
        public GiftDictionary() { }
        public GiftDictionary(IDictionary<string, OpenInterval<int>> dictionary)
            : base(dictionary) { }

        public void ChangeComparisonKinds(string key, ComparisonKinds comparisonKinds)
        {
            this[key] = this[key] with
            {
                ComparisonKinds = comparisonKinds
            };
        }
    }

    private class FamilyGifts
    {
        private readonly AuntGifts[] auntGifts;

        public FamilyGifts(IEnumerable<AuntGifts> gifts)
        {
            auntGifts = gifts.ToArray();
        }

        public int FindSueID(GiftDictionary expectedGifts)
        {
            foreach (var aunt in auntGifts)
            {
                if (aunt.MatchesGifts(expectedGifts))
                    return aunt.SueID;
            }
            return -1;
        }
    }

    private partial class AuntGifts
    {
        private static readonly Regex auntPattern = AuntRegex();
        private static readonly Regex giftPattern = new(@"(?'name'\w*): (?'number'\d*)", RegexOptions.Compiled);

        public int SueID { get; }
        public Dictionary<string, int> Gifts { get; }

        public AuntGifts(int sueID, Dictionary<string, int> gifts)
        {
            SueID = sueID;
            Gifts = gifts;
        }

        public bool MatchesGifts(GiftDictionary expectedGifts)
        {
            foreach (var g in Gifts)
                if (!expectedGifts[g.Key].Contains(g.Value))
                    return false;

            return true;
        }

        public static AuntGifts Parse(string s)
        {
            var groups = auntPattern.Match(s).Groups;
            int sueID = groups["id"].Value.ParseInt32();
            var giftsString = groups["gifts"].Value;

            var gifts = new Dictionary<string, int>();
            var matches = giftPattern.Matches(giftsString);
            // Only implements the non-generic IEnumerable? Why?
            foreach (Match m in matches)
                gifts.Add(m.Groups["name"].Value, m.Groups["number"].Value.ParseInt32());

            return new(sueID, gifts);
        }

        [GeneratedRegex("Sue (?'id'\\d*): (?'gifts'.*)", RegexOptions.Compiled)]
        private static partial Regex AuntRegex();
    }
}
