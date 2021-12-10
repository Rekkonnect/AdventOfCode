#nullable enable

using AdventOfCode.Functions;
using AdventOfCSharp;
using Garyon.Extensions;
using Garyon.Extensions.ArrayExtensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2021;

public class Day10 : Problem<ulong>
{
    private NavigationSystemDocument? document;

    public override ulong SolvePart1()
    {
        return document!.GetTotalInvalidClosingErrorScore();
    }
    public override ulong SolvePart2()
    {
        return document!.GetMiddleMissingClosingErrorScore();
    }

    protected override void LoadState()
    {
        document = new NavigationSystemDocument(FileLines);
    }
    protected override void ResetState()
    {
        document = null;
    }

    private static bool IsOpeningCharacter(char c) => c is '(' or '[' or '{' or '<';
    private static bool IsClosingCharacter(char c) => c is ')' or ']' or '}' or '>';
    private static char GetClosingCharacter(char opening) => opening switch
    {
        '(' => ')',
        '[' => ']',
        '{' => '}',
        '<' => '>',
    };

    private record struct ChunkToken(char Opening)
    {
        public char ExpectedClosing => GetClosingCharacter(Opening);

        public bool IsExpectedClosing(char c) => ExpectedClosing == c;
    }

    private class LineParser
    {
        private readonly Stack<ChunkToken> tokens;
        private readonly string line;
        private int currentColumn;

        private readonly List<SyntaxError> invalidFinalizerErrors = new();
        private readonly List<SyntaxError> missingFinalizerErrors = new();

        public ErroneousLineKind ErroneousLineKind { get; private set; } = ErroneousLineKind.Unknown;

        public IEnumerable<SyntaxError> Errors => invalidFinalizerErrors.Concat(missingFinalizerErrors);

        public ulong InvalidFinalizerErrorScore => invalidFinalizerErrors.Sum(error => error.ErrorScore);

        public ulong MissingFinalizerErrorScore
        {
            get
            {
                ulong sum = 0;
                foreach (var error in missingFinalizerErrors)
                {
                    sum *= 5;
                    sum += error.ErrorScore;
                }
                return sum;
            }
        }

        public LineParser(string parsedLine)
        {
            tokens = new(parsedLine.Length);
            line = parsedLine;
        }

        public void ParseUntilEnd()
        {
            while (currentColumn < line.Length)
                ParseCharacter();
            FinalizeParse();
        }
        public SyntaxError ParseUntilNextError()
        {
            while (currentColumn < line.Length)
            {
                if (!ParseCharacter())
                    return invalidFinalizerErrors.Last();
            }
            FinalizeParse();
            return invalidFinalizerErrors.LastOrDefault();
        }

        private void FinalizeParse()
        {
            foreach (var poppedToken in tokens.PopAll())
            {
                Error(SyntaxError.CreateMissingFinalizer(poppedToken.Opening));
            }
            SetErroneousLineKindIfUnknown(ErroneousLineKind.Valid);
        }
        private bool ParseCharacter()
        {
            bool hasError = false;

            var currentChar = line[currentColumn];
            if (IsOpeningCharacter(currentChar))
            {
                tokens.Push(new(currentChar));
            }
            else
            {
                var chunkToken = tokens.Pop();
                if (!chunkToken.IsExpectedClosing(currentChar))
                {
                    hasError = true;
                    Error(SyntaxError.CreateInvalidFinalizer(chunkToken.Opening, currentChar));
                }
            }

            currentColumn++;
            return !hasError;
        }

        private void Error(SyntaxError error)
        {
            GetErrorList(error.Kind).Add(error);
            RegisterErroneousLineKind(error);
        }
        private List<SyntaxError> GetErrorList(ErrorKind kind) => kind switch
        {
            ErrorKind.InvalidChunkFinalizer => invalidFinalizerErrors,
            ErrorKind.MissingChunkFinalizer => missingFinalizerErrors,
        };

