using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionRealtimeProgress
    {
        bool TryGetRealtimeProgress(Vector2Int position, out float progress, out float remainingSeconds);
    }

    public interface IConstructionLifecycle
    {
        bool IsOperational(Vector2Int position);
        bool TryGetProgress(Vector2Int position, out int completedTurns, out int requiredTurns);
    }

    /// <summary>
    /// Trusted completion/restore seam for save-load and authoritative scenario
    /// setup. It only marks an already-placed building as complete; it never
    /// legalizes placement — callers must commit through the confirmed
    /// placement path first.
    /// </summary>
    public interface IConstructionOperationalRestore
    {
        bool TryRestoreOperational(Vector2Int position);
    }
}
