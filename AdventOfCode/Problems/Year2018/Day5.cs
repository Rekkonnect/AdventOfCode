using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2018
{
    public class Day5 : Problem<int>
    {
        private Polymer polymer;

        public override int SolvePart1()
        {
            return new Polymer(polymer).FullyReact();
        }
        public override int SolvePart2()
        {
            return polymer.MinimumReducedReactedPolymerLength();
        }

        protected override void LoadState()
        {
            polymer = new(FileContents);
        }
        protected override void ResetState()
        {
            polymer = null;
        }

        private class Polymer
        {
            private readonly LinkedList<Unit> unitString = new();

            public int Length => unitString.Count;

            private Polymer(LinkedList<Unit> list) => unitString = list;

            public Polymer(string s)
                : this(new LinkedList<Unit>())
            {
                foreach (char c in s)
                    unitString.AddLast(new Unit(c));
            }
            public Polymer(Polymer polymer)
                : this(new LinkedList<Unit>(polymer.unitString)) { }

            public int MinimumReducedReactedPolymerLength()
            {
                return EachWithOneRemovedType().Select(p => p.FullyReact()).Min();
            }

            private IEnumerable<Polymer> EachWithOneRemovedType()
            {
                for (char c = 'a'; c <= 'z'; c++)
                {
                    var copy = new Polymer(this);

                    LinkedListNode<Unit> next;
                    for (var current = copy.unitString.First; current is not null; current = next)
                    {
                        next = current.Next;

                        if (current.Value.MatchesType(c))
                            copy.unitString.Remove(current);
                    }

                    yield return copy;
                }
            }

            public int FullyReact()
            {
                var current = unitString.First.Next;
                do
                {
                    var next = current.Next;
                    var previous = current.Previous;

                    if (previous is null)
                    {
                        current = next;
                        continue;
                    }

                    if (current.Value.ReactsWith(previous.Value))
                    {
                        unitString.Remove(previous);
                        unitString.Remove(current);
                    }

                    current = next;
                }
                while (current is not null);

                return Length;
            }
        }

        private struct Unit
        {
            private const int capDifference = 'a' - 'A';

            public char Type { get; }

            public Unit(char c) => Type = c;

            public bool MatchesType(char c) => Math.Abs(Type - c) is 0 or capDifference;
            public bool MatchesTypePolarity(char c) => Math.Abs(Type - c) is capDifference;
            public bool ReactsWith(Unit other) => MatchesTypePolarity(other.Type);
        }
    }
}
