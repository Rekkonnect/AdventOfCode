using System;
using System.Linq;
using System.Reflection;

namespace AdventOfCode
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            RunTodaysProblem();
        }

        private static void RunTodaysProblem()
        {
            var currentDate = DateTime.UtcNow - TimeSpan.FromHours(5);
            var currentYear = currentDate.Year;
            var currentDay = currentDate.Day;

            try
            {
                RunProblem(Assembly.GetExecutingAssembly().GetTypes().First(t => t.FullName.EndsWith($"Year{currentYear}.Day{currentDay}")));
            }
            catch
            {
                Console.Error.WriteLine("Today's problem has no solution class. Get back to development you lazy fucking ass.");
            }
        }
        
        private static void RunProblem<T>()
            where T : Problem, new()
        {
            RunProblem(typeof(T));
        }
        private static void RunProblem(Type problemType)
        {
            var instance = problemType.GetConstructor(Type.EmptyTypes).Invoke(null) as Problem;
            int testCases = instance.TestCaseFiles;
            for (int i = 1; i <= testCases; i++)
            {
                Console.WriteLine($"Running test case {i}");
                foreach (var p in instance.TestRunAllParts(i))
                    Console.WriteLine(p);
                Console.WriteLine();
            }
            Console.WriteLine("Running problem");
            foreach (var p in instance.SolveAllParts())
                Console.WriteLine(p);
        }
    }
}
