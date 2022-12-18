using AdventOfCode.Utilities.TwoDimensions;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace AdventOfCode.Problems.Year2022;

public class Day17 : Problem<long>
{
    private MovementInstructionList instructionList;

    public override long SolvePart1()
    {
        return PlayGame(2022);
    }
    public override long SolvePart2()
    {
        const long trillion = 1_000_000_000_000;
        return PlayGame(trillion);
    }

    private long PlayGame(long rounds)
    {
        var game = new TetrisGame(instructionList);
        game.IterateRocks(rounds);
        return game.TowerHeight;
    }

    protected override void LoadState()
    {
        instructionList = new(FileContents);
    }

    private class TetrisGame
    {
        private const int rockStartingRowOffset = 3;

        private readonly MovementInstructionIterator instructionIterator;
        private readonly RockPieceIterator rockIterator;
        private readonly Tower tower = new();

        public int TowerHeight => tower.Height;

        public TetrisGame(MovementInstructionList list)
        {
            instructionIterator = new(list);
            rockIterator = RockPieceIterator.Common();
        }

        public void IterateRocks(long count)
        {
            for (long i = 0; i < count; i++)
            {
                IterateRock();
            }
        }

        public void IterateRock()
        {
            var currentRock = rockIterator.GetCurrentMoveNext();
            var fallingRock = FallingRock(currentRock);

            int remainingFreeMoves = rockStartingRowOffset;
            while (true)
            {
                var instruction = instructionIterator.GetCurrentMoveNext();
                fallingRock.Rock.MoveToDirection(instruction);

                if (tower.OverlapsWith(fallingRock))
                {
                    var inverse = instruction.Inverse();
                    fallingRock.Rock.MoveToDirection(instruction);
                }

                if (fallingRock.BottomRowIndex <= 0)
                {
                    tower.ArestRock(fallingRock);
                    break;
                }

                fallingRock.Fall();

                if (remainingFreeMoves <= 0)
                {
                    bool overlaps = tower.OverlapsWith(fallingRock);
                    if (overlaps)
                    {
                        fallingRock.Lift();
                        tower.ArestRock(fallingRock);
                        break;
                    }
                }

                remainingFreeMoves--;
            }
        }

        private FallingRock FallingRock(Rock rock)
        {
            int bottomRowIndex = TowerHeight + rockStartingRowOffset;
            return new(rock, bottomRowIndex);
        }
    }

    private class Tower
    {
        private readonly List<Row> rows = new();

        public int Height => rows.Count;

        public bool OverlapsWith(FallingRock fallingRock)
        {
            var rock = fallingRock.Rock;
            int rockBottomRowIndex = fallingRock.BottomRowIndex;

            int rockRowIndex = 0;
            int towerRowIndex = rockBottomRowIndex;
            while (true)
            {
                if (rockRowIndex >= rock.Height)
                    break;

                if (towerRowIndex >= Height)
                    break;

                var rockRow = rock.RowAt(rockRowIndex);
                bool overlaps = rows[towerRowIndex].OverlapsWith(rockRow);
                if (overlaps)
                    return true;

                rockRowIndex++;
                towerRowIndex++;
            }

            return false;
        }

        // Arest, to give rest to
        // Not to be confused with: `arrest`
        public void ArestRock(FallingRock fallingRock)
        {
            var rock = fallingRock.Rock;
            int rockBottomRowIndex = fallingRock.BottomRowIndex;

            // Should I point out how fucking dangerously I play?
            var rowSpan = CollectionsMarshal.AsSpan(rows);

            int rockRowIndex = 0;
            int towerRowIndex = rockBottomRowIndex;
            while (true)
            {
                if (rockRowIndex >= rock.Height)
                    break;

                if (towerRowIndex >= Height)
                    break;

                var rockRow = rock.RowAt(rockRowIndex);
                ref var towerRow = ref rowSpan[towerRowIndex];

                towerRow.MergeWith(rockRow);

                rockRowIndex++;
                towerRowIndex++;
            }

            while (rockRowIndex < rock.Height)
            {
                var rockRow = rock.RowAt(rockRowIndex);
                rows.Add(rockRow);

                rockRowIndex++;
            }
        }
    }

    private record struct FallingRock(Rock Rock, int BottomRowIndex)
    {
        public void Fall()
        {
            BottomRowIndex--;
        }
        public void Lift()
        {
            BottomRowIndex++;
        }
    }

    // So much copy-pasted code, roles could come in handy
    private class RockPieceIterator
    {
        private readonly ImmutableArray<Rock> rocks;

        private int round;

        public RockPieceIterator(ImmutableArray<Rock> rocks)
        {
            this.rocks = rocks;
        }

        public void MoveNext()
        {
            round++;
        }

        public Rock GetCurrent()
        {
            return rocks[CurrentIndex()].Clone();
        }

        public Rock GetCurrentMoveNext()
        {
            var current = GetCurrent();
            MoveNext();
            return current;
        }

        private int CurrentIndex()
        {
            return round % rocks.Length;
        }

