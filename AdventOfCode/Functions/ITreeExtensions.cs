using AdventOfCode.Utilities;
using Garyon.DataStructures;
using Garyon.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AdventOfCode.Functions;

public static class ITreeExtensions
{
    public static TTreeNode GetFirstLeaf<TValue, TTree, TTreeNode>(this ITree<TValue, TTree, TTreeNode> tree)
        where TTree : ITree<TValue, TTree, TTreeNode>
        where TTreeNode : ITreeNode<TValue, TTree, TTreeNode>
    {
        return tree.TraversePostOrderNodes().FirstOrDefault();
    }

    public static IEnumerable<TTreeNode> TraverseTopLevelOrderNodes<TValue, TTree, TTreeNode>(this ITree<TValue, TTree, TTreeNode> tree)
        where TTree : ITree<TValue, TTree, TTreeNode>
        where TTreeNode : ITreeNode<TValue, TTree, TTreeNode>
    {
        return GetNodeLevels(tree).Values.Flatten();
    }
    public static IEnumerable<TTreeNode> TraverseBottomLevelOrderNodes<TValue, TTree, TTreeNode>(this ITree<TValue, TTree, TTreeNode> tree)
        where TTree : ITree<TValue, TTree, TTreeNode>
        where TTreeNode : ITreeNode<TValue, TTree, TTreeNode>
    {
        return GetNodeLevels(tree).Values.Reverse().Flatten();
    }

    private static ImmutableSortedDictionary<int, List<TTreeNode>> GetNodeLevels<TValue, TTree, TTreeNode>(ITree<TValue, TTree, TTreeNode> tree)
        where TTree : ITree<TValue, TTree, TTreeNode>
        where TTreeNode : ITreeNode<TValue, TTree, TTreeNode>
    {
        var result = new FlexibleListDictionary<int, TTreeNode>();

        Iterate(tree.Root, 0);

        return result.ToImmutableSortedDictionary();

        void Iterate(TTreeNode node, int depth)
        {
            result[depth].Add(node);

            depth++;
            foreach (var child in node.Children)
                Iterate(child, depth);
        }
    }
}
