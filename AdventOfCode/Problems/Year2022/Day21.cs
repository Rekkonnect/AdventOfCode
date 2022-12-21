using AdventOfCode.Functions;
using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2022;

public class Day21 : Problem<long>
{
    private ImmutableArray<Monkey> monkeys;

    public override long SolvePart1()
    {
        var jungle = new YellingJungle(monkeys);
        jungle.Operate();
        return jungle.RootValue;
    }
    public override long SolvePart2()
    {
        return -1;
    }

    protected override void LoadState()
    {
        var fileSpan = FileContents.AsSpan().Trim();
        monkeys = fileSpan.SelectLines(Monkey.Parse);
    }

    private class YellingJungle
    {
        private const string rootName = "root";

        private readonly Dictionary<string, YellingMonkey> monkeys;
        private readonly FlexibleListDictionary<YellingMonkey, YellingMonkey> monkeyDependencies = new();

        public long RootValue => monkeys[rootName].YelledValue.GetValueOrDefault();

        public YellingJungle(IEnumerable<Monkey> simpleMonkeys)
        {
            var yellingMonkeys = simpleMonkeys.Select(m => new YellingMonkey(m.Name))
                                              .ToImmutableArray();

            var simpleMonkeyDictionary = simpleMonkeys.ToDictionary(m => m.Name);
            monkeys = yellingMonkeys.ToDictionary(m => m.Name);

            foreach (var monkey in yellingMonkeys)
            {
                monkey.LoadFromYellingMonkeys(monkeys, simpleMonkeyDictionary[monkey.Name]);

                if (monkey.Operation is not YellingEquationOperation equation)
                    continue;

                monkeyDependencies[equation.Left].Add(monkey);
                monkeyDependencies[equation.Right].Add(monkey);
            }
        }

        public void Operate()
        {
            var queuedMonkeys = new Queue<YellingMonkey>();
            monkeys.Values.Dissect(m => m.HasYelled, out var yelledSource, out var unyelledSource);

            var yelled = yelledSource.ToListOrExisting();
            var remaining = unyelledSource.ToListOrExisting();

            foreach (var yelledMonkey in yelled)
            {
                // So far the only monkeys that have yelled are the ones
                // that have a literal operation, meaning they have no
                // dependencies to others
                var depending = monkeyDependencies[yelledMonkey];
                queuedMonkeys.EnqueueRange(depending);
            }

            while (queuedMonkeys.Count > 0)
            {
                var current = queuedMonkeys.Dequeue();
                current.ReiterateOperation();
                if (!current.HasYelled)
                    continue;

                var depending = monkeyDependencies[current]
                               .Where(m => !m.HasYelled);
                queuedMonkeys.EnqueueRange(depending);
            }
        }
    }

    private class YellingMonkey
    {
        public string Name { get; }
        public IYellingOperation Operation { get; private set; }
        
        public long? YelledValue { get; private set; }
        public bool HasYelled => YelledValue is not null;
        
        public YellingMonkey(string name)
        {
            Name = name;
        }

        public void ReiterateOperation()
        {
            if (HasYelled)
                return;

            if (Operation.IsReady)
            {
                YelledValue = Operation.Value;
            }
        }

        public void LoadFromYellingMonkeys(IDictionary<string, YellingMonkey> monkeyDictionary, Monkey simpleMonkey)
        {
            var yellingOperation = TransformOperation(monkeyDictionary, simpleMonkey.Operation);
            Operation = yellingOperation;

            if (Operation is LiteralOpeation literal)
            {
                YelledValue = literal.Value;
            }
        }
        private static IYellingOperation TransformOperation(IDictionary<string, YellingMonkey> monkeyDictionary, IParsableOperation operation)
        {
            switch (operation)
            {
                case LiteralOpeation literal:
                    return literal;

                case EquationOperation equation:
                    var left = monkeyDictionary[equation.LeftName];
                    var right = monkeyDictionary[equation.RightName];
                    return new YellingEquationOperation(left, equation.Operator, right);
            }
            return null;
        }
    }

    // TODO: Logic for solving the equation
    private record Monkey(string Name, IParsableOperation Operation)
    {
        public static Monkey Parse(SpanString line)
        {
            line.SplitOnceSpan(": ", out var nameSpan, out var operationSpan);

            var name = nameSpan.ToString();
            var operation = IParsableOperation.FigureParse(operationSpan);
            return new(name, operation);
        }
    }


    private record YellingEquationOperation(YellingMonkey Left,
                                            MathematicalOperator Operator,
                                            YellingMonkey Right)
        : IYellingOperation
    {
        public long Value
        {
            get
            {
                long left = Left.YelledValue.Value;
                long right = Right.YelledValue.Value;
                return Operator.Operate(left, right);
            }
        }
        public bool IsReady => Left.HasYelled && Right.HasYelled;
    }
    private record EquationOperation(string LeftName, MathematicalOperator Operator, string RightName)
        : IParsableOperation
    {
        public static IParsableOperation Parse(SpanString span)
        {
            bool success = span.SplitOnceSpan(' ', out var leftNameSpan, out var operatorRightNameSpan);
            if (!success)
                return null;

            var leftName = leftNameSpan.ToString();
            var operatorChar = operatorRightNameSpan[0];
            var @operator = MathematicalOperators.Parse(operatorChar);

            var rightNameSpan = operatorRightNameSpan.SliceAfter(' ');
            var rightName = rightNameSpan.ToString();

            return new EquationOperation(leftName, @operator, rightName);
        }
    }
    private record LiteralOpeation(long Value)
        : IYellingOperation, IParsableOperation
    {
        bool IYellingOperation.IsReady => true;

        public static IParsableOperation Parse(SpanString span)
        {
            bool success = span.TryParseInt64(out long value);
            if (!success)
                return null;

            return new LiteralOpeation(value);
        }
    }

    private interface IYellingOperation : IOperation
    {
        public long Value { get; }
        public bool IsReady { get; }
    }
    private interface IParsableOperation : IOperation
    {
        public static abstract IParsableOperation Parse(SpanString span);

        public static IParsableOperation FigureParse(SpanString span)
        {
            return LiteralOpeation.Parse(span)
                ?? EquationOperation.Parse(span);
        }
    }
    private interface IOperation
    {
    }
}
