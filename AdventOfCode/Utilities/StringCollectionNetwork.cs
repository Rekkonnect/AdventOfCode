using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Utilities;

public class StringCollectionNetwork : LevelNetwork<char, StringCollectionNetworkNode, StringCollectionNetwork>
{
    public StringCollectionNetwork()
        : base() { }

    public StringCollectionNetwork(IEnumerable<string> strings)
    {
        foreach (var s in strings)
        {
            StringCollectionNetworkNode previousNode = null;
            for (int i = 0; i < s.Length; i++)
            {
                var node = GetOrCreateNode(i, s[i]);
                previousNode?.AddNext(node);
                previousNode = node;
            }
        }
    }

    protected override StringCollectionNetworkNode InitializeNode(int level, char value)
    {
        return new(level, value);
    }

    public IEnumerable<StringCollectionNetworkNodeRange> GetNodeDiamonds(int maxCommonNodes = 0)
    {
        var ranges = new List<StringCollectionNetworkNodeRange>();

        bool hasMaxCommonNodes = maxCommonNodes > 1;

        for (int i = 0; i < LevelCount - 2; i++)
        {
            var startNodes = GetNodes(i);
            var endNodes = GetNodes(i + 2);

            foreach (var start in startNodes)
            {
                foreach (var end in endNodes)
                {
                    var startNext = start.NextNodes;
                    var endPrevious = end.PreviousNodes;

                    var intersection = new HashSet<StringCollectionNetworkNode>(startNext);
                    intersection.IntersectWith(endPrevious);

                    if (intersection.Count < 2)
                        continue;

                    if (hasMaxCommonNodes && intersection.Count > maxCommonNodes)
                        continue;

                    ranges.Add(new(start, end));
                }
            }
        }

        return ranges;
    }

    public string GetCommonCharacters()
    {
        var result = new char[LevelCount];

        for (int i = 0; i < result.Length; i++)
        {
            var nodes = GetNodes(i);
            if (nodes.Count > 1)
                continue;

            result[i] = nodes.SingleOrDefault()?.Value ?? default;
        }

        return new(result);
    }
}

public class StringCollectionNetworkNodeRange : LevelNetworkNodeRange<char, StringCollectionNetworkNode, StringCollectionNetwork>
{
    public StringCollectionNetworkNodeRange(StringCollectionNetworkNode start, StringCollectionNetworkNode end)
        : base(start, end) { }
}
