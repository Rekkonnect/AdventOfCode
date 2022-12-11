using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using Garyon.Exceptions;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2022;

public class Day11 : Problem<long>
{
    private MonkeyJungle jungle;

    public override long SolvePart1()
    {
        return SolvePart(20, true);
    }
    public override long SolvePart2()
    {
        return SolvePart(10000, false);
    }

    private long SolvePart(int rounds, bool reduceWorriness)
    {
        jungle.PerformInspections(rounds, reduceWorriness);
        return jungle.MonkeyBusinessLevel;
    }

    protected override void LoadState()
    {
        var monkeySections = NormalizedFileContents.AsSpan().TrimEnd().SplitToStrings("\n\n");
        var monkeys = monkeySections.Select(Monkey.Parse);
        jungle = new(monkeys);
    }

    private class MonkeyJungle
    {
        private readonly ImmutableArray<Monkey> Monkeys;

        public long MonkeyBusinessLevel
        {
            get
            {
                var orderedInspections = Monkeys.OrderDescending(Monkey.InspectionsComparer.Instance)
                                                .Take(2).ToArray();

                return (long)orderedInspections[0].Inspections
                     * orderedInspections[1].Inspections;
            }
        }

        public MonkeyJungle(IEnumerable<Monkey> monkeys)
        {
            Monkeys = monkeys.ToImmutableArray();
            ApplyCompiledTests();
        }

        public void ResetInspections()
        {
            foreach (var monkey in Monkeys)
            {
                monkey.ResetInspections();
            }
        }

        public void PerformInspections(int rounds, bool reduceWorriness)
        {
            ResetInspections();
            for (int i = 0; i < rounds; i++)
            {
                PerformInspectionsSingleRound(reduceWorriness);
            }
        }
        private void PerformInspectionsSingleRound(bool reduceWorriness)
        {
            foreach (var monkey in Monkeys)
            {
                monkey.InspectAll(reduceWorriness);
            }
        }

        private void ApplyCompiledTests()
        {
            var divisors = Monkeys.Select(m => m.Test.Divisor);
            int lcm = MathFunctions.LCM(divisors);

            for (int i = 0; i < Monkeys.Length; i++)
            {
                var monkey = Monkeys[i];
                var rawTest = monkey.Test as RawTest;
                var compiledTest = CompiledTest.FromMapped(rawTest, lcm, Monkeys);
                Monkeys[i].ApplyCompiledTest(compiledTest);
            }
        }
    }

    private class Monkey
    {
        private readonly ImmutableArray<Item> startingItems;
        private readonly Operation operation;

        private Queue<Item> currentItems;

        public ITest Test { get; private set; }

        public int Inspections { get; private set; }

        private Monkey(IEnumerable<Item> items, Operation operation, RawTest test)
        {
            startingItems = items.ToImmutableArray();

            this.operation = operation;
            Test = test;
        }

        public void ApplyCompiledTest(CompiledTest compiledTest)
        {
            if (Test is CompiledTest)
                ThrowHelper.Throw<InvalidOperationException>();

            if (Test.Divisor != compiledTest.Divisor)
                ThrowHelper.Throw<ArgumentException>();

            Test = compiledTest;
        }

        public void InspectAll(bool reduceWorriness)
        {
            while (currentItems.Count > 0)
            {
                Inspect(reduceWorriness);
            }
        }
        private void Inspect(bool reduceWorriness)
        {
            var item = currentItems.Dequeue();

            item.ApplyOperation(operation);
            item.NormalizeWorriness(Test.DivisorLCM);
            if (reduceWorriness)
            {
                item.ReduceWorriness();
            }

            ThrowItem(item);

            Inspections++;
        }

        private void ThrowItem(Item item)
        {
            var targetMonkey = Test.TargetMonkey(item);
            targetMonkey.CatchItem(item);
        }

        public void ResetInspections()
        {
            currentItems = new(startingItems.Select(i => i.Clone()));
            Inspections = 0;
        }

        private void CatchItem(Item item)
        {
            currentItems.Enqueue(item);
        }

        public static Monkey Parse(string multilineSection)
        {
            return Parse(multilineSection.Split('\n'));
        }
        private static Monkey Parse(string[] lines)
        {
            var startingItemsSpan = lines[1].SubstringSpanAfter(": ");
            var startingWorryLevels = startingItemsSpan.SplitSelect(", ", SpanStringExtensions.ParseInt32);
            var startingItems = startingWorryLevels.Select(ItemPool.Default.Allocate);

            var operationExpression = lines[2].SubstringSpanAfter(" = ");
            var operation = Operation.Parse(operationExpression);

            var test = RawTest.Parse(lines);

            return new(startingItems, operation, test);
        }

        public sealed class InspectionsComparer : IComparer<Monkey>
        {
            public static InspectionsComparer Instance { get; } = new();

            private InspectionsComparer() { }

