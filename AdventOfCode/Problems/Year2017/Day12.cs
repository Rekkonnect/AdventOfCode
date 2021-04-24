using AdventOfCode.Utilities;
using Garyon.DataStructures;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2017
{
    public class Day12 : Problem<int>
    {
        private ProgramSystem programSystem;

        public override int SolvePart1()
        {
            return programSystem.GetGroupedProgramCount(0);
        }
        public override int SolvePart2()
        {
            return programSystem.GetTotalGroups();
        }

        protected override void LoadState()
        {
            programSystem = new(ParsedFileLines(PipeConnection.Parse));
        }
        protected override void ResetState()
        {
            programSystem = null;
        }

        private class ProgramSystem
        {
            private FlexibleDictionary<int, PipeConnection> connections;

            private readonly Graph<int> connectionGraph;
            private readonly Dictionary<int, GraphNode<int>> graphNodeDictionary = new();

            public ProgramSystem(PipeConnection[] pipeConnections)
            {
                connections = new(pipeConnections, p => p.OriginalID);

                foreach (var connection in pipeConnections)
                    graphNodeDictionary.Add(connection.OriginalID, new(connection.OriginalID));
                connectionGraph = new(graphNodeDictionary.Values);

                foreach (var connection in pipeConnections)
                    foreach (var connectedID in connection.ConnectedIDs)
                        graphNodeDictionary[connection.OriginalID].AddConnection(graphNodeDictionary[connectedID]);
            }

            public int GetGroupedProgramCount(int original)
            {
                return graphNodeDictionary[original].GetAllConnectedNodes().Count;
            }
            public int GetTotalGroups()
            {
                return connectionGraph.IsolateGraphGroups().Count();
            }
        }

        private record PipeConnection(int OriginalID, int[] ConnectedIDs)
        {
            private static readonly Regex connectionPattern = new(@"(?'original'\d*) \<\-\> (?'connected'.*)", RegexOptions.Compiled);

            public static PipeConnection Parse(string raw)
            {
                var groups = connectionPattern.Match(raw).Groups;
                int original = groups["original"].Value.ParseInt32();
                var connected = groups["connected"].Value.Split(", ").Select(int.Parse).ToArray();
                return new(original, connected);
            }
        }
    }
}
