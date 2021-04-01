using AdventOfCode.Utilities;
using Garyon.DataStructures;
using Garyon.Extensions;
using System;
using System.Linq;
using System.Text;

namespace AdventOfCode.Problems.Year2020
{
    public class Day23 : Problem<string, ulong>
    {
        private int[] cupLabels;

        public override string SolvePart1()
        {
            var cupArrangement = new CupArrangement(cupLabels);
            cupArrangement.PerformMoves(100);
            var cups = cupArrangement.Cups;

            int aceIndex = 0;
            for (; aceIndex < CupArrangement.CupCount; aceIndex++)
            {
                if (cups[aceIndex] == 1)
                    break;
            }

            char[] chars = new char[CupArrangement.CupCount - 1];
            for (int i = 0; i < chars.Length; i++)
                chars[i] = (char)(cups[(aceIndex + i + 1) % cups.Length] + '0');

            return new(chars);
        }
        public override ulong SolvePart2()
        {
            var cupArrangement = new CircularCupArrangement(cupLabels);
            cupArrangement.PerformMoves(10_000_000);
            return cupArrangement.StarsLabelProduct;
        }

        protected override void LoadState()
        {
            cupLabels = FileContents.Select(CharExtensions.GetNumericValueInteger).ToArray();
        }
        protected override void ResetState()
        {
            cupLabels = null;
        }

        private static int GetPreviousLabel(int cupCount, int label)
        {
            return (cupCount + label - 2) % cupCount + 1;
        }

        public class CircularCupArrangement
        {
            public const int CupCount = 1_000_000;

            private CircularLinkedList<int> labels;
            private CircularLinkedListNode<int> currentSelectedCup;

            private FlexibleDictionary<int, CircularLinkedListNode<int>> hashedLabels;

            private CircularLinkedListNode<int>[] currentlyPickedUp = new CircularLinkedListNode<int>[3];

            public ulong LeftStarLabel => (ulong)hashedLabels[1].Next.Next.Value;
            public ulong RightStarLabel => (ulong)hashedLabels[1].Next.Value;
            public ulong StarsLabelProduct => LeftStarLabel * RightStarLabel;

            public CircularCupArrangement(int[] originalCups, bool addExtraCups = true)
            {
                labels = new(originalCups);
                currentSelectedCup = labels.Head;

                if (addExtraCups)
                    for (int i = originalCups.Length + 1; i <= CupCount; i++)
                        labels.Add(i);

                hashedLabels = new(labels.Count);
                var currentNode = labels.Head;
                do
                {
                    hashedLabels[currentNode.Value] = currentNode;
                    currentNode = currentNode.Next;
                }
                while (currentNode != labels.Head);
            }

            public void PerformMoves(int moves)
            {
                for (int i = 0; i < moves; i++)
                    PerformMove();
            }
            public void PerformMove()
            {
                int destination = PreviousLabel(currentSelectedCup.Value);

                // Those manual operations are so unsafe but they reduce overhead
                var firstPickedUp = currentSelectedCup.Next;
                var lastPickedUp = firstPickedUp;
                for (int i = 1; i < 3; i++)
                {
                    currentlyPickedUp[i - 1] = lastPickedUp;
                    lastPickedUp = lastPickedUp.Next;
                }
                currentlyPickedUp[^1] = lastPickedUp;
                currentSelectedCup.Next = lastPickedUp.Next;

                while (currentlyPickedUp.Any(n => n.Value == destination))
                    destination = PreviousLabel(destination);

                var destinationNode = hashedLabels[destination];

                var originalDestinationNext = destinationNode.Next;
                destinationNode.Next = firstPickedUp;
                originalDestinationNext.Previous = lastPickedUp;

                currentSelectedCup = currentSelectedCup.Next;
            }

            private int PreviousLabel(int label)
            {
                return GetPreviousLabel(labels.Count, label);
            }

            public override string ToString()
            {
                var builder = new StringBuilder(20 * 3);

                int index = 0;
                foreach (var label in labels)
                {
                    if (index >= 20)
                        break;

                    char labelChar = (char)(label + '0');

                    if (currentSelectedCup.Value == label)
                        builder.Append('(').Append(label).Append(')');
                    else
                        builder.Append(' ').Append(label).Append(' ');

                    index++;
                }

                return builder.ToString();
            }
        }
        // I spent 6 hours making this in hopes that it would be useful in part 2
        public struct CupArrangement : IEquatable<CupArrangement>
        {
            public const int CupCount = 9;

            private ulong bits;

            public int CurrentCupLabel => this[CurrentCupIndex];
            public int CurrentCupIndex
            {
                get => this[CupCount];
                private set => this[CupCount] = value;
            }
            public int[] Cups
            {
                get
                {
                    int[] result = new int[CupCount];
                    for (int i = 0; i < CupCount; i++)
                        result[i] = this[i];
                    return result;
                }
            }

