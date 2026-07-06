using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorHeightProjectionApplier : ITileWorldCreatorHeightProjectionApplier
    {
        private readonly ITileWorldCreatorTileTransformCollector _collector;
        private readonly ITileWorldCreatorHeightProjectionOffsetService _offsets;
        private readonly ITileWorldCreatorHeightProjectionDiagnostics _diagnostics;

        public TileWorldCreatorHeightProjectionApplier(
            ITileWorldCreatorTileTransformCollector collector,
            ITileWorldCreatorHeightProjectionOffsetService offsets,
            ITileWorldCreatorHeightProjectionDiagnostics diagnostics)
        {
            _collector = collector;
            _offsets = offsets;
            _diagnostics = diagnostics;
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

            ResolveLocalBounds(state, root, out float minX, out float minZ, out float maxX, out float maxZ);
            var stats = _offsets.ApplyOffsets(state, root, width, height, minX, minZ);
            bool stop = ResolveStableState(state, width, height, renderers, skippedSideWalls, stats);
            MaybeLogAppliedPass(state, root, width, height, renderers, skippedSideWalls, stats, minX, minZ, maxX, maxZ, stop);
            MaybeLogRendererMismatch(state, renderers);

            return new TileWorldCreatorHeightProjectionPassResult(root, renderers, skippedSideWalls, state.ScratchBuffer.Count, state.UsedCells.Count, width * height, stop);
        }

        private bool CanApply(TileWorldCreatorHeightProjectionState state, out int width, out int height)
        {
            width = state.TerrainLevelMap?.GetLength(0) ?? 0;
            height = state.TerrainLevelMap?.GetLength(1) ?? 0;
            if (state.TerrainLevelMap == null)
            {
                _diagnostics.LogPass($"pass={state.ApplyPassIndex} skipped: TerrainLevelMap is null.");
                return false;
            }
            if (width > 0 && height > 0)
                return true;

            _diagnostics.LogPass($"pass={state.ApplyPassIndex} skipped: TerrainLevelMap size is {width}x{height}.");
            return false;
        }

        private TileWorldCreatorHeightProjectionPassResult HandleNoTiles(
            TileWorldCreatorHeightProjectionState state,
            Transform root,
            int rendererCount,
            int skippedSideWalls)
        {
            if (ShouldLogNoTilePass(state, rendererCount, skippedSideWalls))
            {
                _diagnostics.LogPass($"pass={state.ApplyPassIndex} found no tile transforms yet. rendererCount={rendererCount}, skippedSideWallRenderers={skippedSideWalls}, root='{root.name}', childTransforms={root.GetComponentsInChildren<Transform>(true).Length - 1}, trackingLeft={state.TrackingSecondsRemaining:0.###}s.");
                RememberLoggedState(state, rendererCount, 0, 0, skippedSideWalls);
            }

            state.ClearScratch();
            return new TileWorldCreatorHeightProjectionPassResult(root, rendererCount, skippedSideWalls, 0, 0, 0, false);
        }

        private static void ResolveLocalBounds(TileWorldCreatorHeightProjectionState state, Transform root, out float minX, out float minZ, out float maxX, out float maxZ)
        {
            minX = minZ = float.PositiveInfinity;
            maxX = maxZ = float.NegativeInfinity;
            foreach (var sample in state.ScratchBuffer)
            {
                Vector3 local = root.InverseTransformPoint(sample.WorldCenter);
                minX = Mathf.Min(minX, local.x);
                minZ = Mathf.Min(minZ, local.z);
                maxX = Mathf.Max(maxX, local.x);
                maxZ = Mathf.Max(maxZ, local.z);
            }
        }

        private bool ResolveStableState(TileWorldCreatorHeightProjectionState state, int width, int height, int renderers, int skippedSideWalls, TileWorldCreatorHeightProjectionStats stats)
        {
            bool fullCoverage = state.UsedCells.Count >= width * height;
            bool stable = fullCoverage && stats.Changed == 0
                && renderers == state.LastLoggedRendererCount
                && state.ScratchBuffer.Count == state.LastLoggedTileTransformCount
                && skippedSideWalls == state.LastLoggedSkippedSideWallRendererCount;
            state.StableFullCoveragePasses = stable ? state.StableFullCoveragePasses + 1 : 0;
            return state.StableFullCoveragePasses >= 3;
        }

        private void MaybeLogAppliedPass(TileWorldCreatorHeightProjectionState state, Transform root, int width, int height, int renderers, int skippedSideWalls, TileWorldCreatorHeightProjectionStats stats, float minX, float minZ, float maxX, float maxZ, bool stop)
        {
            if (!ShouldLogAppliedPass(state, renderers, skippedSideWalls, stats.Changed, stop))
                return;

            _diagnostics.LogPass($"pass={state.ApplyPassIndex} applied. root='{root.name}', renderers={renderers}, skippedSideWallRenderers={skippedSideWalls}, tileTransforms={state.ScratchBuffer.Count}, usedCells={state.UsedCells.Count}/{width * height}, changed={stats.Changed}, unchanged={stats.Unchanged}, clamped={stats.Clamped}, levelRange={stats.MinLevel}..{stats.MaxLevel}, localBoundsX={minX:0.###}..{maxX:0.###}, localBoundsZ={minZ:0.###}..{maxZ:0.###}, cellSize={state.CellSize}, heightStep={state.HeightStep}, trackingLeft={state.TrackingSecondsRemaining:0.###}s, stableFullCoveragePasses={state.StableFullCoveragePasses}, samples={_diagnostics.FormatSamples(state.SampleApplications)}.");
            RememberLoggedState(state, renderers, state.ScratchBuffer.Count, state.UsedCells.Count, skippedSideWalls);
        }

        private void MaybeLogRendererMismatch(TileWorldCreatorHeightProjectionState state, int renderers)
        {
            if (renderers > 0 && state.ScratchBuffer.Count <= renderers / 4)
                _diagnostics.LogPass($"pass={state.ApplyPassIndex} warning: tileTransforms ({state.ScratchBuffer.Count}) are far fewer than renderers ({renderers}). If this stays low, TWC may still be exposing merged cluster meshes instead of individual tile roots.");
        }

        private static bool ShouldLogNoTilePass(TileWorldCreatorHeightProjectionState state, int renderers, int skipped)
            => state.ApplyPassIndex <= 3 || renderers != state.LastLoggedRendererCount || skipped != state.LastLoggedSkippedSideWallRendererCount || state.ApplyPassIndex % 120 == 0;

        private static bool ShouldLogAppliedPass(TileWorldCreatorHeightProjectionState state, int renderers, int skipped, int changed, bool stop)
            => stop || changed > 0 || renderers != state.LastLoggedRendererCount || state.ScratchBuffer.Count != state.LastLoggedTileTransformCount || state.UsedCells.Count != state.LastLoggedUsedCellCount || skipped != state.LastLoggedSkippedSideWallRendererCount || state.ApplyPassIndex <= 3 || state.ApplyPassIndex % 120 == 0;

        private static void RememberLoggedState(TileWorldCreatorHeightProjectionState state, int renderers, int tileTransforms, int usedCells, int skipped)
        {
            state.LastLoggedRendererCount = renderers;
            state.LastLoggedTileTransformCount = tileTransforms;
            state.LastLoggedUsedCellCount = usedCells;
            state.LastLoggedSkippedSideWallRendererCount = skipped;
        }
    }
}
