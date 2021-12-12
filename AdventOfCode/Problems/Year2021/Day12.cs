#nullable enable

using AdventOfCode.Utilities;
using AdventOfCSharp;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2021;

public class Day12 : Problem<int>
{
    private CaveGraph? caves;

    public override int SolvePart1()
    {
        return caves!.ValidPaths(false);
    }
    public override int SolvePart2()
    {
        return caves!.ValidPaths(true);
    }

    protected override void LoadState()
    {
        caves = CaveGraph.Parse(FileLines);
    }
    protected override void ResetState()
    {
        caves = null;
    }

    private sealed class CaveGraphNode : GraphNode<Cave, CaveGraphNode, CaveGraph>
    {
        public CaveGraphNode(Cave value)
            : base(value) { }
    }

    private sealed class CaveGraph : Graph<Cave, CaveGraphNode, CaveGraph>
    {
        private readonly CaveGraphNode start, end;

        private CaveGraph(IEnumerable<CaveGraphNode> caveNodes)
            : base(caveNodes)
        {
            foreach (var node in caveNodes)
            {
                switch (node.Value.Name)
                {
                    case "start":
                        start = node;
                        break;
                    case "end":
                        end = node;
                        break;
                }
            }
        }

        public CaveGraph(IEnumerable<CaveLink> caveLinks)
            : base()
        {
            var nodeDictionary = GetNodes(caveLinks).ToDictionary(node => node.Value);
            foreach (var link in caveLinks)
            {
                nodeDictionary[link.A].AddConnection(nodeDictionary[link.B]);

                AssignField("start", ref start);
                AssignField("end", ref end);

                void AssignField(string targetName, ref CaveGraphNode? targetField)
                {
                    AssignFieldCave(link.A, targetName, ref targetField);
                    AssignFieldCave(link.B, targetName, ref targetField);
                }
                void AssignFieldCave(Cave cave, string targetName, ref CaveGraphNode? targetField)
                {
                    if (cave.Name == targetName)
                        targetField = nodeDictionary[cave];
                }
            }

            AddUnconnectedRange(nodeDictionary.Values);
        }

        public int ValidPaths(bool permitSecondVisit)
        {
            return ValidPaths(start, end, permitSecondVisit);
        }
        private int ValidPaths(CaveGraphNode a, CaveGraphNode b, bool permitSecondVisit)
        {
            var visitedCaveDictionary = new VisitedCaveDictionary(NodeValues, permitSecondVisit);

            return ValidPathsFrom(a, b);

            int ValidPathsFrom(CaveGraphNode start, CaveGraphNode target)
            {
                if (start.Value == target.Value)
                    return 1;

                visitedCaveDictionary.RegisterVisit(start.Value);

                int count = 0;

                foreach (var connected in start.ConnectedNodes)
                {
                    var connectedCave = connected.Value;

                    if (!visitedCaveDictionary.CanRevisit(connectedCave))
                        continue;

                    count += ValidPathsFrom(connected, target);
                }

                visitedCaveDictionary.UndoVisit(start.Value);
                return count;
            }
        }

        protected override CaveGraph InitializeNewInstance(IEnumerable<CaveGraphNode> graphNodes)
        {
            return new(graphNodes);
        }

        public static CaveGraph Parse(string[] rawLinks)
        {
            return new(rawLinks.Select(CaveLink.Parse).ToArray());
        }
        private static IEnumerable<CaveGraphNode> GetNodes(IEnumerable<CaveLink> links)
        {
            var caveSet = new HashSet<Cave>();

            foreach (var link in links)
                caveSet.AddRange(link.A, link.B);

            return caveSet.Select(cave => new CaveGraphNode(cave));
        }

        private class VisitedCaveDictionary
        {
            private readonly Dictionary<Cave, int> caves;
            private readonly bool permitSecondVisit;
            private bool hasSecondVisit;

            public VisitedCaveDictionary(IEnumerable<Cave> nodes, bool permitSecondSmallCaveVisit)
            {
                permitSecondVisit = permitSecondSmallCaveVisit;

                var smallCaves = nodes.Where(cave => cave.IsSmall).ToArray();
                caves = new(smallCaves.Length);

                foreach (var cave in smallCaves)
                    caves.Add(cave, 0);
            }

            public void RegisterVisit(Cave cave)
            {
                AdjustVisits(cave, 1);
            }
            public void UndoVisit(Cave cave)
            {
                AdjustVisits(cave, -1);
            }

            public void AdjustVisits(Cave cave, int visits)
            {
                if (cave.IsBig)
                    return;

                int previousVisits = caves[cave];
                int currentVisits = caves[cave] += visits;

                if (hasSecondVisit)
                {
                    if (previousVisits is 2 && currentVisits < 2)
                        hasSecondVisit = false;
                }
                else
                {
                    if (currentVisits is 2)
                        hasSecondVisit = true;
                }
            }

            public bool CanRevisit(Cave cave)
            {
                if (cave.IsBig)
                    return true;

                if (!permitSecondVisit || cave.IsStartOrEnd || hasSecondVisit)
                    return caves[cave] < 1;

                return caves[cave] < 2;
            }

            public void ResetVisits()
            {
                foreach (var cave in caves.Keys)
                    caves[cave] = 0;
            }
        }
    }
    private sealed record class Cave(string Name)
    {
        public bool IsSmall => Name[0].IsLower();
        public bool IsBig => Name[0].IsUpper();

        public bool IsStartOrEnd => Name is "start" or "end";

        public override int GetHashCode() => Name.GetHashCode();
    }
    private readonly record struct CaveLink(Cave A, Cave B)
    {
        public static CaveLink Parse(string rawLink)
        {
            var split = rawLink.Split('-');
            var rawA = split[0];
            var rawB = split[1];
            return new(new(rawA), new(rawB));
        }
    }
}
