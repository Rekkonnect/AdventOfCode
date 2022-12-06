using AdventOfCode.Functions;
using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2017;

public partial class Day7 : Problem<string, int>
{
    private CircusProgram program;

    public override string SolvePart1()
    {
        return program.BottomProgram.Name;
    }
    public override int SolvePart2()
    {
        return program.FindUnbalancedWeight();
    }

    protected override void LoadState()
    {
        program = new(ParsedFileLines(ProgramNodeDeclaration.Parse));
    }
    protected override void ResetState()
    {
        program = null;
    }

    private class CircusProgram
    {
        private readonly ProgramTree programTree;

        public ProgramNode BottomProgram => programTree.Root.Value;

        public CircusProgram(IEnumerable<ProgramNodeDeclaration> programNodes)
        {
            var nodeDictionary = new FlexibleDictionary<string, ProgramTreeNode>();
            // Register all nodes
            foreach (var node in programNodes)
                nodeDictionary[node.Name] = new(new(node));

            // Declare the relationships
            var availableChildren = new HashSet<string>(nodeDictionary.Keys);
            foreach (var node in programNodes)
            {
                if (node.IsLeafNode)
                    continue;

                foreach (var child in node.ChildrenNodes)
                {
                    nodeDictionary[node.Name].AddChild(nodeDictionary[child]);
                    availableChildren.Remove(child);
                }
            }

            // Identify the tree root
            var rootName = availableChildren.Single();
            programTree = new(nodeDictionary[rootName]);
        }

        public int FindUnbalancedWeight()
        {
            var totalWeights = new FlexibleListDictionary<int, ProgramTreeNode>();

            foreach (var current in programTree.TraverseBottomLevelOrderNodes())
            {
                foreach (var c in current.Children)
                    totalWeights[c.TotalWeight].Add(c);

                if (totalWeights.Count > 1)
                {
                    var keys = totalWeights.Keys;

                    int correctWeight = 0;
                    int wrongWeight = 0;
                    ProgramTreeNode wronglyWeightedProgram = null;

                    foreach (int weight in keys)
                    {
                        if (totalWeights[weight].Count == 1)
                        {
                            wronglyWeightedProgram = totalWeights[weight].First();
                            wrongWeight = weight;
                        }
                        else
                            correctWeight = weight;
                    }

                    return wronglyWeightedProgram.Value.Weight + (correctWeight - wrongWeight);
                }

                totalWeights.Clear();
            }

            return -1;
        }
    }

    private class ProgramTree : Tree<ProgramNode, ProgramTree, ProgramTreeNode>
    {
        public ProgramTree()
            : base() { }
        public ProgramTree(ProgramTreeNode root)
            : base(root) { }

        protected override ProgramTreeNode InitializeNewNode(ProgramNode value = null)
        {
            return new(value);
        }
        protected override ProgramTreeNode InitializeNewNode(ProgramTree baseTree, ProgramNode value = null)
        {
            return new(baseTree, value);
        }
        protected override ProgramTreeNode InitializeNewNode(ProgramTreeNode parentNode, ProgramNode value = null)
        {
            return new(parentNode, value);
        }
    }
    private class ProgramTreeNode : TreeNode<ProgramNode, ProgramTree, ProgramTreeNode>
    {
        private int? totalWeight;

        public int TotalWeight => totalWeight ??= Children.Sum(c => c.TotalWeight) + Value.Weight;

        public ProgramTreeNode(ProgramNode value = null)
            : base(value) { }
        public ProgramTreeNode(ProgramTree baseTree, ProgramNode value = null)
            : base(baseTree, value) { }
        public ProgramTreeNode(ProgramTreeNode parentNode, ProgramNode value = null)
            : base(parentNode, value) { }

        protected override ProgramTreeNode InitializeNewNode(ProgramNode value = null)
        {
            return new(value);
        }
        protected override ProgramTreeNode InitializeNewNode(ProgramTree baseTree, ProgramNode value = null)
        {
            return new(baseTree, value);
        }
        protected override ProgramTreeNode InitializeNewNode(ProgramTreeNode parentNode, ProgramNode value = null)
        {
            return new(parentNode, value);
        }
    }

    private record ProgramNode(string Name, int Weight)
    {
        public ProgramNode(ProgramNodeDeclaration programNode)
            : this(programNode.Name, programNode.Weight) { }
    }

    private partial record ProgramNodeDeclaration(string Name, int Weight, string[] ChildrenNodes)
    {
        private static readonly Regex nodePattern = NodeRegex();

        public bool IsLeafNode => ChildrenNodes[0].Length is 0;

        public ProgramNode ToSingleProgramNode() => new(Name, Weight);

        public static ProgramNodeDeclaration Parse(string raw)
        {
            var groups = nodePattern.Match(raw).Groups;
            var name = groups["name"].Value;
            int weight = groups["weight"].Value.ParseInt32();
            var children = groups["children"].Value;
            return new(name, weight, children.Split(", "));
        }

        [GeneratedRegex("(?'name'\\w*) \\((?'weight'\\d*)\\)( -\\> (?'children'.*))*", RegexOptions.Compiled)]
        private static partial Regex NodeRegex();
    }
}
