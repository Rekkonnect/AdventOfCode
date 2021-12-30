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
        return next.AddPrevious(This);
    }
    public bool RemoveNext(TNetworkNode next)
    {
        return next.RemovePrevious(This);
    }

    // Unconventional system design; consider migrating virtualization to AddNext
    public virtual bool AddPrevious(TNetworkNode previous)
    {
        if (previous == this)
            return false;

        if (previousNodes.Contains(previous))
            return false;

        previous.nextNodes.Add(This);
        return previousNodes.Add(previous);
    }
    public bool RemovePrevious(TNetworkNode previous)
    {
        if (!previousNodes.Remove(previous))
            return false;

        previous.nextNodes.Remove(This);
        return true;
    }

    public bool IsConnectedTo(TNetworkNode other) => nextNodes.Contains(other) || previousNodes.Contains(other);

    public void Isolate()
    {
        foreach (var next in nextNodes)
            next.previousNodes.Remove(This);

        foreach (var previous in previousNodes)
            previous.nextNodes.Remove(This);

        nextNodes.Clear();
        previousNodes.Clear();
    }

    public override string ToString() => Value.ToString();
}
