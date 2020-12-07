using AdventOfCode.Functions;
using System;
using System.IO;
using System.Linq;

namespace AdventOfCode
{
    public abstract class Problem
    {
        protected int CurrentTestCase;

        protected string BaseDirectory => $@"Inputs\{Year}";
        protected string FileContents => GetFileContents(CurrentTestCase);
        protected string NormalizedFileContents => GetFileContents(CurrentTestCase).NormalizeLineEndings();
        protected string[] FileLines => GetFileLines(CurrentTestCase);

        public int Year => int.Parse(GetType().Namespace.Split('.').Last()[^4..]);
        public int Day => int.Parse(GetType().Name["Day".Length..]);
        public int TestCaseFiles => Directory.GetFiles(BaseDirectory).Where(f => f.Replace('\\', '/').Split('/').Last().StartsWith($"{Day}T")).Count();

        public object[] RunAllParts() => RunAllParts("RunPart", null);
        public object[] TestRunAllParts(int testCase) => RunAllParts("TestRunPart", new object[] { testCase });

        private object[] RunAllParts(string methodPrefix, object[] parameters)
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

        private string GetFileContents(int testCase) => File.ReadAllText(GetFileLocation(testCase));
        private string[] GetFileLines(int testCase) => GetFileContents(testCase).GetLines();

        private string GetFileLocation(int testCase) => $"{BaseDirectory}/{Day}{(testCase > 0 ? $"T{testCase}" : "")}.txt";
    }

    public abstract class Problem<T1, T2> : Problem
    {
        #region Normal Part Running
        public abstract T1 RunPart1();
        public abstract T2 RunPart2();
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
        public abstract T3 RunPart3();
        #endregion

        #region Test Part Running
        public T3 TestRunPart3(int testCase)
        {
            return TestRunPart(RunPart3, testCase);
        }
        #endregion
    }
}
