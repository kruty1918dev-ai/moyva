using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridDiagnostics : IConstructionBuildGridDiagnostics
    {
        private const string Tag = "[MoyvaBuildGridDiag]";
        private readonly IConstructionDiagnosticsSettingsProvider _settingsProvider;

        [Inject]
        public ConstructionBuildGridDiagnostics(
            [InjectOptional] IConstructionDiagnosticsSettingsProvider settingsProvider = null)
        {
            _settingsProvider = settingsProvider;
        }

        public void LogInitialized(string shaderName, bool materialReady, bool projectionReady)
        {
            if (!VerboseLogs)
                return;

            Debug.Log($"{Tag} initialized shader='{shaderName}' materialReady={materialReady} projectionReady={projectionReady}");
        }

        public void LogStateTransition(
            BuildModeGridState previousState,
            BuildModeGridState currentState,
            string previousBuildingId,
            string currentBuildingId,
            string reason)
        {
            if (VerboseLogs)
            {
                Debug.Log(
                    $"{Tag} state {previousState}->{currentState} " +
                    $"building='{previousBuildingId ?? "none"}'->'{currentBuildingId ?? "none"}' reason='{reason}'");
            }
        }

        public void LogFullRefreshRequested(BuildModeGridState state, string buildingId)
        {
            if (VerboseLogs)
                Debug.Log($"{Tag} full-refresh-requested state={state} building='{buildingId ?? "none"}'");
        }

        public void LogPartialRefreshRequested(Vector2Int position, int radius)
        {
            if (VerboseLogs)
                Debug.Log($"{Tag} partial-refresh-requested center={position} radius={radius}");
        }

        public void LogHoverChanged(bool hasTile, Vector2Int position, ConstructionBuildGridTileVisualState visualState)
        {
            if (VerboseLogs)
                Debug.Log($"{Tag} hover hasTile={hasTile} tile={position} visualState={visualState}");
        }

        public void LogRebuildSkipped(string reason)
        {
            if (VerboseLogs)
                Debug.LogWarning($"{Tag} rebuild-skipped reason='{reason}'");
        }

        public void LogRebuildCompleted(ConstructionBuildGridCollectionStats stats)
        {
            if (!VerboseLogs)
                return;

            Debug.Log(
                $"{Tag} full-refresh-completed entries={stats.EntriesCreated} scanned={stats.PositionsScanned} " +
                $"tileData={stats.PositionsWithTileData}, filtered={stats.FilteredOut}, skipped={stats.SkippedEntries}, " +
                $"missingSurface={stats.MissingSurfaceData}");
        }

        public void LogChunkMaskUpdated(RectInt tileRect, int general, int valid, int invalid, int hidden)
        {
            if (!VerboseLogs)
                return;

            Debug.Log(
                $"{Tag} chunk-refresh-completed area={tileRect} updated={general + valid + invalid} " +
                $"general={general} valid={valid} invalid={invalid} hidden={hidden}");
        }

        public void LogEntriesPruned(int prunedCount, int remainingCount)
        {
            if (VerboseLogs && prunedCount > 0)
                Debug.Log($"{Tag} pruned stale={prunedCount} remaining={remainingCount}");
        }

        private bool VerboseLogs => _settingsProvider?.EnableVerboseLogs ?? (Application.isEditor && Debug.isDebugBuild);
    }
}
