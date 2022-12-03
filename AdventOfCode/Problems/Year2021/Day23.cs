#if DEBUG
//#define PRINT_STATES
#endif

using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;
using Garyon.Objects;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2021;

public class Day23 : Problem<int>
{
    private AmphipodGrid grid;
#if PRINT_STATES
    [PrintsToConsole]
#endif
    public override int SolvePart1() => SolvePart(grid);
#if PRINT_STATES
    [PrintsToConsole]
#endif
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

    private record struct ContainerIndex(AmphipodContainerRegion Container, int Index = 0)
    {
        public int HallwayIndex => Container.HallwayIndex(Index);

        public Amphipod Remove() => Container.Remove(Index);
        public void Add(Amphipod amphipod) => Container.Add(amphipod, Index);

        public bool CanRemove() => Container.CanRemove(Index);
    }
    private class AmphipodMove
    {
        // I still don't get why initializing fields or properties through some construct
        // does not exist, and primary constructors cannot be used
        public ContainerIndex Previous { get; }
        public ContainerIndex Next { get; }
        public Amphipod Amphipod { get; }

        public int Moves { get; }
        public int Energy => Amphipod.EnergyPerMove * Moves;

        public AmphipodMove(ContainerIndex previous, ContainerIndex next, Amphipod amphipod)
        {
            Previous = previous;
            Next = next;
            Amphipod = amphipod;
            Moves = GetMoves();
        }

        private int GetMoves()
        {
            int moves = Math.Abs(Next.HallwayIndex - Previous.HallwayIndex);
            moves += Next.Container.FirstEmptyHeight + Previous.Container.FirstEmptyHeight;

            // In the case of a stack, the move has not been performed yet,
            // meaning the extra space will be occupied later
            if (Next.Container is not AmphipodStack)
            {
                moves++;
            }
            else
            {
                if (Previous.Container is AmphipodStack)
                    moves++;
            }

            return moves;
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
            for (int i = 0; i < intermediateSlots.Length; i++)
                intermediateSlots[i] = new(i);

            amphipods = stacks.SelectMany(stack => stack.Amphipods).ToImmutableArray();
        }
        private AmphipodGrid(IReadOnlyCollection<AmphipodStack> amphipodStacks)
            : this(amphipodStacks.ToArray()) { }

        private GridStateCode CurrentStateCode() => new(this);

        private bool IsOrganized()
        {
            foreach (var amphipod in amphipods)
                if (!amphipod.Organized)
                    return false;

            return true;
        }

