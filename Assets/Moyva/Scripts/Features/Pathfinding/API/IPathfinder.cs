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

public delegate bool PathTraversalCostResolver(
    Vector2Int from,
    Vector2Int to,
    out float cost);

public interface ICostAwarePathfinder : IPathfinder
{
    List<Vector2Int> FindPathWithCosts(
        Vector2Int start,
        Vector2Int end,
        PathTraversalCostResolver traversalCostResolver);
}
}
