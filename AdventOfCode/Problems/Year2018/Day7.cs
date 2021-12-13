using AdventOfCode.Utilities;

namespace AdventOfCode.Problems.Year2018;

public class Day7 : Problem<string, int>
{
    private InstructionSet instructionSet;

    public override string SolvePart1()
    {
        return instructionSet.GetOrderedStepString();
    }
    public override int SolvePart2()
    {
        return instructionSet.GetTotalWorkTime(5);
    }

    protected override void LoadState()
    {
        instructionSet = new(ParsedFileLines(StepRequirementDeclaration.Parse));
    }
    protected override void ResetState()
    {
        instructionSet = null;
    }

    private class InstructionSet
    {
        private readonly StepNetworkNodeDictionary nodeDictionary = new();

        public InstructionSet(StepRequirementDeclaration[] declarations)
        {
            foreach (var declaration in declarations)
                nodeDictionary[declaration.RequiredStep].AddNext(nodeDictionary[declaration.NextStep]);
        }

        public int GetTotalWorkTime(int workerCount)
        {
            return WorkSteps(workerCount, out _);
        }

        private int WorkSteps(int workerCount, out string stepString)
        {
            int totalTime = 0;

            var stepList = new List<char>(nodeDictionary.Count);

            var workerPool = new WorkerPool(workerCount);
            var available = new SortedSet<StepNetworkNode>();

            var remainingRequirements = new ValueCounterDictionary<StepNetworkNode>();
            foreach (var node in nodeDictionary.Values)
            {
                remainingRequirements[node] = node.PreviousNodes.Count;
                MarkAvailable(node);
            }

            while (remainingRequirements.Any() || available.Any())
            {
                foreach (var assigned in workerPool.AssignWorkingNodes(available))
                {
                    available.Remove(assigned);
                }

                totalTime += workerPool.WorkForMinRemainingTime(out var finishedSteps);

                foreach (var finishedStep in finishedSteps)
                {
                    stepList.Add(finishedStep.Value);

                    foreach (var next in finishedStep.NextNodes)
                    {
                        remainingRequirements.Subtract(next);
                        MarkAvailable(next);
                    }
                }
            }

            stepString = new(stepList.ToArray());
            return totalTime;

            void MarkAvailable(StepNetworkNode node)
            {
                if (remainingRequirements[node] > 0)
                    return;

                available.Add(node);
                remainingRequirements.Remove(node);
            }
        }

        public string GetOrderedStepString()
        {
            WorkSteps(1, out var result);
            return result;
        }
    }

    private class WorkerPool
    {
        private readonly Worker[] workers;

        public int MinRemainingTime
        {
            get
            {
                var filteredWorkers = workers.Where(w => w.IsBusy).ToList();

                if (!filteredWorkers.Any())
                    return 0;

                return filteredWorkers.Min(w => w.RemainingTime);
            }
        }

        public bool AnyBusy => workers.Any(w => w.IsBusy);

        public WorkerPool(int count)
        {
            workers = new Worker[count];
        }

        public int WorkForMinRemainingTime(out IEnumerable<StepNetworkNode> finishedWokingNodes)
        {
            int min = MinRemainingTime;

            var finishedWorkList = new List<StepNetworkNode>(workers.Length);

            for (int i = 0; i < workers.Length; i++)
            {
                if (workers[i].WorkFor(min, out var worked) && worked is not null)
                    finishedWorkList.Add(worked);
            }

            finishedWokingNodes = finishedWorkList;
            return min;
        }

        public List<StepNetworkNode> AssignWorkingNodes(IEnumerable<StepNetworkNode> workingNodes)
        {
            if (!workingNodes.Any())
                return new(0);

            var result = new List<StepNetworkNode>();
            var enumerator = workingNodes.GetEnumerator();
            enumerator.Reset();

            for (int i = 0; i < workers.Length; i++)
            {
                if (workers[i].IsBusy)
                    continue;

                if (!enumerator.MoveNext())
                {
                    enumerator.Dispose();
                    break;
                }

                var selected = enumerator.Current;
                workers[i].WorkingNode = selected;
                result.Add(selected);
            }

            return result;
        }
    }

    private struct Worker
    {
        private StepNetworkNode workingNode;

        public StepNetworkNode WorkingNode
        {
            get => workingNode;
            set
            {
                if (IsBusy)
                    return;

                workingNode = value;
                RemainingTime = workingNode.WorkerTime;
            }
        }
        public int RemainingTime { get; private set; }

        public bool IsBusy => WorkingNode is not null;

        public bool WorkFor(int time, out StepNetworkNode workedStep)
        {
            workedStep = workingNode;

            RemainingTime -= time;

            if (RemainingTime <= 0)
                workingNode = null;

            return !IsBusy;
        }
    }

    private class StepNetworkNodeDictionary : FlexibleDictionary<char, StepNetworkNode>
    {
        private void EnsureExistence(char step)
        {
            if (!ContainsKey(step))
                Add(step, new(step));
        }

        public override StepNetworkNode this[char step]
        {
            get
            {
                EnsureExistence(step);
                return Dictionary[step];
            }
        }
    }

    private class StepNetwork : HeadedNetwork<char, StepNetworkNode, StepNetwork>
    {
        public StepNetwork(IEnumerable<StepNetworkNode> nodes)
            : base(nodes) { }

        protected override StepNetworkNode InitializeNode(char value) => new(value);
    }
    private class StepNetworkNode : NetworkNode<char, StepNetworkNode, StepNetwork>, IComparable<StepNetworkNode>
    {
        public int WorkerTime => 61 + Value - 'A';

        public StepNetworkNode(char value)
            : base(value) { }

        protected override StepNetworkNode InitializeNode(char value) => new(value);

        public int CompareTo(StepNetworkNode other) => Value.CompareTo(other.Value);
    }

    private record StepRequirementDeclaration(char RequiredStep, char NextStep)
    {
        private static readonly Regex stepPattern = new(@"Step (?'required'\w) must be finished before step (?'next'\w) can begin.", RegexOptions.Compiled);

        public static StepRequirementDeclaration Parse(string raw)
        {
            var groups = stepPattern.Match(raw).Groups;
            char required = groups["required"].Value[0];
            char next = groups["next"].Value[0];
            return new(required, next);
        }
    }
}
