using AdventOfCode.Problems;
using AdventOfCode.Utilities;
using Garyon.Extensions.ArrayExtensions;
using Garyon.Reflection;
using System;
using System.Linq;
using static Garyon.Functions.ConsoleUtilities;
using static System.Console;

namespace AdventOfCode;

public static class Program
{
    public static void Main(string[] args)
    {
        RunTodaysProblem();
    }

    private static void ValidateAllSolutions()
    {
        var allProblems = ProblemsIndex.Instance.AllProblems();
        foreach (var problem in allProblems)
        {
            var instance = problem.ProblemType.ProblemClass?.InitializeInstance<Problem>();
            if (instance is null)
                continue;

            var runner = new ProblemRunner(instance);

            WriteLine($"Validating Year {problem.Year} Day {problem.Day}");
            ValidatePart(1);
            ValidatePart(2);
            WriteLine();

            void ValidatePart(int part)
            {
                if (problem.StatusForPart(part) is not PartSolutionStatus.Valid)
                    return;
                if (!runner.ValidatePart(part))
                    WriteLineWithColor($"Part {part} yielded an invalid answer", ConsoleColor.Red);
            }
        }
    }

    private static void EnterMainMenu()
    {
        var problemsIndex = ProblemsIndex.Instance;

        WriteLegend();

        int selectedYear = SelectYear();
        int selectedDay = SelectDay(selectedYear);

        // Run problem
        WriteLine();
        RunProblem(problemsIndex[selectedYear, selectedDay]);
    }

    private static int SelectYear()
    {
        WriteLine("Available Years:");
        var yearSummary = ProblemsIndex.Instance.GetGlobalYearSummary();
        var availableYears = yearSummary.AvailableYears.ToArray().Sort();
        int minYear = availableYears.First();
        int maxYear = availableYears.Last();
        for (int year = maxYear; year >= minYear; year--)
            WriteSummary(yearSummary[year]);

        WriteLine();
        return ReadConditionalValue(year => yearSummary.Contains(year), "Year ");
    }
    private static int SelectDay(int selectedYear)
    {
        const int minDay = 1;
        int maxDay = 25;

        var currentDate = ServerClock.Now;

        if (selectedYear == currentDate.Year && currentDate.Month == 12)
            maxDay = currentDate.Day;

        WriteLine("\nAvailable Days:");
        var (leftOffset, topOffset) = GetCursorPosition();

        var yearProblemInfo = ProblemsIndex.Instance.GetYearProblemInfo(selectedYear);
        for (int day = minDay; day <= maxDay; day++)
        {
            int column = Math.DivRem(day - 1, 5, out int line);
            SetCursorPosition(leftOffset + column * 7, topOffset + line);

            WriteProblemInfo(selectedYear, day);
        }

        // Useful to ensure the cursor goes in its place
        // after finishing writing the final non-25th day
        SetCursorPosition(0, topOffset + Math.Min(maxDay, 5) + 1);
        return ReadConditionalValue(IsValidDay, "Day  ");

        bool IsValidDay(int day) => yearProblemInfo.Contains(day);
    }

    private static ConsoleColor ItemColorForAvailability(bool available)
    {
        return available ? ConsoleColor.Cyan : ConsoleColor.DarkGray;
    }
    private static int ReadConditionalValue(Predicate<int> validator, string requestMessage = null)
    {
        Write(requestMessage);
        var (left, top) = GetCursorPosition();

        while (true)
        {
            ClearUntilCursorReposition(left, top);

            if (!int.TryParse(ReadLineWithColor(ConsoleColor.Cyan), out int value))
                continue;

            if (validator(value))
                return value;
        }
    }

    #region Pretty console writing
    // This kind of functionality must be available somewhere
    // I should browse some packages
    private static void ClearUntilCursorReposition(int startLeft, int startTop)
    {
        ClearUntilCursor(startLeft, startTop);
        SetCursorPosition(startLeft, startTop);
    }
    private static void ClearUntilCursor(int startLeft, int startTop)
    {
        int length = GetConsoleBufferDifference(startLeft, startTop);

        CursorTop = startTop;
        CursorLeft = startLeft;

        var clearString = new string(' ', length);
        Write(clearString);
    }
    private static int GetConsoleBufferDifference(int startLeft, int startTop)
    {
        var (endLeft, endTop) = GetCursorPosition();
        return GetConsoleBufferDifference(startLeft, startTop, endLeft, endTop);
    }
    private static int GetConsoleBufferDifference(int startLeft, int startTop, int endLeft, int endTop)
    {
        int width = BufferWidth;
        int differenceLeft = endLeft - startLeft;
        int differenceTop = endTop - startTop;
        return differenceTop * width - differenceLeft;
    }
    #endregion