        public int MinimumEnergyToOrganize()
        {
            var cache = new GridStateCache(new(CurrentStateCode()));

#if PRINT_STATES
            var writer = new DeepConsoleWriter();
#endif

            IterateAllAmphipods(cache.Head);

            return cache.CalculateMinEnergy();

            // CURRENT BUG: Extended grid stacks DO NOT remove amphipods
            void IterateAllAmphipods(GridStateNetworkNode currentStateNode)
            {
                if (currentStateNode.Value.TotalEnergy >= cache.MinTotalEnergy)
                    return;

#if PRINT_STATES
                writer.PushLevel();
#endif

                foreach (var amphipod in amphipods)
                {
                    if (!amphipod.CanRemove())
                        continue;

                    Iterate(amphipod, currentStateNode);
                }

                currentStateNode.LockIterationState();

#if PRINT_STATES
                writer.PopLevel();
#endif
            }

            void Iterate(Amphipod amphipod, GridStateNetworkNode currentStateNode)
            {
                if (currentStateNode.Value.TotalEnergy >= cache.MinTotalEnergy)
                    return;

                var container = amphipod.Container;
                switch (container)
                {
                    case AmphipodStack:
                        if (IterateAttemptPushIntoStackFromStack(amphipod, currentStateNode))
                            break;

                        IterateContainersFromStack(amphipod, currentStateNode);
                        break;

                    default:
                        IterateAttemptPushIntoStack(amphipod, currentStateNode);
                        break;
                }
            }
            void IterateAttemptPushIntoStack(Amphipod amphipod, GridStateNetworkNode currentStateNode)
            {
                if (!CanPushIntoTargetStack(amphipod, out var targetStack))
                    return;

                if (amphipod.Container is AmphipodSideContainer sideContainer)
                {
                    if (sideContainer.HasBlockedEntrance(amphipod.ContainerIndex.Index))
                        return;
                }

                int targetStackIndex = amphipod.Index;
                int slotIndex = amphipod.Container switch
                {
                    AmphipodIntermediateSlot intermediate => intermediate.Index,
                    AmphipodSideContainer side => side.SlotContainerIndex,
                };

                int min, max;

                /*
                 * LL 0 1 2 RS
                 *   0 1 2 3
                 */
                if (slotIndex < targetStackIndex)
                {
                    min = slotIndex + 1;
                    max = targetStackIndex - 1;
                }
                else
                {
                    min = targetStackIndex;
                    max = slotIndex - 1;
                }

                for (int i = min; i <= max; i++)
                {
                    if (intermediateSlots[i].IsFull)
                        return;
                }

                IterateContainer(amphipod, targetStack, currentStateNode);
            }
            bool IterateAttemptPushIntoStackFromStack(Amphipod amphipod, GridStateNetworkNode currentStateNode)
            {
                if (!CanPushIntoTargetStack(amphipod, out var targetStack))
                    return false;

                var startingStack = (amphipod.Container as AmphipodStack)!;

                MathFunctions.Order(startingStack.Index, targetStack.Index, out int min, out int max);

                // Order does not matter, any being full blocks the entire path
                for (int intermediateIndex = min; intermediateIndex < max; intermediateIndex++)
                {
                    if (!intermediateSlots[intermediateIndex].CanAdd(amphipod, 0))
                        return false;
                }

                IterateContainer(amphipod, targetStack, currentStateNode);
                return true;
            }
            bool CanPushIntoTargetStack(Amphipod amphipod, out AmphipodStack targetStack)
            {
                targetStack = TargetAmphipodStack(amphipod);
                return targetStack.CanAdd(amphipod, 0);
            }
            void IterateContainersFromStack(Amphipod amphipod, GridStateNetworkNode currentStateNode)
            {
                var stack = (amphipod.Container as AmphipodStack)!;
                int index = stack.Index;

                IterateLeft();
                IterateRight();

                void IterateLeft()
                {
                    for (int intermediateIndex = index - 1; intermediateIndex >= 0; intermediateIndex--)
                    {
                        var slot = intermediateSlots[intermediateIndex];
                        if (!slot.CanAdd(amphipod, 0))
                            return;

                        IterateContainer(amphipod, slot, currentStateNode);
                    }

                    IterateSideContainer(amphipod, left, currentStateNode);
                }
                void IterateRight()
                {
                    for (int intermediateIndex = index; intermediateIndex < intermediateSlots.Length; intermediateIndex++)
                    {
                        var slot = intermediateSlots[intermediateIndex];
                        if (!slot.CanAdd(amphipod, 0))
                            return;

                        IterateContainer(amphipod, slot, currentStateNode);
                    }

                    IterateSideContainer(amphipod, right, currentStateNode);
                }
                void IterateSideContainer(Amphipod amphipod, AmphipodSideContainer sideContainer, GridStateNetworkNode currentStateNode)
                {
                    for (int i = 1; i >= 0; i--)
                    {
                        if (!sideContainer.CanAdd(amphipod, i))
                            continue;

                        IterateContainerIndex(amphipod, new(sideContainer, i), currentStateNode);
                    }
                }
            }
            void IterateContainer(Amphipod amphipod, AmphipodContainerRegion targetContainer, GridStateNetworkNode currentStateNode)
            {
                IterateContainerIndex(amphipod, new(targetContainer), currentStateNode);
            }
            void IterateContainerIndex(Amphipod amphipod, ContainerIndex targetContainerIndex, GridStateNetworkNode currentStateNode)
            {
                var previousContainerIndex = amphipod.ContainerIndex;

                var move = new AmphipodMove(previousContainerIndex, targetContainerIndex, amphipod);
                int energy = move.Energy;
                int nextEnergy = currentStateNode.Value.TotalEnergy + energy;

                if (nextEnergy >= cache.MinTotalEnergy)
                    return;

                amphipod.ContainerIndex = targetContainerIndex;

                var nextStateCode = CurrentStateCode();
                var nextStateNode = cache.GetNode(nextStateCode);

#if PRINT_STATES
                writer.WriteLine($"{nextEnergy}\n{this}");
#endif

                if (nextStateNode is null)
                {
                    // Discover the new state and recursively identify it
                    var entry = new GridStateEntry(nextStateCode, nextEnergy);
                    nextStateNode = cache.AddEntry(currentStateNode, entry, move);

                    if (IsOrganized())
                    {
                        // It is asserted that the current total energy
                        // is definitely less than the found min
                        cache.SetSolved(nextStateNode);
                    }
                    else
                    {
                        IterateAllAmphipods(nextStateNode);
                    }
                }
                else
                {
                    cache.AddMove(currentStateNode, nextStateNode, move);
                }

                amphipod.ContainerIndex = previousContainerIndex;
            }
        }

