using AdventOfCode.Utilities.TwoDimensions;
using Garyon.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AdventOfCode.Problems.Year2018;

public class Day3 : Problem<int>
{
    private FabricGrid grid;

    public override int SolvePart1()
    {
        return grid.OverlappingCells;
    }
    public override int SolvePart2()
    {
        return grid.GetNonOverlappingClaimID();
    }

    protected override void LoadState()
    {
        grid = new(ParsedFileLines(Claim.Parse));
    }
    protected override void ResetState()
    {
        grid = null;
    }

    private class FabricGrid : SquareGrid2D<FabricGridCell>
    {
        public int OverlappingCells => ValueCounters[FabricGridCell.OverlappingCell];
        public int UnclaimedCells => ValueCounters[FabricGridCell.UnclaimedCell];
        public int ClaimedCells => TotalElements - UnclaimedCells;

        private readonly Claim[] claims;

        public FabricGrid(Claim[] claims)
            : base(1000)
        {
            ApplyClaims(this.claims = claims);
        }

        private void ApplyClaims(IEnumerable<Claim> claims)
        {
            foreach (var claim in claims)
                ApplyClaim(claim);
        }
        private void ApplyClaim(Claim claim)
        {
            for (int offsetX = 0; offsetX < claim.Width; offsetX++)
            {
                int x = claim.StartX + offsetX;

                for (int offsetY = 0; offsetY < claim.Height; offsetY++)
                {
                    int y = claim.StartY + offsetY;

                    this[x, y] = this[x, y].WithClaim(claim.ID);
                }
            }
        }

        public int GetNonOverlappingClaimID()
        {
            foreach (var claim in claims)
            {
                // continue claimLoop when?

                // First check the corners
                foreach (var corner in claim.Corners)
                    if (this[corner].Overlapping)
                        goto end;

                // Then check the rest of the claim
                // Prioritizing the sides probably doesn't help much
                for (int x = 0; x < claim.Width; x++)
                {
                    for (int y = 0; y < claim.Height; y++)
                    {
                        if (this[claim.StartX + x, claim.StartY + y].Overlapping)
                            goto end;
                    }
                }

                return claim.ID;

            end:;
            }

            return -1;
        }
    }

    private struct FabricGridCell : IEquatable<FabricGridCell>
    {
        private const int overlapped = -1;
        private const int unclaimed = 0;

        public static FabricGridCell OverlappingCell => new(overlapped);
        public static FabricGridCell UnclaimedCell => default;

        public int ClaimValue { get; set; }

        public bool Unclaimed
        {
            get => ClaimValue is unclaimed;
            set => ClaimValue = unclaimed;
        }
        public bool Overlapping
        {
            get => ClaimValue is overlapped;
            set => ClaimValue = overlapped;
        }

        public FabricGridCell(int claimValue) => ClaimValue = claimValue;

        public bool Equals(FabricGridCell other) => ClaimValue == other.ClaimValue;

        public FabricGridCell WithClaim(int claim)
        {
            if (Unclaimed)
                ClaimValue = claim;
            else
                Overlapping = true;

            return this;
        }

        public override bool Equals(object obj) => obj is FabricGridCell cell && Equals(cell);
        public override int GetHashCode() => ClaimValue;
        public override string ToString()
        {
            return Overlapping ? "X" : ClaimValue.ToString();
        }
    }

    private record Claim(int ID, int StartX, int StartY, int Width, int Height)
    {
        private static readonly Regex claimPattern = new(@"#(?'id'\d*) @ (?'startX'\d*),(?'startY'\d*): (?'width'\d*)x(?'height'\d*)", RegexOptions.Compiled);

        public int EndX => StartX + Width - 1;
        public int EndY => StartY + Height - 1;

        public Location2D TopLeft => (StartX, StartY);
        public Location2D TopRight => (EndX, StartY);
        public Location2D BottomLeft => (StartX, EndY);
        public Location2D BottomRight => (EndX, EndY);

        public Location2D[] Corners => new[]
        {
                TopLeft,
                TopRight,
                BottomLeft,
                BottomRight,
            };

        public static Claim Parse(string raw)
        {
            var groups = claimPattern.Match(raw).Groups;
            int id = groups["id"].Value.ParseInt32();
            int startX = groups["startX"].Value.ParseInt32();
            int startY = groups["startY"].Value.ParseInt32();
            int width = groups["width"].Value.ParseInt32();
            int height = groups["height"].Value.ParseInt32();
            return new(id, startX, startY, width, height);
        }
    }
}
