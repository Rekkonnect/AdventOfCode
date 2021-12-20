using AdventOfCode.Utilities.TwoDimensions;

namespace AdventOfCode.Problems.Year2021;

public class Day20 : Problem<int>
{
    private ImageEnhancer enhancer;
    private Image image;

    public override int SolvePart1() => SolvePart(2);
    public override int SolvePart2() => SolvePart(50);

    private int SolvePart(int enhancements)
    {
        return enhancer.Enhance(new Image(image), enhancements).LitPixels;
    }

    protected override void LoadState()
    {
        var sections = NormalizedFileContents.Trim().Split("\n\n");
        enhancer = ImageEnhancer.Parse(sections[0]);
        image = Image.Parse(sections[1].GetLines(false));
    }
    protected override void ResetState()
    {
        enhancer = null;
        image = null;
    }

#nullable enable

    // Another far too common an enum
    private enum PixelState
    {
        Off,
        On,
    }
    private static PixelState ParsePixel(char pixel) => pixel switch
    {
        '#' => PixelState.On,
        _ => PixelState.Off,
    };

    private sealed class Image : PrintableGrid2D<PixelState>
    {
        public PixelState InfinitePixel { get; set; }

        public int LitPixels => InfinitePixel switch
        {
            PixelState.Off => ValueCounters[PixelState.On],
            PixelState.On => int.MaxValue,
        };

        public Image(int width, int height)
            : base(width, height) { }
        public Image(Location2D dimensions)
            : base(dimensions) { }
        public Image(Image other)
            : base(other) { }

        public override char GetPrintableCharacter(PixelState value)
        {
            return value switch
            {
                PixelState.Off => '.',
                PixelState.On => '#',
            };
        }

        // Having to write this exact parsing function with little to no variation is starting to get on my nerves
        // Presumably this is going to be the last year that I'm doing it by hand
        public static Image Parse(string[] rawLines)
        {
            int height = rawLines.Length;
            int width = rawLines[0].Length;

            var image = new Image(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = ParsePixel(rawLines[y][x]);
                }
            }

            return image;
        }
    }

    private sealed class ImageEnhancer
    {
        private readonly PixelState[] table;

        private ImageEnhancer(PixelState[] pixelTable)
        {
            table = pixelTable;
        }

        public Image Enhance(Image given, int iterations)
        {
            for (int i = 0; i < iterations; i++)
                given = Enhance(given);
            return given;
        }
        public Image Enhance(Image given)
        {
            const int offset = 1;
            var result = new Image(given.Dimensions + new Location2D(offset * 2));

            for (int x = 0; x < result.Width; x++)
            {
                for (int y = 0; y < result.Height; y++)
                {
                    int index = GetTableIndex(given, x - offset, y - offset);
                    result[x, y] = table[index];
                }
            }

            result.InfinitePixel = OutputForInfinitePixel(given.InfinitePixel);

            return result;
        }

        private PixelState OutputForInfinitePixel(PixelState infinitePixel)
        {
            return table[infinitePixel switch
            {
                PixelState.On => 0b1_1111_1111,
                PixelState.Off => 0,
            }];
        }

        private static int GetTableIndex(Image given, int centerX, int centerY)
        {
            const int squareRegionSize = 3;
            const int maxIndexBit = 1 << (squareRegionSize * squareRegionSize - 1);

            int index = 0;

            for (int y = 0; y < squareRegionSize; y++)
            {
                int currentY = centerY + y - 1;

                for (int x = 0; x < squareRegionSize; x++)
                {
                    int currentX = centerX + x - 1;
                    var currentPixel = given.InfinitePixel;

                    if (given.IsValidLocation(currentX, currentY))
                        currentPixel = given[currentX, currentY];

                    if (currentPixel is PixelState.Off)
                        continue;

                    int bitIndex = y * squareRegionSize + x;
                    index |= maxIndexBit >> bitIndex;
                }
            }
            return index;
        }

        public static ImageEnhancer Parse(string rawTable)
        {
            var table = new PixelState[rawTable.Length];
            for (int i = 0; i < table.Length; i++)
                table[i] = ParsePixel(rawTable[i]);
            return new ImageEnhancer(table);
        }
    }
}
