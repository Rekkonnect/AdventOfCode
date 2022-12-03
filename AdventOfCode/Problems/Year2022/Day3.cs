using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2022;

public class Day3 : Problem<int>
{
    private Rucksack[] rucksacks;

    public override int SolvePart1()
    {
        return rucksacks.Select(r => r.GetCommonItem())
                        .Sum(i => i.Priority);
    }
    public override int SolvePart2()
    {
        const int groupSize = 3;

        var rucksackGroups = new RucksackGroup[rucksacks.Length / groupSize];
        for (int i = 0; i < rucksackGroups.Length; i++)
        {
            var groupSegment = new ArraySegment<Rucksack>(rucksacks, i * groupSize, groupSize);
            var group = new RucksackGroup(groupSegment);
            rucksackGroups[i] = group;
        }
        return rucksackGroups.Sum(r => r.GetCommonItem().Priority);
    }

    protected override void LoadState()
    {
        rucksacks = FileLines.Select(Rucksack.Parse).ToArray();
        base.LoadState();
    }
    protected override void ResetState()
    {
        base.ResetState();
    }

    private class RucksackGroup
    {
        private readonly IReadOnlyList<Rucksack> rucksacks;

        public RucksackGroup(IReadOnlyList<Rucksack> rucksacks)
        {
            this.rucksacks = rucksacks;
        }

        public Item GetCommonItem()
        {
            var types = new HashSet<char>();
            types.AddRange(rucksacks[0].DistinctTypes);

            for (int i = 1; i < rucksacks.Count; i++)
            {
                types.IntersectWith(rucksacks[i].DistinctTypes);
            }
            return new(types.First());
        }
    }

    private class Rucksack
    {
        private readonly Compartment leftCompartment;
        private readonly Compartment rightCompartment;

        private readonly HashSet<char> distinctTypes = new();

        public ISet<char> DistinctTypes => distinctTypes;

        public Rucksack(Compartment left, Compartment right)
        {
            leftCompartment = left;
            rightCompartment = right;

            distinctTypes.UnionWith(leftCompartment.DistinctTypes);
            distinctTypes.UnionWith(rightCompartment.DistinctTypes);
        }

        public Item GetCommonItem()
        {
            foreach (var item in rightCompartment.Items)
            {
                bool contains = leftCompartment.ContainsItemType(item);
                if (contains)
                    return item;
            }
            return default;
        }

        public static Rucksack Parse(string line)
        {
            var lineSpan = line.AsSpan();
            int half = lineSpan.Length / 2;
            var leftLine = lineSpan[..half];
            var rightLine = lineSpan[half..];

            return new(Compartment.Parse(leftLine), Compartment.Parse(rightLine));
        }
    }

    private readonly struct Compartment
    {
        private readonly HashSet<char> distinctTypes = new();

        public ImmutableArray<Item> Items { get; }
        public ISet<char> DistinctTypes => distinctTypes;

        public Compartment(ImmutableArray<Item> items)
        {
            Items = items;

            foreach (var item in items)
            {
                distinctTypes.Add(item.Type);
            }
        }

        public bool ContainsItemType(Item item)
        {
            return distinctTypes.Contains(item.Type);
        }

        public static Compartment Parse(ReadOnlySpan<char> line)
        {
            var itemsBuilder = ImmutableArray.CreateBuilder<Item>(line.Length);
            for (int i = 0; i < line.Length; i++)
            {
                itemsBuilder.Add(new(line[i]));
            }

            return new(itemsBuilder.ToImmutable());
        }
    }

    private record struct Item(char Type)
    {
        public int Priority
        {
            get
            {
                if (Type is >= 'a' and <= 'z')
                    return Type - 'a' + 1;
                if (Type is >= 'A' and <= 'Z')
                    return Type - 'A' + 27;

                return 0;
            }
        }
    }
}
