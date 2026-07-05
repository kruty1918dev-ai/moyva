using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public readonly struct BuildingPlacementSimulationEntry
    {
        public BuildingPlacementSimulationEntry(Vector2Int position, string buildingId)
        {
            Position = position;
            BuildingId = buildingId;
        }

        public Vector2Int Position { get; }
        public string BuildingId { get; }
    }
}
