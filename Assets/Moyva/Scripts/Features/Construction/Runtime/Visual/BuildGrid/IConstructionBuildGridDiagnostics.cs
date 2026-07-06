namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridDiagnostics
    {
        void LogInitialized(string shaderName, bool materialReady, bool projectionReady);
        void LogModeChanged(bool active);
        void LogRebuildSkipped(string reason);
        void LogRebuildCompleted(ConstructionBuildGridCollectionStats stats);
        void LogEntriesPruned(int prunedCount, int remainingCount);
    }
}
