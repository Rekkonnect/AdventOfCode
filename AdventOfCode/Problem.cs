using AdventOfCode.Functions;
using AdventOfCode.Utilities;
using Garyon.Extensions;
using Garyon.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace AdventOfCode;

// This class should be split into containing the problem logic and the running logic
public abstract class Problem
{
    public const string RunPartMethodPrefix = "RunPart";
    public const string SolvePartMethodPrefix = "SolvePart";

    private int currentTestCase;

    protected bool StateLoaded { get; private set; }

    protected int CurrentTestCase
    {
        get => currentTestCase;
        set
        {
            if (currentTestCase == value)
                return;

            currentTestCase = value;
            ResetLoadedState();
        }
    }

    protected string BaseDirectory => $@"{Maintenance.GetBaseCodePath()}Inputs\{Year}";
    protected string FileContents => GetFileContents(CurrentTestCase);
    protected string NormalizedFileContents => GetFileContents(CurrentTestCase).NormalizeLineEndings();
    protected string[] FileLines => GetFileLines(CurrentTestCase);
    protected int[] FileNumbersInt32 => ParsedFileLines(int.Parse);
    protected long[] FileNumbersInt64 => ParsedFileLines(long.Parse);

    public int Year => GetType().Namespace[^4..].ParseInt32();
    public int Day => GetType().Name["Day".Length..].ParseInt32();
    public int TestCaseFiles => Directory.GetFiles(BaseDirectory).Count(f => Path.GetFileName(f).StartsWith($"{Day}T"));

    protected T[] ParsedFileLines<T>(Parser<T> parser) => ParsedFileLinesEnumerable(parser).ToArray();
    protected T[] ParsedFileLines<T>(Parser<T> parser, int skipFirst, int skipLast) => ParsedFileLinesEnumerable(parser, skipFirst, skipLast).ToArray();
    protected IEnumerable<T> ParsedFileLinesEnumerable<T>(Parser<T> parser) => ParsedFileLinesEnumerable(parser, 0, 0);
    protected IEnumerable<T> ParsedFileLinesEnumerable<T>(Parser<T> parser, int skipFirst, int skipLast) => FileLines.RemoveEmptyElements().Skip(skipFirst).SkipLast(skipLast).Select(new Func<string, T>(parser));

    public object[] SolveAllParts(bool displayExecutionTimes = true) => SolveAllParts(0, displayExecutionTimes);
    public object[] SolveAllParts(int testCase, bool displayExecutionTimes = true)
    {
        var methods = GetType().GetMethods().Where(m => m.Name.StartsWith(RunPartMethodPrefix)).ToArray();
        var result = new object[methods.Length];

        CurrentTestCase = testCase;
        DisplayExecutionTimes(displayExecutionTimes, "State loading", EnsureLoadedState);

        for (int i = 0; i < result.Length; i++)
        {
            DisplayExecutionTimes(displayExecutionTimes, $"Part {i + 1}", () =>
            {
                result[i] = methods[i].Invoke(this, null);
            });
        }
        return result;
    }

    private static void DisplayExecutionTimes(bool displayExecutionTimes, string title, Action action)
    {
        if (!displayExecutionTimes)
            return;

        var executionTime = BasicBenchmarking.MeasureExecutionTime(action);
        Console.WriteLine($"{title} execution time: {executionTime.TotalMilliseconds:N2} ms");
    }

    protected virtual void LoadState() { }
    protected virtual void ResetState() { }

    protected void EnsureLoadedState()
    {
        HandleStateLoading(true, LoadState);
    }
    private void ResetLoadedState()
    {
        HandleStateLoading(false, ResetState);
    }

    private void HandleStateLoading(bool targetStateLoadedStatus, Action stateHandler)
    {
        if (StateLoaded == targetStateLoadedStatus)
            return;
        stateHandler();
        StateLoaded = targetStateLoadedStatus;
    }

    private string GetFileContents(int testCase)
    {
        var fileLocation = GetFileLocation(testCase);
        if (!File.Exists(fileLocation))
            return DownloadInput();

        var input = File.ReadAllText(fileLocation);

        // TODO: Fix this logic
        // Only download the input if it's the main input case
        if (testCase > 0)
            return input;

        if (input.Length > 0)
            return input;

        return DownloadInput();
    }
    private string[] GetFileLines(int testCase) => GetFileContents(testCase).GetLines();

    private string GetFileLocation(int testCase) => $@"{BaseDirectory}\{Day}{GetTestInputFileSuffix(testCase)}.txt";
    private static string GetTestInputFileSuffix(int testCase) => testCase > 0 ? $"T{testCase}" : null;

    private string DownloadInput()
    {
        using var client = new HttpClient();

        while (true)
        {
            try
            {
                Console.WriteLine("Downloading input from the website...");
                var inputURI = GetProblemInputURI();
                client.DefaultRequestHeaders.Add("cookie", Secrets.Instance.ToString());
                var response = client.GetAsync(inputURI).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                File.WriteAllText(GetFileLocation(0), responseString);
                Console.WriteLine("Input downloaded");
                return responseString;
            }
            catch (HttpRequestException requestException)
            {
                ConsoleUtilities.WriteExceptionInfo(requestException);
            }
            // Other exceptions are not to be handled
        }
    }
    private string GetProblemInputURI() => $"https://adventofcode.com/{Year}/day/{Day}/input";
    private string GetProblemURI() => $"https://adventofcode.com/{Year}/day/{Day}";
}

public abstract class Problem<T1, T2> : Problem
{
    public T1 RunPart1()
    {
        EnsureLoadedState();
        return SolvePart1();
    }
    public T2 RunPart2()
    {
        EnsureLoadedState();
        return SolvePart2();
    }

    public abstract T1 SolvePart1();
    public abstract T2 SolvePart2();
}

public abstract class Problem<T> : Problem<T, T> { }
