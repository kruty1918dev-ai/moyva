using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorHeightProjectionService : ITileWorldCreatorHeightProjectionService
    {
        private readonly ITileWorldCreatorHeightProjectionApplier _applier;
        private readonly ITileWorldCreatorHeightProjectionDiagnostics _diagnostics;
        private readonly ITileWorldCreatorHeightProjectionStableActionService _stableActions;

        public TileWorldCreatorHeightProjectionService(
            ITileWorldCreatorHeightProjectionApplier applier,
            ITileWorldCreatorHeightProjectionDiagnostics diagnostics,
            ITileWorldCreatorHeightProjectionStableActionService stableActions)
        {
            _applier = applier;
            _diagnostics = diagnostics;
            _stableActions = stableActions;
        }

        public void Configure(
            TileWorldCreatorHeightProjectionState state,
            MonoBehaviour owner,
            Transform targetRoot,
            int[,] terrainLevelMap,
            float cellSize,
            int heightStep,
            float trackingSeconds)
        {
            state.TargetRoot = targetRoot != null ? targetRoot : owner.transform;
            state.TerrainLevelMap = terrainLevelMap;
            state.CellSize = cellSize > 0.0001f ? cellSize : TileWorldCreatorHeightProjectionState.DefaultCellSize;
            state.HeightStep = Mathf.Max(1, heightStep);
            state.TrackingSecondsRemaining = Mathf.Max(0f, trackingSeconds);
            state.ResetRuntime();

            _diagnostics.LogConfigured(state);
            _diagnostics.LogWorldStart(state);
            ApplyOnce(state, owner);
        }

        public void Tick(TileWorldCreatorHeightProjectionState state, MonoBehaviour owner)
        {
            if (state.TerrainLevelMap == null || state.TrackingSecondsRemaining <= 0f)
                return;

            state.TrackingSecondsRemaining -= Time.unscaledDeltaTime;
            ApplyOnce(state, owner);
            if (state.TrackingSecondsRemaining <= 0f && !state.WorldGenDiagEndLogged)
            {
                state.WorldGenDiagEndLogged = true;
                _diagnostics.LogWorldEnd(state);
            }
        }

        private void ApplyOnce(TileWorldCreatorHeightProjectionState state, MonoBehaviour owner)
        {
            var result = _applier.Apply(state, owner.transform);
            if (result.ShouldStopTracking)
            {
                _stableActions.Execute(state, result);
                state.TrackingSecondsRemaining = 0f;
                _diagnostics.LogPass($"pass={state.ApplyPassIndex} tracking stopped: stable full coverage reached. root='{result.Root.name}', renderers={result.RendererCount}, tileTransforms={result.TileTransformCount}, usedCells={result.UsedCellCount}/{result.TotalCellCount}, skippedSideWallRenderers={result.SkippedSideWallRenderers}.");
            }

            state.ClearScratch();
        }
    }
}
