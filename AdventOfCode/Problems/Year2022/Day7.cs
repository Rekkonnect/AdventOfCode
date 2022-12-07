using AdventOfCode.Functions;
using System.Collections.Immutable;

namespace AdventOfCode.Problems.Year2022;

public class Day7 : Problem<long>
{
    private DeviceFileSystem fileSystem;

    public override long SolvePart1()
    {
        return fileSystem.DirectoriesWithSizeLessThan(100000).Sum(d => d.Value);
    }
    public override long SolvePart2()
    {
        const long totalSize = 70000000;
        const long updateSize = 30000000;
        long unusedSize = totalSize - fileSystem.Root.TotalSize;
        long requiredSize = updateSize - unusedSize;

        var directories = fileSystem.DirectoriesWithSizeGreaterThan(requiredSize);
        long smallestDirectorySize = directories.Min(d => d.Value);
        return smallestDirectorySize;
    }

    protected override void LoadState()
    {
        var terminalSyntaxes = ParsedFileLines(ITerminalSyntax.Parse);

        fileSystem = new();

        // The first syntax is "$ cd /"
        var currentDirectory = fileSystem.Root;

        for (int syntaxIndex = 1; syntaxIndex < terminalSyntaxes.Length; syntaxIndex++)
        {
            var currentSyntax = terminalSyntaxes[syntaxIndex];
            switch (currentSyntax)
            {
                case ListInstruction:
                    var listedItems = new List<IDeviceItem>();

                    while (true)
                    {
                        if (syntaxIndex >= terminalSyntaxes.Length - 1)
                            break;

                        if (terminalSyntaxes[syntaxIndex + 1] is not ItemListingSyntax itemListing)
                            break;

                        syntaxIndex++;
                        var listedItem = itemListing.GetListedItem(currentDirectory);
                        listedItems.Add(listedItem);
                    }

                    currentDirectory.Add(listedItems);

                    break;

                case ChangeDirectoryInstruction changeDirectory:
                    if (changeDirectory.ToRoot)
                    {
                        currentDirectory = fileSystem.Root;
                    }
                    else if (changeDirectory.MovesOut)
                    {
                        currentDirectory = currentDirectory.Parent;
                    }
                    else
                    {
                        var nextDirectory = currentDirectory.Directories[changeDirectory.NewDirectory];
                        currentDirectory = nextDirectory;
                    }
                    break;
            }
        }
    }

    private interface ITerminalSyntax
    {
        public static ITerminalSyntax Parse(string line)
        {
            return ITerminalInstruction.Parse(line)
                ?? ItemListingSyntax.Parse(line)
                as ITerminalSyntax;
        }
    }

    private record ItemListingSyntax(string Left, string Name)
        : ITerminalSyntax
    {
        public IDeviceItem GetListedItem(DeviceDirectory parent)
        {
            if (Left is "dir")
                return new DeviceDirectory(Name, parent);

            return new DeviceFile(Name, int.Parse(Left), parent);
        }

        public static ItemListingSyntax Parse(string line)
        {
            int splitter = line.IndexOf(' ', out int nextIndex);
            var left = line[..splitter];
            var right = line[nextIndex..];
            return new(left, right);
        }
    }

    private interface ITerminalInstruction : ITerminalSyntax
    {
        public static new ITerminalInstruction Parse(string line)
        {
            var command = line.SubstringAfter("$ ");
            if (command.Length == line.Length)
                return null;

            return command switch
            {
                "ls" => ListInstruction.Instance,
                _ => ChangeDirectoryInstruction.Parse(command),
            };
        }
    }

    private record ChangeDirectoryInstruction(string NewDirectory)
        : ITerminalInstruction
    {
        public bool ToRoot => NewDirectory is "/";
        public bool MovesOut => NewDirectory is "..";

        public static ChangeDirectoryInstruction Parse(string line)
        {
            var directory = line.SubstringAfter("cd ");
            return new(directory);
        }
    }
    private class ListInstruction : ITerminalInstruction
    {
        public static ListInstruction Instance { get; } = new();
        private ListInstruction() { }
    }

    private class DeviceFileSystem
    {
        public DeviceDirectory Root { get; }

        public DeviceFileSystem()
        {
            Root = DeviceDirectory.InitializeRoot();
        }