        private AmphipodStack TargetAmphipodStack(Amphipod amphipod) => stacks[amphipod.Index];

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

#region ToString
        private static readonly string[] baseGridTopLines = new[]
        {
            "#############",
            "#...........#",
            "###.#.#.#.###",
        };
        private static readonly string[] baseGridIntermediateLines = new[]
        {
            "  #.#.#.#.#  ",
        };
        private static readonly string[] baseGridBottomLines = new[]
        {
            "  #########  ",
        };

        public override string ToString()
        {
            const int leftOffset = 1;

            int intermediates = stacks.First().Height - 1;

            var lineBuilder = new LineStringBuilder();

            lineBuilder.AddLines(baseGridTopLines);
            lineBuilder.AddRepeatedLine(baseGridIntermediateLines[0], intermediates);
            lineBuilder.AddLines(baseGridBottomLines);

            const int hallwayLineIndex = 1;

            foreach (var amphipod in amphipods)
            {
                // Specially handle the stack
                if (amphipod.Container is AmphipodStack)
                    continue;

                int columnIndex = leftOffset + amphipod.HallwayIndex;
                lineBuilder[hallwayLineIndex, columnIndex] = amphipod.Letter;
            }

            foreach (var stack in stacks)
            {
                int stackColumnIndex = leftOffset + stack.HallwayIndex(0);

                int lineIndex = lineBuilder.LineCount - 2;
                foreach (var amphipod in stack.Amphipods.Reverse())
                {
                    lineBuilder[lineIndex, stackColumnIndex] = amphipod.Letter;
                    lineIndex--;
                }
            }

            return lineBuilder.ToString();
        }
#endregion

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

        private enum IterationState
        {
            Unknown,
            DeadEnd,
            Valid,
            Solved,
        }

        private record GridStateMove(GridStateNetworkNode Previous, GridStateNetworkNode Next, AmphipodMove Move)
        {
            public int NextEnergy => Previous.Value.TotalEnergy + Move.Energy;

            public void AssignMinNextEnergy()
            {
                int next = NextEnergy;
                if (Next.Value.TotalEnergy > next)
                    Next.Value.TotalEnergy = next;
            }
        }

        private class GridStateCache
        {
            private readonly GridStateNetwork network;
            private readonly Dictionary<GridStateCode, GridStateNetworkNode> nodeDictionary = new();
            private readonly FlexibleListDictionary<GridStateCode, GridStateMove> moveDictionary = new();

            public GridStateNetworkNode Head => network.Head;

            public GridStateNetworkNode? Solved { get; private set; }
            public int MinTotalEnergy => Solved?.Value.TotalEnergy ?? int.MaxValue;

            public GridStateCache(GridStateEntry initialEntry)
            {
                network = new(initialEntry);
                nodeDictionary.Add(initialEntry.StateCode, network.Head);
            }

            public GridStateNetworkNode? GetNode(GridStateCode code) => nodeDictionary.ValueOrDefault(code);

