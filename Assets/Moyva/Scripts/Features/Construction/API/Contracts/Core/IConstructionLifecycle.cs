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
}
