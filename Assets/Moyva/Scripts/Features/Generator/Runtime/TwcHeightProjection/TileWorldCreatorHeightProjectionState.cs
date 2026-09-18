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
        public bool SideWallsRefreshedAfterStable;
        public bool MeshOptimizationRequestedAfterStable;
        public int ApplyPassIndex;
        public int StableFullCoveragePasses;
        public int PreviousRendererCount = -1;
        public int PreviousTileTransformCount = -1;
        public int PreviousSkippedSideWallRendererCount = -1;

        public readonly Dictionary<int, float> AppliedYOffsetByTransformId = new Dictionary<int, float>();
        internal readonly List<TileWorldCreatorTileTransformSample> ScratchBuffer = new List<TileWorldCreatorTileTransformSample>(256);
        public readonly HashSet<int> CollectedTransformIds = new HashSet<int>();
        public readonly HashSet<Vector2Int> UsedCells = new HashSet<Vector2Int>();

        public void ResetRuntime()
        {
            ApplyPassIndex = 0;
            StableFullCoveragePasses = 0;
            PreviousRendererCount = -1;
            PreviousTileTransformCount = -1;
            PreviousSkippedSideWallRendererCount = -1;
            SideWallsRefreshedAfterStable = false;
            MeshOptimizationRequestedAfterStable = false;
            AppliedYOffsetByTransformId.Clear();
            ClearScratch();
        }

        public void ClearScratch()
        {
            ScratchBuffer.Clear();
            CollectedTransformIds.Clear();
            UsedCells.Clear();
        }
    }
}
