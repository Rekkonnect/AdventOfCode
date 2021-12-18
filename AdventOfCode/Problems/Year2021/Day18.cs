using AdventOfCSharp.Extensions;
using System.Diagnostics;

namespace AdventOfCode.Problems.Year2021;

public class Day18 : Problem<int>
{
    private Homework homework;

    public override int SolvePart1()
    {
        return homework.Added.Magnitude;
    }
    public override int SolvePart2()
    {
        return homework.LargestMagnitudeAddingTwo;
    }

    protected override void LoadState()
    {
        homework = Homework.Parse(FileLines);
    }
    protected override void ResetState()
    {
        homework = null;
    }

#nullable enable

    private abstract class BaseParser
    {
        protected readonly BaseParser? ParentParser;

        private readonly string? rawTree;
        private int currentIndex;

        protected BaseParser(string original)
        {
            rawTree = original;
        }
        protected BaseParser(BaseParser parent)
        {
            ParentParser = parent;
        }

        protected char Read()
        {
            if (ParentParser is not null)
                return ParentParser.Read();

            char c = rawTree![currentIndex];
            currentIndex++;
            return c;
        }
    }
    private sealed class NumberTreeParser : BaseParser
    {
        private readonly PairNodeParser pairNodeParser;
        private readonly RegularNumberNodeParser regularNumberNodeParser;

        public NumberTreeParser(string original)
            : base(original)
        {
            pairNodeParser = new(this);
            regularNumberNodeParser = new(this);
        }

        public NumberTree Construct()
        {
            // In this problem, all trees have a pair node as a root
            // Because otherwise, we would encounter quite a bit of trouble
            // dealing with splitting
            // An interesting idea for expanding this solution
            return new((ParseChildNode() as PairNode)!);
        }

        private NumberTreeNodeParser ParserForInitialNodeCharacter(char c)
        {
            return c switch
            {
                '[' => pairNodeParser,
                _   => regularNumberNodeParser
            };
        }

        private NumberTreeNodeParser ReadGetParser(out char initialCharacter)
        {
            initialCharacter = Read();
            return ParserForInitialNodeCharacter(initialCharacter);
        }
        private NumberNode ParseChildNode()
        {
            var parser = ReadGetParser(out char initialCharacter);
            return parser.ConstructNode(initialCharacter);
        }

        private abstract class NumberTreeNodeParser : BaseParser
        {
            public NumberTreeParser ParentTreeParser => (ParentParser as NumberTreeParser)!;

            public NumberTreeNodeParser(NumberTreeParser parent)
                : base(parent) { }

            public abstract NumberNode ConstructNode(char initialCharacter);
        }
        private sealed class PairNodeParser : NumberTreeNodeParser
        {
            public PairNodeParser(NumberTreeParser parent)
                : base(parent) { }

            public override NumberNode ConstructNode(char initialCharacter)
            {
                var leftNode = ParentTreeParser.ParseChildNode();
                Read(',');
                var rightNode = ParentTreeParser.ParseChildNode();
                Read(']');
                return new PairNode(leftNode, rightNode);
            }

            private void Read(char assertedChar)
            {
                char c = Read();
                Debug.Assert(c == assertedChar);
            }
        }
        private sealed class RegularNumberNodeParser : NumberTreeNodeParser
        {
            public RegularNumberNodeParser(NumberTreeParser parent)
                : base(parent) { }

            public override NumberNode ConstructNode(char initialCharacter)
            {
                return new RegularNumberNode(initialCharacter.GetNumericValueInteger());
            }
        }
    }

    private sealed record NumberTree(PairNode Root)
    {
        public int Magnitude => Root.Magnitude;

        public void Reduce()
        {
            bool alive;
            do
            {
                alive = false;

                foreach (var pairNode in Root.TraversePairNodesInOrder())
                {
                    if (pairNode.RecursiveDepth >= 4)
                        pairNode.Explode();
                }

                foreach (var regularNumberNode in Root.TraverseRegularNumberNodesInOrder())
                {
                    if (regularNumberNode.Value >= 10)
                    {
                        regularNumberNode.Split();
                        // An additional implementation is recursing Reduce in this point
                        // But semantically, it feels like a hack
                        alive = true;
                        break;
                    }
                }
            }
            while (alive);
        }

        public NumberTree RootClone() => new(Root.Clone());

        public override string ToString() => Root.ToString()!;

        public static NumberTree operator +(NumberTree left, NumberTree right)
        {
            var result = new NumberTree(new PairNode(left.Root, right.Root)).RootClone();
            result.Reduce();
            return result;
        }

        public static NumberTree Parse(string number)
        {
            return new NumberTreeParser(number).Construct();
        }
    }

    private enum ParentSide
    {
        None,
        Left,
        Right,
    }
    private static ParentSide OppositeSide(ParentSide side) => side switch
    {
        ParentSide.None => ParentSide.None,

        ParentSide.Left => ParentSide.Right,
        ParentSide.Right => ParentSide.Left,
    };

    private abstract class NumberNode
    {
        public PairNode? Parent { get; set; }
        public ParentSide ParentSide { get; set; }

        // OPTIMIZATION: Fix this and use it in reduction
        public int Depth { get; set; }

        public abstract int Magnitude { get; }

        // TODO: Make this worthless
        public int RecursiveDepth
        {
            get
            {
                int depth = 0;
                var current = Parent;
                while (current is not null)
                {
                    current = current.Parent;
                    depth++;
                }
                return depth;
            }
        }

