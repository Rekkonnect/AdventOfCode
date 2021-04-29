namespace AdventOfCode.Utilities
{
    public class StringCollectionNetworkNode : LevelNetworkNode<char, StringCollectionNetworkNode, StringCollectionNetwork>
    {
        public StringCollectionNetworkNode(int level, char value)
            : base(level, value) { }
    }
}
