//#define PRINT

using AdventOfCode.Utilities;
using AdventOfCode.Utilities.TwoDimensions;
using AdventOfCSharp;
using Garyon.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2018;

public class Day13 : Problem<Location2D>
{
    private Track baseTrack;

    public override Location2D SolvePart1()
    {
        var track = new Track(baseTrack);
        return track.IterateUntilCrash().Location;
    }
    public override Location2D SolvePart2()
    {
        var track = new Track(baseTrack);
        return track.IterateUntilLastStanding()?.Location ?? default;
    }

    protected override void LoadState()
    {
        baseTrack = Track.Parse(FileContents);
    }
    protected override void ResetState()
    {
        baseTrack = null;
    }

    private record CartCrash(Cart A, Cart B) : IEnumerable<Cart>
    {
        public Location2D Location => A.Location;

        public void ApplyCrash()
        {
            A.Crashed = true;
            B.Crashed = true;
        }

        public bool Contains(Cart cart) => A == cart || B == cart;

        public IEnumerator<Cart> GetEnumerator() => new PairEnumerator<Cart>(A, B);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class Track : PrintableGrid2D<TrackCell>
    {
        private readonly List<Cart> carts;

        public Track(int width, int height)
            : base(width, height)
        {
            carts = new();
        }
        public Track(Track track)
            : base(track)
        {
            carts = new(track.carts.Select(cart => cart.Clone()));
        }

        public CartCrash IterateUntilCrash()
        {
#if PRINT
                PrintGrid();
#endif
            while (true)
            {
                var result = Iterate();
#if PRINT
                    PrintGrid();
#endif

                if (result.Any())
                    return result.First();
            }
        }
        public Cart IterateUntilLastStanding()
        {
            while (carts.Count > 1)
                Iterate();

            return carts.SingleOrDefault();
        }

        public IEnumerable<CartCrash> Iterate()
        {
            var crashes = new List<CartCrash>();

            foreach (var cart in carts)
            {
                if (cart.Crashed)
                    continue;

                this[cart].Cart = null;

                cart.MoveAlong(this[cart].Connection);

                var colliding = this[cart].Cart;

                if (colliding is not null)
                {
                    var crash = new CartCrash(cart, colliding);
                    crash.ApplyCrash();

                    crashes.Add(crash);
                    this[colliding].Cart = null;
                }
                else
                {
                    this[cart].Cart = cart;
                }
            }

            foreach (var crash in crashes)
                foreach (var crashed in crash)
                    carts.Remove(crashed);

            SortCarts();

            return crashes;
        }

        private void SortCarts()
        {
            carts.Sort(Cart.CompareLocations);
        }

        public static Track Parse(string raw)
        {
            var lines = raw.GetLines();
            int height = lines.Length;
            int width = lines[0].Length;
            var track = new Track(width, height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    var character = lines[y][x];
                    var cell = track.Values[x, y] = TrackCell.Parse(character, x, y);

                    if (cell.Cart is not null)
                        track.carts.Add(cell.Cart);
                }

            return track;
        }

        protected override IDictionary<TrackCell, char> GetPrintableCharacters()
        {
            return new TrackCellCharDictionary();
        }

        private ref TrackCell this[Cart c]
        {
            get => ref Values[c.Location.X, c.Location.Y];
        }

        // The hack that tops it all off
        // Could be better off using a IKeyValueMatcher<TKey, TValue>
        // Normal dictionaries can be wrapped from a wrapper type implementing that interface
        private class TrackCellCharDictionary : IDictionary<TrackCell, char>
        {
            public char this[TrackCell key]
            {
                get
                {
                    if (key.Cart is not null)
                    {
                        return key.Cart.FacingDirection.Direction switch
                        {
                            Direction.Up => '^',
                            Direction.Down => 'v',
                            Direction.Left => '<',
                            Direction.Right => '>',
                        };
                    }

                    return key.Connection switch
                    {
                        TrackCellConnection.Intersection => '+',
                        TrackCellConnection.UpLeft => '/',
                        TrackCellConnection.UpRight => '\\',
                        TrackCellConnection.StraightHorizontal => '-',
                        TrackCellConnection.StraightVertical => '|',
                        _ => ' ',
                    };
                }
                set => throw new System.NotImplementedException();
            }

            #region Hack bloat
            ICollection<TrackCell> IDictionary<TrackCell, char>.Keys => throw new System.NotImplementedException();
            ICollection<char> IDictionary<TrackCell, char>.Values => throw new System.NotImplementedException();

            int ICollection<KeyValuePair<TrackCell, char>>.Count => throw new System.NotImplementedException();

            bool ICollection<KeyValuePair<TrackCell, char>>.IsReadOnly => true;

            void IDictionary<TrackCell, char>.Add(TrackCell key, char value)
            {
                throw new System.NotImplementedException();
            }

            void ICollection<KeyValuePair<TrackCell, char>>.Add(KeyValuePair<TrackCell, char> item)
            {
                throw new System.NotImplementedException();
            }

            void ICollection<KeyValuePair<TrackCell, char>>.Clear()
            {
                throw new System.NotImplementedException();
            }

            bool ICollection<KeyValuePair<TrackCell, char>>.Contains(KeyValuePair<TrackCell, char> item)
            {
                throw new System.NotImplementedException();
            }

            bool IDictionary<TrackCell, char>.ContainsKey(TrackCell key)
            {
                throw new System.NotImplementedException();
            }

            void ICollection<KeyValuePair<TrackCell, char>>.CopyTo(KeyValuePair<TrackCell, char>[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }

            IEnumerator<KeyValuePair<TrackCell, char>> IEnumerable<KeyValuePair<TrackCell, char>>.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            bool IDictionary<TrackCell, char>.Remove(TrackCell key)
            {
                throw new System.NotImplementedException();
            }

            bool ICollection<KeyValuePair<TrackCell, char>>.Remove(KeyValuePair<TrackCell, char> item)
            {
                throw new System.NotImplementedException();
            }

            bool IDictionary<TrackCell, char>.TryGetValue(TrackCell key, out char value)
            {
                throw new System.NotImplementedException();
            }
            #endregion
        }
    }

    private class Cart
    {
        private int intersectionIndex;

        private Location2D location;
        private DirectionalLocation facingDirection;

        public bool Crashed { get; set; }

        public Location2D Location => location;
        public DirectionalLocation FacingDirection => facingDirection;

        public Location2D FacingLocation => location + facingDirection.LocationOffset;

        public Cart(Location2D startingLocation, Direction startingFacingDirection)
        {
            location = startingLocation;
            facingDirection = new(startingFacingDirection, invertY: true);
        }

        public void MoveAlong(TrackCellConnection connection)
        {
            switch ((connection, facingDirection.Direction))
            {
                case (TrackCellConnection.Intersection, _):
                    CrossIntersection();
                    break;

                case (TrackCellConnection.UpLeft, Direction.Up):
                case (TrackCellConnection.DownRight, Direction.Down):
                case (TrackCellConnection.DownLeft, Direction.Left):
                case (TrackCellConnection.UpRight, Direction.Right):
                    TurnRight();
                    goto default;

                case (TrackCellConnection.UpLeft, Direction.Left):
                case (TrackCellConnection.DownRight, Direction.Right):
                case (TrackCellConnection.DownLeft, Direction.Down):
                case (TrackCellConnection.UpRight, Direction.Up):
                    TurnLeft();
                    goto default;

                // No need to check if the straight connection and the direction match
                // The cart would've then gone off-track, implying a logical error
                default:
                    Forward();
                    break;
            }
        }

        public Cart Clone()
        {
            return new(location, facingDirection.Direction);
        }

        public void Forward()
        {
            location = FacingLocation;
        }

        public void TurnLeft()
        {
            facingDirection.TurnLeft();
        }
        public void TurnRight()
        {
            facingDirection.TurnRight();
        }

        public void CrossIntersection()
        {
            switch (intersectionIndex % 3)
            {
                case 0:
                    facingDirection.TurnLeft();
                    break;
                case 2:
                    facingDirection.TurnRight();
                    break;
            }

            Forward();
            intersectionIndex++;
        }

        public static int CompareLocations(Cart a, Cart b)
        {
            int result = a.location.Y.CompareTo(b.location.Y);
            if (result is 0)
                result = a.location.X.CompareTo(b.location.X);

            return result;
        }
    }

    private struct TrackCell
    {
        public TrackCellConnection Connection { get; }

        public Cart Cart { get; set; }

        public TrackCell(TrackCellConnection connection)
        {
            Connection = connection;
            Cart = null;
        }

        public static TrackCell Parse(char c, int x, int y)
        {
            var connection = ParseConnection(c, out var hasCart);
            var cell = new TrackCell(connection);

            if (hasCart)
                cell.Cart = new((x, y), ParseDirection(c));

            return cell;
        }

        private static Direction ParseDirection(char c)
        {
            return c switch
            {
                '<' => Direction.Left,
                '^' => Direction.Up,
                '>' => Direction.Right,
                'v' => Direction.Down,
            };
        }

        private static TrackCellConnection ParseConnection(char c, out bool hasCart)
        {
            hasCart = c is '<' or '^' or '>' or 'v';

            // The input does not contain carts on top of non-straight connections
            // Upping the ante candidate
            if (hasCart)
                return TrackCellConnection.Straight;

            return c switch
            {
                '-' => TrackCellConnection.StraightHorizontal,
                '|' => TrackCellConnection.StraightVertical,
                '/' => TrackCellConnection.UpLeft,
                '\\' => TrackCellConnection.UpRight,
                '+' => TrackCellConnection.Intersection,
                _ => TrackCellConnection.None,
            };
        }
    }

    private enum TrackCellConnection
    {
        None = 0,

        Straight = StraightHorizontal,
        StraightHorizontal = 1,
        StraightVertical = 2,

        DownLeft = 3,
        UpRight = DownLeft,

        UpLeft = 4,
        DownRight = UpLeft,

        Intersection = 5,
    }
}
