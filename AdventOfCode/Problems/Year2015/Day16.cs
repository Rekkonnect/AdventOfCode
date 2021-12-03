using AdventOfCode.Utilities;
using Garyon.Extensions;
using Garyon.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2015;

using GiftDictionary = Dictionary<string, ValueComparison<int>>;

public class Day16 : Problem<int>
{
    private static GiftDictionary receivedGiftsPart1 = new()
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
    private static GiftDictionary receivedGiftsPart2;

    static Day16()
    {
        // TODO: Explore better syntax for replacing these values
        receivedGiftsPart2 = new(receivedGiftsPart1);
        receivedGiftsPart2["cats"] = receivedGiftsPart2["cats"] with { ComparisonKinds = ComparisonKinds.Greater };
        receivedGiftsPart2["trees"] = receivedGiftsPart2["trees"] with { ComparisonKinds = ComparisonKinds.Greater };
        receivedGiftsPart2["pomeranians"] = receivedGiftsPart2["pomeranians"] with { ComparisonKinds = ComparisonKinds.Less };
        receivedGiftsPart2["goldfish"] = receivedGiftsPart2["goldfish"] with { ComparisonKinds = ComparisonKinds.Less };
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

    private class AuntGifts
    {
        private static readonly Regex auntPattern = new(@"Sue (?'id'\d*): (?'gifts'.*)", RegexOptions.Compiled);
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
                if (!expectedGifts[g.Key].MatchesComparison(g.Value))
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
    }
}
