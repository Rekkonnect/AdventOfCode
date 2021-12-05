using AdventOfCode.Utilities;
using AdventOfCSharp;
using Garyon.Extensions;
using System;
using System.Linq;

namespace AdventOfCode.Problems.Year2018;

public class Day14 : Problem<long>
{
    private RecipeTable table;
    private int recipeCount;
    private int[] sequence;

    public override long SolvePart1()
    {
        table.IterateUntilCount(recipeCount + 10);
        return table.GetNextRecipeScore(recipeCount, 10);
    }
    public override long SolvePart2()
    {
        return table.GetFirstSequenceIndex(sequence);
    }

    protected override void LoadState()
    {
        var contents = FileContents;
        sequence = contents.Select(s => s.GetNumericValueInteger()).ToArray();
        recipeCount = contents.ParseInt32();

        table = new();
    }
    protected override void ResetState()
    {
        table = null;
    }

    private class RecipeTable
    {
        private readonly CircularLinkedList<int> recipes = new(3, 7);
        private CircularLinkedListNode<int> elf0, elf1;

        public int RecipeCount => recipes.Count;

        public RecipeTable()
        {
            elf0 = recipes.Head;
            elf1 = recipes.Head.Next;
        }

        public long GetNextRecipeScore(int start, int count)
        {
            long value = 0;
            var currentRecipe = recipes.GetNode(start);
            for (int i = 0; i < count; i++)
            {
                value *= 10;
                value += currentRecipe.Value;

                currentRecipe = currentRecipe.Next;
            }

            return value;
        }

        private bool MatchesSequence(CircularLinkedListNode<int> start, int[] sequence)
        {
            var current = start;
            for (int i = 0; i < sequence.Length; i++)
            {
                if (sequence[i] != current.Value)
                    return false;

                current = current.Next;
            }

            return true;
        }

        public int GetFirstSequenceIndex(int[] sequence)
        {
            IterateUntilCount(sequence.Length);

            int currentIndex = 0;
            var current = recipes.Head;

            for (; currentIndex < RecipeCount - sequence.Length + 1; currentIndex++)
            {
                if (MatchesSequence(current, sequence))
                    return currentIndex;

                current = current.Next;
            }

            while (true)
            {
                Iterate();

                if (MatchesSequence(current, sequence))
                    return currentIndex;

                current = current.Next;
                currentIndex++;
            }
        }
        public void IterateUntilCount(int recipeCount)
        {
            while (RecipeCount < recipeCount)
                Iterate();
        }

        public void Iterate()
        {
            int sum = elf0.Value + elf1.Value;

            int decades = Math.DivRem(sum, 10, out int remainder);
            if (decades > 0)
                recipes.Add(decades);
            recipes.Add(remainder);

            elf0 = recipes.GetNext(elf0, elf0.Value + 1);
            elf1 = recipes.GetNext(elf1, elf1.Value + 1);
        }
    }
}