            public void SetSolved(GridStateNetworkNode node)
            {
                node.IterationState = IterationState.Solved;

                if (Solved is not null)
                {
                    if (Solved.Value.TotalEnergy <= node.Value.TotalEnergy)
                    {
                        return;
                    }
                }

                Solved = node;
            }

            public void AddMove(GridStateNetworkNode previousNode, GridStateNetworkNode nextNode, AmphipodMove move)
            {
                moveDictionary[previousNode.Value.StateCode].Add(new(previousNode, nextNode, move));
                nextNode.AddPrevious(previousNode);
            }
            public GridStateNetworkNode AddEntry(GridStateNetworkNode previousNode, GridStateEntry nextState, AmphipodMove move)
            {
                var nextNode = RegisterEntry(nextState);
                AddMove(previousNode, nextNode, move);
                return nextNode;
            }

            private GridStateNetworkNode RegisterEntry(GridStateEntry entry) => RegisterEntry(entry, out _);
            private GridStateNetworkNode RegisterEntry(GridStateEntry entry, out bool added)
            {
                return nodeDictionary.GetOrAddValue(entry.StateCode, NodeCreator, out added);

                GridStateNetworkNode NodeCreator() => new(entry);
            }

            public void LockIterationStates()
            {
                Head.LockIterationState();
            }

            public int CalculateMinEnergy()
            {
                ApplyMinEnergy();
                return MinTotalEnergy;
            }
            public void ApplyMinEnergy()
            {
                LockIterationStates();

                var nodeQueue = new Queue<GridStateNetworkNode>();
                nodeQueue.Enqueue(Head);

                while (nodeQueue.Any())
                {
                    var currentNode = nodeQueue.Dequeue();

                    var moves = moveDictionary[currentNode.Value.StateCode];
                    foreach (var move in moves)
                    {
                        if (move.Next.IterationState is IterationState.DeadEnd)
                            continue;

                        move.AssignMinNextEnergy();
                        nodeQueue.Enqueue(move.Next);
                    }
                }
            }
        }

        private class GridStateNetwork : HeadedNetwork<GridStateEntry, GridStateNetworkNode, GridStateNetwork>
        {
            public GridStateNetwork(GridStateEntry initialEntry)
                : base(new GridStateNetworkNode(initialEntry)) { }

            protected override GridStateNetworkNode InitializeNode(GridStateEntry value) => new(value);
        }
        private class GridStateNetworkNode : NetworkNode<GridStateEntry, GridStateNetworkNode, GridStateNetwork>
        {
            public IterationState IterationState { get; set; } = IterationState.Unknown;

            public GridStateNetworkNode(GridStateEntry entry)
                : base(entry) { }

            protected override GridStateNetworkNode InitializeNode(GridStateEntry value) => new(value);

            public void LockIterationState()
            {
                if (IterationState is not IterationState.Unknown)
                    return;

                foreach (var next in NextNodes)
                {
                    next.LockIterationState();

                    if (next.IterationState is IterationState.DeadEnd)
                        continue;

                    IterationState = IterationState.Valid;
                }

                if (IterationState is IterationState.Unknown)
                    IterationState = IterationState.DeadEnd;
            }
        }

        private record GridStateEntry(GridStateCode StateCode)
        {
            public int TotalEnergy { get; set; }

            public GridStateEntry(GridStateCode stateCode, int totalEnergy)
                : this(stateCode)
            {
                TotalEnergy = totalEnergy;
            }
        }

        private unsafe struct GridStateCode : IEquatable<GridStateCode>
        {
            // Not the most compact, but definitely the most convenient to design
            /*
             * 01 2 3 4 56
             *
             *   9 B D F
             *   8 A C E
             *
             *   1 3 5 7
             *   0 2 4 6
             * (The stacks are ordered in their popping order)
             */
            private uint hallwayBits;
            private uint lowerStackBits;
            private uint upperStackBits;

            public GridStateCode(AmphipodGrid grid)
                : this()
            {
                SetHallwayBits(grid);
                InitializeGridStacks(grid);
            }

