using AdventOfCode.Problems;
using AdventOfCode.Utilities;
using Garyon.Extensions.ArrayExtensions;
using System;
using System.Linq;
using static Garyon.Functions.ConsoleUtilities;
using static System.Console;

namespace AdventOfCode;

public static class Program
{
    public static void Main(string[] args)
    {
        EnterMainMenu();
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
        var currentYear = currentDate.Year;
        var currentMonth = currentDate.Month;
        var currentDay = currentDate.Day;

        if (selectedYear == currentYear && currentMonth == 12)
            maxDay = currentDay;

        WriteLine("\nAvailable Days:");
        var yearProblemInfo = ProblemsIndex.Instance.GetYearProblemInfo(selectedYear);
        var (leftOffset, topOffset) = GetCursorPosition();
        for (int day = minDay; day <= maxDay; day++)
        {
            int column = Math.DivRem(day - 1, 5, out int line);
            SetCursorPosition(leftOffset + column * 7, topOffset + line);

            WriteProblemInfo(selectedYear, day);
        }

        SetCursorPosition(0, topOffset + Math.Min(maxDay, 5) + 1);
        return ReadConditionalValue(IsValidDay, "Day  ");

        bool IsValidDay(int day) => yearProblemInfo.Contains(day);
    }

    private static ConsoleColor ItemColorForAvailability(bool available)
    {
        return available ? ConsoleColor.Cyan : ConsoleColor.DarkGray;
    }
    private static int ReadConditionalValue(Predicate<int> verifier, string requestMessage = null)
    {
        Write(requestMessage);
        var initialPosition = GetCursorPosition();
        while (true)
        {
            ClearUntilCursorReposition(initialPosition.Left, initialPosition.Top);

            if (!int.TryParse(ReadLineWithColor(ConsoleColor.Cyan), out int value))
                continue;

            if (verifier(value))
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

        try
        {
            RunProblem(currentYear, currentDay, testCases);
        }
        catch
        {
            WriteLine("Today's problem has no solution class.\nGet back to development you lazy fucking ass.");
        }
    }

    private static void RunThisYearsProblem(int day, bool testCases = true)
    {
        var currentDate = ServerClock.Now;
        var currentYear = currentDate.Year;
        RunProblem(currentYear, day, testCases);
    }
    private static void RunProblem(int year, int day, bool testCases = true)
    {
        RunProblem(ProblemsIndex.Instance[year, day], testCases);
    }

    private static void RunProblem<T>()
        where T : Problem, new()
    {
        RunProblem(typeof(T));
    }
    private static void RunProblem(ProblemInfo problemInfo, bool testCases = true)
    {
        RunProblem(problemInfo.ProblemType.ProblemClass, testCases);
    }
    private static void RunProblem(Type problemType, bool testCases = true)
    {
        var instance = problemType.GetConstructor(Type.EmptyTypes).Invoke(null) as Problem;
        RunProblemWithTestCases(instance, testCases);
    }
    private static void RunProblemWithTestCases(Problem instance, bool testCases)
    {
        if (testCases)
            RunProblemTestCases(instance);
        RunProblem(instance);
    }
    private static void RunProblem(Problem instance)
    {
        WriteLine("Running problem");
        foreach (var p in instance.SolveAllParts())
            WriteLine(p);
    }
    private static void RunProblemTestCases(Problem instance)
    {
        int testCases = instance.TestCaseFiles;
        for (int i = 1; i <= testCases; i++)
        {
            WriteLine($"Running test case {i}");
            foreach (var p in instance.SolveAllParts(i))
                WriteLine(p);
            WriteLine();
        }
    }
}
