using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;
using Garyon.DataStructures;
using Garyon.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day20 : Problem<ulong, int>
    {
        private const string seaMonsterString =
@"                  # 
#    ##    ##    ###
 #  #  #  #  #  #   ";

        private static readonly Grid2D<PixelColor> seaMonster;
        private static readonly List<Location2D> seaMonsterLocationOffsets = new();

        static Day20()
        {
            var seaMonsterLines = seaMonsterString.GetLines();

            int height = seaMonsterLines.Length;
            int width = seaMonsterLines[0].Length;

            seaMonster = new(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    seaMonster[x, y] = ParseColor(seaMonsterLines[y][x]);
                    if (seaMonster[x, y] == PixelColor.White)
                        seaMonsterLocationOffsets.Add((x, y));
                }
        }

        private TileCollection tiles;

        public override ulong SolvePart1()
        {
            var corners = tiles.GetCorners();
            ulong product = 1;
            foreach (var c in corners)
                product *= (ulong)c.TileID;
            return product;
        }
        public override int SolvePart2()
        {
            tiles.ConnectTiles();
            var fullImage = tiles.ConstructFullImage();
            fullImage = FindImageOrientation(fullImage);
            return fullImage.ValueCounters[PixelColor.White];
        }

        protected override void LoadState()
        {
            tiles = new(NormalizedFileContents.Split("\n\n").Select(Tile.Parse));
        }
        protected override void ResetState()
        {
            tiles = null;
        }

        private Grid2D<PixelColor> FindImageOrientation(Grid2D<PixelColor> fullImage)
        {
            // This pattern plays out too well
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        if (FindSeaMonsters(fullImage))
                            return fullImage;

                        fullImage = fullImage.RotateClockwise(1);
                    }
                    fullImage = fullImage.FlipHorizontally();
                }
                fullImage = fullImage.FlipVertically();
            }

            return fullImage;
        }

        private bool FindSeaMonsters(Grid2D<PixelColor> fullImage)
        {
            for (int y = 0; y < fullImage.Height - seaMonster.Height; y++)
            {
                for (int x = 0; x < fullImage.Width - seaMonster.Width; x++)
                {
                    bool isSeaMonster = true;

                    // Detect if sea monster exists in that location
                    foreach (var l in seaMonsterLocationOffsets)
                        if (fullImage[(x, y) + l] != PixelColor.White)
                        {
                            isSeaMonster = false;
                            break;
                        }

                    if (!isSeaMonster)
                        continue;

                    // Find sea monster
                    foreach (var l in seaMonsterLocationOffsets)
                        fullImage[(x, y) + l] = PixelColor.SeaMonster;
                }
            }

            return fullImage.ValueCounters[PixelColor.SeaMonster] > 0;
        }

        private class TileCollection
        {
            private KeyedObjectDictionary<int, Tile> tileDictionary;

            private TileSideHashDictionary sideHashes = new();
            private ValueCounterDictionary<Tile> edgyTiles = new();

            private TileMatchingState[,] tileMatchingStates;

            private Tile[,] connectedTiles;

            public int Count => tileDictionary.Count;

            public TileCollection(IEnumerable<Tile> tiles)
            {
                tileDictionary = new(tiles);

                tileMatchingStates = new TileMatchingState[Count, Count];
                for (int i = 1; i < Count; i++)
                    for (int j = 0; j < i; j++)
                        tileMatchingStates[i, j] = new();

                GetMatchingTiles();
            }

            private void GetMatchingTiles()
            {
                foreach (var t in tileDictionary.Values)
                    sideHashes.AddFromTile(t);
            }

            public Grid2D<PixelColor> ConstructFullImage()
            {
                int finalTileSize = connectedTiles[0, 0].Image.Size - 2;

                int wholeTileWidth = connectedTiles.GetLength(0);
                int wholeTileHeight = connectedTiles.GetLength(1);

                int totalImageWidth = wholeTileWidth * finalTileSize;
                int totalImageHeight = wholeTileHeight * finalTileSize;

                var completeImage = new Grid2D<PixelColor>(totalImageWidth, totalImageHeight);

                for (int i = 0; i < wholeTileWidth; i++)
                    for (int j = 0; j < wholeTileHeight; j++)
                    {
                        for (int x = 0; x < finalTileSize; x++)
                            for (int y = 0; y < finalTileSize; y++)
                            {
                                // Pray that this is correct
                                completeImage[i * finalTileSize + x, j * finalTileSize + y] = connectedTiles[i, j].Image[x, y];
                            }
                    }

                return completeImage;
            }

            public IEnumerable<Tile> GetCorners()
            {
                return GetEdgyTileDictionary().Where(counter => counter.Value == 4).Select(kvp => kvp.Key);
            }

            public Tile[,] ConnectTiles()
            {
                if (connectedTiles != null)
                    return connectedTiles;

                int size = (int)Math.Sqrt(Count);
                connectedTiles = new Tile[size, size];

                var remainingTiles = new TileSideHashDictionary(sideHashes);

                var corners = GetCorners().ToList();
                var edges = edgyTiles.Keys.Where(t => !corners.Contains(t)).ToList();

                // Probably most of this copy-pasted code can be abstracted somewhere better
                // but like I said, it's getting too late

                // Consider top-left corner
                var topLeftCorner = corners.First();
                connectedTiles[0, 0] = topLeftCorner;
                corners.RemoveAt(0);
                //remainingTiles.RemoveFromTile(topLeftCorner);

                OrientTileEdgeSides(topLeftCorner, remainingTiles, TileSides.TopLeft);

                // Fill entire top row with missing edges
                var previousPiece = topLeftCorner;
                for (int x = 1; x < size - 1; x++)
                {
                    var pieces = remainingTiles[previousPiece.RightSideHash];
                    pieces = pieces.Where(p => edges.Contains(p.Tile)).ToList();
                    if (pieces.Count < 1)
                        throw new InvalidOperationException("Something broke");

                    var piece = pieces.First().Tile;
                    connectedTiles[x, 0] = piece;
                    previousPiece = piece;

                    edges.Remove(piece);
                    remainingTiles.RemoveFromTile(piece);

                    OrientTileEdgeSides(piece, remainingTiles, TileSides.TopLeft);
                }

                // Find top-right corner
                var topRightCorner = remainingTiles[previousPiece.RightSideHash].Where(p => corners.Contains(p.Tile)).First().Tile;
                connectedTiles[size - 1, 0] = topRightCorner;
                corners.Remove(topRightCorner);
                remainingTiles.RemoveFromTile(topRightCorner);

                OrientTileEdgeSides(topRightCorner, remainingTiles, TileSides.TopRight);

                // Fill entire left row with missing edges
                previousPiece = topLeftCorner;
                for (int y = 1; y < size - 1; y++)
                {
                    var pieces = remainingTiles[previousPiece.BottomSideHash];
                    pieces = pieces.Where(p => edges.Contains(p.Tile)).ToList();
                    if (pieces.Count < 1)
                        throw new InvalidOperationException("Something broke");

                    var piece = pieces.First().Tile;
                    connectedTiles[0, y] = piece;
                    previousPiece = piece;

                    edges.Remove(piece);
                    remainingTiles.RemoveFromTile(piece);

                    OrientTileEdgeSides(piece, remainingTiles, TileSides.TopLeft);
                }

                // Find bottom-left corner
                var bottomLeftCorner = remainingTiles[previousPiece.BottomSideHash].Where(p => corners.Contains(p.Tile)).First().Tile;
                connectedTiles[0, size - 1] = bottomLeftCorner;
                corners.Remove(bottomLeftCorner);
                remainingTiles.RemoveFromTile(bottomLeftCorner);

                // Find bottom-right corner
                var bottomRightCorner = corners.First();
                corners.Clear();

                // Fill remaining edges

                // Fill the outermost layer repeatedly, until no pieces are left

                return connectedTiles;
            }

            private void OrientTileEdgeSides(Tile tile, TileSideHashDictionary dictionary, TileSides newOrientation)
            {
                var originalOrientation = GetEdgeSideOrientation(tile, dictionary);
                if (originalOrientation == newOrientation)
                    return;

                int turns = 0;

                switch (originalOrientation)
                {
                    case TileSides.Top:
                    case TileSides.Right:
                    case TileSides.Bottom:
                    case TileSides.Left:
                        switch (newOrientation)
                        {
                            case TileSides.Top:
                            case TileSides.Right:
                            case TileSides.Bottom:
                            case TileSides.Left:
                                break;

                            default:
                                throw new ArgumentException("Invalid new orientation");
                        }

                        while (originalOrientation != newOrientation)
                        {
                            newOrientation = (TileSides)((int)newOrientation << 1);
                            turns++;
                        }
                        break;

                    default:
                        turns = (originalOrientation, newOrientation) switch
                        {
                            // Please for my sanity change that code to something else someday
                            // TL, BL, BR, TR, TL, BL, BR, ...
                            (TileSides.TopLeft, TileSides.BottomLeft) => 1,
                            (TileSides.TopLeft, TileSides.BottomRight) => 2,
                            (TileSides.TopLeft, TileSides.TopRight) => 3,

                            (TileSides.BottomLeft, TileSides.BottomRight) => 1,
                            (TileSides.BottomLeft, TileSides.TopRight) => 2,
                            (TileSides.BottomLeft, TileSides.TopLeft) => 3,

                            (TileSides.BottomRight, TileSides.TopRight) => 1,
                            (TileSides.BottomRight, TileSides.TopLeft) => 2,
                            (TileSides.BottomRight, TileSides.BottomLeft) => 3,

                            (TileSides.TopRight, TileSides.TopLeft) => 1,
                            (TileSides.TopRight, TileSides.BottomLeft) => 2,
                            (TileSides.TopRight, TileSides.BottomRight) => 3,

                            _ => 0, // How the fuck
                        };
                        break;
                }

                tile.RotateCounterClockwise(turns);
            }
            private TileSides GetEdgeSideOrientation(Tile tile, TileSideHashDictionary dictionary)
            {
                var orientation = TileSides.None;

                // I hate that this is copypasted, but this solution is taking far too long
                // Might consider refactoring someday in the future:tm:
                if (dictionary[tile.TopSideHash.HashValue].Count < 2)
                    orientation |= TileSides.Top;
                if (dictionary[tile.RightSideHash.HashValue].Count < 2)
                    orientation |= TileSides.Right;
                if (dictionary[tile.BottomSideHash.HashValue].Count < 2)
                    orientation |= TileSides.Bottom;
                if (dictionary[tile.LeftSideHash.HashValue].Count < 2)
                    orientation |= TileSides.Left;

                return orientation;
            }

            private ValueCounterDictionary<Tile> GetEdgyTileDictionary()
            {
                foreach (var sideHash in sideHashes)
                {
                    var tiles = sideHash.Value;

                    if (tiles.Count > 1)
                        continue;

                    edgyTiles.Add(tiles.First().Tile);
                }

                return edgyTiles;
            }

            public Tile this[int tileID] => tileDictionary[tileID];
        }

        private enum TileSides
        {
            None = 0,
            Top = 1,
            Right = 1 << 1,
            Bottom = 1 << 2,
            Left = 1 << 3,

            TopRight = Top | Right,
            TopBottom = Top | Bottom,
            TopLeft = Top | Left,
            BottomRight = Bottom | Right,
            BottomLeft = Bottom | Left,
            RightLeft = Right | Left,

            All = Top | Right | Bottom | Left,
        }

        private class TileSideHashDictionary : FlexibleListDictionary<int, TileSideHash>
        {
            public TileSideHashDictionary()
                : base() { }
            public TileSideHashDictionary(TileSideHashDictionary other)
                : base(other) { }

            public void Add(TileSideHash hash)
            {
                this[hash.HashValue].Add(hash);
                this[hash.InvertedHashValue].Add(hash);
            }
            public void AddFromTile(Tile tile)
            {
                Add(tile.TopSideHash);
                Add(tile.BottomSideHash);
                Add(tile.LeftSideHash);
                Add(tile.RightSideHash);
            }

            public void Remove(TileSideHash hash)
            {
                this[hash.HashValue].Remove(hash);
                this[hash.InvertedHashValue].Remove(hash);
            }
            public void RemoveFromTile(Tile tile)
            {
                Remove(tile.TopSideHash);
                Remove(tile.BottomSideHash);
                Remove(tile.LeftSideHash);
                Remove(tile.RightSideHash);
            }

            public List<TileSideHash> this[TileSideHash hash] => this[hash.HashValue];
        }

        private class TileMatchingState
        {
            private BitArray bits = new(16);

            public bool this[Direction faceRotation, Direction direction]
            {
                get => bits[GetIndex(faceRotation, direction)];
                set => bits[GetIndex(faceRotation, direction)] = value;
            }

            private static int GetIndex(Direction faceRotation, Direction direction) => (int)faceRotation * 4 + (int)direction;
        }

        private class TileMatch : IKeyedObject<int>
        {
            public Tile OriginalTile { get; }
            public Tile MatchingTile { get; }
            public Direction OriginalTileDirection { get; }
            public int MatchingSideHash { get; }

            public int Key => OriginalTile.TileID;

            public TileMatch(Tile originalTile, Tile matchingTile, Direction direction, int sideHash)
            {
                OriginalTile = originalTile;
                MatchingTile = matchingTile;
                OriginalTileDirection = direction;
                MatchingSideHash = sideHash;
            }
        }

        #region Side hashes
        // I don't know how this mess happened
        private abstract class TileSideHash
        {
            public Tile Tile { get; init; }
            public int HashValue { get; init; }

            public int InvertedHashValue
            {
                get
                {
                    int hash = HashValue;
                    int inverted = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        // Some weird black magic
                        int bit = hash & (1 << i);
                        inverted |= (bit > 0) ? (1 << (9 - i)) : 0;
                    }
                    return inverted;
                }
            }

            public abstract Direction SideDirection { get; }

            public TileSideHash(Tile tile)
            {
                HashValue = GetSideHash(GetSideValues((Tile = tile).Image));
            }

            protected abstract PixelColor[] GetSideValues(Grid2D<PixelColor> grid);
            protected abstract bool InvertSideHashBitIndex();

            private Index GetSideHashBitIndex(int index)
            {
                bool inverted = InvertSideHashBitIndex();
                return new(index + Convert.ToInt32(inverted), inverted);
            }

            private int GetSideHash(PixelColor[] values)
            {
                int hash = 0;
                for (int i = 0; i < values.Length; i++)
                    hash |= (int)values[GetSideHashBitIndex(i)] << i;
                return hash;
            }

            public override string ToString() => HashValue.GetBinaryRepresentation(10);
        }
        private class TileTopSideHash : TileSideHash
        {
            public override Direction SideDirection => Direction.Up;

            public TileTopSideHash(Tile tile)
                : base(tile) { }

            protected override PixelColor[] GetSideValues(Grid2D<PixelColor> grid) => grid.GetXLine(0);
            protected override bool InvertSideHashBitIndex() => true;
        }
        private class TileBottomSideHash : TileSideHash
        {
            public override Direction SideDirection => Direction.Down;

            public TileBottomSideHash(Tile tile)
                : base(tile) { }

            protected override PixelColor[] GetSideValues(Grid2D<PixelColor> grid) => grid.GetXLine(^1);
            protected override bool InvertSideHashBitIndex() => false;
        }
        private class TileLeftSideHash : TileSideHash
        {
            public override Direction SideDirection => Direction.Left;

            public TileLeftSideHash(Tile tile)
                : base(tile) { }

            protected override PixelColor[] GetSideValues(Grid2D<PixelColor> grid) => grid.GetYLine(0);
            protected override bool InvertSideHashBitIndex() => false;
        }
        private class TileRightSideHash : TileSideHash
        {
            public override Direction SideDirection => Direction.Right;

            public TileRightSideHash(Tile tile)
                : base(tile) { }

            protected override PixelColor[] GetSideValues(Grid2D<PixelColor> grid) => grid.GetYLine(^1);
            protected override bool InvertSideHashBitIndex() => true;
        }
        #endregion

        private class Tile : IKeyedObject<int>
        {
            private SquareGrid2D<PixelColor> image;

            public int TileID { get; }
            public SquareGrid2D<PixelColor> Image
            {
                get => image;
                private set
                {
                    image = value;
                    CalculateSideHashes();

                    // DEBUG
                    //PrintState();
                }
            }

            public TileTopSideHash TopSideHash { get; private set; }
            public TileBottomSideHash BottomSideHash { get; private set; }
            public TileLeftSideHash LeftSideHash { get; private set; }
            public TileRightSideHash RightSideHash { get; private set; }

            public int Key => TileID;

            public Tile(int tileID, SquareGrid2D<PixelColor> image)
            {
                TileID = tileID;
                Image = image;
            }

            private void CalculateSideHashes()
            {
                TopSideHash = new(this);
                BottomSideHash = new(this);
                LeftSideHash = new(this);
                RightSideHash = new(this);
            }

            public void RotateClockwise(int turns)
            {
                Image = Image.RotateClockwise(turns);
            }
            public void RotateCounterClockwise(int turns)
            {
                Image = Image.RotateCounterClockwise(turns);
            }

            #region Printing
            private void PrintGrid()
            {
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Console.Write(GetColorCharacter(image[x, y]));
                    }
                    Console.WriteLine();
                }
            }
            private void PrintState()
            {
                PrintGrid();
                Console.WriteLine($"Top side hash: {TopSideHash}");
                Console.WriteLine($"Bottom side hash: {BottomSideHash}");
                Console.WriteLine($"Left side hash: {LeftSideHash}");
                Console.WriteLine($"Right side hash: {RightSideHash}");
            }
            #endregion

            public override bool Equals(object obj) => obj is Tile tile && TileID == tile.TileID;
            public override int GetHashCode() => TileID;
            public override string ToString() => TileID.ToString();

            public static Tile Parse(string rawTile)
            {
                var lines = rawTile.GetLines(false);
                int tileID = lines[0][("Tile ".Length)..^1].ParseInt32();

                int size = lines[1].Length;

                var grid = new SquareGrid2D<PixelColor>(size);
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        grid[x, y] = ParseColor(lines[y + 1][x]);

                return new(tileID, grid);
            }
        }

        private static char GetColorCharacter(PixelColor c) => c switch
        {
            PixelColor.White => '#',
            PixelColor.SeaMonster => 'O',
            _ => '.',
        };
        private static PixelColor ParseColor(char c) => c switch
        {
            '#' => PixelColor.White,
            _ => PixelColor.Black,
        };

        private enum PixelColor : byte
        {
            Black,
            White,
            SeaMonster
        }
    }
}
