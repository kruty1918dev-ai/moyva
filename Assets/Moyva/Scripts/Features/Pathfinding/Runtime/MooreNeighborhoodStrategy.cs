using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Pathfinding.API;
using UnityEngine;

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
    public sealed class MooreNeighborhoodStrategy : INeighborhoodStrategy
    {
        public GridNeighborhoodMode Mode => GridNeighborhoodMode.Moore8;

        public IEnumerable<Vector2Int> GetNeighbors(Vector2Int position, IGridService gridService)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    var next = new Vector2Int(position.x + x, position.y + y);
                    if (gridService.TryGetTileData(next, out _))
                        yield return next;
                }
            }
        }

        public float GetStepCost(Vector2Int from, Vector2Int to)
            => from.x != to.x && from.y != to.y ? 1.41421356f : 1f;

        public float EstimateDistance(Vector2Int from, Vector2Int to)
        {
            float dx = Mathf.Abs(from.x - to.x);
            float dy = Mathf.Abs(from.y - to.y);
            return (dx + dy) + (1.41421356f - 2f) * Mathf.Min(dx, dy);
        }
    }
}