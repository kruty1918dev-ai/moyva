using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public sealed class BuildingPlacementBlocker
    {
        public BuildingPlacementBlockerKind Kind;
        public string Message;
        public Vector2Int? Position;
        public string BuildingId;
        public int Radius;
    }
}
