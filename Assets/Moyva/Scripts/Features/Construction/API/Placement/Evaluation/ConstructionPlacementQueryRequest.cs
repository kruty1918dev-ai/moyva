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
            bool includePendingPlacements = true,
            ConstructionPlacementAttemptSource attemptSource = ConstructionPlacementAttemptSource.Unknown,
            bool allowUniquePreviewRelocation = false)
        {
            BuildingId = buildingId;
            Position = position;
            IgnoredPendingPosition = ignoredPendingPosition;
            IgnoredOccupiedPosition = ignoredOccupiedPosition;
            IncludeResources = includeResources;
            IncludeDetails = includeDetails;
            OwnerId = ownerId;
            IncludePendingPlacements = includePendingPlacements;
            AttemptSource = attemptSource;
            AllowUniquePreviewRelocation = allowUniquePreviewRelocation;
        }

        public string BuildingId { get; }
        public Vector2Int Position { get; }
        public Vector2Int? IgnoredPendingPosition { get; }
        public Vector2Int? IgnoredOccupiedPosition { get; }
        public bool IncludeResources { get; }
        public bool IncludeDetails { get; }
        public string OwnerId { get; }

        /// <summary>
        /// When false, evaluates only persistent world state and ignores temporary previews.
        /// Confirmation and direct placement normally keep this true and explicitly identify
        /// the preview currently being confirmed or moved.
        /// </summary>
        public bool IncludePendingPlacements { get; }

        /// <summary>
        /// Identifies the caller for structured placement diagnostics.
        /// </summary>
        public ConstructionPlacementAttemptSource AttemptSource { get; }

        /// <summary>
        /// When enabled, a selected movable unique building (for example a one-per-owner castle)
        /// automatically ignores its existing preview or placed origin while evaluating a new
        /// target. Authoritative direct placement must pass false.
        /// </summary>
        public bool AllowUniquePreviewRelocation { get; }

        public ConstructionPlacementQueryRequest WithIgnoredPositions(
            Vector2Int? ignoredPendingPosition,
            Vector2Int? ignoredOccupiedPosition)
        {
            return new ConstructionPlacementQueryRequest(
                BuildingId,
                Position,
                ignoredPendingPosition,
                ignoredOccupiedPosition,
                IncludeResources,
                IncludeDetails,
                OwnerId,
                IncludePendingPlacements,
                AttemptSource,
                AllowUniquePreviewRelocation);
        }
    }
}