        public RegularNumberNode? Next => NextAtSide(ParentSide.Right);
        public RegularNumberNode? Previous => NextAtSide(ParentSide.Left);

        public abstract NumberNode Clone();

        protected void ReplaceThisNodeForParent(NumberNode replaced)
        {
            Parent!.SetNodeAtSide(ParentSide, replaced);
        }

        private RegularNumberNode? NextAtSide(ParentSide side)
        {
            var otherSideChild = this;

            while (true)
            {
                if (otherSideChild is null)
                    return null;

                if (otherSideChild.ParentSide == OppositeSide(side))
                    break;

                otherSideChild = otherSideChild.Parent;
            }

            // It is guaranteed that the child has a parent
            var parent = otherSideChild.Parent!;
            var iteratedChild = parent.NodeAtSide(side);
            while (iteratedChild is PairNode pairChild)
            {
                iteratedChild = pairChild.NodeAtSide(OppositeSide(side));
            }
            // In a fully parsed tree, there is no null regular number node
            return iteratedChild as RegularNumberNode;
        }

        public abstract override string ToString();
    }
    // Unfortunately, primary constructors do not allow this kind of logic
    private sealed class PairNode : NumberNode
    {
        private NumberNode left, right;

        public NumberNode Left
        {
            get => left;
            set => AssignChild(ref left, value, ParentSide.Left);
        }
        public NumberNode Right
        {
            get => right;
            set => AssignChild(ref right, value, ParentSide.Right);
        }

        public override int Magnitude => 3 * Left.Magnitude + 2 * Right.Magnitude;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PairNode(NumberNode left, NumberNode right)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Left = left;
            Right = right;
        }

        public override PairNode Clone()
        {
            return new(Left.Clone(), Right.Clone());
        }

        public void SetNodeAtSide(ParentSide side, NumberNode value)
        {
            switch (side)
            {
                case ParentSide.Left:
                    Left = value;
                    break;

                case ParentSide.Right:
                    Right = value;
                    break;
            }
        }
        public NumberNode NodeAtSide(ParentSide side) => side switch
        {
            ParentSide.Left => Left,
            ParentSide.Right => Right,
        };

        private void AssignChild(ref NumberNode field, NumberNode child, ParentSide parentSide)
        {
            if (ReferenceEquals(field, child))
                return;

            // Disown
            if (field is not null)
                field.Parent = null;

            field = child;
            field.Parent = this;
            field.Depth = Depth + 1;
            field.ParentSide = parentSide;
        }

        public void Explode()
        {
            var previous = Previous;
            if (previous is not null)
                previous.Value += (Left as RegularNumberNode)!.Value;

            var next = Next;
            if (next is not null)
                next.Value += (Right as RegularNumberNode)!.Value;

            var newThis = new RegularNumberNode();
            ReplaceThisNodeForParent(newThis);
        }

        // This copy-pasted mess is faster than other abstractions
        public IEnumerable<PairNode> TraversePairNodesInOrder()
        {
            if (Left is PairNode leftPair)
            {
                foreach (var traversed in leftPair.TraversePairNodesInOrder())
                    yield return traversed;
            }

            yield return this;

            if (Right is PairNode rightPair)
            {
                foreach (var traversed in rightPair.TraversePairNodesInOrder())
                    yield return traversed;
            }
        }
        public IEnumerable<RegularNumberNode> TraverseRegularNumberNodesInOrder()
        {
            if (Left is PairNode leftPair)
            {
                foreach (var traversed in leftPair.TraverseRegularNumberNodesInOrder())
                    yield return traversed;
            }
            else if (Left is RegularNumberNode leftRegular)
            {
                yield return leftRegular;
            }

            if (Right is PairNode rightPair)
            {
                foreach (var traversed in rightPair.TraverseRegularNumberNodesInOrder())
                    yield return traversed;
            }
            else if (Right is RegularNumberNode rightRegular)
            {
                yield return rightRegular;
            }
        }

        public override string ToString() => $"[{Left},{Right}]";
    }
    private sealed class RegularNumberNode : NumberNode
    {
        public int Value { get; set; }

        public override int Magnitude => Value;

        public RegularNumberNode()
            : this(0) { }
        public RegularNumberNode(int value)
        {
            Value = value;
        }

        public override RegularNumberNode Clone()
        {
            return new(Value);
        }

        public void Split()
        {
            int left = Value / 2;
            int right = Value - left;

            var leftNode = new RegularNumberNode(left);
            var rightNode = new RegularNumberNode(right);
            var newThis = new PairNode(leftNode, rightNode);
            ReplaceThisNodeForParent(newThis);
        }

        public override string ToString() => $"{Value}";
    }

    private sealed class Homework
    {
        private readonly NumberTree[] trees;

        public NumberTree Added
        {
            get
            {
                var result = trees.First().RootClone();
                result.Reduce();
                foreach (var other in trees.Skip(1))
                    result += other;
                
                return result;
            }
        }
        public int LargestMagnitudeAddingTwo
        {
            get
            {
                int max = 0;

                for (int i = 0; i < trees.Length; i++)
                {
                    for (int j = 0; j < trees.Length; j++)
                    {
                        if (i == j)
                            continue;

                        int sum = (trees[i] + trees[j]).Magnitude;
                        if (sum > max)
                            max = sum;
                    }
                }

                return max;
            }
        }

        private Homework(NumberTree[] numberTrees)
        {
            trees = numberTrees;
        }

        public static Homework Parse(string[] numbers)
        {
            return new(numbers.SelectArray(NumberTree.Parse));
        }
    }
}
