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
            string ownerId = null,
            bool includePendingPlacements = true)
        {
            BuildingId = buildingId;
            Position = position;
            IgnoredPendingPosition = ignoredPendingPosition;
            IgnoredOccupiedPosition = ignoredOccupiedPosition;
            IncludeResources = includeResources;
            IncludeDetails = includeDetails;
            OwnerId = ownerId;
            IncludePendingPlacements = includePendingPlacements;
        }

        public string BuildingId { get; }
        public Vector2Int Position { get; }
        public Vector2Int? IgnoredPendingPosition { get; }
        public Vector2Int? IgnoredOccupiedPosition { get; }
        public bool IncludeResources { get; }
        public bool IncludeDetails { get; }
        public string OwnerId { get; }
        /// <summary>
        /// When false, evaluates the persistent world state without temporary placement
        /// previews. Authoritative placement and confirmation requests keep the default true.
        /// </summary>
        public bool IncludePendingPlacements { get; }
    }
}
