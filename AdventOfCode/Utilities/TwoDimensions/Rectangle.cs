namespace AdventOfCode.Utilities.TwoDimensions
{
    public struct Rectangle
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }

        public Location2D TopLeft
        {
            get => (Left, Top);
            set => (Left, Top) = value;
        }
        public Location2D TopRight
        {
            get => (Right, Top);
            set => (Right, Top) = value;
        }
        public Location2D BottomLeft
        {
            get => (Left, Bottom);
            set => (Left, Bottom) = value;
        }
        public Location2D BottomRight
        {
            get => (Right, Bottom);
            set => (Right, Bottom) = value;
        }

        public Rectangle(int left, int right, int bottom, int top)
        {
            Left = left;
            Right = right;
            Bottom = bottom;
            Top = top;
        }
        public Rectangle(Location2D topLeft, Location2D topRight, Location2D bottomLeft, Location2D bottomRight)
            : this()
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
        }
    }
}
