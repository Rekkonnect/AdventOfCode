using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Utilities;

public abstract class HeadedNetwork<TValue, TNetworkNode, TNetwork> : NetworkBase<TValue, TNetworkNode, TNetwork>
    where TNetworkNode : NetworkNode<TValue, TNetworkNode, TNetwork>
    where TNetwork : HeadedNetwork<TValue, TNetworkNode, TNetwork>
{
    public TNetworkNode Head { get; private set; }

    protected HeadedNetwork(IEnumerable<TNetworkNode> nodes)
    {
        var previous = nodes;
        while (previous.Any())
        {
            Head = previous.First();
            previous = Head.PreviousNodes;
        }
    }

    protected abstract TNetworkNode InitializeNode(TValue value);
}

public class HeadedNetwork<TValue> : HeadedNetwork<TValue, NetworkNode<TValue>, HeadedNetwork<TValue>>
{
    public HeadedNetwork(IEnumerable<NetworkNode<TValue>> nodes)
        : base(nodes) { }

    protected override NetworkNode<TValue> InitializeNode(TValue value) => new(value);
}
