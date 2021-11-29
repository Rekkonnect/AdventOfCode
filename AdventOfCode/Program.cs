﻿using AdventOfCode.Problems;
using System;
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
        var currentDate = DateTime.UtcNow - TimeSpan.FromHours(5);
        var currentYear = currentDate.Year;
        var currentMonth = currentDate.Month;
        var currentDay = currentDate.Day;

        var problemsIndex = ProblemsIndex.Instance;

        // Print years
        const int minYear = 2015;
        int maxYear = currentYear;
        if (currentMonth < 12)
            maxYear--;

        WriteLine("Available Years:");
        var years = problemsIndex.GetAvailableYears();
        for (int year = minYear; year <= maxYear; year++)
            WriteLineWithColor(year.ToString(), GetConditionColor(years.Contains(year)));

        WriteLine();
        int selectedYear = ReadConditionalValue(IsValidYear, "Year ");

        // Print Days
        const int minDay = 1;
        int maxDay = 25;
        if (selectedYear == currentYear && currentMonth == 12)
            maxDay = currentDay;

        WriteLine("\nAvailable Days:");
        var days = problemsIndex.GetAvailableDays(selectedYear);
        var (leftOffset, topOffset) = GetCursorPosition();
        for (int day = minDay; day <= maxDay; day++)
        {
            int column = Math.DivRem(day - 1, 5, out int line);
            SetCursorPosition(leftOffset + column * 4, topOffset + line);

            WriteWithColor($"{day,4}", GetConditionColor(days.Contains(day)));
        }

        SetCursorPosition(0, topOffset + Math.Min(maxDay, 5) + 1);
        int selectedDay = ReadConditionalValue(IsValidDay, "Day  ");

        // Run problem
        WriteLine();
        RunProblem(problemsIndex[selectedYear, selectedDay]);

        // Functions
        bool IsValidYear(int year) => year >= minYear && year <= maxYear && years.Contains(year);
        bool IsValidDay(int day) => day >= minDay && day <= maxDay && days.Contains(day);

        static ConsoleColor GetConditionColor(bool condition)
        {
            return condition ? ConsoleColor.Cyan : ConsoleColor.DarkGray;
        }
    }

    private static int ReadConditionalValue(Predicate<int> verifier, string requestMessage = null)
    {
        int value;
        do
        {
            Write(requestMessage);
            int.TryParse(ReadLineWithColor(ConsoleColor.Cyan), out value);
        }
        while (!verifier(value));
        return value;
    }

    private static void RunTodaysProblem(bool testCases = true)
    {
        var currentDate = DateTime.UtcNow - TimeSpan.FromHours(5);
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
        var currentDate = DateTime.UtcNow - TimeSpan.FromHours(5);
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
