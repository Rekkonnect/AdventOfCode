namespace AdventOfCode.Problems.Year2016;

public class Day4 : Problem<int>
{
    private RoomData[] rooms;
    private RoomData[] realRooms;

    public override int SolvePart1()
    {
        realRooms = rooms.Where(r => r.Real).ToArray();
        return realRooms.Sum(r => r.SectorID);
    }
    public override int SolvePart2()
    {
        return realRooms.Where(r => r.DecryptedName.Contains("north")).First().SectorID;
    }

    protected override void ResetState()
    {
        rooms = null;
    }
    protected override void LoadState()
    {
        rooms = ParsedFileLines(RoomData.Parse);
    }

    private record RoomData(string Name, int SectorID, string Checksum)
    {
        private static readonly Regex dataPattern = new(@"(?'name'[\w\-]*)\-(?'sectorID'\d*)\[(?'checksum'\w*)\]", RegexOptions.Compiled);

        private bool? real;
        private string decryptedName;

        public bool Real => real ??= DetermineReal();
        public string DecryptedName => decryptedName ??= DecryptName();

        private bool DetermineReal()
        {
            var counters = new ValueCounterDictionary<char>(Name);
            counters.Remove('-');
            var occurrences = counters.Select(kvp => new CharacterOccurrences(kvp.Key, kvp.Value)).ToArray();
            if (occurrences.Length < Checksum.Length)
                return false;

            Array.Sort(occurrences, DescendingSortComparer);

            for (int i = 0; i < Checksum.Length; i++)
                if (occurrences[i].Character != Checksum[i])
                    return false;

            return true;

            static int DescendingSortComparer<T>(T a, T b)
                where T : IComparable<T>
            {
                return b.CompareTo(a);
            }
        }
        private string DecryptName()
        {
            char[] chars = Name.Replace('-', ' ').ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == ' ')
                    continue;

                chars[i] = ShiftLetterRight(chars[i], SectorID);
            }

            return new(chars);
        }

        private static char ShiftLetterRight(char letter, int shift)
        {
            int offset = letter - 'a';
            return (char)((offset + shift) % 26 + 'a');
        }

        public static RoomData Parse(string raw)
        {
            var groups = dataPattern.Match(raw).Groups;
            var name = groups["name"].Value;
            int sectorID = groups["sectorID"].Value.ParseInt32();
            var checksum = groups["checksum"].Value;
            return new(name, sectorID, checksum);
        }
    }

    private struct CharacterOccurrences : IComparable<CharacterOccurrences>
    {
        public char Character { get; }
        public int Occurrences { get; }

        public CharacterOccurrences(char character, int occurrences) => (Character, Occurrences) = (character, occurrences);

        public int CompareTo(CharacterOccurrences other)
        {
            int result = Occurrences.CompareTo(other.Occurrences);
            if (result != 0)
                return result;

            return other.Character.CompareTo(Character);
        }

        public override string ToString() => $"{Character} - {Occurrences}";
    }
}
