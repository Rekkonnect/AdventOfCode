namespace AdventOfCode.Utilities
{
    public class LevelNetworkNodeRange<TValue, TLevelNetworkNode, TLevelNetwork>
        where TLevelNetworkNode : LevelNetworkNode<TValue, TLevelNetworkNode, TLevelNetwork>
        where TLevelNetwork : LevelNetwork<TValue, TLevelNetworkNode, TLevelNetwork>
    {
        public TLevelNetworkNode Start { get; }
        public TLevelNetworkNode End { get; }

        public LevelNetworkNodeRange(TLevelNetworkNode start, TLevelNetworkNode end)
        {
            Start = start;
            End = end;
        }
    }
}
