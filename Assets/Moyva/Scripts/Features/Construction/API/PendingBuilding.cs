using System;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public readonly struct PendingBuilding : IEquatable<PendingBuilding>
    {
        public readonly string TypeId;
        public readonly Vector2Int Position;
        public readonly string TempId;

        public PendingBuilding(string typeId, Vector2Int position, string tempId)
        {
            TypeId = typeId;
            Position = position;
            TempId = tempId;
        }

        public bool Equals(PendingBuilding other) => TempId == other.TempId;
        public override bool Equals(object obj) => obj is PendingBuilding other && Equals(other);
        public override int GetHashCode() => TempId?.GetHashCode() ?? 0;
    }
}
