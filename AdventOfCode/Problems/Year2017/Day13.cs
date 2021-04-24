using Garyon.DataStructures;
using Garyon.Extensions;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2017
{
    public class Day13 : Problem<int>
    {
        private Firewall firewall;

        public override int SolvePart1()
        {
            return firewall.GetTripSeverity(0);
        }
        public override int SolvePart2()
        {
            return firewall.GetFirstUncaughtTime();
        }

        protected override void LoadState()
        {
            firewall = new(ParsedFileLines(Layer.Parse));
        }
        protected override void ResetState()
        {
            firewall = null;
        }

        private class Firewall
        {
            private readonly FlexibleDictionary<int, Layer> layers = new();
            private readonly int layerCount;

            public Firewall(Layer[] firewallLayers)
            {
                foreach (var layer in firewallLayers)
                    layers.Add(layer.Depth, layer);

                layerCount = layers.Keys.Max() + 1;
            }

            public int GetTripSeverity(int packetSendTime)
            {
                int severity = 0;
                for (int i = 0; i < layerCount; i++)
                    severity += layers[i]?.SeverityAt(packetSendTime + i) ?? 0;
                return severity;
            }

            public bool IsCaught(int packetSendTime)
            {
                for (int i = 0; i < layerCount; i++)
                    if (layers[i]?.CaughtAt(packetSendTime + i) ?? false)
                        return true;
                return false;
            }

            public int GetFirstUncaughtTime()
            {
                // I don't like how this is brute force; but can it really be optimized?
                for (int time = 0; ; time++)
                    if (!IsCaught(time))
                        return time;
            }
        }

        private record Layer(int Depth, int Range)
        {
            private static readonly Regex layerPattern = new(@"(?'depth'\d*): (?'range'\d*)", RegexOptions.Compiled);

            public int ScannerCycleTime => (Range - 1) * 2;
            public int Severity => Depth * Range;

            public bool CaughtAt(int time) => ScannerPositionAt(time) is 0;
            public int SeverityAt(int time) => CaughtAt(time) ? Severity : 0;

            public int ScannerPositionAt(int time)
            {
                time %= ScannerCycleTime;
                if (time < Range)
                    return time;

                return Range - 1 - (time - Range);
            }

            public static Layer Parse(string raw)
            {
                var groups = layerPattern.Match(raw).Groups;
                int depth = groups["depth"].Value.ParseInt32();
                var range = groups["range"].Value.ParseInt32();
                return new(depth, range);
            }
        }
    }
}
