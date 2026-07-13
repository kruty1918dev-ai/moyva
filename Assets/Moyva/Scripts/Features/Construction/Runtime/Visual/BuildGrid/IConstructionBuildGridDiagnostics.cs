namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridDiagnostics
    {
        void LogStateTransition(
            Kruty1918.Moyva.Construction.API.BuildModeGridState previousState,
            Kruty1918.Moyva.Construction.API.BuildModeGridState currentState,
            string previousBuildingId,
            string currentBuildingId,
            string reason);
        void LogInitialized(string shaderName, bool materialReady, bool projectionReady);
        void LogFullRefreshRequested(Kruty1918.Moyva.Construction.API.BuildModeGridState state, string buildingId);
        void LogPartialRefreshRequested(UnityEngine.Vector2Int position, int radius);
        void LogHoverChanged(bool hasTile, UnityEngine.Vector2Int position, ConstructionBuildGridTileVisualState visualState);
        void LogRebuildSkipped(string reason);
        void LogRebuildCompleted(ConstructionBuildGridCollectionStats stats);
        void LogChunkMaskUpdated(UnityEngine.RectInt tileRect, int general, int valid, int invalid, int hidden);
        void LogEntriesPruned(int prunedCount, int remainingCount);
    }
}