            int IComparer<Monkey>.Compare(Monkey x, Monkey y)
            {
                return x.Inspections.CompareTo(y.Inspections);
            }
        }
    }

    private sealed class ItemPool
    {
        public static ItemPool Default { get; } = new();

        private int currentID;

        public Item Allocate(int worryLevel)
        {
            int allocatedID = currentID;
            currentID++;
            return new(allocatedID, worryLevel);
        }

        public void Reset()
        {
            currentID = 0;
        }
    }

    private class Item
    {
        public int ID { get; }
        public long WorryLevel { get; private set; }

        public Item(int id, long worryLevel)
        {
            ID = id;
            WorryLevel = worryLevel;
        }

        public Item Clone() => new(ID, WorryLevel);

        public void ApplyOperation(Operation operation)
        {
            WorryLevel = operation.Operate(WorryLevel);
        }
        public void ReduceWorriness()
        {
            WorryLevel /= 3;
        }

        public void NormalizeWorriness(int lcm)
        {
            WorryLevel %= lcm;
        }
    }

    private interface ITest
    {
        public int Divisor { get; }
        public int DivisorLCM { get; }
        public Monkey WhenTrue { get; }
        public Monkey WhenFalse { get; }

        public Monkey TargetMonkey(Item item);
    }

    private record CompiledTest(int Divisor, int DivisorLCM, Monkey WhenTrue, Monkey WhenFalse)
        : ITest
    {
        public Monkey TargetMonkey(Item item)
        {
            return TargetMonkey(item.WorryLevel);
        }
        private Monkey TargetMonkey(long worryLevel)
        {
            long remainder = worryLevel % Divisor;
            if (remainder is 0)
                return WhenTrue;

            return WhenFalse;
        }

        public static CompiledTest FromMapped(RawTest test, int divisorLCM, IReadOnlyList<Monkey> monkeyMap)
        {
            var whenTrue = monkeyMap[test.WhenTrueID];
            var whenFalse = monkeyMap[test.WhenFalseID];
            return new(test.Divisor, divisorLCM, whenTrue, whenFalse);
        }
    }

    private record RawTest(int Divisor, int WhenTrueID, int WhenFalseID)
        : ITest
    {
        // Why not
        int ITest.DivisorLCM => throw new InvalidOperationException();
        Monkey ITest.WhenTrue => throw new InvalidOperationException();
        Monkey ITest.WhenFalse => throw new InvalidOperationException();

        Monkey ITest.TargetMonkey(Item item) => throw new InvalidOperationException();

        public static RawTest Parse(string[] monkeySectionLines)
        {
            var lineSpan = monkeySectionLines.AsSpan()[^3..];
            int divisor = lineSpan[0].AsSpan().ParseLastInt32();
            int whenTrueID = lineSpan[1].AsSpan().ParseLastInt32();
            int whenFalseID = lineSpan[2].AsSpan().ParseLastInt32();
            return new(divisor, whenTrueID, whenFalseID);
        }
    }

    private readonly record struct Operation(OperationArgument Left, OperationOperator Operator, OperationArgument Right)
    {
        public long Operate(long oldWorryLevel)
        {
            long left = Left.Substitute(oldWorryLevel);
            long right = Right.Substitute(oldWorryLevel);

            return Operator switch
            {
                OperationOperator.Addition => left + right,
                OperationOperator.Subtraction => left - right,
                OperationOperator.Multiplication => left * right,
                OperationOperator.Division => left / right,
                _ => -1,
            };
        }

        public static Operation Parse(ReadOnlySpan<char> expressionChars)
        {
            int operatorFrontSpaceTriviaIndex = expressionChars.IndexOf(' ', out int operatorIndex);
            int rightFrontSpaceTriviaIndex = operatorIndex + 2;
            var leftSpan = expressionChars[..operatorFrontSpaceTriviaIndex];
            char operatorChar = expressionChars[operatorIndex];
            var rightSpan = expressionChars[rightFrontSpaceTriviaIndex..];

            var left = OperationArgument.Parse(leftSpan);
            var right = OperationArgument.Parse(rightSpan);
            var @operator = ParseOperator(operatorChar);

            return new(left, @operator, right);
        }

        private static OperationOperator ParseOperator(char c)
        {
            return c switch
            {
                '+' => OperationOperator.Addition,
                '-' => OperationOperator.Subtraction,
                '*' => OperationOperator.Multiplication,
                '/' => OperationOperator.Division,

                // just ignore other possibilities
                _ => default,
            };
        }
    }

    private readonly record struct OperationArgument(long Value)
    {
        private const long old = long.MinValue;

        public static OperationArgument Old { get; } = new(old);

        public bool IsOld => Value is old;

        public long Substitute(long oldArgument)
        {
            if (IsOld)
                return oldArgument;

            return Value;
        }

        public static OperationArgument Parse(ReadOnlySpan<char> text)
        {
            if (text is "old")
                return Old;

            return new(text.ParseInt32());
        }
    }

    private enum OperationOperator
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
    }
}
