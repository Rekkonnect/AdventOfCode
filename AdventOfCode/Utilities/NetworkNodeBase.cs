namespace AdventOfCode.Utilities;

public abstract class NetworkNodeBase<TValue, TNetworkNode, TNetwork> : INode<TValue, TNetworkNode, TNetwork>
    where TNetworkNode : NetworkNodeBase<TValue, TNetworkNode, TNetwork>
    where TNetwork : NetworkBase<TValue, TNetworkNode, TNetwork>
{
    private readonly HashSet<TNetworkNode> previousNodes = new();
    private readonly HashSet<TNetworkNode> nextNodes = new();

    public IReadOnlySet<TNetworkNode> PreviousNodes => previousNodes;
    public IReadOnlySet<TNetworkNode> NextNodes => nextNodes;

    public bool IsHead => previousNodes.Count == 0;
    public bool IsTail => nextNodes.Count == 0;

    public TNetworkNode This => this as TNetworkNode;

    public TValue Value { get; }

    protected NetworkNodeBase(TValue value = default)
    {
        Value = value;
    }

    public bool AddNext(TNetworkNode next)
    {
        if (next == this)
            return false;

        if (previousNodes.Contains(next))
            return false;

        next.previousNodes.Add(This);
        return nextNodes.Add(next);
    }
    public bool RemoveNext(TNetworkNode next) => nextNodes.Remove(next);

    public bool AddPrevious(TNetworkNode previous)
    {
        if (previous == this)
            return false;

        if (previousNodes.Contains(previous))
            return false;

        previous.nextNodes.Add(This);
        return previousNodes.Add(previous);
    }
    public bool RemovePrevious(TNetworkNode previous) => previousNodes.Remove(previous);

    public bool IsConnectedTo(TNetworkNode other) => nextNodes.Contains(other) || previousNodes.Contains(other);

    public void Isolate()
    {
        nextNodes.Clear();
        previousNodes.Clear();
    }

    public override string ToString() => Value.ToString();
}
