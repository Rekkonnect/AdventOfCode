using AdventOfCode.Functions;
using AdventOfCode.Utilities.TwoDimensions;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2021;

public class Day23 : Problem<int>
{
    private AmphipodGrid grid;

    public override int SolvePart1() => SolvePart(grid);
    public override int SolvePart2() => SolvePart(grid.ExpandForPart2());

    private static int SolvePart(AmphipodGrid grid) => grid.MinimumEnergyToOrganize();

    protected override void LoadState()
    {
        grid = AmphipodGrid.Parse(FileLines);
    }
    protected override void ResetState()
    {
        grid = null;
    }

#nullable enable

    private record struct ContainerIndex(AmphipodContainerRegion Container, int Index)
    {
        public int HallwayIndex => Container.HallwayIndex(Index);
    }
    private record struct AmphipodMove(ContainerIndex Previous, ContainerIndex Next, Amphipod Amphipod)
    {
        public int Energy => Amphipod.EnergyPerMove * Moves;
        public int Moves
        {
            get
            {
                int distance = Math.Abs(Next.HallwayIndex - Previous.HallwayIndex);
                distance += Next.Container.FirstEmptyHeight + Previous.Container.FirstEmptyHeight + 1;
                return distance;
            }
        }
    }

    private class AmphipodGrid
    {
        private readonly AmphipodStack[] stacks;
        private readonly AmphipodIntermediateSlot[] intermediateSlots;
        private readonly AmphipodSideContainer left = new(Direction.Left),
                                               right = new(Direction.Right);

        private readonly ImmutableArray<Amphipod> amphipods;

        private AmphipodGrid(AmphipodStack[] initializedStacks)
        {
            stacks = initializedStacks;
            intermediateSlots = new AmphipodIntermediateSlot[stacks.Length - 1];
            amphipods = stacks.SelectMany(stack => stack.Amphipods).ToImmutableArray();
        }
        private AmphipodGrid(IReadOnlyCollection<AmphipodStack> amphipodStacks)
            : this(amphipodStacks.ToArray()) { }

        private bool IsOrganized()
        {
            for (int i = 0; i < stacks.Length; i++)
                if (!stacks[i].IsOrganized)
                    return false;

            return true;
        }

        public int MinimumEnergyToOrganize()
        {
            int min = int.MaxValue;

            foreach (var amphipod in amphipods)
                Iterate(amphipod);

            return min;

            void Iterate(Amphipod amphipod, int depth = 0)
            {
                // TODO: Implement
                throw new NotImplementedException();

                if (IsOrganized())
                {
                    min.AssignMin(depth);
                }

                amphipod.Organized = false;
            }
        }

        public AmphipodGrid ExpandForPart2()
        {
            var newStacks = new AmphipodStack[stacks.Length];
            for (int i = 0; i < newStacks.Length; i++)
                newStacks[i] = stacks[i].Expand(2);

            Insert(newStacks[0], 'D', 'D');
            Insert(newStacks[1], 'B', 'C');
            Insert(newStacks[2], 'A', 'B');
            Insert(newStacks[3], 'C', 'A');

            return new(newStacks);

            static void Insert(AmphipodStack stack, char bottom, char top)
            {
                var popped = stack.Pop();
                stack.Push(bottom);
                stack.Push(top);
                stack.Push(popped);
            }
        }

        public static AmphipodGrid Parse(string[] lines)
        {
            int height = 0;
            while (true)
            {
                if (lines[^(height + 1)][0] != ' ')
                    break;

                height++;
            }

            var bottomAmphipodLine = lines[^2];
            var amphipodStacks = new List<AmphipodStack>();

            for (int column = 0; column < bottomAmphipodLine.Length; column++)
            {
                if (!bottomAmphipodLine[column].IsLetter())
                    continue;

                var stack = new AmphipodStack(amphipodStacks.Count, height);
                for (int i = 0; ; i++)
                {
                    char amphipod = lines[^(2 + i)][column];
                    if (!amphipod.IsLetter())
                        break;

                    stack.Push(amphipod);
                }
                amphipodStacks.Add(stack);
            }

            return new(amphipodStacks);
        }
    }

    private class Amphipod
    {
        public readonly char Letter;

        public readonly int Index;
        public readonly int EnergyPerMove;

        public AmphipodContainerRegion Container;
        public bool Organized;

        public Amphipod(char letter, AmphipodContainerRegion container)
        {
            Letter = letter;
            Index = LetterIndex(letter);
            EnergyPerMove = (int)Math.Pow(10, Index);
            Container = container;
        }

        private static int LetterIndex(char letter) => letter - 'A';
    }

