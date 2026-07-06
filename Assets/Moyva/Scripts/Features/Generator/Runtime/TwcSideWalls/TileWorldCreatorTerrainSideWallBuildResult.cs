using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct TileWorldCreatorTerrainSideWallBuildResult
    {
        public TileWorldCreatorTerrainSideWallBuildResult(
            bool skipped,
            string skipReason,
            TileWorldCreatorTerrainSideWallBuildStats stats,
            TileWorldCreatorTerrainSideWallArtifactDiagnostics artifacts,
            IndexFormat indexFormat)
        {
            Skipped = skipped;
            SkipReason = skipReason;
            Stats = stats;
            Artifacts = artifacts;
            IndexFormat = indexFormat;
        }

        public bool Skipped { get; }
        public string SkipReason { get; }
        public TileWorldCreatorTerrainSideWallBuildStats Stats { get; }
        public TileWorldCreatorTerrainSideWallArtifactDiagnostics Artifacts { get; }
        public IndexFormat IndexFormat { get; }
    }

    internal struct TileWorldCreatorTerrainSideWallBuildStats
    {
        public int Width;
        public int Height;
        public int MinLevel;
        public int MaxLevel;
        public int EdgeLevel;
        public int EastWalls;
        public int WestWalls;
        public int NorthWalls;
        public int SouthWalls;
        public int SkippedBoundaryWalls;
        public int TotalLevelDifference;
        public int MaxLevelDifference;
        public int VertexCount;
        public int TriangleCount;
        public readonly int WallCount => EastWalls + WestWalls + NorthWalls + SouthWalls;
        public SortedDictionary<int, int> DifferenceHistogram { get; set; }
    }
}
