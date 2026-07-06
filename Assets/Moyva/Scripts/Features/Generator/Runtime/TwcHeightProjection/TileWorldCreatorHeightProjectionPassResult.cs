using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct TileWorldCreatorHeightProjectionPassResult
    {
        public TileWorldCreatorHeightProjectionPassResult(
            Transform root,
            int rendererCount,
            int skippedSideWallRenderers,
            int tileTransformCount,
            int usedCellCount,
            int totalCellCount,
            bool shouldStopTracking)
        {
            Root = root;
            RendererCount = rendererCount;
            SkippedSideWallRenderers = skippedSideWallRenderers;
            TileTransformCount = tileTransformCount;
            UsedCellCount = usedCellCount;
            TotalCellCount = totalCellCount;
            ShouldStopTracking = shouldStopTracking;
        }

        public Transform Root { get; }
        public int RendererCount { get; }
        public int SkippedSideWallRenderers { get; }
        public int TileTransformCount { get; }
        public int UsedCellCount { get; }
        public int TotalCellCount { get; }
        public bool ShouldStopTracking { get; }
    }
}
