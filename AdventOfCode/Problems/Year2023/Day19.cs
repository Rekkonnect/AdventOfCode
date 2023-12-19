using AdventOfCode.Utilities;
using System.Data;
using System.Diagnostics;
using System.Numerics;

namespace AdventOfCode.Problems.Year2023;

public class Day19 : Problem<int>
{
    private ImmutableArray<XmasValue> _xmasValues;
    private WorkflowList _workflows;

    public override int SolvePart1()
    {
        return _xmasValues
            .Where(_workflows.Accepts)
            .Sum(x => x.ComponentSum);
    }
    public override int SolvePart2()
    {
        return -1;
    }

    protected override void LoadState()
    {
        NormalizedFileContents.AsMemory()
            .SplitOnce("\n\n", out var rulesSection, out var valuesSection);

        _workflows = ParseWorkflows(rulesSection);
        _xmasValues = ParseValues(valuesSection);
    }
    protected override void ResetState()
    {
        _xmasValues = default;
        _workflows = null;
    }

    private static ImmutableArray<XmasValue> ParseValues(MemoryString valuesSection)
    {
        var currentValues = valuesSection;
        var workflows = ImmutableArray.CreateBuilder<XmasValue>();
        while (true)
        {
            bool found = currentValues.SplitOnce('\n', out var left, out var right);

            if (!found)
            {
                break;
            }

            workflows.Add(ParseValue(left));

            currentValues = right;
        }

        return workflows.ToImmutable();
    }

    private static WorkflowList ParseWorkflows(MemoryString rulesSection)
    {
        var currentRules = rulesSection;
        var workflows = new List<Workflow>();
        while (true)
        {
            bool found = currentRules.SplitOnce('\n', out var left, out var right);

            if (!found)
            {
                break;
            }

            workflows.Add(ParseWorkflow(left));

            currentRules = right;
        }

        return new(workflows);
    }

    private static Workflow ParseWorkflow(MemoryString line)
    {
        line = line[..^1];
        line.SplitOnce('{', out var name, out var rulesString);

        var rules = ImmutableArray.CreateBuilder<ConditionalRule>();
        ConstantRule constant;

        while (true)
        {
            bool split = rulesString.SplitOnce(',', out var rule, out var rest);
            var parsedRule = ParseRule(rule);
            if (!split)
            {
                constant = parsedRule as ConstantRule;
                Debug.Assert(constant is not null);
                break;
            }

            var conditionalRule = parsedRule as ConditionalRule;
            Debug.Assert(conditionalRule is not null);
            rules.Add(conditionalRule);

            rulesString = rest;
        }

        return new(name, rules.ToImmutable(), constant);
    }
    private static WorkflowRule ParseRule(MemoryString s)
    {
        bool containsColon = s.SplitOnce(':', out var condition, out var result);
        if (!containsColon)
        {
            var constantOutput = ParseWorkflowOutput(s);
            return new ConstantRule(constantOutput);
        }

        var parsedCondition = ParseCondition(condition);
        var output = ParseWorkflowOutput(result);
        return new ConditionalRule(parsedCondition, output);
    }
    private static Condition ParseCondition(MemoryString s)
    {
        var span = s.Span;
        var componentChar = span[0];
        var component = ParseComponent(componentChar);
        var operatorChar = span[1];
        var @operator = ParseOperator(operatorChar);
        var value = span[2..].ParseInt32();
        return new(component, @operator, value);
    }
    private static WorkflowOutput ParseWorkflowOutput(MemoryString s)
    {
        if (s.Length is 1)
        {
            var c = s.Span[0];
            if (c is 'A')
                return new ConstantWorkflowOutput(true);

            if (c is 'R')
                return new ConstantWorkflowOutput(false);
        }

        return new WorkflowPointer(s);
    }
    private static XmasValue ParseValue(MemoryString line)
    {
        var current = line[1..^1];

        var result = new XmasValue();

        while (true)
        {
            current.SplitOnce(',', out var left, out var rest);
            if (left.Length is 0)
                break;

            var span = left.Span;
            current = rest;
            var componentChar = span[0];
            var component = ParseComponent(componentChar);
            var value = span[2..].ParseInt32();
            result.SetComponent(component, value);
        }

        return result;  
    }

