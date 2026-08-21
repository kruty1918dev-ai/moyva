using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotPerceptionService
    {
        void Refresh(
            string ownerId,
            Vector2Int startPosition,
            IReadOnlyList<BotUnitSnapshot> ownUnits,
            IReadOnlyList<BotBuildingSnapshot> ownBuildings);

        bool IsVisible(string ownerId, Vector2Int cell);
        bool IsExplored(string ownerId, Vector2Int cell);

        IReadOnlyCollection<Vector2Int> GetVisibleCells(string ownerId);
        IReadOnlyCollection<Vector2Int> GetExploredCells(string ownerId);
    }
}
