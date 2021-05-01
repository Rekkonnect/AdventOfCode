namespace AdventOfCode.Utilities
{
    public abstract class NetworkBase<TValue, TNetworkNode, TNetwork> : INodedStructure<TValue, TNetworkNode, TNetwork>
        where TNetworkNode : NetworkNodeBase<TValue, TNetworkNode, TNetwork>
        where TNetwork : NetworkBase<TValue, TNetworkNode, TNetwork>
    {
        public TNetwork This => this as TNetwork;
    }
}
