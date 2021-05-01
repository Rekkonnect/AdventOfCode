using AdventOfCode.Functions;
using Garyon.DataStructures;
using Garyon.Extensions;
using Garyon.Extensions.ArrayExtensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2018
{
    public class Day4 : Problem<int>
    {
        private GuardPost post;

        public override int SolvePart1()
        {
            return post.SleepiestGuard.MinuteIDCode;
        }
        public override int SolvePart2()
        {
            return post.GetMostFrequentlySleepyGuardCode();
        }

        protected override void LoadState()
        {
            post = new(ParsedFileLines(GuardPostRecord.Parse));
        }
        protected override void ResetState()
        {
            post = null;
        }

        private class GuardPost
        {
            private readonly GuardDictionary guards = new();

            public Guard SleepiestGuard
            {
                get
                {
                    return guards.Values.MaxSource(guard => guard.SleepingTime);

                    int maxSleepingTime = 0;
                    Guard sleepiest = null;

                    foreach (var guard in guards.Values)
                    {
                        if (guard.SleepingTime > maxSleepingTime)
                        {
                            sleepiest = guard;
                            maxSleepingTime = guard.SleepingTime;
                        }
                    }

                    return sleepiest;
                }
            }

            public GuardPost(GuardPostRecord[] records)
            {
                var sorted = records.ToArray().Sort();
                ApplyRecords(sorted);
            }

            public int GetMostFrequentlySleepyGuardCode()
            {
                var guard = GetMostFrequentlySleepyGuard(out int minute);
                return guard.ID * minute;
            }
            public Guard GetMostFrequentlySleepyGuard(out int minute)
            {
                int maxTimesSlept = 0;
                Guard sleepiest = null;
                minute = 0;

                foreach (var guard in guards.Values)
                {
                    int mostSleptMinute = guard.GetMostSleptMinute(out int timesSlept);
                    if (timesSlept > maxTimesSlept)
                    {
                        sleepiest = guard;
                        maxTimesSlept = timesSlept;
                        minute = mostSleptMinute;
                    }
                }

                return sleepiest;
            }

            private void ApplyRecords(GuardPostRecord[] records)
            {
                int currentShift = 0;

                for (int i = 0; i < records.Length; i++)
                {
                    switch (records[i])
                    {
                        case ShiftBeginRecord shiftBegin:
                            currentShift = shiftBegin.GuardID;
                            break;

                        case AsleepingRecord asleeping:
                            var awakening = records[i + 1] as AwakeningRecord;
                            guards[currentShift].RegisterSleepingRecords(asleeping, awakening);
                            i++;
                            break;
                    }
                }
            }
        }

        private class GuardDictionary : FlexibleDictionary<int, Guard>
        {
            public override Guard this[int id]
            {
                get
                {
                    if (!ContainsKey(id))
                        Dictionary[id] = new(id);
                    return Dictionary[id];
                }
            }
        }

        private class Guard
        {
            private readonly int[] minuteSleepFrequencies = new int[60];

            public int ID { get; }

            public int SleepingTime { get; private set; }

            public int MinuteIDCode => GetMostSleptMinute(out _) * ID;

            public Guard(int id)
            {
                ID = id;
            }

            public int GetMostSleptMinute(out int timesSlept)
            {
                return minuteSleepFrequencies.MaxIndex(out timesSlept);
            }

            public int TimesSleptAtMinute(int minute) => minuteSleepFrequencies[minute];

            public void RegisterSleepingRecords(AsleepingRecord asleeping, AwakeningRecord awakening)
            {
                for (int min = asleeping.Timestamp.Minute; min < awakening.Timestamp.Minute; min++)
                    minuteSleepFrequencies[min]++;

                SleepingTime += (awakening.Timestamp - asleeping.Timestamp).Minutes;
            }
        }

        private sealed record ShiftBeginRecord(DateTime Timestamp, int GuardID) : GuardPostRecord(Timestamp)
        {
            protected override string ActionString => $"Guard #{GuardID} begins shift";
        }
        private sealed record AsleepingRecord(DateTime Timestamp) : GuardPostRecord(Timestamp)
        {
            protected override string ActionString => $"falls asleep";
        }
        private sealed record AwakeningRecord(DateTime Timestamp) : GuardPostRecord(Timestamp)
        {
            protected override string ActionString => $"wakes up";
        }

        private abstract record GuardPostRecord(DateTime Timestamp) : IComparable<GuardPostRecord>
        {
            private static readonly Regex recordPattern = new(@"\[(?'timestamp'.*)\] (?'action'.*)", RegexOptions.Compiled);
            private static readonly Regex shiftBeginPattern = new(@"#(?'guardID'\d*) begins shift", RegexOptions.Compiled);

            private const string timestampFormat = "yyyy-MM-dd HH:mm";

            protected abstract string ActionString { get; }

            public static GuardPostRecord Parse(string raw)
            {
                var groups = recordPattern.Match(raw).Groups;
                var timestamp = DateTime.ParseExact(groups["timestamp"].Value, timestampFormat, null);

                var action = groups["action"].Value;
                switch (action)
                {
                    case "falls asleep":
                        return new AsleepingRecord(timestamp);
                    case "wakes up":
                        return new AwakeningRecord(timestamp);
                }

                int guardID = shiftBeginPattern.Match(action).Groups["guardID"].Value.ParseInt32();

                return new ShiftBeginRecord(timestamp, guardID);
            }

            public int CompareTo(GuardPostRecord other)
            {
                return Timestamp.CompareTo(other.Timestamp);
            }

            public override string ToString()
            {
                return $"[{Timestamp.ToString(timestampFormat)}] {ActionString}";
            }
        }
    }
}
