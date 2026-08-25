using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridDiagnostics {
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
            }
        }

        public void LogFullRefreshRequested(BuildModeGridState state, string buildingId)
        {
        }

        public void LogPartialRefreshRequested(Vector2Int position, int radius)
        {
        }

        public void LogHoverChanged(bool hasTile, Vector2Int position, ConstructionBuildGridTileVisualState visualState)
        {
        }

        public void LogRebuildSkipped(string reason)
        {
        }

        public void LogRebuildCompleted(ConstructionBuildGridCollectionStats stats)
        {
            if (!VerboseLogs)
                return;
        }

        public void LogChunkMaskUpdated(RectInt tileRect, int general, int valid, int invalid, int hidden)
        {
            if (!VerboseLogs)
                return;
        }

        public void LogEntriesPruned(int prunedCount, int remainingCount)
        {
        }

        private bool VerboseLogs => _settingsProvider?.EnableVerboseLogs ?? false;
    }
}