        public static RockPieceIterator Common()
        {
            return new(Rock.AllRocksOrdered);
        }
    }

    private class MovementInstructionIterator
    {
        private readonly MovementInstructionList list;

        private int round;

        public MovementInstructionIterator(MovementInstructionList instructionList)
        {
            list = instructionList;
        }

        public void MoveNext()
        {
            round++;
        }

        public Direction GetCurrent()
        {
            int index = CurrentIndex();
            return list.DirectionForIndex(index);
        }

        public Direction GetCurrentMoveNext()
        {
            var current = GetCurrent();
            MoveNext();
            return current;
        }

        private int CurrentIndex()
        {
            return round % list.Count;
        }
    }

    private class MovementInstructionList
    {
        private readonly string directions;

        public int Count => directions.Length;

        public MovementInstructionList(string directionString)
        {
            directions = directionString.Trim();
        }

        public Direction DirectionForIndex(int index)
        {
            return ParseDirection(directions[index]);
        }
        private static Direction ParseDirection(char direction)
        {
            return direction switch
            {
                '<' => Direction.Left,
                '>' => Direction.Right,
            };
        }
    }

    private class Rock
    {
        #region Common Rocks
        private static readonly Rock pad = FromString("####");
        private static readonly Rock plus = FromString(".#.\n###\n.#.");
        private static readonly Rock chair = FromString("..#\n..#\n###");
        private static readonly Rock pole = FromString("#\n#\n#\n#");
        private static readonly Rock box = FromString("##\n##");

        public static Rock Pad => pad.Clone();
        public static Rock Plus => plus.Clone();
        public static Rock Chair => chair.Clone();
        public static Rock Pole => pole.Clone();
        public static Rock Box => box.Clone();

        public static ImmutableArray<Rock> AllRocksOrdered => new[]
        {
            Pad,
            Plus,
            Chair,
            Pole,
            Box,
        }
        .ToImmutableArray();
        #endregion

        private readonly Row[] rows;

        public int Height => rows.Length;

        private Rock(Row[] rows)
        {
            this.rows = rows;
        }

        public ref readonly Row RowAt(int index)
        {
            return ref rows[index];
        }

        public void MoveToDirection(Direction direction)
        {
            for (int i = 0; i < Height; i++)
            {
                var row = rows[i];
                row.MoveToDirection(direction);
                rows[i] = row;
            }
        }

        public Rock Clone()
        {
            return new(rows.CopyArray());
        }

        public static Rock FromString(string str)
        {
            var rowLines = str.ReplaceLineEndings().GetLines(false);
            var rowBuilder = ImmutableArray.CreateBuilder<Row>(rowLines.Length);

            foreach (var line in rowLines.Reverse())
            {
                var row = ParseRow(line);
                rowBuilder.Add(row);
            }

            return new(rowBuilder.ToArray());
        }
        private static Row ParseRow(string line)
        {
            var result = new Row();

            for (int column = 0; column < line.Length; column++)
            {
                bool isSet = line[column] is '#';
                result.SetAt(column + 2, isSet);
            }

            return result;
        }
    }

    private struct Row
    {
        // Indexing:   0100110
        //             0123456
        private const int Width = 7;
        private const int StartIndexMask = 0b100_0000;
        private const int RowMask = 0b111_1111;

        private byte bits;

        public bool IndexAt(int index)
        {
            return (bits & MaskForIndex(index)) is not 0;
        }

        public void SetAt(int index, int value)
        {
            // Just to set one fucking bit the algorithm has to be typed out
            // I've always wondered how it could be improved without compromising
            // on performance
            int keptMask = ~MaskForIndex(index) & RowMask;
            int keptBits = bits & keptMask;
            int setMask = value << index;
            int newBits = keptBits | setMask;
            bits = (byte)newBits;
        }
        public void SetAt(int index, bool value)
        {
            int intValue = value ? 1 : 0;
            SetAt(index, intValue);
        }

        public bool IsUnsetAt(int index)
        {
            return IndexAt(index) is false;
        }
        public bool IsSetAt(int index)
        {
            return IndexAt(index) is true;
        }

        public bool CanMoveToDirection(Direction direction)
        {
            return direction switch
            {
                Direction.Left => IsUnsetAt(0),
                Direction.Right => IsUnsetAt(Width - 1),
                _ => true
            };
        }

        public void MoveToDirection(Direction direction)
        {
            int movedBits = MoveToDirection(bits, direction);
            bits = (byte)movedBits;
        }

        public bool OverlapsWith(Row other)
        {
            return (bits & other.bits) is 0;
        }

        public void MergeWith(Row other)
        {
            bits |= other.bits;
        }

        private static int MaskForIndex(int index)
        {
            return StartIndexMask >> index;
        }
        private static int MoveToDirection(int bits, Direction direction)
        {
            return direction switch
            {
                Direction.Left => bits << 1,
                Direction.Right => bits >> 1,
                _ => bits,
            };
        }
    }
}
