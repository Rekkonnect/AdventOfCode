using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2022;

using ValueList = CircularLinkedList<long>;
using ValueNode = CircularLinkedListNode<long>;

public partial class Day20 : Problem<long>
{
    private ImmutableArray<long> originalValues;

    public override long SolvePart1()
    {
        var mixer = new CircularValueMixer(originalValues);
        mixer.Mix();
        return mixer.GroveCoordinateSum;
    }
    public override long SolvePart2()
    {
        const int decryptionKey = 811589153;
        var multipliedValues = originalValues.Select(v => v * decryptionKey)
                                             .ToImmutableArray();

        var mixer = new CircularValueMixer(multipliedValues);
        mixer.Mix(10);
        return mixer.GroveCoordinateSum;
    }

    protected override void LoadState()
    {
        var fileSpan = FileContents.AsSpan();
        originalValues = fileSpan.Trim().SelectLines(SpanStringExtensions.ParseInt64);
    }

    // Optimization idea: build a bucketed circular linked list that
    // buckets multiple segments of the linked list for skipping multiple
    // elements quicker
    private sealed class CircularValueMixer
    {
        private readonly ImmutableArray<ValueNode> originalNodeOrder;
        private readonly ValueList list;
        private readonly ValueNode zeroNode;

        public int ValueCount => originalNodeOrder.Length;

        public long GroveCoordinateSum
        {
            get
            {
                var next1000 = zeroNode.GetNext(1000);
                var next2000 = next1000.GetNext(1000);
                var next3000 = next2000.GetNext(1000);

                return next1000.Value
                     + next2000.Value
                     + next3000.Value;
            }
        }

        public CircularValueMixer(IEnumerable<long> originalList)
        {
            list = new(originalList);
            originalNodeOrder = list.GetNodeEnumerator().ToImmutableArray();

            zeroNode = list.NodeOf(0);
        }

        public void Mix(int times)
        {
            for (int i = 0; i < times; i++)
            {
                Mix();
            }
        }
        public void Mix()
        {
            foreach (var node in originalNodeOrder)
            {
                MixNode(node);
            }
        }
        private void MixNode(ValueNode node)
        {
            int steps = (int)(node.Value % (ValueCount - 1));
            if (steps is 0)
                return;

            if (steps < 0)
            {
                node.ShiftLeft(Math.Abs(steps));
            }
            else
            {
                node.ShiftRight(steps);
            }
        }
    }
}
