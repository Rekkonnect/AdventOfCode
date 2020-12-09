﻿using AdventOfCode.Functions;
using System;
using System.IO;
using System.Linq;

namespace AdventOfCode
{
    public abstract class Problem
    {
        private int currentTestCase;

        protected int CurrentTestCase
        {
            get => currentTestCase;
            set
            {
                if (currentTestCase == value)
                    return;

                currentTestCase = value;
                ResetState();
            }
        }

        protected string BaseDirectory => $@"Inputs\{Year}";
        protected string FileContents => GetFileContents(CurrentTestCase);
        protected string NormalizedFileContents => GetFileContents(CurrentTestCase).NormalizeLineEndings();
        protected string[] FileLines => GetFileLines(CurrentTestCase);
        protected int[] FileNumbersInt32 => FileLines.Select(n => int.Parse(n)).ToArray();
        protected long[] FileNumbersInt64 => FileLines.Select(n => long.Parse(n)).ToArray();

        public int Year => int.Parse(GetType().Namespace.Split('.').Last()[^4..]);
        public int Day => int.Parse(GetType().Name["Day".Length..]);
        public int TestCaseFiles => Directory.GetFiles(BaseDirectory).Where(f => f.Replace('\\', '/').Split('/').Last().StartsWith($"{Day}T")).Count();

        public object[] SolveAllParts() => SolveAllParts("RunPart", null);
        public object[] TestRunAllParts(int testCase) => SolveAllParts("TestRunPart", new object[] { testCase });

        private object[] SolveAllParts(string methodPrefix, object[] parameters)
        {
            var methods = GetType().GetMethods().Where(m => m.Name.StartsWith(methodPrefix)).ToArray();
            var result = new object[methods.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = methods[i].Invoke(this, parameters);
            return result;
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

        private string GetFileContents(int testCase) => File.ReadAllText(GetFileLocation(testCase));
        private string[] GetFileLines(int testCase) => GetFileContents(testCase).GetLines();

        private string GetFileLocation(int testCase) => $"{BaseDirectory}/{Day}{(testCase > 0 ? $"T{testCase}" : "")}.txt";
    }

    public abstract class Problem<T1, T2> : Problem
    {
        #region Normal Part Running
        public T1 RunPart1()
        {
            LoadState();
            return SolvePart1();
        }
        public T2 RunPart2()
        {
            LoadState();
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
    // Just in case there are 3-part problems
    public abstract class Problem<T1, T2, T3> : Problem<T1, T2>
    {
        #region Normal Part Running
        public T3 RunPart3()
        {
            LoadState();
            return SolvePart3();
        }

        public abstract T3 SolvePart3();
        #endregion

        #region Test Part Running
        public T3 TestRunPart3(int testCase)
        {
            return TestRunPart(RunPart3, testCase);
        }
        #endregion
    }
}