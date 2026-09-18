using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public enum UnitTraversalMode
    {
        Preview = 0,
        Pathfinding = 1,
        Execute = 2,
    }

    public interface IUnitTraversalPolicy
    {
        bool TryEvaluateStep(
            string unitId,
            Vector2Int from,
            Vector2Int to,
            float availableMovement,
            UnitTraversalMode mode,
            out float cost,
            out string reason);

        void InvalidateStaticCache();
    }
}
