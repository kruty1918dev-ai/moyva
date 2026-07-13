using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public readonly struct ConstructionPlacementQueryRequest
    {
        public ConstructionPlacementQueryRequest(
            string buildingId,
            Vector2Int position,
            Vector2Int? ignoredPendingPosition = null,
            Vector2Int? ignoredOccupiedPosition = null,
            bool includeResources = false,
            bool includeDetails = false,
            string ownerId = null)
        {
            BuildingId = buildingId;
            Position = position;
            IgnoredPendingPosition = ignoredPendingPosition;
            IgnoredOccupiedPosition = ignoredOccupiedPosition;
            IncludeResources = includeResources;
            IncludeDetails = includeDetails;
            OwnerId = ownerId;
        }

        public string BuildingId { get; }
        public Vector2Int Position { get; }
        public Vector2Int? IgnoredPendingPosition { get; }
        public Vector2Int? IgnoredOccupiedPosition { get; }
        public bool IncludeResources { get; }
        public bool IncludeDetails { get; }
        public string OwnerId { get; }
    }
}
