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
}