        private void RegisterErroneousLineKind(SyntaxError error)
        {
            SetErroneousLineKindIfUnknown(FromErrorKind(error.Kind));
        }
        private void SetErroneousLineKindIfUnknown(ErroneousLineKind newKind)
        {
            if (ErroneousLineKind is ErroneousLineKind.Unknown)
                ErroneousLineKind = newKind;
        }

        private static ErroneousLineKind FromErrorKind(ErrorKind kind) => kind switch
        {
            ErrorKind.InvalidChunkFinalizer => ErroneousLineKind.Corrupted,
            ErrorKind.MissingChunkFinalizer => ErroneousLineKind.Incomplete,
        };

        public record struct SyntaxError(ErrorKind Kind, char Opening, char Closing)
        {
            public ulong ErrorScore => Kind switch
            {
                ErrorKind.None => 0,

                ErrorKind.InvalidChunkFinalizer => GetIllegalCharacterScore(Closing),
                ErrorKind.MissingChunkFinalizer => GetCompletionCharacterScore(Closing),
            };

            private static ulong GetIllegalCharacterScore(char closing) => closing switch
            {
                ')' => 3,
                ']' => 57,
                '}' => 1197,
                '>' => 25137,
            };
            private static ulong GetCompletionCharacterScore(char closing) => closing switch
            {
                ')' => 1,
                ']' => 2,
                '}' => 3,
                '>' => 4,
            };

            public static SyntaxError CreateInvalidFinalizer(char opening, char closing)
            {
                return new(ErrorKind.InvalidChunkFinalizer, opening, closing);
            }
            public static SyntaxError CreateMissingFinalizer(char opening)
            {
                return new(ErrorKind.MissingChunkFinalizer, opening, GetClosingCharacter(opening));
            }
        }
 
        public enum ErrorKind
        {
            None,

            InvalidChunkFinalizer,
            MissingChunkFinalizer,
        }
    }

    private enum ErroneousLineKind
    {
        Unknown = -1,
        // For the sake of completeness
        Valid = 0,

        Corrupted,
        Incomplete,
    }

    private class NavigationSystemDocumentLine
    {
        private LineParser? parser;

        public readonly string Line;

        public ErroneousLineKind LineKind => parser?.ErroneousLineKind ?? ErroneousLineKind.Unknown;

        public NavigationSystemDocumentLine(string line)
        {
            Line = line;
        }

        private void EnsureInitializedParser()
        {
            if (parser == null)
                parser = new(Line);
        }

        public ulong GetInvalidClosingErrorScore()
        {
            EnsureInitializedParser();
            parser!.ParseUntilNextError();
            return ScoreIfTargetErroneousLineKind(parser.InvalidFinalizerErrorScore, ErroneousLineKind.Corrupted);
        }
        public ulong GetMissingFinalizerErrorScore()
        {
            EnsureInitializedParser();
            parser!.ParseUntilNextError();
            return ScoreIfTargetErroneousLineKind(parser.MissingFinalizerErrorScore, ErroneousLineKind.Incomplete);
        }

        private ulong ScoreIfTargetErroneousLineKind(ulong score, ErroneousLineKind targetLineKind)
        {
            return LineKind == targetLineKind ? score : 0;
        }
    }

    private class NavigationSystemDocument
    {
        private readonly NavigationSystemDocumentLine[] lines;

        public NavigationSystemDocument(string[] sourceLines)
        {
            lines = sourceLines.Select(line => new NavigationSystemDocumentLine(line)).ToArray();
        }

        public ulong GetTotalInvalidClosingErrorScore()
        {
            return lines.Select(line => line.GetInvalidClosingErrorScore()).Sum();
        }
        public ulong GetMiddleMissingClosingErrorScore()
        {
            var missingScores = lines.Select(line => line.GetMissingFinalizerErrorScore()).Where(score => score > 0).ToArray().Sort();
            return missingScores[missingScores.Length / 2];
        }
    }
}
