namespace AdventOfCode.Problems.Year2023;

public class Day5 : Problem<uint>
{
    private ProblemMaps _maps;
    private ImmutableArray<uint> _seeds;

    public override uint SolvePart1()
    {
        var mapped = _maps.MapSeeds(_seeds);
        return mapped.Min();
    }
    [PartSolution(PartSolutionStatus.Uninitialized)]
    public override uint SolvePart2()
    {
        return 0;
    }

    protected override void LoadState()
    {
        var sections = NormalizedFileContents.Split("\n\n");
        var seedsSection = sections[0];
        _seeds = ParseSeeds(seedsSection);
        var mapsSections = sections.AsSpan()[1..];
        _maps = ParseMaps(mapsSections);
    }
    protected override void ResetState()
    {
        _maps = null;
        _seeds = default;
    }

#nullable enable

    private static ImmutableArray<uint> ParseSeeds(string seedsLine)
    {
        seedsLine.SplitOnceSpan(':', out _, out var seedValues);
        return Parsing.ParseAllUInt32(seedValues);
    }

    private static ProblemMaps ParseMaps(Span<string> sections)
    {
        MapContents seed_soil = null!;
        MapContents soil_fertilizer = null!;
        MapContents fertilizer_water = null!;
        MapContents water_light = null!;
        MapContents light_temperature = null!;
        MapContents temperature_humidity = null!;
        MapContents humidity_location = null!;

        foreach (var section in sections)
        {
            var lines = section.AsSpan().Trim().GetLines();
            var header = lines[0];
            var mappingLines = lines.AsSpan()[1..];
            var mapContents = ParseMapContents(mappingLines);
            SetMapContentsForHeader(header, mapContents);
        }

        return new(
            seed_soil,
            soil_fertilizer,
            fertilizer_water,
            water_light,
            light_temperature,
            temperature_humidity,
            humidity_location);

        void SetMapContentsForHeader(string header, MapContents value)
        {
            header.SplitOnceSpan(' ', out var left, out _);
            switch (left)
            {
                case "seed-to-soil":
                    seed_soil = value;
                    break;
                case "soil-to-fertilizer":
                    soil_fertilizer = value;
                    break;
                case "fertilizer-to-water":
                    fertilizer_water = value;
                    break;
                case "water-to-light":
                    water_light = value;
                    break;
                case "light-to-temperature":
                    light_temperature = value;
                    break;
                case "temperature-to-humidity":
                    temperature_humidity = value;
                    break;
                case "humidity-to-location":
                    humidity_location = value;
                    break;
            }
        }
    }

    private static MapContents ParseMapContents(ReadOnlySpan<string> lines)
    {
        var mappings = lines.SelectArray(ParseMapping);
        return new(mappings);
    }

    private static Mapping ParseMapping(string s)
    {
        var span = s.AsSpan();
        span.SplitOnce(' ', out var targetSpan, out var rest);
        rest.SplitOnce(' ', out var sourceSpan, out var lengthSpan);
        var sourceStart = sourceSpan.ParseUInt32();
        var targetStart = targetSpan.ParseUInt32();
        var length = lengthSpan.ParseUInt32();
        return new(sourceStart, targetStart, length);
    }

    private class ProblemMaps
    {
        private readonly MapContents
            _seed_soil,
            _soil_fertilizer,
            _fertilizer_water,
            _water_light,
            _light_temperature,
            _temperature_humidity,
            _humidity_location
            ;

        public ProblemMaps(
            MapContents seed_soil,
            MapContents soil_fertilizer,
            MapContents fertilizer_water,
            MapContents water_light,
            MapContents light_temperature,
            MapContents temperature_humidity,
            MapContents humidity_location)
        {
            _seed_soil = seed_soil;
            _soil_fertilizer = soil_fertilizer;
            _fertilizer_water = fertilizer_water;
            _water_light = water_light;
            _light_temperature = light_temperature;
            _temperature_humidity = temperature_humidity;
            _humidity_location = humidity_location;
        }

        public ImmutableArray<uint> MapSeeds(ImmutableArray<uint> seeds)
        {
            var result = ImmutableArray.CreateBuilder<uint>(seeds.Length);
            for (int i = 0; i < seeds.Length; i++)
            {
                result.Add(MapSeed(seeds[i]));
            }
            return result.ToImmutable();
        }
        public uint MapSeed(uint seed)
        {
            return _humidity_location.Map(
                _temperature_humidity.Map(
                _light_temperature.Map(
                _water_light.Map(
                _fertilizer_water.Map(
                _soil_fertilizer.Map(
                _seed_soil.Map(seed)))))));
        }
    }

    private class MapContents
    {
        private readonly Mapping[] _mappings;

        public MapContents(IEnumerable<Mapping> mappings)
        {
            _mappings = mappings.ToArrayOrExisting()
                .SortBy(Mapping.AscendingSourceStart.Instance);
        }

        private Mapping GetMappingForValue(uint value)
        {
            var valueMapped = new Mapping(value, 0, 0);
            int result = Array.BinarySearch(
                _mappings, valueMapped, Mapping.AscendingSourceStart.Instance);

            int index = result;
            if (result < 0)
            {
                index = ~result - 1;
                if (index < 0)
                    index = 0;
            }

            return _mappings[index];
        }

        public uint Map(uint value)
        {
            var mapping = GetMappingForValue(value);
            return mapping.Map(value);
        }
    }

    private readonly record struct Mapping(uint SourceStart, uint TargetStart, uint Length)
    {
        // Both ends are inclusive
        public uint SourceEnd => SourceStart + Length - 1;
        public uint TargetEnd => TargetStart + Length - 1;

        public uint Map(uint value)
        {
            if (value < SourceStart)
                return value;

            uint offset = value - SourceStart;
            if (offset < Length)
            {
                return TargetStart + offset;
            }

            return value;
        }

        public sealed class AscendingSourceStart : IComparer<Mapping>
        {
            public static AscendingSourceStart Instance { get; } = new();

            int IComparer<Mapping>.Compare(Mapping x, Mapping y)
            {
                return x.SourceStart.CompareTo(y.SourceStart);
            }
        }
    }
}