    private abstract class AmphipodContainerRegion
    {
        protected const int Columns = 4;
        protected const int SideLength = 2;

        public abstract bool IsFull { get; }
        public abstract bool IsEmpty { get; }

        public abstract int FirstEmptyHeight { get; }
        public abstract int HallwayIndex(int index);

        public bool CanAdd(Amphipod amphipod, int index)
        {
            if (IsFull)
                return false;

            return CanAddImpl(amphipod, index);
        }
        protected abstract bool CanAddImpl(Amphipod amphipod, int index);

        public abstract void Add(Amphipod amphipod, int index);

        public bool CanRemove(int index)
        {
            if (IsEmpty)
                return false;

            return CanRemoveImpl(index);
        }
        protected abstract bool CanRemoveImpl(int index);

        public abstract Amphipod Remove(int index);
    }

    private abstract class AmphipodSlotContainer : AmphipodContainerRegion
    {
        protected sealed override bool CanAddImpl(Amphipod amphipod, int index) => GetSlot(index) is null;

        public sealed override void Add(Amphipod amphipod, int index) => GetSlot(index) = amphipod;

        protected sealed override bool CanRemoveImpl(int index)
        {
            return GetSlot(index) is not null;
        }

        public sealed override Amphipod Remove(int index)
        {
            ref var slot = ref GetSlot(index);
            var removed = slot!;
            slot = null;
            return removed;
        }

        protected abstract ref Amphipod? GetSlot(int index);
    }
    private sealed class AmphipodIntermediateSlot : AmphipodSlotContainer
    {
        private Amphipod? slot;

        public override bool IsFull => slot is not null;
        public override bool IsEmpty => slot is null;

        public override int FirstEmptyHeight => 0;

        public int Index { get; }

        public AmphipodIntermediateSlot(int index)
        {
            Index = index;
        }

        public override int HallwayIndex(int index) => SideLength + 1 + Index * 2;

        protected override ref Amphipod? GetSlot(int index) => ref slot;
    }
    private sealed class AmphipodSideContainer : AmphipodSlotContainer
    {
        private const int rightIndexStart = SideLength + Columns + Columns - 1; 

        private readonly Amphipod?[] amphipods = new Amphipod[2];

        public override bool IsFull => amphipods[0] is not null && amphipods[1] is not null;
        public override bool IsEmpty => amphipods[0] is null && amphipods[1] is null;

        public override int FirstEmptyHeight => 0;

        public Direction Side { get; }

        public AmphipodSideContainer(Direction side)
        {
            Side = side;
        }

        public override int HallwayIndex(int index)
        {
            return Side switch
            {
                Direction.Left => 1 - index,
                Direction.Right => rightIndexStart + index,
            };
        }

        protected override ref Amphipod? GetSlot(int index) => ref amphipods[index];
    }

    private sealed class AmphipodStack : AmphipodContainerRegion
    {
        private readonly Stack<Amphipod> stack;

        public int Height { get; }
        public int Index { get; }

        public override int FirstEmptyHeight => Height - stack.Count;

        public IEnumerable<Amphipod> Amphipods => stack;
        public int Count => stack.Count;

        public bool IsOrganized
        {
            get
            {
                if (stack.Count < Height)
                    return false;

                if (CanRemove())
                    return false;

                return true;
            }
        }

        public override bool IsFull => stack.Count >= Height;
        public override bool IsEmpty => stack.Count is 0;

        private AmphipodStack(int index, int height, Stack<Amphipod> initializedStack)
        {
            Index = index;
            Height = height;
            stack = initializedStack;
        }
        public AmphipodStack(int index, int height)
            : this(index, height, new(height)) { }

        public AmphipodStack Expand(int additionalHeight) => new(Index, Height + additionalHeight, new(stack));

        public void Push(Amphipod amphipod) => stack.Push(amphipod);
        public void Push(char letter) => stack.Push(new(letter, this));
        public Amphipod Pop() => stack.Pop();

        public override int HallwayIndex(int index) => SideLength + Index * 2;

        protected override bool CanAddImpl(Amphipod amphipod, int index)
        {
            return amphipod.Index == index;
        }
        protected override bool CanRemoveImpl(int index)
        {
            return CanRemove();
        }

        private bool CanRemove()
        {
            foreach (var amphipod in stack)
                if (amphipod.Index != Index)
                    return false;

            return true;
        }

        public override void Add(Amphipod amphipod, int index)
        {
            stack.Push(amphipod);
        }
        public override Amphipod Remove(int index)
        {
            return stack.Pop();
        }
    }
}
