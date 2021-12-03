using AdventOfCode.Functions;
using System;
using System.Linq;
using System.Reflection;

namespace AdventOfCode;

public sealed class ProblemRunner
{
    public static readonly string RunPartMethodPrefix = nameof(Problem<int>.RunPart1)[..^1];
    public static readonly string SolvePartMethodPrefix = nameof(Problem<int>.SolvePart1)[..^1];

    public Problem Problem { get; }

    public ProblemRunner(Problem problem)
    {
        Problem = problem;
    }

    public object[] SolveAllParts(bool displayExecutionTimes = true) => SolveAllParts(0, displayExecutionTimes);
    public object[] SolveAllParts(int testCase, bool displayExecutionTimes = true)
    {
        var methods = Problem.GetType().GetMethods().Where(m => m.Name.StartsWith(RunPartMethodPrefix)).ToArray();
        return SolveParts(testCase, methods, displayExecutionTimes);
    }

    public object SolvePart(int part, bool displayExecutionTimes = true) => SolvePart(part, 0, displayExecutionTimes);
    public object SolvePart(int part, int testCase, bool displayExecutionTimes = true)
    {
        var methods = new[] { Problem.GetType().GetMethod(RunPartMethodName(part)) };
        return SolveParts(testCase, methods, displayExecutionTimes)[0];

    }

    private string SolvePartMethodName(int part) => ExecutePartMethodName(SolvePartMethodPrefix, part);
    private string RunPartMethodName(int part) => ExecutePartMethodName(RunPartMethodPrefix, part);
    private string ExecutePartMethodName(string prefix, int part) => $"{prefix}{part}";

    private object[] SolveParts(int testCase, MethodInfo[] solutionMethods, bool displayExecutionTimes)
    {
        var result = new object[solutionMethods.Length];

        Problem.CurrentTestCase = testCase;
        DisplayExecutionTimes(displayExecutionTimes, "Input", Problem.EnsureLoadedState);

        for (int i = 0; i < result.Length; i++)
        {
            DisplayExecutionTimes(displayExecutionTimes, $"Part {i + 1}", SolveAssignResult);

            void SolveAssignResult()
            {
                result[i] = solutionMethods[i].Invoke(Problem, null);
            }
        }
        return result;
    }

    private static void DisplayExecutionTimes(bool displayExecutionTimes, string title, Action action)
    {
        var executionTime = BasicBenchmarking.MeasureExecutionTime(action);

        if (!displayExecutionTimes)
            return;

        Console.WriteLine($"{$"{title}:",9}{executionTime.TotalMilliseconds,13:N2} ms");
    }
}
