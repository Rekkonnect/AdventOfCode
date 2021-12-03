using AdventOfCode.Utilities;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode;

/*
 * This class should be split into:
 * - Input loading and providing
 * - Running
 * - Solving
 */
public abstract class Problem
{
    private bool stateLoaded;
    private int currentTestCase;

    public int CurrentTestCase
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

    protected string BaseInputDirectory => BaseStreamDirectory(ProblemStreamKind.Input);
    protected string FileContents => GetInputFileContents(CurrentTestCase);
    protected string NormalizedFileContents => GetInputFileContents(CurrentTestCase).NormalizeLineEndings();
    protected string[] FileLines => GetFileLines(CurrentTestCase);
    protected int[] FileNumbersInt32 => ParsedFileLines(int.Parse);
    protected long[] FileNumbersInt64 => ParsedFileLines(long.Parse);

    public int Year => GetType().Namespace[^4..].ParseInt32();
    public int Day => GetType().Name["Day".Length..].ParseInt32();
    public int TestCaseFiles => Directory.GetFiles(BaseInputDirectory).Count(f => Path.GetFileName(f).StartsWith($"{Day}T"));

    protected T[] ParsedFileLines<T>(Parser<T> parser) => ParsedFileLinesEnumerable(parser).ToArray();
    protected T[] ParsedFileLines<T>(Parser<T> parser, int skipFirst, int skipLast) => ParsedFileLinesEnumerable(parser, skipFirst, skipLast).ToArray();
    protected IEnumerable<T> ParsedFileLinesEnumerable<T>(Parser<T> parser) => ParsedFileLinesEnumerable(parser, 0, 0);
    protected IEnumerable<T> ParsedFileLinesEnumerable<T>(Parser<T> parser, int skipFirst, int skipLast) => FileLines.Skip(skipFirst).SkipLast(skipLast).Select(new Func<string, T>(parser));

    protected virtual void LoadState() { }
    protected virtual void ResetState() { }

    public void EnsureLoadedState()
    {
        HandleStateLoading(true, LoadState);
    }
    public void ResetLoadedState()
    {
        HandleStateLoading(false, ResetState);
    }

    private void HandleStateLoading(bool targetStateLoadedStatus, Action stateHandler)
    {
        if (stateLoaded == targetStateLoadedStatus)
            return;
        stateHandler();
        stateLoaded = targetStateLoadedStatus;
    }

    // TODO: Introduce a property in the input handler that toggles performing the download if unavailable input
    private string GetInputFileContents(int testCase, bool performDownload = false)
    {
        var fileLocation = GetInputFileLocation(testCase);
        if (!File.Exists(fileLocation))
            return DownloadInputIfMainInput(testCase, performDownload);

        var input = File.ReadAllText(fileLocation);

        if (input.Length > 0)
            return input;

        return DownloadInputIfMainInput(testCase, performDownload);
    }
    private string[] GetFileLines(int testCase) => GetInputFileContents(testCase).Trim().GetLines();

    private string BaseStreamDirectory(ProblemStreamKind kind) => $@"{Input.GetBaseCodePath()}{kind}s\{Year}";

    private string GetInputFileLocation(int testCase) => GetFileLocation(ProblemStreamKind.Input, testCase);
    private string GetOutputFileLocation(int testCase) => GetFileLocation(ProblemStreamKind.Output, testCase);
    private string GetFileLocation(ProblemStreamKind streamKind, int testCase) => $@"{BaseStreamDirectory(streamKind)}\{Day}{GetTestInputFileSuffix(testCase)}.txt";

    private static string GetTestInputFileSuffix(int testCase) => testCase > 0 ? $"T{testCase}" : null;

    private string DownloadInputIfMainInput(int testCase, bool performDownload)
    {
        if (testCase is 0 && performDownload)
            return DownloadSaveInput();

        return "";
    }
    private string DownloadSaveInput()
    {
        var input = WebsiteScraping.DownloadInput(Year, Day);
        File.WriteAllText(GetInputFileLocation(0), input);
        return input;
    }

    private ProblemOutput DownloadSaveCorrectOutput()
    {
        var output = WebsiteScraping.DownloadAnsweredCorrectOutputs(Year, Day);
        File.WriteAllText(GetOutputFileLocation(0), output.GetFileString());
        return output;
    }

    private enum ProblemStreamKind
    {
        Input,
        Output,
    }
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
