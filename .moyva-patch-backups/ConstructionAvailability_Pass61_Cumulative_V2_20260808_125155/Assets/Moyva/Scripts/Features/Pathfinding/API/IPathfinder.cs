using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Pathfinding.API
{
    public interface IPathfinder
    {
        // Повертає список координат від старту до фінішу
        List<Vector2Int> FindPath(Vector2Int start, Vector2Int end);
        
        // Повертає список сусідів для певної координати
        IEnumerable<Vector2Int> GetNeighbors(Vector2Int position);
    }

    /// <summary>
    /// Optional extension for pathfinders that can selectively traverse cells
    /// occupied by passable construction objects. Existing IPathfinder fakes
    /// and alternative implementations remain source-compatible.
    /// </summary>
    public interface IOccupiedCellPathfinder : IPathfinder
    {
        List<Vector2Int> FindPath(
            Vector2Int start,
            Vector2Int end,
            Func<Vector2Int, bool> canTraverseOccupied);
    }
}