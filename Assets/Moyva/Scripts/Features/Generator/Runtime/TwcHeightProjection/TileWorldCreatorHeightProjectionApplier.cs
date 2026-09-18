using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorHeightProjectionApplier : ITileWorldCreatorHeightProjectionApplier
    {
        private readonly ITileWorldCreatorTileTransformCollector _collector;
        private readonly ITileWorldCreatorHeightProjectionOffsetService _offsets;

        public TileWorldCreatorHeightProjectionApplier(
            ITileWorldCreatorTileTransformCollector collector,
            ITileWorldCreatorHeightProjectionOffsetService offsets)
        {
            _collector = collector;
            _offsets = offsets;
        }

        public TileWorldCreatorHeightProjectionPassResult Apply(TileWorldCreatorHeightProjectionState state, Transform fallbackRoot)
        {
            state.ApplyPassIndex++;
            if (!CanApply(state, out int width, out int height))
                return default;

            var root = state.TargetRoot != null ? state.TargetRoot : fallbackRoot;
            int renderers = _collector.Collect(root, state.ScratchBuffer, state.CollectedTransformIds, out int skippedSideWalls);
            if (state.ScratchBuffer.Count == 0)
                return HandleNoTiles(state, root, renderers, skippedSideWalls);

            ResolveLocalOrigin(state, root, out float minX, out float minZ);
            var stats = _offsets.ApplyOffsets(state, root, width, height, minX, minZ);
            bool stop = ResolveStableState(state, width, height, renderers, skippedSideWalls, stats);
            RememberObservedState(state, renderers, skippedSideWalls);

            return new TileWorldCreatorHeightProjectionPassResult(root, renderers, skippedSideWalls, state.ScratchBuffer.Count, state.UsedCells.Count, width * height, stop);
        }

        private bool CanApply(TileWorldCreatorHeightProjectionState state, out int width, out int height)
        {
            width = state.TerrainLevelMap?.GetLength(0) ?? 0;
            height = state.TerrainLevelMap?.GetLength(1) ?? 0;
            return state.TerrainLevelMap != null && width > 0 && height > 0;
        }

        private TileWorldCreatorHeightProjectionPassResult HandleNoTiles(
            TileWorldCreatorHeightProjectionState state,
            Transform root,
            int rendererCount,
            int skippedSideWalls)
        {
            return new TileWorldCreatorHeightProjectionPassResult(root, rendererCount, skippedSideWalls, 0, 0, 0, false);
        }

        private static void ResolveLocalOrigin(
            TileWorldCreatorHeightProjectionState state,
            Transform root,
            out float minX,
            out float minZ)
        {
            minX = minZ = float.PositiveInfinity;
            foreach (var sample in state.ScratchBuffer)
            {
                Vector3 local = root.InverseTransformPoint(sample.WorldCenter);
                minX = Mathf.Min(minX, local.x);
                minZ = Mathf.Min(minZ, local.z);
            }
        }

        private bool ResolveStableState(TileWorldCreatorHeightProjectionState state, int width, int height, int renderers, int skippedSideWalls, TileWorldCreatorHeightProjectionStats stats)
        {
            bool fullCoverage = state.UsedCells.Count >= width * height;
            bool stable = fullCoverage && stats.Changed == 0
                && renderers == state.PreviousRendererCount
                && state.ScratchBuffer.Count == state.PreviousTileTransformCount
                && skippedSideWalls == state.PreviousSkippedSideWallRendererCount;
            state.StableFullCoveragePasses = stable ? state.StableFullCoveragePasses + 1 : 0;
            return state.StableFullCoveragePasses >= 3;
        }

        private static void RememberObservedState(
            TileWorldCreatorHeightProjectionState state,
            int renderers,
            int skippedSideWalls)
        {
            state.PreviousRendererCount = renderers;
            state.PreviousTileTransformCount = state.ScratchBuffer.Count;
            state.PreviousSkippedSideWallRendererCount = skippedSideWalls;
        }
    }
}