            public CupArrangement(int[] values)
                : this()
            {
                for (int i = 0; i < CupCount; i++)
                    this[i] = values[i];
            }
            public CupArrangement(CupArrangement other)
            {
                bits = other.bits;
            }

            #region Operations
            public void PerformMoves(int moves)
            {
                for (int i = 0; i < moves; i++)
                    PerformMove();
            }
            public void PerformMove()
            {
                int selectedCupIndex = CurrentCupIndex;
                int label = this[selectedCupIndex];

                int[] pickedUp = new int[3];
                for (int i = 0; i < 3; i++)
                    pickedUp[i] = this[(selectedCupIndex + 1 + i) % CupCount];

                // Update stored values
                int rightShift = selectedCupIndex + 4 - CupCount;
                if (rightShift > 0)
                {
                    ShiftRight(rightShift, CupCount, rightShift);
                    // No need for modulo because if it were to underflow,
                    // there would never be the need to right shift the contents
                    CurrentCupIndex -= rightShift;
                }
                else if (rightShift < 0)
                {
                    ShiftRight(CupCount + rightShift, -rightShift, 3);
                }

                // Find destination index
                int destinationLabel = PreviousLabel(label);
                while (pickedUp.Contains(destinationLabel))
                    destinationLabel = PreviousLabel(destinationLabel);

                int destinationIndex = 0;
                for (; destinationIndex < CupCount; destinationIndex++)
                {
                    if (this[destinationIndex] == destinationLabel)
                        break;
                }

                // Place the picked up cups down
                ShiftLeft(destinationIndex + 1, CupCount - 4 - destinationIndex, 3);
                if (destinationIndex < CurrentCupIndex)
                    CurrentCupIndex += 3;

                for (int i = 0; i < 3; i++)
                    this[destinationIndex + 1 + i] = pickedUp[i];

                CurrentCupIndex++;
                CurrentCupIndex %= CupCount;

                static int PreviousLabel(int label)
                {
                    return GetPreviousLabel(CupCount, label);
                }
            }

            // TODO: Abstract those functions somewhere in BitManipulations
            // Imagine not having slept well enough due to playing Yakuza 0
            // resulting in 20% productivity
            private void ShiftLeft(int startingIndex, int length, int shift)
            {
                ulong initialMask = GetMaskRange(startingIndex, length, 4);
                ulong movedBits = bits & initialMask;
                ulong destinationMask = initialMask << (shift * 4);
                movedBits <<= (shift * 4);
                ulong originalValueMask = ~destinationMask;
                bits = (bits & originalValueMask) | movedBits;
            }
            private void ShiftRight(int startingIndex, int length, int shift)
            {
                ulong initialMask = GetMaskRange(startingIndex, length, 4);
                ulong movedBits = bits & initialMask;
                ulong destinationMask = initialMask >> (shift * 4);
                movedBits >>= (shift * 4);
                ulong originalValueMask = ~destinationMask;
                bits = (bits & originalValueMask) | movedBits;
            }
            private static ulong GetMaskRange(int startingIndex, int length, int elementSize)
            {
                // It just works?
                int endingIndex = startingIndex + length - 1;
                ulong mask = ulong.MaxValue;
                mask >>= startingIndex * elementSize;
                mask <<= sizeof(ulong) * 8 - length * elementSize;
                mask >>= sizeof(ulong) * 8 - (endingIndex + 1) * elementSize;
                return mask;
            }
            #endregion

            public bool Equals(CupArrangement other) => bits == other.bits;
            public override bool Equals(object obj) => obj is CupArrangement other && Equals(other);

            // The main concept behind this hash code generation is that knowing the placement of 8 out of the 9 cups only has one unique arrangement
            // The only missing information in the hash is the current cup index, which will be resolved via the comparison upon collission
            public override unsafe int GetHashCode() => *(int*)bits;

            public override string ToString()
            {
                char[] chars = new char[3 * CupCount];
                Array.Fill(chars, ' ');

                for (int i = 0; i < CupCount; i++)
                    chars[i * 3 + 1] = (char)(this[i] + '0');

                int currentIndex = CurrentCupIndex;
                chars[currentIndex * 3] = '(';
                chars[currentIndex * 3 + 2] = ')';

                return new(chars);
            }

            public int this[int index]
            {
                get => (int)((bits >> (index * 4)) & 0b1111UL);
                set
                {
                    ulong movedValue = (ulong)value << (index * 4);
                    ulong mask = ~(0b1111UL << (index * 4));
                    bits = (bits & mask) | movedValue;
                }
            }
        }
    }
}
