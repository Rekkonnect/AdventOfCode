using Garyon.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Utilities
{
    public abstract class LevelNetworkNode<TValue, TLevelNetworkNode, TLevelNetwork> : NetworkNodeBase<TValue, TLevelNetworkNode, TLevelNetwork>
        where TLevelNetworkNode : LevelNetworkNode<TValue, TLevelNetworkNode, TLevelNetwork>
        where TLevelNetwork : LevelNetwork<TValue, TLevelNetworkNode, TLevelNetwork>
    {
        public int Level { get; }

        protected LevelNetworkNode(int level, TValue value = default)
            : base(value)
        {
            Level = level;
        }
    }

    public abstract class LevelNetwork<TValue, TLevelNetworkNode, TLevelNetwork> : NetworkBase<TValue, TLevelNetworkNode, TLevelNetwork>
        where TLevelNetworkNode : LevelNetworkNode<TValue, TLevelNetworkNode, TLevelNetwork>
        where TLevelNetwork : LevelNetwork<TValue, TLevelNetworkNode, TLevelNetwork>
    {
        private readonly FlexibleHashSetList<TLevelNetworkNode> levels = new();

        public TLevelNetwork This => this as TLevelNetwork;

        public int LevelCount => levels.Count;

        public int Count => levels.Sum(nodes => nodes.Count);

        protected LevelNetwork() { }
        protected LevelNetwork(IEnumerable<TLevelNetworkNode> graphNodes)
        {
            foreach (var node in graphNodes)
                levels[node.Level].Add(node);
        }

        public IReadOnlySet<TLevelNetworkNode> GetNodes(int level) => levels[level];

        public TLevelNetworkNode GetOrCreateNode(int level, TValue value)
        {
            var filtered = levels[level].Where(n => n.Value.Equals(value)).ToList();
            if (filtered.Any())
                return filtered.First();

            var generated = InitializeNode(level, value);
            levels[level].Add(generated);
            return generated;
        }

        public void Clear() => levels.Clear();

        protected abstract TLevelNetworkNode InitializeNode(int level, TValue value);
    }

    public class LevelNetworkNode<TValue> : LevelNetworkNode<TValue, LevelNetworkNode<TValue>, LevelNetwork<TValue>>
    {
        public LevelNetworkNode(int level, TValue value = default)
            : base(level, value) { }
    }
    public class LevelNetwork<TValue> : LevelNetwork<TValue, LevelNetworkNode<TValue>, LevelNetwork<TValue>>
    {
        public LevelNetwork()
            : base() { }
        public LevelNetwork(IEnumerable<LevelNetworkNode<TValue>> graphNodes)
            : base(graphNodes) { }

        protected override LevelNetworkNode<TValue> InitializeNode(int level, TValue value)
        {
            return new(level, value);
        }
    }
}
