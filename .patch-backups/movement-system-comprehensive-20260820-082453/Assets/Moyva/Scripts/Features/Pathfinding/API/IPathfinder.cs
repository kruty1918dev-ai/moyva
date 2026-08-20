using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Pathfinding.API
{
    public interface IPathfinder
    {
        List<Vector2Int> FindPath(Vector2Int start, Vector2Int end);
        IEnumerable<Vector2Int> GetNeighbors(Vector2Int position);
    }

    public interface IOccupiedCellPathfinder : IPathfinder
    {
        List<Vector2Int> FindPath(
            Vector2Int start,
            Vector2Int end,
            Func<Vector2Int, bool> canTraverseOccupied);
    }

    public interface ITraversalPathfinder : IPathfinder
    {
        List<Vector2Int> FindPathWithTraversal(
            Vector2Int start,
            Vector2Int end,
            Func<Vector2Int, bool> canTraverseCell);
    }
}
