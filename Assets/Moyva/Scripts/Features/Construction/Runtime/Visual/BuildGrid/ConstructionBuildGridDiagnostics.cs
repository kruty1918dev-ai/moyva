using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridDiagnostics : IConstructionBuildGridDiagnostics
    {
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

            Debug.Log($"[ConstructionBuildGrid] Initialized. Shader='{shaderName}', materialReady={materialReady}, projectionReady={projectionReady}");
        }

        public void LogModeChanged(bool active)
        {
            if (VerboseLogs)
                Debug.Log($"[ConstructionBuildGrid] Construction mode active={active}. Overlay marked dirty.");
        }

        public void LogRebuildSkipped(string reason)
        {
            if (VerboseLogs)
                Debug.LogWarning($"[ConstructionBuildGrid] Rebuild skipped: {reason}");
        }

        public void LogRebuildCompleted(ConstructionBuildGridCollectionStats stats)
        {
            if (!VerboseLogs)
                return;

            Debug.Log(
                $"[ConstructionBuildGrid] Rebuilt. entries={stats.EntriesCreated}, scanned={stats.PositionsScanned}, " +
                $"tileData={stats.PositionsWithTileData}, filtered={stats.FilteredOut}, skipped={stats.SkippedEntries}, " +
                $"missingSurface={stats.MissingSurfaceData}");
        }

        public void LogEntriesPruned(int prunedCount, int remainingCount)
        {
            if (VerboseLogs && prunedCount > 0)
                Debug.Log($"[ConstructionBuildGrid] Pruned stale entries={prunedCount}, remaining={remainingCount}");
        }

        private bool VerboseLogs => _settingsProvider?.EnableVerboseLogs ?? (Application.isEditor && Debug.isDebugBuild);
    }
}
