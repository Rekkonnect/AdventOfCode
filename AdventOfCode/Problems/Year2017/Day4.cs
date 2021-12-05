using AdventOfCSharp;
using AdventOfCSharp.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2017;

public class Day4 : Problem<int>
{
    private Passphrase[] passphrases;

    public override int SolvePart1()
    {
        return passphrases.Count(p => p.Valid);
    }
    public override int SolvePart2()
    {
        return passphrases.Count(p => p.NewPolicyValid);
    }

    protected override void LoadState()
    {
        passphrases = ParsedFileLines(Passphrase.New);
    }
    protected override void ResetState()
    {
        passphrases = null;
    }

    private class Passphrase
    {
        private readonly HashSet<string> distinctWords;

        public string RawPassphrase { get; }
        public bool Valid { get; }

        public bool NewPolicyValid
        {
            get
            {
                if (!Valid)
                    return false;

                var valueCountersSet = new HashSet<NextValueCounterDictionary<char>>();
                foreach (var word in distinctWords)
                    if (!valueCountersSet.Add(new(word)))
                        return false;

                return true;
            }
        }

        public Passphrase(string phrase)
        {
            RawPassphrase = phrase;

            var words = phrase.Split(' ');
            distinctWords = new(words);

            Valid = words.Length == distinctWords.Count;
        }

        // For the Parser<T> delegate
        public static Passphrase New(string phrase) => new(phrase);
    }
}
