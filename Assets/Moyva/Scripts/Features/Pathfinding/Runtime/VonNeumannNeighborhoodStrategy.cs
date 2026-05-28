using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Pathfinding.API;
using UnityEngine;

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
    public sealed class VonNeumannNeighborhoodStrategy : INeighborhoodStrategy
    {
        private static readonly Vector2Int[] Offsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
        };

        public GridNeighborhoodMode Mode => GridNeighborhoodMode.VonNeumann4;

        public IEnumerable<Vector2Int> GetNeighbors(Vector2Int position, IGridService gridService)
        {
            for (int i = 0; i < Offsets.Length; i++)
            {
                var next = position + Offsets[i];
                if (gridService.TryGetTileData(next, out _))
                    yield return next;
            }
        }

        public float GetStepCost(Vector2Int from, Vector2Int to) => 1f;

        public float EstimateDistance(Vector2Int from, Vector2Int to)
            => Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
}