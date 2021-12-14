using AdventOfCode.Functions;
using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2019;

public class Day8 : Problem<int, IGlyphGrid>
{
    private ImageLayer[] layers;

    public override int SolvePart1()
    {
        var layer = layers.MinBy(layer => layer.GetValueCounter(0));
        return layer.GetValueCounter(1) * layer.GetValueCounter(2);
    }
    public override IGlyphGrid SolvePart2()
    {
        var result = new ImageLayer(layers[0]);
        for (int i = 0; i < layers.Length; i++)
            result = result.RenderAbove(layers[i]);
        return result;
    }

    protected override void LoadState()
    {
        var chars = FileContents.ToCharArray();

        layers = new ImageLayer[chars.Length / ImageLayer.PixelCount];
        for (int i = 0; i < layers.Length; i++)
            layers[i] = new ImageLayer(chars, i * ImageLayer.PixelCount);
    }

    private delegate T ImageProcessor<T>(ImageLayer[] layers);

    private class ImageLayer : PrintableGlyphGrid2D<char>
    {
        public new const int Width = 25;
        public new const int Height = 6;
        public const int PixelCount = Width * Height;

        private ImageLayer()
            : base(Width, Height) { }

        private ImageLayer(char[] chars)
            : this(chars, 0) { }
        public unsafe ImageLayer(in char[] chars, int offset)
            : this()
        {
            for (int i = 0; i < PixelCount; i++)
            {
                var (row, column) = Math.DivRem(i, Width);
                this[column, row] = chars[offset + i];
            }
        }
        public ImageLayer(ImageLayer other)
            : base(other) { }
        private unsafe ImageLayer(char[,] values)
            : this()
        {
            fixed (char* privateValues = Values)
            fixed (char* givenValues = values)
            {
                for (int i = 0; i < PixelCount; i++)
                    privateValues[i] = givenValues[i];
            }
        }

        public int GetValueCounter(int pixel) => ValueCounters[pixel.ToHexChar()];

        public unsafe ImageLayer RenderAbove(ImageLayer other)
        {
            var rendered = new char[Width, Height];
            fixed (char* pixels = Values)
            fixed (char* otherPixels = other.Values)
            fixed (char* renderedPixels = rendered)
            {
                for (int i = 0; i < PixelCount; i++)
                    renderedPixels[i] = (pixels[i] == '2' ? otherPixels : pixels)[i];
            }
            return new ImageLayer(rendered);
        }

        protected override bool IsDrawnPixel(char value) => value is '1';
    }
}
