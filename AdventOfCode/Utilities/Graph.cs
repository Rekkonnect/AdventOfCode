using Garyon.Extensions;
using Garyon.Functions;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Utilities;

// This should be moved to Garyon sometime

public interface INode<TValue>
{
    public TValue Value { get; }
}
public interface INode<TValue, TNode, TNodedStructure> : INode<TValue>
    where TNode : INode<TValue, TNode, TNodedStructure>
    where TNodedStructure : INodedStructure<TValue, TNode, TNodedStructure>
{
}

public interface INodedStructure<TValue>
{
}
public interface INodedStructure<TValue, TNode, TNodedStructure> : INodedStructure<TValue>
    where TNode : INode<TValue, TNode, TNodedStructure>
    where TNodedStructure : INodedStructure<TValue, TNode, TNodedStructure>
{
}

public abstract class GraphNode<TValue, TGraphNode, TGraph> : INode<TValue, TGraphNode, TGraph>
    where TGraphNode : GraphNode<TValue, TGraphNode, TGraph>
    where TGraph : Graph<TValue, TGraphNode, TGraph>
{
    private readonly HashSet<TGraphNode> connectedNodes = new();

    public IReadOnlySet<TGraphNode> ConnectedNodes => connectedNodes;
    public TGraphNode This => this as TGraphNode;

    public TValue Value { get; init; }

    protected GraphNode(TValue value = default)
    {
        Value = value;
    }

    public bool AddConnection(TGraphNode other)
    {
        if (other == this)
            return false;

        other.connectedNodes.Add(This);
        return connectedNodes.Add(other);
    }
    public bool RemoveConnection(TGraphNode other) => connectedNodes.Remove(other);

    public bool IsConnectedTo(TGraphNode other) => connectedNodes.Contains(other);

    public void Isolate() => connectedNodes.Clear();

    public ISet<TGraphNode> GetAllConnectedNodes()
    {
        var resultingNodes = new HashSet<TGraphNode>();
        var queuedNodes = new Queue<TGraphNode>();

        resultingNodes.Add(This);
        queuedNodes.EnqueueRange(connectedNodes);

        while (queuedNodes.Any())
        {
            var dequeuedNode = queuedNodes.Dequeue();
            if (!resultingNodes.Add(dequeuedNode))
                continue;

            foreach (var dequeuedNodeConnection in dequeuedNode.connectedNodes)
            {
                if (resultingNodes.Contains(dequeuedNodeConnection))
                    continue;

                queuedNodes.Enqueue(dequeuedNodeConnection);
            }
        }

        return resultingNodes;
    }
}

public abstract class Graph<TValue, TGraphNode, TGraph> : INodedStructure<TValue, TGraphNode, TGraph>
    where TGraphNode : GraphNode<TValue, TGraphNode, TGraph>
    where TGraph : Graph<TValue, TGraphNode, TGraph>
{
    private readonly HashSet<TGraphNode> nodes;

    public TGraph This => this as TGraph;

    public int Count => nodes.Count;

    public IEnumerable<TGraphNode> Nodes => nodes.Select(Selectors.SelfObjectReturner);
    public IEnumerable<TValue> NodeValues => nodes.Select(node => node.Value);

    protected Graph()
    {
        nodes = new();
    }
    protected Graph(IEnumerable<TGraphNode> graphNodes)
    {
        nodes = new(graphNodes);
    }

    public void AddRange(IEnumerable<TGraphNode> nodeRange)
    {
        var remainingNodeSet = nodeRange.ToHashSet();
        while (remainingNodeSet.Any())
        {
            var firstRemaining = remainingNodeSet.First();
            var connected = firstRemaining.GetAllConnectedNodes();
            remainingNodeSet.ExceptWith(connected);
            nodes.UnionWith(connected);
        }
    }
    protected void AddUnconnectedRange(IEnumerable<TGraphNode> nodeRange)
    {
        nodes.UnionWith(nodeRange);
    }

    public void Add(TGraphNode node)
    {
        nodes.UnionWith(node.GetAllConnectedNodes());
    }
    public bool Remove(TGraphNode node)
    {
        return nodes.Remove(node);
    }

    public void Clear()
    {
        nodes.Clear();
    }

    public IEnumerable<TGraph> IsolateGraphGroups()
    {
        var remainingNodes = new HashSet<TGraphNode>(nodes);

        while (remainingNodes.Any())
        {
            var group = remainingNodes.First().GetAllConnectedNodes();
            remainingNodes.ExceptWith(group);
            yield return InitializeNewInstance(group);
        }
    }

    protected abstract TGraph InitializeNewInstance(IEnumerable<TGraphNode> graphNodes);
}

public class GraphNode<TValue> : GraphNode<TValue, GraphNode<TValue>, Graph<TValue>>
{
    public GraphNode(TValue value = default)
        : base(value) { }
}
public class Graph<TValue> : Graph<TValue, GraphNode<TValue>, Graph<TValue>>
{
    public Graph(IEnumerable<GraphNode<TValue>> graphNodes)
        : base(graphNodes) { }

    protected override Graph<TValue> InitializeNewInstance(IEnumerable<GraphNode<TValue>> graphNodes)
    {
        return new(graphNodes);
    }
}
