namespace AdventOfCode.Problems.Year2022;

public partial class Day19 : Problem<int>
{
    private ImmutableArray<Blueprint> blueprints;

    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart1()
    {
        return -1;
    }
    [PartSolution(PartSolutionStatus.Uninitialized)]
    public override int SolvePart2()
    {
        return -1;
    }

    protected override void LoadState()
    {
        blueprints = ParsedFileLinesEnumerable(Blueprint.Parse).ToImmutableArray();
    }

    private class Game
    {
        private Resources resources;
        private Resources growthRate;

        private readonly Blueprint blueprint;

        public Game(Blueprint blueprint)
        {
            this.blueprint = blueprint;
            growthRate = new(1, 0, 0, 0);
        }

        // This should be a DFS method but ok
        public void IterateMinutes(int minutes)
        {
            for (int i = 0; i < minutes; i++)
            {
                IterateMinute();
            }
        }
        private void IterateMinute()
        {

        }
    }

    private readonly record struct Resources(int Ore, int Clay, int Obsidian, int Geode)
    {
        public int ResourceValue(ResourceType type)
        {
            return type switch
            {
                ResourceType.Ore => Ore,
                ResourceType.Clay => Clay,
                ResourceType.Obsidian => Obsidian,
                ResourceType.Geode => Geode,
            };
        }

        public Resources Add(ResourceValue value)
        {
            return this + FromValue(value);
        }
        public Resources Subtract(ResourceValue value)
        {
            return this - FromValue(value);
        }

        public Resources Subtract(RobotCost cost)
        {
            return Subtract(cost.CostA)
                  .Subtract(cost.CostB);
        }

        public bool CoversCost(ResourceValue cost)
        {
            var available = ResourceValue(cost.ResourceType);
            return available >= cost.Value;
        }
        public bool CoversCost(RobotCost cost)
        {
            return CoversCost(cost.CostA)
                && CoversCost(cost.CostB);
        }

        private static Resources FromValue(ResourceValue value)
        {
            return value.ResourceType switch
            {
                ResourceType.Ore      => new(value.Value, 0, 0, 0),
                ResourceType.Clay     => new(0, value.Value, 0, 0),
                ResourceType.Obsidian => new(0, 0, value.Value, 0),
                ResourceType.Geode    => new(0, 0, 0, value.Value),
            };
        }

        public static Resources operator +(Resources left, Resources right)
        {
            return new(left.Ore + right.Ore,
                       left.Clay + right.Clay,
                       left.Obsidian + right.Obsidian,
                       left.Geode + right.Geode);
        }
        public static Resources operator -(Resources left, Resources right)
        {
            return new(left.Ore - right.Ore,
                       left.Clay - right.Clay,
                       left.Obsidian - right.Obsidian,
                       left.Geode - right.Geode);
        }
    }

    private partial record Blueprint(int ID, RobotCosts RobotCosts)
    {
        private static readonly Regex blueprintPattern = BlueprintRegex();

        [GeneratedRegex(@"Blueprint (?'id'\d*): (?'robotCosts'.*)")]
        private static partial Regex BlueprintRegex();

        public static Blueprint Parse(string line)
        {
            var match = blueprintPattern.Match(line);
            int id = match.Groups["id"].Value.ParseInt32();
            var robotCosts = RobotCosts.Parse(match.Groups["robotCosts"].Value);
            return new(id, robotCosts);
        }
    }
    private partial class RobotCosts
    {
        private static readonly Regex robotCostsPattern = RobotCostRegex();

        [GeneratedRegex(@"Each (?'resourceType'\w*) robot costs (?'resourceCosts'.*)\.")]
        private static partial Regex RobotCostRegex();

        private RobotCost oreCost;
        private RobotCost clayCost;
        private RobotCost obsidianCost;
        private RobotCost geodeCost;
        
        public RobotCost OreCost => oreCost;
        public RobotCost ClayCost => clayCost;
        public RobotCost ObsidianCost => obsidianCost;
        public RobotCost GeodeCost => geodeCost;

        private RobotCosts() { }

        public RobotCosts(RobotCost oreCost, RobotCost clayCost, RobotCost obsidianCost, RobotCost geodeCost)
        {
            this.oreCost = oreCost;
            this.clayCost = clayCost;
            this.obsidianCost = obsidianCost;
            this.geodeCost = geodeCost;
        }

        public ref RobotCost CostFor(ResourceType resourceType)
        {
            // ref switch expression when
            switch (resourceType)
            {
                case ResourceType.Ore:
                    return ref oreCost;
                case ResourceType.Clay:
                    return ref clayCost;
                case ResourceType.Obsidian:
                    return ref obsidianCost;
                case ResourceType.Geode:
                    return ref geodeCost;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resourceType));
            }
        }

        public static RobotCosts Parse(string line)
        {
            var result = new RobotCosts();

            var costMatches = robotCostsPattern.Matches(line);
            foreach (var costMatch in costMatches.Cast<Match>())
            {
                var targetResourceTypeName = costMatch.Groups["resourceType"].Value;
                var targetResourceType = ParseResourceType(targetResourceTypeName);

                ref var costVar = ref result.CostFor(targetResourceType);

                var robotCostString = costMatch.Groups["resourceType"].Value;
                var robotCost = ParseIndividualCost(robotCostString);
                costVar = robotCost;
            }

            return result;
        }
        private static RobotCost ParseIndividualCost(string line)
        {
            bool multicost = line.SplitOnceSpan(" and ", out var left, out var right);

            var costA = ParseResourceValue(left);
            var costB = ResourceValue.None;
            if (multicost)
            {
                costB = ParseResourceValue(right);
            }
            return new(costA, costB);
        }
        private static ResourceValue ParseResourceValue(SpanString span)
        {
            span.SplitOnce(' ', out var valueSpan, out var resourceTypeSpan);
            int value = valueSpan.ParseInt32();
            var resource = ParseResourceType(resourceTypeSpan);
            return new(resource, value);
        }

        private static ResourceType ParseResourceType(SpanString name)
        {
            return name switch
            {
                "ore" => ResourceType.Ore,
                "clay" => ResourceType.Clay,
                "obsidian" => ResourceType.Obsidian,
                "geode" => ResourceType.Geode,
            };
        }
    }
    private readonly record struct RobotCost(ResourceValue CostA, ResourceValue CostB)
    {
        public RobotCost(ResourceValue onlyCost)
            : this(onlyCost, ResourceValue.None) { }
    }
    private readonly record struct ResourceValue(ResourceType ResourceType, int Value)
    {
        public static ResourceValue None { get; } = new(ResourceType.Ore, 0);
    }

    private enum ResourceType
    {
        Ore,
        Clay,
        Obsidian,
        Geode
    }
}
