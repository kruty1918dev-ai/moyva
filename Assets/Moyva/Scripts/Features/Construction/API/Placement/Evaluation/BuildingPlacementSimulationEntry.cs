using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public readonly struct BuildingPlacementSimulationEntry
    {
        public BuildingPlacementSimulationEntry(Vector2Int position, string buildingId)
            : this(
                position,
                buildingId,
                ownerId: null,
                ConstructionRotation.Degrees0)
        {
        }

        public BuildingPlacementSimulationEntry(
            Vector2Int position,
            string buildingId,
            string ownerId,
            ConstructionRotation rotation =
                ConstructionRotation.Degrees0)
        {
            Position = position;
            BuildingId = buildingId;
            OwnerId = ownerId;
            Rotation = rotation;
        }

        public Vector2Int Position { get; }
        public string BuildingId { get; }
        public string OwnerId { get; }
        public ConstructionRotation Rotation { get; }
    }
}
