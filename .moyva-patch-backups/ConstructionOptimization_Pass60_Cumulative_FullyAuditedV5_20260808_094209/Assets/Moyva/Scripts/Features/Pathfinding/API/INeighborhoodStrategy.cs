using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Pathfinding.API
{
    public interface INeighborhoodStrategy
    {
        GridNeighborhoodMode Mode { get; }
        IEnumerable<Vector2Int> GetNeighbors(Vector2Int position, IGridService gridService);
        float GetStepCost(Vector2Int from, Vector2Int to);
        float EstimateDistance(Vector2Int from, Vector2Int to);
    }
}