            private void SetHallwayBits(AmphipodGrid grid)
            {
                InitializeHallwayBits(grid.left.GetAtSlot(1), 0);
                InitializeHallwayBits(grid.left.GetAtSlot(0), 1);
                InitializeHallwayBits(grid.intermediateSlots[0].Slot, 2);
                InitializeHallwayBits(grid.intermediateSlots[1].Slot, 3);
                InitializeHallwayBits(grid.intermediateSlots[2].Slot, 4);
                InitializeHallwayBits(grid.right.GetAtSlot(0), 5);
                InitializeHallwayBits(grid.right.GetAtSlot(1), 6);
            }
            private void InitializeHallwayBits(Amphipod? amphipod, int index)
            {
                InitializeAmphipodBits(ref hallwayBits, amphipod, index);
            }

            private void InitializeGridStacks(AmphipodGrid grid)
            {
                foreach (var stack in grid.stacks)
                    InitializeStack(stack);
            }
            private void InitializeStack(AmphipodStack stack)
            {
                int amphipodIndex = 0;
                foreach (var amphipod in stack.Amphipods)
                {
                    int bitIndex = 2 * stack.Index + amphipodIndex;
                    ref var bits = ref lowerStackBits;
                    switch (amphipodIndex)
                    {
                        case 0 or 1:
                            bits = ref lowerStackBits;
                            break;

                        case 2 or 3:
                            bits = ref upperStackBits;
                            bitIndex -= 2;
                            break;
                    }

                    InitializeAmphipodBits(ref bits, amphipod, bitIndex);
                    amphipodIndex++;
                }
            }

            private static void InitializeAmphipodBits(ref uint field, Amphipod? amphipod, int index)
            {
                if (amphipod is null)
                    return;

                // For clarity during debugging
                const int maskBitCount = 4;
                const uint baseMask = 0b111U;
                const uint existenceBit = 0b100U;

                int shifts = index * maskBitCount;

                uint value = existenceBit | (uint)amphipod.Index;
                field |= value << shifts;
            }

            public static bool operator ==(GridStateCode left, GridStateCode right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(GridStateCode left, GridStateCode right)
            {
                return !(left == right);
            }

            public bool Equals(GridStateCode code)
            {
                return this.GetBytes().SequenceEqual(code.GetBytes());
            }
            public override bool Equals(object? obj)
            {
                return obj is GridStateCode code && Equals(code);
            }
            public override int GetHashCode()
            {
                return (int)(hallwayBits ^ lowerStackBits ^ upperStackBits);
            }
            public override string ToString()
            {
                return $"{hallwayBits:X8} {lowerStackBits:X8} {upperStackBits:X8}";
            }
        }
    }

    private class Amphipod
    {
        public readonly char Letter;

        public readonly int Index;
        public readonly int EnergyPerMove;

        public bool Organized;

        private ContainerIndex containerIndex;
        public ContainerIndex ContainerIndex
        {
            get => containerIndex;
            set
            {
                containerIndex.Remove();
                value.Add(this);

                containerIndex = value;
            }
        }

        public AmphipodContainerRegion Container
        {
            get => ContainerIndex.Container;
            set => ContainerIndex = new(value);
        }

        public bool AnalyticIsOrganized => Container is AmphipodStack stack && stack.Index == Index;

        public int HallwayIndex => Container.HallwayIndex(ContainerIndex.Index);

        public Amphipod(char letter, AmphipodStack stack)
        {
            Letter = letter;
            Index = LetterIndex(letter);
            EnergyPerMove = (int)Math.Pow(10, Index);
            containerIndex = new(stack);
        }

        public Amphipod WithStack(AmphipodStack stack) => new(Letter, stack);

        public bool CanRemove()
        {
            if (Organized)
                return false;

            return Container switch
            {
                AmphipodStack stack => stack.Peek() == this,
                _ => ContainerIndex.CanRemove(),
            };
        }

        private static int LetterIndex(char letter) => letter - 'A';

        public override string ToString() => Letter.ToString();
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

        public abstract override string ToString();
    }

    private abstract class AmphipodSlotContainer : AmphipodContainerRegion
    {
        protected override bool CanAddImpl(Amphipod amphipod, int index) => GetSlot(index) is null;

