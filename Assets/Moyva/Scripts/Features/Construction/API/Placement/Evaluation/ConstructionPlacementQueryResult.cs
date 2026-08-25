namespace Kruty1918.Moyva.Construction.API
{
    public readonly struct ConstructionPlacementQueryResult
    {
        /// <summary>
        /// Compatibility constructor for callers that only distinguish spatial and
        /// resource validation. Availability and authority are treated as valid.
        /// </summary>
        public ConstructionPlacementQueryResult(
            bool isSpatiallyValid,
            bool resourcesValid,
            bool isGateReplacement,
            string reason = null,
            BuildingPlacementEvaluationResult evaluationResult = null)
            : this(
                availabilityValid: true,
                spatialValid: isSpatiallyValid,
                resourcesValid: resourcesValid,
                authorityValid: true,
                isGateReplacement: isGateReplacement,
                reason: reason,
                evaluationResult: evaluationResult)
        {
        }

        public ConstructionPlacementQueryResult(
            bool availabilityValid,
            bool spatialValid,
            bool resourcesValid,
            bool authorityValid,
            bool isGateReplacement,
            string reason = null,
            BuildingPlacementEvaluationResult evaluationResult = null)
        {
            AvailabilityValid = availabilityValid;
            SpatialValid = spatialValid;
            ResourcesValid = resourcesValid;
            AuthorityValid = authorityValid;
            IsGateReplacement = isGateReplacement;
            Reason = reason;
            EvaluationResult = evaluationResult;
        }

        /// <summary>
        /// Global/non-positional requirements, such as a valid definition,
        /// prerequisites and per-owner limits.
        /// </summary>
        public bool AvailabilityValid { get; }

        /// <summary>
        /// Position-dependent requirements, including bounds, footprint,
        /// occupancy, terrain, fog, influence and spacing.
        /// </summary>
        public bool SpatialValid { get; }

        /// <summary>
        /// Compatibility alias retained for existing integrations.
        /// </summary>
        public bool IsSpatiallyValid => SpatialValid;

        public bool ResourcesValid { get; }
        public bool AuthorityValid { get; }
        public bool IsGateReplacement { get; }
        public string Reason { get; }
        public BuildingPlacementEvaluationResult EvaluationResult { get; }

        public bool CanSelect => AvailabilityValid;
        public bool CanPreview => AvailabilityValid && SpatialValid;
        public bool CanCommit => CanPreview && ResourcesValid && AuthorityValid;

        /// <summary>
        /// Compatibility alias. A historically valid result represented a
        /// commit-ready placement, so it intentionally maps to <see cref="CanCommit"/>.
        /// </summary>
        public bool IsValid => CanCommit;
    }

    /// <summary>
    /// Non-spatial availability used by construction menus and authoritative
    /// selection guards. Resource validation can be deferred only when no
    /// funding context can be resolved yet (for example before a settlement
    /// position is known).
    /// </summary>
    public readonly struct ConstructionSelectionAvailabilityResult
    {
        public ConstructionSelectionAvailabilityResult(
            bool globalAvailabilityValid,
            bool resourcesValid,
            bool resourceCheckPerformed,
            string reason = null,
            string reasonCode = null)
        {
            GlobalAvailabilityValid = globalAvailabilityValid;
            ResourcesValid = resourcesValid;
            ResourceCheckPerformed = resourceCheckPerformed;
            Reason = reason;
            ReasonCode = reasonCode;
        }

        public bool GlobalAvailabilityValid { get; }
        public bool ResourcesValid { get; }
        public bool ResourceCheckPerformed { get; }
        public string Reason { get; }
        public string ReasonCode { get; }

        public bool CanSelect =>
            GlobalAvailabilityValid
            && (!ResourceCheckPerformed || ResourcesValid);
    }

    public interface IConstructionSelectionAvailabilityQuery
    {
        ConstructionSelectionAvailabilityResult EvaluateSelectionAvailability(
            string buildingId,
            string ownerId = null,
            UnityEngine.Vector2Int? preferredFundingPosition = null,
            bool includePendingPlacements = true);
    }

}
