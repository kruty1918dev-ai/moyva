using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public sealed class TileWorldCreatorHeightProjectionState
    {
        public const float DefaultCellSize = 1f;

        public Transform TargetRoot;
        public int[,] TerrainLevelMap;
        public float CellSize = DefaultCellSize;
        public int HeightStep = 1;
        public float TrackingSecondsRemaining;
        public float WorldGenDiagStartTime;
        public bool WorldGenDiagEndLogged;
        public bool SideWallsRefreshedAfterStable;
        public bool MeshOptimizationRequestedAfterStable;
        public int ApplyPassIndex;
        public int StableFullCoveragePasses;
        public int LastLoggedRendererCount = -1;
        public int LastLoggedTileTransformCount = -1;
        public int LastLoggedUsedCellCount = -1;
        public int LastLoggedSkippedSideWallRendererCount = -1;

        public readonly Dictionary<int, float> AppliedYOffsetByTransformId = new Dictionary<int, float>();
        public readonly List<TileWorldCreatorTileTransformSample> ScratchBuffer = new List<TileWorldCreatorTileTransformSample>(256);
        public readonly HashSet<int> CollectedTransformIds = new HashSet<int>();
        public readonly HashSet<Vector2Int> UsedCells = new HashSet<Vector2Int>();
        public readonly List<string> SampleApplications = new List<string>(12);

        public void ResetRuntime()
        {
            ApplyPassIndex = 0;
            StableFullCoveragePasses = 0;
            LastLoggedRendererCount = -1;
            LastLoggedTileTransformCount = -1;
            LastLoggedUsedCellCount = -1;
            LastLoggedSkippedSideWallRendererCount = -1;
            SideWallsRefreshedAfterStable = false;
            MeshOptimizationRequestedAfterStable = false;
            WorldGenDiagStartTime = Time.realtimeSinceStartup;
            WorldGenDiagEndLogged = false;
            AppliedYOffsetByTransformId.Clear();
            ClearScratch();
        }

        public void ClearScratch()
        {
            ScratchBuffer.Clear();
            CollectedTransformIds.Clear();
            UsedCells.Clear();
            SampleApplications.Clear();
        }
    }
}
