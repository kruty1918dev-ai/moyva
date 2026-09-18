using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorHeightProjectionService : ITileWorldCreatorHeightProjectionService
    {
        private readonly ITileWorldCreatorHeightProjectionApplier _applier;
        private readonly ITileWorldCreatorHeightProjectionStableActionService _stableActions;

        public TileWorldCreatorHeightProjectionService(
            ITileWorldCreatorHeightProjectionApplier applier,
            ITileWorldCreatorHeightProjectionStableActionService stableActions)
        {
            _applier = applier;
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

            ApplyOnce(state, owner);
        }

        public void Tick(TileWorldCreatorHeightProjectionState state, MonoBehaviour owner)
        {
            if (state.TerrainLevelMap == null || state.TrackingSecondsRemaining <= 0f)
                return;

            state.TrackingSecondsRemaining -= Time.unscaledDeltaTime;
            ApplyOnce(state, owner);
        }

        private void ApplyOnce(TileWorldCreatorHeightProjectionState state, MonoBehaviour owner)
        {
            var result = _applier.Apply(state, owner.transform);
            if (result.ShouldStopTracking)
            {
                _stableActions.Execute(state, result);
                state.TrackingSecondsRemaining = 0f;
            }

            state.ClearScratch();
        }
    }
}
