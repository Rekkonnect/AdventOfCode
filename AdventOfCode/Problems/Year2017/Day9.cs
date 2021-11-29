using Garyon.DataStructures;
using Garyon.Extensions;
using System.Linq;

namespace AdventOfCode.Problems.Year2017;

public class Day9 : Problem<int>
{
    private GarbageStream stream;

    public override int SolvePart1()
    {
        return stream.Score;
    }
    public override int SolvePart2()
    {
        return stream.GarbageCharacterCount;
    }

    protected override void LoadState()
    {
        stream = new(FileContents);
    }
    protected override void ResetState()
    {
        stream = null;
    }

    private class GarbageStream
    {
        private string stream;
        private StreamTree streamTree;

        public int Score => streamTree.TraversePreOrderNodes().Sum(n => n.Depth);
        public int GroupCount => streamTree.Count - 1;

        public int GarbageCharacterCount { get; private set; }

        public GarbageStream(string streamContents)
        {
            stream = streamContents;
            AnalyzeStream();
        }

        private void AnalyzeStream()
        {
            streamTree = new();
            var currentNode = streamTree.Root;

            bool isCancelled = false;
            bool withinGarbage = false;
            foreach (char c in stream)
            {
                if (isCancelled)
                {
                    isCancelled = false;
                    continue;
                }

                switch (c)
                {
                    case '!':
                        isCancelled = true;
                        continue;

                    case '<':
                        if (withinGarbage)
                            break;

                        withinGarbage = true;
                        continue;

                    case '>':
                        withinGarbage = false;
                        break;

                    case '{':
                        if (withinGarbage)
                            break;

                        currentNode = currentNode.CreateGroup();
                        break;

                    case '}':
                        if (withinGarbage)
                            break;

                        currentNode = currentNode.Parent;
                        break;
                }

                if (withinGarbage)
                    GarbageCharacterCount++;
            }
        }
    }

    private class StreamTree : Tree<Group, StreamTree, StreamTreeNode>
    {
        public StreamTree()
            : base(new RootStreamTreeNode()) { }

        protected override StreamTreeNode InitializeNewNode(Group value = default)
        {
            return new(value);
        }
        protected override StreamTreeNode InitializeNewNode(StreamTree baseTree, Group value = default)
        {
            return new(baseTree, value);
        }
        protected override StreamTreeNode InitializeNewNode(StreamTreeNode parentNode, Group value = default)
        {
            return new(parentNode, value);
        }
    }
    private sealed class RootStreamTreeNode : StreamTreeNode
    {
        public RootStreamTreeNode()
            : base(new(0)) { }
    }

    private class StreamTreeNode : TreeNode<Group, StreamTree, StreamTreeNode>
    {
        public StreamTreeNode(Group value = default)
            : base(value) { }
        public StreamTreeNode(StreamTree baseTree, Group value = default)
            : base(baseTree, value) { }
        public StreamTreeNode(StreamTreeNode parentNode, Group value = default)
            : base(parentNode, value) { }

        public StreamTreeNode CreateGroup()
        {
            return AddChild(Value.Deeper);
        }

        protected override StreamTreeNode InitializeNewNode(Group value = default)
        {
            return new(value);
        }
        protected override StreamTreeNode InitializeNewNode(StreamTree baseTree, Group value = default)
        {
            return new(baseTree, value);
        }
        protected override StreamTreeNode InitializeNewNode(StreamTreeNode parentNode, Group value = default)
        {
            return new(parentNode, value);
        }
    }

    private struct Group
    {
        public int Depth { get; }

        public Group Deeper => new(Depth + 1);

        public Group(int depth) => Depth = depth;
    }
}
