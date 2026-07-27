using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public readonly struct BuildingPlacementSimulationEntry
    {
        public BuildingPlacementSimulationEntry(Vector2Int position, string buildingId)
            : this(position, buildingId, ownerId: null)
        {
        }

        public BuildingPlacementSimulationEntry(
            Vector2Int position,
            string buildingId,
            string ownerId)
        {
            Position = position;
            BuildingId = buildingId;
            OwnerId = ownerId;
        }

        public Vector2Int Position { get; }
        public string BuildingId { get; }
        public string OwnerId { get; }
    }
}
