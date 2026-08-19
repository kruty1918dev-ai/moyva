using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitMovementQuery
    {
        IReadOnlyList<UnitMovementTileSnapshot> GetMovementTiles(string unitId);
    }

    public readonly struct UnitMovementTileSnapshot
    {
        public UnitMovementTileSnapshot(
            Vector2Int position,
            bool isReachable,
            float cost,
            string reason = null)
        {
            Position = position;
            IsReachable = isReachable;
            Cost = cost;
            Reason = reason;
        }

        public Vector2Int Position { get; }
        public bool IsReachable { get; }
        public float Cost { get; }
        public string Reason { get; }
    }
}