    private static ComparisonOperator ParseOperator(char c)
    {
        return c switch
        {
            '<' => ComparisonOperator.LessThan,
            '>' => ComparisonOperator.GreaterThan,
        };
    }
    private static XmasComponent ParseComponent(char c)
    {
        return c switch
        {
            'x' => XmasComponent.X,
            'm' => XmasComponent.M,
            'a' => XmasComponent.A,
            's' => XmasComponent.S,
        };
    }

    private class WorkflowList
    {
        private readonly MemoryStringDictionary<Workflow> _workflows;
        private readonly Workflow _in;

        public WorkflowList(IEnumerable<Workflow> workflows)
        {
            _workflows = workflows.ToMemoryStringDictionary(
                w => w.Name, MemoryStringComparerHash3.Instance);
            _in = _workflows["in"];
        }

        public bool Accepts(XmasValue value)
        {
            return Accepts(value, _in);
        }
        private bool Accepts(XmasValue value, Workflow workflow)
        {
            var output = workflow.GetOutput(value);

            if (output is ConstantWorkflowOutput constantOutput)
            {
                return constantOutput.Accept;
            }
            if (output is WorkflowPointer pointer)
            {
                var other = _workflows[pointer.Name];
                return Accepts(value, other);
            }

            return false;
        }
    }

    private record Workflow(
        MemoryString Name, ImmutableArray<ConditionalRule> Conditions, ConstantRule ConstantRule)
    {
        public WorkflowOutput GetOutput(XmasValue value)
        {
            for (int i = 0; i < Conditions.Length; i++)
            {
                var condition = Conditions[i];
                if (condition.Condition.AcceptsValue(value))
                {
                    return condition.Output;
                }
            }

            return ConstantRule.Output;
        }
    }

    private abstract record WorkflowOutput;
    private sealed record ConstantWorkflowOutput(bool Accept)
        : WorkflowOutput;
    private sealed record WorkflowPointer(MemoryString Name)
        : WorkflowOutput;

    private abstract record WorkflowRule(WorkflowOutput Output);

    private sealed record ConstantRule(WorkflowOutput Output)
        : WorkflowRule(Output);
    private sealed record ConditionalRule(Condition Condition, WorkflowOutput Output)
        : WorkflowRule(Output);

    private sealed record Condition(
        XmasComponent XmasComponent, ComparisonOperator Operator, int Value)
    {
        public bool AcceptsValue(XmasValue value)
        {
            int component = value.GetComponent(XmasComponent);
            return SatisfiesOperator(component, Value, Operator);
        }
    }

    private enum XmasComponent
    {
        X,
        M,
        A,
        S,
    }

    private enum ComparisonOperator
    {
        LessThan,
        GreaterThan,
    }

    private static bool SatisfiesOperator<T>(T left, T right, ComparisonOperator @operator)
        where T : IComparisonOperators<T, T, bool>
    {
        return @operator switch
        {
            ComparisonOperator.LessThan => left < right,
            ComparisonOperator.GreaterThan => left > right,
            _ => false,
        };
    }

    private record struct XmasValue(int X, int M, int A, int S)
    {
        public readonly int ComponentSum => X + M + A + S;

        public void SetComponent(XmasComponent component, int value)
        {
            switch (component)
            {
                case XmasComponent.X:
                    X = value;
                    break;
                case XmasComponent.M:
                    M = value;
                    break;
                case XmasComponent.A:
                    A = value;
                    break;
                case XmasComponent.S:
                    S = value;
                    break;
            }
        }

        public readonly int GetComponent(XmasComponent component)
        {
            return component switch
            {
                XmasComponent.X => X,
                XmasComponent.M => M,
                XmasComponent.A => A,
                XmasComponent.S => S,
            };
        }
    }
}
