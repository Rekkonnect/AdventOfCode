using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2022;

public partial class Day16 : Problem<int>
{
    private RoomElephant elephant;

    // Part 1 was accidentally solved due to convenience in the input
    // However, it does not solve the example input, which poses questions
    [PartSolution(PartSolutionStatus.Refactoring)]
    public override int SolvePart1()
    {
        return elephant.MaxPressureScore(30, 1);
    }
    [PartSolution(PartSolutionStatus.WIP)]
    public override int SolvePart2()
    {
        return elephant.MaxPressureScore(26, 2);
    }

    protected override void LoadState()
    {
        var valves = ParsedFileLines(Valve.Parse);
        var graph = new ValveGraph(valves);
        elephant = new(graph);
    }

    private record RoomElephant(ValveGraph Graph)
    {
        public const string GeneralStart = "AA";

        public int MaxPressureScoreAnyStart(int minutes)
        {
            var dummy = Graph.CreateDummyStartValve("GLOBAL");
            return MaxPressureScore(minutes + 1, 1, dummy);
        }

        public int MaxPressureScore(int minutes, int traversers, string start = GeneralStart)
        {
            return MaxPressureScore(minutes, traversers, Graph.GetValve(start));
        }
        public int MaxPressureScore(int minutes, int traversers, CompiledValve valve)
        {
            int maxScore = 0;

            valve.Starting = true;

            var traverserValves = new CompiledValve[traversers];
            traverserValves.Fill(valve);

            Iterate(minutes, 0, 0);

            valve.Starting = false;

            return maxScore;

            void Iterate(int remainingMinutes, int currentTraverser, int currentScore)
            {
                // Early exiting
                if (remainingMinutes <= 0)
                {
                    // Register current score, end the process
                    maxScore.AssignMax(currentScore);
                    return;
                }

                // Traverser valves
                var previousValve = traverserValves[currentTraverser];

                int nextTraverser = currentTraverser;
                if (traversers > 1)
                {
                    nextTraverser = currentTraverser + 1;
                    if (nextTraverser >= traversers)
                    {
                        nextTraverser = 0;
                    }
                }

                if (currentTraverser is 0)
                {
                    remainingMinutes--;
                }

                // Early exiting
                int maxScoreGoal = maxScore - currentScore;
                if (maxScoreGoal > 0)
                {
                    int remainingPressure = Graph.MaxRemainingPressure(remainingMinutes);
                    if (remainingPressure < maxScoreGoal)
                    {
                        return;
                    }
                }

                // Iterating
                if (previousValve.HasFlow && !previousValve.Opened)
                {
                    previousValve.Opened = true;

                    int bonusScore = previousValve.RemainingPressure(remainingMinutes);
                    int nextScore = currentScore + bonusScore;

                    IterateNext();

                    previousValve.Opened = false;
                }

                previousValve.Visited = true;

                IterateNext();

                previousValve.Visited = false;

                maxScore.AssignMax(currentScore);

                void IterateNext()
                {
                    ref var currentValve = ref traverserValves[currentTraverser];

                    foreach (var nextValve in previousValve.ConnectedValves)
                    {
                        if (!nextValve.Visited || nextValve.Opened)
                            continue;

                        if (!nextValve.HasFlow)
                            break;

                        currentValve = nextValve;

                        Iterate(remainingMinutes, nextTraverser, currentScore);
                    }

                    foreach (var nextValve in previousValve.ConnectedValves)
                    {
                        currentValve = nextValve;

                        Iterate(remainingMinutes, nextTraverser, currentScore);
                    }

                    currentValve = previousValve;

                    if (previousValve.HasFlow && !previousValve.Opened)
                    {
                        Iterate(remainingMinutes, nextTraverser, currentScore);
                    }
                }
            }
        }
    }

    // This should be reworked so that it calculates the subgraph with the
    // valves that have flow, and containing the information for the shortest
    // distance between each other
    // This is how the graph gets reduced enough so that the puzzle be
    // solvable within shorter than 5 seconds
    private class ValveGraph
    {
        private readonly Dictionary<string, CompiledValve> valveDictionary;

        private readonly ImmutableArray<CompiledValve> sortedValves;

        public ValveGraph(IEnumerable<Valve> valves)
        {
            var uncompiledDictionary = valves.ToDictionary(v => v.Name);
            var compiled = valves.Select(v => v.Compile()).ToArray();

            valveDictionary = compiled.ToDictionary(v => v.Name);

            sortedValves = compiled.Where(v => v.HasFlow)
                                   .OrderByDescending(v => v.FlowRate)
                                   .ToImmutableArray();

            foreach (var valve in compiled)
            {
                var uncompiled = uncompiledDictionary[valve.Name];
                valve.CompileConnectedValves(valveDictionary, uncompiled.ConnectedNames);
            }
        }

        public int MaxRemainingPressure(int remainingMinutes)
        {
            int sum = 0;

            foreach (var valve in sortedValves)
            {
                if (valve.Opened)
                    continue;

                remainingMinutes -= 2;
                if (remainingMinutes < 0)
                    break;

                sum += valve.RemainingPressure(remainingMinutes);
            }

            return sum;
        }

        public CompiledValve GetValve(string name) => valveDictionary.ValueOrDefault(name);

        public CompiledValve CreateDummyStartValve(string name)
        {
            return CompiledValve.CreateDummyStartValve(name, valveDictionary.Values);
        }
    }

    private interface IValve
    {
        public string Name { get; }
        public int FlowRate { get; }
    }

    private sealed record CompiledValve(string Name, int FlowRate)
        : IValve
    {
        public ImmutableArray<CompiledValve> ConnectedValves { get; private set; }

        public bool Visited { get; set; }
        public bool Opened { get; set; }
        public bool Starting { get; set; }

        public bool VisitedOpened => Visited && Opened;

        public bool HasFlow => FlowRate > 0;

        public int RemainingPressure(int minutes)
        {
            return minutes * FlowRate;
        }

        public void CompileConnectedValves(IDictionary<string, CompiledValve> valves, IEnumerable<string> valveNames)
        {
            ConnectedValves = valveNames.Select(name => valves[name])
                                        .OrderByDescending(valve => valve.FlowRate)
                                        .ToImmutableArray();
        }

        public static CompiledValve CreateDummyStartValve(string name, IEnumerable<CompiledValve> otherValves)
        {
            return new(name, 0)
            {
                ConnectedValves = otherValves.ToImmutableArray(),
            };
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    private partial record Valve(string Name, int FlowRate, ImmutableArray<string> ConnectedNames)
        : IValve
    {
        public ImmutableArray<Valve> ConnectedValves { get; private set; }

        private static readonly Regex valvePattern = ValveRegex();

        [GeneratedRegex(@"Valve (?'name'\w*) has flow rate=(?'flowRate'\d*); tunnel(s?) lead(s?) to valve(s?) (?'connected'[\w, ]*)")]
        private static partial Regex ValveRegex();

        public CompiledValve Compile()
        {
            return new(Name, FlowRate);
        }

        public static Valve Parse(string line)
        {
            var groups = valvePattern.Match(line).Groups;
            var name = groups["name"].Value;
            int flowRate = groups["flowRate"].Value.ParseInt32();
            var connected = groups["connected"].Value;
            var connectedNames = connected.Split(", ").ToImmutableArray();
            return new(name, flowRate, connectedNames);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
