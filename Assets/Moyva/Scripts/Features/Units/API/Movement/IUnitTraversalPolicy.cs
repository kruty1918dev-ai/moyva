using Kruty1918.Moyva.Grid.API;
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

        /// <summary>
        /// Classifies the physical terrain transition between two adjacent cells
        /// for a movement profile: direct walk, generated stair step or blocked.
        /// </summary>
        bool TryEvaluateTransition(
            Vector2Int from,
            Vector2Int to,
            string movementProfileId,
            out TerrainTransitionEvaluation evaluation);

        void InvalidateStaticCache();
    }
}
