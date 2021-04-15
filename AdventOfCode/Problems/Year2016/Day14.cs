//#define PRINT
//#define PRINT_REHASHES

using AdventOfCode.Problems.Utilities;
using AdventOfCode.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode.Problems.Year2016
{
    public class Day14 : Problem<int>
    {
        private string secretKey;

        public override int SolvePart1()
        {
            return IdentifyOTP<OTPHackerPart1>();
        }
        public override int SolvePart2()
        {
            return IdentifyOTP<OTPHackerPart2>();
        }

        private int IdentifyOTP<T>()
            where T : OTPHacker, new()
        {
            return new T().IdentifyOTP(secretKey);
        }

        protected override void ResetState()
        {
            secretKey = null;
        }
        protected override void LoadState()
        {
            secretKey = FileContents;
        }

        private sealed class OTPHackerPart1 : OTPHacker
        {
        }
        private sealed class OTPHackerPart2 : OTPHacker
        {
            protected override bool DetermineHashValidity(byte[] hash)
            {
                // Eric, mercy my computer please
#if PRINT_REHASHES
                Console.WriteLine($"{0,15} - {StringifyHash(hash)}");
#endif
                for (int i = 0; i < 2016; i++)
                {
                    hash = Hasher.ComputeHash(Encoding.UTF8.GetBytes(StringifyHash(hash)));
#if PRINT_REHASHES
                    Console.WriteLine($"{i + 1,15} - {StringifyHash(hash)}");
#endif
                }

                return base.DetermineHashValidity(hash);
            }
        }

        private abstract class OTPHacker : MD5HashBruteForcer
        {
            public const int KeyCount = 64;

            private readonly List<QueuedHash> hashes = new();
            private readonly SortedCollection<int> keyIndices = new(KeyCount + 5);

            private bool DoneIdentifying
            {
                get
                {
                    if (keyIndices.Count < KeyCount)
                        return false;

                    if (hashes.Count == 0)
                        return true;

                    if (hashes.First().Index < keyIndices.Last())
                        return false;

                    return true;
                }
            }

            public int IdentifyOTP(string secretKey)
            {
                do
                {
                    FindSuitableHash(secretKey, out _);
                }
                while (!DoneIdentifying);

                return keyIndices[KeyCount - 1];
            }

            protected override bool DetermineHashValidity(byte[] hash)
            {
#if PRINT
                Console.WriteLine($"{CurrentIndex,5} - {StringifyHash(hash)}");
#endif

                if (!ValidTriple(hash, out var consecutivity))
                    return false;

#if PRINT
                Console.WriteLine($"Detected triple of {consecutivity.Value}");
#endif

                // Remove outdated hashes
                int invalidated = 0;
                for (; invalidated < hashes.Count; invalidated++)
                {
                    if (hashes[invalidated].Index + 1000 >= CurrentIndex)
                        break;
                }
                hashes.RemoveRange(0, invalidated);

#if PRINT
                if (invalidated > 0)
                    Console.WriteLine($"Invalidated {invalidated} outdated cached hashes");
#endif

                // Add the found triple
                hashes.Add(new(CurrentIndex, consecutivity));

                var quintupleConsecutivity = consecutivity;
                while (ValidQuintuple(hash, quintupleConsecutivity, out quintupleConsecutivity))
                {
#if PRINT
                    Console.WriteLine($"Detected quintuple of {quintupleConsecutivity.Value}");
#endif

                    // Find and match the queued triple
                    for (int matched = hashes.Count - 2; matched >= 0; matched--)
                    {
                        var queuedHash = hashes[matched];
                        if (queuedHash.Consecutivity.Value != quintupleConsecutivity.Value)
                            continue;

#if PRINT
                        Console.WriteLine($"Matched index {queuedHash.Index}");
#endif

                        hashes.RemoveAt(matched);
                        keyIndices.Add(queuedHash.Index);
                    }

                    quintupleConsecutivity = quintupleConsecutivity.WithNoConsecutivityInfo();
                }

                return true;
            }

            private static bool ValidTriple(byte[] hash, out Consecutivity consecutivity)
            {
                consecutivity = Consecutivity.Default;
                return ValidConsecutive(hash, 3, ref consecutivity);
            }
            private static bool ValidQuintuple(byte[] hash, Consecutivity previousConsecutivity, out Consecutivity quintupleConsecutivity)
            {
                quintupleConsecutivity = previousConsecutivity;
                return ValidConsecutive(hash, 5, ref quintupleConsecutivity);
            }
            private static bool ValidConsecutive(byte[] hash, int desiredConsecutive, ref Consecutivity consecutivity)
            {
                int value = consecutivity.Value;
                int consecutive = consecutivity.Consecutive;
                int startHexIndex = consecutivity.NextIndex;

                int index = Math.DivRem(startHexIndex, 2, out int parity);
                if (parity is 1)
                {
                    AnalyzeConsecutivity(hash[index], 1, ref consecutivity);
                    index++;
                }

                ConsecutivityAnalysisResult analysisResult;
                for (; index < hash.Length; index++)
                {
                    byte b = hash[index];

                    // Couldn't this pattern be better abstractable?
                    analysisResult = AnalyzeConsecutivity(b, 0, ref consecutivity);
                    if (analysisResult is not ConsecutivityAnalysisResult.Undetermined)
                        return analysisResult is ConsecutivityAnalysisResult.Valid;

                    analysisResult = AnalyzeConsecutivity(b, 1, ref consecutivity);
                    if (analysisResult is not ConsecutivityAnalysisResult.Undetermined)
                        return analysisResult is ConsecutivityAnalysisResult.Valid;
                }

                return false;

                ConsecutivityAnalysisResult AnalyzeConsecutivity(byte hashByte, int indexOffset, ref Consecutivity consecutivity)
                {
                    int next = indexOffset switch
                    {
                        0 => hashByte >> 4,
                        1 => hashByte & 0xF,
                    };

                    if (next == value)
                        consecutive++;
                    else
                    {
                        consecutive = 1;
                        value = next;
                    }

                    bool result = consecutive >= desiredConsecutive;
                    if (result)
                        consecutivity = new(value, consecutive, index * 2 + indexOffset + 1);

                    return result ? ConsecutivityAnalysisResult.Valid : ConsecutivityAnalysisResult.Undetermined;
                }
            }

            // Struct records pretty please?

            private enum ConsecutivityAnalysisResult
            {
                Undetermined,
                Valid,
            }

            protected struct QueuedHash
            {
                public int Index { get; }
                public Consecutivity Consecutivity { get; }

                public QueuedHash(int index, Consecutivity consecutivity)
                {
                    Index = index;
                    Consecutivity = consecutivity;
                }
            }

            protected struct Consecutivity
            {
                public static readonly Consecutivity Default = FromNextIndex(0);

                public int Value { get; }
                public int Consecutive { get; }
                public int NextIndex { get; }

                public Consecutivity(int value, int consecutive, int nextIndex)
                {
                    Value = value;
                    Consecutive = consecutive;
                    NextIndex = nextIndex;
                }

                public Consecutivity WithNoConsecutivityInfo() => FromNextIndex(NextIndex);

                public static Consecutivity FromNextIndex(int nextIndex) => new(int.MaxValue, 0, nextIndex);
            }
        }
    }
}