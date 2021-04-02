using AdventOfCode.Functions;
using Garyon.Extensions;
using System;
using System.IO;
using System.Linq;

namespace AdventOfCode
{
    public abstract class Problem
    {
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

        protected string BaseDirectory => $@"Inputs\{Year}";
        protected string FileContents => GetFileContents(CurrentTestCase);
        protected string NormalizedFileContents => GetFileContents(CurrentTestCase).NormalizeLineEndings();
        protected string[] FileLines => GetFileLines(CurrentTestCase);
        protected int[] FileNumbersInt32 => FileLines.Select(int.Parse).ToArray();
        protected long[] FileNumbersInt64 => FileLines.Select(long.Parse).ToArray();

        public int Year => GetType().Namespace.Split('.').Last()[^4..].ParseInt32();
        public int Day => GetType().Name["Day".Length..].ParseInt32();
        public int TestCaseFiles => Directory.GetFiles(BaseDirectory).Where(f => f.Replace('\\', '/').Split('/').Last().StartsWith($"{Day}T")).Count();

        public object[] SolveAllParts(bool displayExecutionTimes = true) => SolveAllParts("RunPart", null, displayExecutionTimes);
        public object[] TestRunAllParts(int testCase, bool displayExecutionTimes = true) => SolveAllParts("TestRunPart", new object[] { testCase }, displayExecutionTimes);

        private object[] SolveAllParts(string methodPrefix, object[] parameters, bool displayExecutionTimes)
        {
            var methods = GetType().GetMethods().Where(m => m.Name.StartsWith(methodPrefix)).ToArray();
            var result = new object[methods.Length];

            DisplayExecutionTimes(displayExecutionTimes, "State loading", EnsureLoadedState);

            for (int i = 0; i < result.Length; i++)
            {
                DisplayExecutionTimes(displayExecutionTimes, $"Part {i + 1}", () =>
                {
                    result[i] = methods[i].Invoke(this, parameters);
                });
            }
            return result;
        }

        private static void DisplayExecutionTimes(bool displayExecutionTimes, string title, Action action)
        {
            if (!displayExecutionTimes)
                return;

            var executionTime = BasicBenchmarking.MeasureExecutionTime(action);
            Console.WriteLine($"{title} execution time: {executionTime.TotalMilliseconds:N2}ms");
        }

        protected T TestRunPart<T>(Func<T> runner, int testCase)
        {
            CurrentTestCase = testCase;
            T result = runner();
            CurrentTestCase = 0;
            return result;
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

        private string GetFileContents(int testCase) => File.ReadAllText(GetFileLocation(testCase));
        private string[] GetFileLines(int testCase) => GetFileContents(testCase).GetLines();

        private string GetFileLocation(int testCase) => $"{BaseDirectory}/{Day}{(testCase > 0 ? $"T{testCase}" : "")}.txt";
    }

    public abstract class Problem<T1, T2> : Problem
    {
        #region Normal Part Running
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
        #endregion

        #region Test Part Running
        public T1 TestRunPart1(int testCase)
        {
            return TestRunPart(RunPart1, testCase);
        }
        public T2 TestRunPart2(int testCase)
        {
            return TestRunPart(RunPart2, testCase);
        }
        #endregion
    }

    public abstract class Problem<T> : Problem<T, T> { }
}