        public IDictionary<DeviceDirectory, long> DirectoriesWithSizeLessThan(long size)
        {
            var result = new DeviceDirectorySizeDictionary();

            var directories = new Queue<DeviceDirectory>();
            directories.Enqueue(Root);

            while (directories.Count > 0)
            {
                var currentDirectory = directories.Dequeue();
                if (currentDirectory.TotalSize <= size)
                {
                    result.AddWithSubdirectoriesDeep(currentDirectory);
                }
                else
                {
                    var subdirectories = currentDirectory.Directories.Values;
                    directories.EnqueueRange(subdirectories);
                }
            }

            return result;
        }
        public IDictionary<DeviceDirectory, long> DirectoriesWithSizeGreaterThan(long size)
        {
            var result = new DeviceDirectorySizeDictionary();

            var directories = new Queue<DeviceDirectory>();
            directories.Enqueue(Root);

            while (directories.Count > 0)
            {
                var currentDirectory = directories.Dequeue();
                if (currentDirectory.TotalSize >= size)
                {
                    result.Add(currentDirectory);
                    var subdirectories = currentDirectory.Directories.Values;
                    directories.EnqueueRange(subdirectories);
                }
            }

            return result;
        }
    }

    private sealed class DeviceDirectorySizeDictionary : Dictionary<DeviceDirectory, long>
    {
        public void Add(DeviceDirectory directory)
        {
            Add(directory, directory.TotalSize);
        }

        public void AddRange(IEnumerable<DeviceDirectory> directories)
        {
            foreach (var directory in directories)
            {
                Add(directory);
            }    
        }

        public void AddWithSubdirectoriesDeep(DeviceDirectory directory)
        {
            Add(directory);
            AddRange(directory.EnumerateSubdirectoriesDeep());
        }
    }

    private interface IDeviceItem
    {
        public DeviceDirectory Parent { get; }

        public string Name { get; }
        public DeviceItemType ItemType { get; }
    }

    private class DeviceDirectory : IDeviceItem
    {
        public Dictionary<string, DeviceFile> Files { get; }
        public Dictionary<string, DeviceDirectory> Directories { get; }

        public string Name { get; }
        public DeviceDirectory Parent { get; }

        public int Height
        {
            get
            {
                if (Directories.Count is 0)
                    return 0;

                return Directories.Values.Max(d => d.Height) + 1;
            }
        }

        public DeviceItemType ItemType => DeviceItemType.Directory;

        public long TotalSize
        {
            get
            {
                return Files.Sum(f => f.Value.Size)
                     + Directories.Sum(d => d.Value.TotalSize);
            }
        }

        private DeviceDirectory(string name)
            : this(name, null)
        {
        }
        public DeviceDirectory(string name, DeviceDirectory? parent)
        {
            Name = name;
            Parent = parent;
            Files = new();
            Directories = new();
        }
        public DeviceDirectory(IEnumerable<IDeviceItem> items, string name, DeviceDirectory? parent)
        {
            SplitItems(items, out var files, out var directories);
            Files = files.ToDictionary(f => f.Name);
            Directories = directories.ToDictionary(d => d.Name);
            Name = name;
            Parent = parent;
        }

        public void Add(IEnumerable<IDeviceItem> items)
        {
            SplitItems(items, out var files, out var directories);

            // Too much copy-paste for the time being
            foreach (var file in files)
            {
                if (Files.ContainsKey(file.Name))
                {
                    continue;
                }

                Files.Add(file.Name, file);
            }
            foreach (var directory in directories)
            {
                if (Directories.ContainsKey(directory.Name))
                {
                    continue;
                }

                Directories.Add(directory.Name, directory);
            }
        }

        public IList<DeviceDirectory> EnumerateSubdirectoriesDeep()
        {
            var result = new List<DeviceDirectory>(Directories.Values);

            for (int i = 0; i < result.Count; i++)
            {
                var children = result[i].Directories.Values;
                result.AddRange(children);
            }

            return result;
        }

        private static void SplitItems(IEnumerable<IDeviceItem> items,
                                       out ICollection<DeviceFile> files,
                                       out ICollection<DeviceDirectory> directories)
        {
            files = items.OfType<DeviceFile>().ToList();
            directories = items.OfType<DeviceDirectory>().ToList();
        }

        public static DeviceDirectory InitializeRoot()
        {
            return new("/");
        }
    }
    private record DeviceFile(string Name, long Size, DeviceDirectory Parent)
        : IDeviceItem
    {
        public DeviceItemType ItemType => DeviceItemType.File;
    }

    private enum DeviceItemType
    {
        File,
        Directory,
    }
}
