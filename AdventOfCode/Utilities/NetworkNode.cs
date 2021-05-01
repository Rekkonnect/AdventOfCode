namespace AdventOfCode.Utilities
{
    public abstract class NetworkNode<TValue, TNetworkNode, TNetwork> : NetworkNodeBase<TValue, TNetworkNode, TNetwork>
        where TNetworkNode : NetworkNode<TValue, TNetworkNode, TNetwork>
        where TNetwork : HeadedNetwork<TValue, TNetworkNode, TNetwork>
    {
        protected NetworkNode(TValue value = default)
            : base(value) { }

        protected abstract TNetworkNode InitializeNode(TValue value);
    }

    public class NetworkNode<TValue> : NetworkNode<TValue, NetworkNode<TValue>, HeadedNetwork<TValue>>
    {
        public NetworkNode(TValue value = default)
            : base(value) { }

        protected override NetworkNode<TValue> InitializeNode(TValue value) => new(value);
    }
}
