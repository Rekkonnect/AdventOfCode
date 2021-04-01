using Garyon.DataStructures;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day18 : Problem<ulong>
    {
        private IEnumerable<ArithmeticExpression> expressions;

        public override ulong SolvePart1()
        {
            return EvaluateExpressions<SimpleMathEvaluator>();
        }
        public override ulong SolvePart2()
        {
            return EvaluateExpressions<AdvancedMathEvaluator>();
        }

        protected override void LoadState()
        {
            expressions = NormalizedFileContents.Replace(" ", "").GetLines(false).Select(s => new ArithmeticExpression(s));
        }
        protected override void ResetState()
        {
            expressions = null;
        }

        private ulong EvaluateExpressions<T>()
            where T : SimpleMathEvaluator, new()
        {
            var evaluator = new T();
            return expressions.Select(e => evaluator.EvaluateFromTree(e)).Sum();
        }

        private class AdvancedMathEvaluator : SimpleMathEvaluator
        {
            protected override int GetPriority(ArithmeticOperation operation) => operation switch
            {
                ArithmeticOperation.Addition => 2,
                ArithmeticOperation.Multiplication => 1,
                _ => 0,
            };
        }

        private class SimpleMathEvaluator
        {
            private readonly Stack<Stack<ArithmeticExpressionTreeNode>> stacks = new();

            public ulong EvaluateFromTree(ArithmeticExpression expression)
            {
                var tree = GetTree(expression);

                var results = new Stack<ulong>();

                foreach (var component in tree.TraversePostOrder())
                {
                    switch (component)
                    {
                        case ArithmeticExpressionNumericalComponent n:
                            results.Push(n.Value);
                            break;
                        case ArithmeticExpressionOperationComponent op:
                            ulong a = results.Pop();
                            ulong b = results.Pop();
                            results.Push(op.Operation switch
                            {
                                ArithmeticOperation.Addition => a + b,
                                ArithmeticOperation.Multiplication => a * b,
                            });
                            break;
                    }
                }

                return results.First();
            }

            protected virtual ArithmeticExpressionTree GetTree(ArithmeticExpression expression)
            {
                var subtree = GetSubtree(expression.RawExpression, 0, out _);
                return new ArithmeticExpressionTree(subtree);
            }
            private ArithmeticExpressionTreeNode GetSubtree(string rawExpression, int startingIndex, out int endingIndex)
            {
                int previousPriority = GetPriority(ArithmeticOperation.Initialization);

                var currentStack = new Stack<ArithmeticExpressionTreeNode>();
                stacks.Push(currentStack);

                for (endingIndex = startingIndex; endingIndex < rawExpression.Length; endingIndex++)
                {
                    char c = rawExpression[endingIndex];

                    if (c.IsDigit())
                    {
                        ulong value = (ulong)c.GetNumericValueInteger();
                        currentStack.Push(new(value));
                        continue;
                    }

                    if (c == ')')
                    {
                        break;
                    }

                    if (c == '(')
                    {
                        var subtree = GetSubtree(rawExpression, endingIndex + 1, out endingIndex);
                        currentStack.Push(subtree);
                        continue;
                    }

                    var operation = c switch
                    {
                        '+' => ArithmeticOperation.Addition,
                        '*' => ArithmeticOperation.Multiplication,
                    };

                    int priority = GetPriority(operation);
                    if (priority <= previousPriority)
                    {
                        FinalizeCurrentExpression();
                    }
                    var newParent = new ArithmeticExpressionTreeNode(operation);
                    newParent.LeftChild = currentStack.Pop();
                    currentStack.Push(newParent);

                    previousPriority = priority;
                }

                while (currentStack.Count > 1)
                    FinalizeCurrentExpression();
                // https://www.youtube.com/watch?v=wNpht4ztFCQ
                return stacks.Pop().Pop();
            }
            private void FinalizeCurrentExpression()
            {
                var currentStack = stacks.Peek();
                // WARNING: Do NOT simplify the following two lines; ordering of the expressions will be heavily confusing
                var finalExpression = currentStack.Pop();
                SetCurrentHeadRightChild(finalExpression);
            }
            private void SetCurrentHeadRightChild(ArithmeticExpressionTreeNode node)
            {
                var currentStack = stacks.Peek();

                if (currentStack.Count > 0)
                    currentStack.Peek().RightChild = node;
                else
                    currentStack.Push(node);
            }

            protected virtual int GetPriority(ArithmeticOperation operation) => operation switch
            {
                ArithmeticOperation.Initialization => 0,
                _ => 1
            };
        }

        private abstract record ArithmeticExpressionComponentBase;
        private record ArithmeticExpressionNumericalComponent(ulong Value) : ArithmeticExpressionComponentBase
        {
            public override string ToString() => Value.ToString();
        }
        private record ArithmeticExpressionOperationComponent(ArithmeticOperation Operation) : ArithmeticExpressionComponentBase
        {
            public override string ToString() => Operation.ToString();
        }

        private class ArithmeticExpressionTreeNode : BinaryTreeNode<ArithmeticExpressionComponentBase>
        {
            public ArithmeticExpressionTreeNode(ArithmeticExpressionComponentBase value)
                : base(value) { }
            public ArithmeticExpressionTreeNode(ArithmeticOperation operation)
                : this(new ArithmeticExpressionOperationComponent(operation)) { }
            public ArithmeticExpressionTreeNode(ulong value)
                : this(new ArithmeticExpressionNumericalComponent(value)) { }
        }
        private class ArithmeticExpressionTree : BinaryTree<ArithmeticExpressionComponentBase>
        {
            public ArithmeticExpressionTree()
                : base() { }
            public ArithmeticExpressionTree(ArithmeticExpressionComponentBase value)
                : base(value) { }
            public ArithmeticExpressionTree(ArithmeticExpressionTreeNode root)
                : base(root) { }
        }

        private record ArithmeticExpression(string RawExpression)
        {
            public override string ToString() => RawExpression;
        }

        private enum ArithmeticOperation
        {
            Initialization,
            Addition,
            Multiplication,
        }
    }
}
