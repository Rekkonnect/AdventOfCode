using AdventOfCode.Functions;
using Garyon.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2016
{
    public class Day9 : Problem<long>
    {
        private CompressedFile compressedFile;

        public override long SolvePart1()
        {
            return compressedFile.SinglyDecompressedLength;
        }
        public override long SolvePart2()
        {
            return compressedFile.FullyDecompressedLength;
        }

        protected override void ResetState()
        {
            compressedFile = null;
        }
        protected override void LoadState()
        {
            compressedFile = CompressedFile.Parse(FileContents);
        }

        private record CompressedFile(CompressedFileSegmentRoot SegmentRoot)
        {
            private static readonly Regex markerPattern = new(@"\((?'length'\d*)x(?'repetitions'\d*)\)", RegexOptions.Compiled);

            public long SinglyDecompressedLength => SegmentRoot.SinglyDecompressedLength;
            public long FullyDecompressedLength => SegmentRoot.FullyDecompressedLength;

            public static CompressedFile Parse(string raw)
            {
                var segmentRoot = new CompressedFileSegmentRoot();

                var matches = markerPattern.Matches(raw);
                long currentUnmarkedContentIndex = 0;
                int markerEndIndex = 0;

                foreach (Match match in matches)
                {
                    int length = match.Groups["length"].Value.ParseInt32();
                    int repetitions = match.Groups["repetitions"].Value.ParseInt32();

                    long intermediateContentLength = match.Index - markerEndIndex;
                    currentUnmarkedContentIndex += intermediateContentLength;

                    var marker = new Marker(currentUnmarkedContentIndex, length, repetitions);
                    segmentRoot.AddSubmarker(marker);

                    markerEndIndex = match.Index + match.Length;
                }

                currentUnmarkedContentIndex += raw.Length - markerEndIndex;
                segmentRoot.Length = currentUnmarkedContentIndex;
                return new(segmentRoot);
            }
        }

        private record CompressedFileSegmentRoot() : CompressedFileSegment((Marker)null)
        {
            public override long Index => 0;
            public override long Length { get; set; }
        }

#nullable enable
        private record CompressedFileSegment(Marker? CompressionMarker)
        {
            private long markerStringLength = 0;

            protected readonly List<CompressedFileSegment> ChildSegments = new();

            public virtual long OccupiedChildrenContentSpace { get; private set; }
            public long VacantChildrenContentSpace => Length - OccupiedChildrenContentSpace;

            public long EndIndex => Index + Length;

            public long RawSerializedLength => (CompressionMarker?.MarkerStringLength ?? 0) + RawSerializedBodyLength;
            public long RawSerializedBodyLength
            {
                get
                {
                    long result = VacantChildrenContentSpace;

                    foreach (var child in ChildSegments)
                        result += child.RawSerializedLength;

                    return result;
                }
            }

            public long RawSinglyDecompressedLength => RawSerializedBodyLength * CompressionMarker!.Repetitions;

            public long SinglyDecompressedLength
            {
                get
                {
                    long result = VacantChildrenContentSpace;

                    foreach (var child in ChildSegments)
                        result += child.RawSinglyDecompressedLength;

                    return result;
                }
            }
            public long FullyDecompressedLength
            {
                get
                {
                    long result = VacantChildrenContentSpace;

                    foreach (var child in ChildSegments)
                        result += child.FullyDecompressedLength;

                    return result * (CompressionMarker?.Repetitions ?? 1);
                }
            }

            public virtual long Index => CompressionMarker!.Index;
            public virtual long Length
            {
                get => CompressionMarker!.Length - markerStringLength;
                set { }
            }

            public void AddSubmarker(Marker marker)
            {
                markerStringLength += marker.MarkerStringLength;

                var lastSegment = ChildSegments.LastOrDefault();
                if (lastSegment is null || marker.Index >= lastSegment.EndIndex)
                {
                    // The segment will be appended in this segment's child segment list
                    var newSegment = new CompressedFileSegment(marker);
                    OccupiedChildrenContentSpace += marker.Length;
                    ChildSegments.Add(newSegment);
                }
                else
                {
                    OccupiedChildrenContentSpace -= marker.MarkerStringLength;
                    lastSegment.AddSubmarker(marker);
                }
            }
        }
#nullable disable

        private record Marker(long Index, int Length, int Repetitions)
        {
            private int? cachedStringLength;

            public long MarkedContentIndex => Index + MarkerStringLength;

            public int MarkerStringLength => cachedStringLength ??= GetMarkerStringLength();
            public int DecompressionLength => Repetitions * Length;
            public int RepetitionLengthDifference => DecompressionLength - Length;
            public int DecompressionLengthDifference => RepetitionLengthDifference - MarkerStringLength;

            private int GetMarkerStringLength()
            {
                return Length.GetDigitCount() + Repetitions.GetDigitCount() + "(x)".Length;
            }

            public override string ToString()
            {
                return $"({Length}x{Repetitions})";
            }
        }

        #region Feel free to fuck your RAM up
        private class MarkerSegmentCollection : IReadOnlyList<MarkerSegment>
        {
            private readonly List<MarkerSegment> segments;
            private long nextAvailableIndex;

            public long EndIndex { get; set; }

            public int Count => segments.Count;
            public int AllMarkerCount => segments.Sum(s => s.AllMarkerCount);

            public IEnumerable<Marker> TopLevelMarkers => segments.Select(s => s.TopLevelMarker);

            public MarkerSegment this[int index] => segments[index];

            public MarkerSegmentCollection()
            {
                segments = new();
            }
            public MarkerSegmentCollection(int capacity)
            {
                segments = new(capacity);
            }

            public void Add(Marker marker)
            {
                var currentSegment = segments.LastOrDefault();
                bool topLevel = marker.Index >= nextAvailableIndex;

                if (topLevel)
                {
                    currentSegment = new(marker, new List<Marker>());
                    segments.Add(currentSegment);

                    nextAvailableIndex = marker.MarkedContentIndex + marker.Length;
                }
                else
                {
                    currentSegment.Submarkers.Add(marker);
                }
            }

            public MarkerSegmentCollection DecompressMarkers()
            {
                var result = new MarkerSegmentCollection(segments.Count);

                long currentLength = segments.First().Index;
                for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
                {
                    var segment = segments[segmentIndex];
                    var topLevel = segment.TopLevelMarker;

                    long topLevelEndIndex = topLevel.MarkedContentIndex;
                    long newMarkerOffset = currentLength;

                    for (int i = 0; i < topLevel.Repetitions; i++)
                    {
                        long repetitionOffset = topLevel.Length * i;
                        foreach (var submarker in segment.Submarkers)
                        {
                            long submarkerOffset = submarker.Index - topLevelEndIndex;
                            var newMarker = submarker with
                            {
                                Index = newMarkerOffset + repetitionOffset + submarkerOffset,
                            };
                            result.Add(newMarker);
                        }
                    }

                    long nextIndex = EndIndex;
                    if (segmentIndex + 1 < segments.Count)
                        nextIndex = segments[segmentIndex + 1].Index;

                    currentLength += segment.DecompressionDistanceFrom(nextIndex);
                }

                result.EndIndex = currentLength;
                return result;
            }

            public IEnumerator<MarkerSegment> GetEnumerator() => segments.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => segments.GetEnumerator();
        }
        private record MarkerSegment(Marker TopLevelMarker, IList<Marker> Submarkers)
        {
            public long Index => TopLevelMarker.Index;
            public long MarkedContentIndex => TopLevelMarker.MarkedContentIndex;
            public int Length => TopLevelMarker.Length;
            public int DecompressionLength => TopLevelMarker.DecompressionLength;
            public int Repetitions => TopLevelMarker.Repetitions;

            public int AllMarkerCount => Submarkers.Count + 1;

            public long EmptySpaceBetween(long nextIndex)
            {
                long indexDifference = nextIndex - MarkedContentIndex;
                return indexDifference - Length;
            }
            public long DecompressionDistanceFrom(long nextIndex)
            {
                return DecompressionLength + EmptySpaceBetween(nextIndex);
            }
        }
        #endregion
    }
}