    private static void WriteLegend()
    {
        ResetColor();
        WriteLine("Legend:");

        // Valid solutions
        WriteWithColor($"*", GetStarColor(PartSolutionStatus.Valid));
        Write(" = valid solution (includes ");
        WriteWithColor("unoptimized", GetStarColor(PartSolutionStatus.Unoptimized));
        WriteLine(" solutions)");

        // Unoptimized solutions
        WriteWithColor("*", GetStarColor(PartSolutionStatus.Unoptimized));
        WriteLine(" = unoptimized solution");

        // WIP solutions
        WriteWithColor("*", GetStarColor(PartSolutionStatus.WIP));
        WriteLine(" = WIP solution");

        // Uninitialized solutions
        WriteWithColor("*", GetStarColor(PartSolutionStatus.Uninitialized));
        WriteLine(" = uninitialized solution (empty solution)");

        // Unavailable free stars
        WriteWithColor("*", GetStarColor(PartSolutionStatus.UnavailableFreeStar));
        WriteLine(" = unavailable free star");

        WriteLine();
    }

    private static void WriteProblemInfo(int year, int day)
    {
        var info = ProblemsIndex.Instance[year, day];
        WriteWithColor($" {day,2} ", ItemColorForAvailability(ProblemsIndex.Instance.GetYearProblemInfo(year).Contains(day)));
        WriteStar(info.Part1Status);
        WriteStar(info.Part2Status);
        Write(' ');
    }
    private static void WriteSummary(YearSummary summary)
    {
        WriteWithColor($"{summary.Year}  ", ItemColorForAvailability(summary.HasAvailableSolutions));
        WriteSummaryStars(summary.StatusCounters.TotalValidSolutions, GetStarColor(PartSolutionStatus.Valid));
        WriteSummaryStars(summary, PartSolutionStatus.Unoptimized);
        WriteSummaryStars(summary, PartSolutionStatus.WIP);
        WriteLine();
    }
    private static void WriteSummaryStars(YearSummary summary, PartSolutionStatus status)
    {
        WriteSummaryStars(summary.StatusCounters[status], GetStarColor(status));
    }
    private static void WriteSummaryStars(int starCount, ConsoleColor starColor)
    {
        WriteStar(starColor);
        var countColor = starColor.Darken();
        WriteWithColor($" {starCount,2}  ", countColor);
    }

    private static void WriteStar(PartSolutionStatus status)
    {
        WriteStar(GetStarColor(status));
    }
    private static void WriteStar(ConsoleColor starColor)
    {
        WriteWithColor("*", starColor);
    }
    private static ConsoleColor GetStarColor(PartSolutionStatus status) => status switch
    {
        PartSolutionStatus.Valid => ConsoleColor.DarkYellow,
        PartSolutionStatus.Unoptimized => ConsoleColor.Magenta,
        PartSolutionStatus.WIP => ConsoleColor.Blue,
        PartSolutionStatus.Uninitialized => ConsoleColor.DarkGray,
        PartSolutionStatus.UnavailableFreeStar => ConsoleColor.DarkRed,
    };

    private static void RunTodaysProblem(bool testCases = true)
    {
        var currentDate = ServerClock.Now;
        var currentYear = currentDate.Year;
        var currentDay = currentDate.Day;

        if (!RunProblem(currentYear, currentDay, testCases))
        {
            WriteLine($@"
It seems today's problem has no solution class
Focus on development, you lazy fucking ass
              --A happy AoC solver, to himself
");
        }
    }

    private static bool RunThisYearsProblem(int day, bool testCases = true)
    {
        var currentDate = ServerClock.Now;
        var currentYear = currentDate.Year;
        return RunProblem(currentYear, day, testCases);
    }
    private static bool RunProblem(int year, int day, bool testCases = true)
    {
        return RunProblem(ProblemsIndex.Instance[year, day], testCases);
    }

    private static bool RunProblem<T>()
        where T : Problem, new()
    {
        return RunProblem(typeof(T));
    }
    private static bool RunProblem(ProblemInfo problemInfo, bool testCases = true)
    {
        return RunProblem(problemInfo.ProblemType.ProblemClass, testCases);
    }
    private static bool RunProblem(Type problemType, bool testCases = true)
    {
        var instance = problemType?.GetConstructor(Type.EmptyTypes).Invoke(null) as Problem;
        if (instance is null)
            return false;

        RunProblemWithTestCases(instance, testCases);
        return true;
    }
    private static void RunProblemWithTestCases(Problem instance, bool testCases)
    {
        if (testCases)
            RunProblemTestCases(instance);
        RunProblem(instance);
    }
    private static void RunProblem(Problem instance)
    {
        RunProblemCase(instance, 0);
    }
    private static void RunProblemTestCases(Problem instance)
    {
        int testCases = instance.TestCaseFiles;
        for (int i = 1; i <= testCases; i++)
            RunProblemCase(instance, i);
    }

    private static void RunProblemCase(Problem instance, int testCase)
    {
        WriteLine($"Year {instance.Year} Day {instance.Day}");
        WriteLine(testCase switch
        {
            0 => "Running problem\n",
            _ => $"Running test case {testCase}\n",
        });
        var parts = new ProblemRunner(instance).SolveAllParts(testCase);
        WriteLine();
        foreach (var part in parts)
            WriteLine(AnswerStringConversion.Convert(part));
        WriteLine();
    }
}
