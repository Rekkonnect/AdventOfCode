using AdventOfCode.Functions;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2015
{
    public class Day14 : Problem<int>
    {
        private ReindeerRace reindeerRace;

        public override int SolvePart1()
        {
            return reindeerRace.GetWinningDistance(2503);
        }
        public override int SolvePart2()
        {
            return reindeerRace.GetWinningPoints(2503);
        }

        protected override void ResetState()
        {
            reindeerRace = null;
        }
        protected override void LoadState()
        {
            reindeerRace = new(FileLines.Select(Reindeer.Parse));
        }

        private class ReindeerRace
        {
            private readonly Reindeer[] reindeer;

            public ReindeerRace(IEnumerable<Reindeer> reindeerCollection)
            {
                reindeer = reindeerCollection.ToArray();
            }

            public int GetWinningDistance(int seconds)
            {
                return reindeer.Select(r => r.GetDistance(seconds)).Max();
            }
            public int GetWinningPoints(int seconds)
            {
                var points = reindeer.ToDefaultValueDictionary<Reindeer, int>();
                var distances = reindeer.ToDefaultValueDictionary<Reindeer, int>();
                int maxDistance = 0;
                for (int second = 0; second < seconds; second++)
                {
                    foreach (var r in reindeer)
                    {
                        if (r.IsFlying(second))
                        {
                            int newDistance = distances[r] += r.Speed;
                            if (newDistance > maxDistance)
                                maxDistance = newDistance;
                        }
                    }
                    foreach (var r in reindeer)
                    {
                        if (distances[r] == maxDistance)
                            points[r]++;
                    }
                }

                return points.Values.Max();
            }
        }

        private record Reindeer(string Name, int Speed, int FlyTime, int RestTime)
        {
            private static readonly Regex reindeerPattern = new(@"(?'name'\w*) can fly (?'speed'\d*) km/s for (?'fly'\d*) seconds, but then must rest for (?'rest'\d*) seconds\.", RegexOptions.Compiled);

            public int IterationDistance => Speed * FlyTime;
            public int LoopTime => FlyTime + RestTime;

            public bool IsFlying(int seconds)
            {
                int remainingSeconds = seconds % LoopTime;
                return remainingSeconds < FlyTime;
            }

            public int GetDistance(int seconds)
            {
                int iterations = Math.DivRem(seconds, LoopTime, out int remainingSeconds);
                int distance = iterations * IterationDistance;
                int remainingFlySeconds = Math.Min(remainingSeconds, FlyTime);
                return distance + remainingFlySeconds * Speed;
            }

            public static Reindeer Parse(string s)
            {
                var groups = reindeerPattern.Match(s).Groups;
                var name = groups["name"].Value;
                var speed = groups["speed"].Value.ParseInt32();
                int flyTime = groups["fly"].Value.ParseInt32();
                int restTime = groups["rest"].Value.ParseInt32();
                return new(name, speed, flyTime, restTime);
            }
        }
    }
}