        public sealed override void Add(Amphipod amphipod, int index) => GetSlot(index) = amphipod;

        protected override bool CanRemoveImpl(int index)
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

        public Amphipod? GetAtSlot(int index) => GetSlot(index);
    }
    private sealed class AmphipodIntermediateSlot : AmphipodSlotContainer
    {
        private Amphipod? slot;

        public override bool IsFull => slot is not null;
        public override bool IsEmpty => slot is null;

        public override int FirstEmptyHeight => 0;

        public int Index { get; }

        public int LeftStackIndex => Index;
        public int RightStackIndex => Index + 1;

        public Amphipod? Slot => slot;

        public AmphipodIntermediateSlot(int index)
        {
            Index = index;
        }

        public override int HallwayIndex(int index) => SideLength + 1 + Index * 2;

        protected override ref Amphipod? GetSlot(int index) => ref slot;

        public override string ToString()
        {
            return $"{nameof(AmphipodIntermediateSlot)} - {Index} (between stacks {Index}, {Index + 1})";
        }
    }
    private sealed class AmphipodSideContainer : AmphipodSlotContainer
    {
        private const int rightIndexStart = SideLength + Columns + Columns - 1; 

        private readonly Amphipod?[] amphipods = new Amphipod[2];

        public override bool IsFull => amphipods[0] is not null && amphipods[1] is not null;
        public override bool IsEmpty => amphipods[0] is null && amphipods[1] is null;

        public override int FirstEmptyHeight => 0;

        public Direction Side { get; }

        public int SlotContainerIndex => Side switch
        {
            Direction.Left => -1,
            Direction.Right => Columns - 1,
        };
        public int NearestSlotContainerIndex => Side switch
        {
            Direction.Left => 0,
            Direction.Right => Columns - 2,
        };

        public AmphipodSideContainer(Direction side)
        {
            Side = side;
        }

        protected override bool CanAddImpl(Amphipod amphipod, int index)
        {
            return !HasBlockedEntrance(index) && base.CanAddImpl(amphipod, index);
        }

        protected override bool CanRemoveImpl(int index)
        {
            return !HasBlockedEntrance(index) && base.CanRemoveImpl(index);
        }

        public bool HasBlockedEntrance(int index)
        {
            return index is 1 && amphipods[0] is not null;
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

        public override string ToString()
        {
            return $"{nameof(AmphipodSideContainer)} - {Side}";
        }
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

                if (!AreAllAmphipodsOrganized())
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

        public AmphipodStack Expand(int additionalHeight)
        {
            int expandedHeight = Height + additionalHeight;
            var amphipodStack = new AmphipodStack(Index, expandedHeight);
            amphipodStack.stack.PushRange(stack.Select(amphipod => amphipod.WithStack(amphipodStack)).Reverse());
            return amphipodStack;
        }

        public void Push(char letter) => Push(new Amphipod(letter, this));
        public void Push(Amphipod amphipod)
        {
            stack.Push(amphipod);
            if (AreAllAmphipodsOrganized())
                amphipod.Organized = true;
        }

        public Amphipod Pop() => stack.Pop();
        public Amphipod Peek() => stack.Peek();

        public override int HallwayIndex(int index) => SideLength + Index * 2;

        protected override bool CanAddImpl(Amphipod amphipod, int index)
        {
            return AreAllAmphipodsOrganized() && amphipod.Index == Index;
        }
        protected override bool CanRemoveImpl(int index)
        {
            return !AreAllAmphipodsOrganized();
        }

        public bool AreAllAmphipodsOrganized()
        {
            foreach (var amphipod in stack)
                if (amphipod.Index != Index)
                    return false;

            return true;
        }

        public override void Add(Amphipod amphipod, int index)
        {
            if (CanAddImpl(amphipod, index))
                amphipod.Organized = true;

            Push(amphipod);
        }
        public override Amphipod Remove(int index)
        {
            var popped = Pop();
            popped.Organized = false;
            return popped;
        }

        public override string ToString()
        {
            return $"{nameof(AmphipodStack)} - {Index} - Count: {Count}";
        }
    }
}
