using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionLifecycle
    {
        bool IsOperational(Vector2Int position);
        bool TryGetProgress(Vector2Int position, out int completedTurns, out int requiredTurns);
    }
}
