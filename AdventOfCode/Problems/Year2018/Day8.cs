using Garyon.DataStructures;
using System.Linq;

namespace AdventOfCode.Problems.Year2018
{
    public class Day8 : Problem<int>
    {
        private LicenseFile licenseFile;

        public override int SolvePart1()
        {
            return licenseFile.MetadataEntriesSum;
        }
        public override int SolvePart2()
        {
            return licenseFile.RootNodeValue;
        }

        protected override void LoadState()
        {
            licenseFile = new(FileContents.Split(' ').Select(int.Parse).ToArray());
        }
        protected override void ResetState()
        {
            licenseFile = null;
        }

        private class LicenseFile
        {
            private readonly LicenseTree licenseTree = new();

            public int MetadataEntriesSum => licenseTree.TraverseLevelOrderNodes().Sum(n => n.Value?.Sum ?? 0);

            public int RootNodeValue => licenseTree.Root.Children[0].NodeValue;

            public LicenseFile(int[] numbers)
            {
                var currentNode = licenseTree.Root;

                for (int i = 0; i < numbers.Length; )
                {
                    int childrenCount = numbers[i];
                    int metadataEntryCount = numbers[i + 1];

                    // Consider the indices iterated
                    i += 2;

                    var child = new LicenseTreeNode(childrenCount, metadataEntryCount);
                    currentNode.AddChild(child);
                    currentNode = child;

                    while (currentNode.HasEnoughChildren)
                    {
                        int childMetadataCount = currentNode.MetadataEntryCount;
                        currentNode.Value = new(numbers[i..(i + childMetadataCount)]);
                        currentNode = currentNode.Parent;

                        // Indices are iterated
                        i += childMetadataCount;
                    }
                }
            }
        }

        private record MetadataEntries(int[] Entries)
        {
            public int Sum { get; } = Entries.Sum();
        }

        private class RootLicenseTreeNode : LicenseTreeNode
        {
            public sealed override bool HasEnoughChildren => false;

            public RootLicenseTreeNode()
                : base(int.MaxValue, 0) { }
        }
        private class LicenseTreeNode : TreeNode<MetadataEntries, LicenseTree, LicenseTreeNode>
        {
            public int ExpectedChildrenCount { get; init; }
            public int MetadataEntryCount { get; init; }

            public virtual bool HasEnoughChildren => ChildrenCount >= ExpectedChildrenCount;

            public int NodeValue
            {
                get
                {
                    if (ChildrenCount is 0)
                        return Value.Sum;

                    int sum = 0;

                    foreach (var entry in Value.Entries)
                    {
                        if (entry > ChildrenCount)
                            continue;

                        sum += Children[entry - 1].NodeValue;
                    }

                    return sum;
                }
            }

            public LicenseTreeNode(int expectedChildrenCount, int metadataEntryCount)
            {
                ExpectedChildrenCount = expectedChildrenCount;
                MetadataEntryCount = metadataEntryCount;
            }

            public LicenseTreeNode(MetadataEntries entries = null)
                : base(entries) { }
            public LicenseTreeNode(LicenseTree baseTree, MetadataEntries value = null)
                : base(baseTree, value) { }
            public LicenseTreeNode(LicenseTreeNode parentNode, MetadataEntries value = null)
                : base(parentNode, value) { }

            protected override LicenseTreeNode InitializeNewNode(MetadataEntries value) => new(value);
            protected override LicenseTreeNode InitializeNewNode(LicenseTree baseTree, MetadataEntries value) => new(baseTree, value);
            protected override LicenseTreeNode InitializeNewNode(LicenseTreeNode parentNode, MetadataEntries value) => new(parentNode, value);
        }
        private class LicenseTree : Tree<MetadataEntries, LicenseTree, LicenseTreeNode>
        {
            public LicenseTree()
            {
                InternalRoot = new RootLicenseTreeNode();
            }

            protected override LicenseTreeNode InitializeNewNode(MetadataEntries value) => new(value);
            protected override LicenseTreeNode InitializeNewNode(LicenseTree baseTree, MetadataEntries value) => new(baseTree, value);
            protected override LicenseTreeNode InitializeNewNode(LicenseTreeNode parentNode, MetadataEntries value) => new(parentNode, value);
        }
    }
}
