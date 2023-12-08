namespace AdventOfCode.Problems.Year2023;

using MemoryString = ReadOnlyMemory<char>;

public class Day8 : Problem<int>
{
    private NodeMap _nodes;
    private ImmutableArray<Direction> _instructions;

    public override int SolvePart1()
    {
        return CalculateSteps();
    }
    [PartSolution(PartSolutionStatus.Uninitialized)]
    public override int SolvePart2()
    {
        return -1;
    }

    protected override void LoadState()
    {
        var contents = NormalizedFileContents;
        contents.AsMemory()
            .Trim()
            .SplitOnce("\n\n", out var directions, out var nodes);
        _instructions = ParseInstructions(directions.Span);
        _nodes = ParseNodeMap(nodes);
    }
    protected override void ResetState()
    {
        _nodes = null;
        _instructions = default;
    }

    private int CalculateSteps()
    {
        const string start = "AAA";
        const string target = "ZZZ";

        var current = _nodes.NodeWithName(start);
        int passes = 0;

        while (true)
        {
            foreach (var instruction in _instructions)
            {
                current = _nodes.NodeAtDirection(current, instruction);
            }

            passes++;

            if (current.Name.Span is target)
            {
                return passes * _instructions.Length;
            }
        }
    }

    private static ImmutableArray<Direction> ParseInstructions(SpanString contents)
    {
        var result = ImmutableArray.CreateBuilder<Direction>(contents.Length);

        for (int i = 0; i < contents.Length; i++)
        {
            var c = contents[i];
            var direction = ParseDirection(c);
            result.Add(direction);
        }

        return result.ToImmutable();
    }
    private static Direction ParseDirection(char c) => c switch
    {
        'L' => Direction.Left,
        'R' => Direction.Right,
        _ => default,
    };

    private static NodeMap ParseNodeMap(MemoryString nodes)
    {
        var map = new NodeMap();
        var currentMemory = nodes;
        while (true)
        {
            bool found = currentMemory.SplitOnce('\n', out var left, out var right);

            if (!found)
            {
                if (left.Length > 0)
                {
                    var finalNode = ParseNode(left);
                    map.AddNode(finalNode);
                }
                break;
            }

            var nextNode = ParseNode(left);
            map.AddNode(nextNode);

            currentMemory = right;
        }
        return map;
    }

    private static Node ParseNode(MemoryString s)
    {
        s.SplitOnce(" = ", out var name, out var links);
        links = links[1..^1];
        links.SplitOnce(", ", out var left, out var right);
        return new(name, left, right);
    }

    private class NodeMap
    {
        private readonly Dictionary<MemoryString, Node> _nodes = new(SpanComparer.Instance);

        public IEnumerable<Node> AllNodes => _nodes.Values;

        public void AddNode(Node node)
        {
            _nodes.Add(node.Name, node);
        }

        public Node NodeAtDirection(Node node, Direction direction)
        {
            var name = node.NameAt(direction);
            return NodeWithName(name);
        }
        public Node NodeWithName(MemoryString name)
        {
            return _nodes[name];
        }
        public Node NodeWithName(string name)
        {
            return NodeWithName(name.AsMemory());
        }

        private sealed class SpanComparer : IEqualityComparer<MemoryString>
        {
            public static SpanComparer Instance { get; } = new();

            public bool Equals(MemoryString x, MemoryString y)
            {
                return x.Span.SequenceEqual(y.Span);
            }

            public int GetHashCode(MemoryString obj)
            {
                // This is specifically for names
                if (obj.Length < 3)
                    return obj.Length;

                var span = obj.Span;
                return (span[0] << 16)
                    | (span[1] << 8)
                    | span[2];
            }
        }
    }

    private readonly record struct Node(
        MemoryString Name, MemoryString LeftName, MemoryString RightName)
    {
        public MemoryString NameAt(Direction direction)
        {
            return direction switch
            {
                Direction.Left => LeftName,
                Direction.Right => RightName,
            };
        }
    }

    private enum Direction
    {
        Left,
        Right,
    }
}
