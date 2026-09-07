using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public bool TryDirectPlace(string buildingId, Vector2Int position, string placedByFactionId)
        {
            if (!TryAuthorizeConstructionMutation(
                    placedByFactionId,
                    "direct construction placement",
                    requireLocalOwner: false,
                    out string authorizedOwnerId,
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                return false;
            }

            return TryCommitAuthoritativePlacement(
                buildingId,
                position,
                authorizedOwnerId,
                ConstructionPlacementCommitIntent.None,
                ConstructionPlacementAttemptSource.DirectPlace,
                includePendingPlacements: true);
        }

        public bool TryPlaceAuthoritatively(
            string buildingId,
            Vector2Int position,
            string ownerId,
            ConstructionPlacementCommitIntent intent)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    "Authoritative placement owner is empty.");
                return false;
            }

            string normalizedOwnerId = ownerId.Trim();
            if (IsConfirmedPlacementAlreadyApplied(
                    buildingId,
                    position,
                    normalizedOwnerId))
            {
                return true;
            }

            if (!TryAuthorizeConstructionMutation(
                    normalizedOwnerId,
                    "host-authoritative construction placement",
                    requireLocalOwner: false,
                    out string authorizedOwnerId,
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    turnReason);
                return false;
            }

            return TryCommitAuthoritativePlacement(
                buildingId,
                position,
                authorizedOwnerId,
                intent,
                ConstructionPlacementAttemptSource.NetworkRequest,
                includePendingPlacements: false,
                consumeResources: true);
        }

        public bool TryPlacePrepaidAuthoritatively(
            string buildingId,
            Vector2Int position,
            string ownerId,
            ConstructionPlacementCommitIntent intent)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    "Prepaid placement owner is empty.");
                return false;
            }

            string normalizedOwnerId = ownerId.Trim();
            if (!TryAuthorizeConstructionMutation(
                    normalizedOwnerId,
                    "prepaid construction placement",
                    requireLocalOwner: false,
                    out string authorizedOwnerId,
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                LogPlacementCommitRejected(buildingId, position, turnReason);
                return false;
            }

            return TryCommitAuthoritativePlacement(
                buildingId,
                position,
                authorizedOwnerId,
                intent,
                ConstructionPlacementAttemptSource.DirectPlace,
                includePendingPlacements: false,
                consumeResources: false);
        }

        private bool TryCommitAuthoritativePlacement(
            string buildingId,
            Vector2Int position,
            string placedByFactionId,
            ConstructionPlacementCommitIntent intent,
            ConstructionPlacementAttemptSource attemptSource,
            bool includePendingPlacements,
            bool consumeResources = true)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    "Building id is empty.");
                return false;
            }

            string ownerId = NormalizeOwnerId(placedByFactionId);
            Vector2Int? relocationSource =
                intent.RelocationSourcePosition;
            if (relocationSource.HasValue
                && IsCastleBuilding(buildingId))
            {
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    "Castle cannot be relocated.");
                return false;
            }

            if (relocationSource.HasValue
                && relocationSource.Value == position)
            {
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    "Relocation source is the same as target position.");
                return false;
            }

            bool isRelocation = relocationSource.HasValue;
            if (isRelocation
                && IsCastleBuilding(buildingId))
            {
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    "Castle cannot be relocated.");
                return false;
            }

            if (isRelocation
                && !TryValidateOwnedRelocationSource(
                    buildingId,
                    relocationSource.Value,
                    ownerId))
            {
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    $"Relocation source {relocationSource.Value} is not an owned movable '{buildingId}'.");
                return false;
            }

            ConstructionPlacementQueryResult placement = EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    position,
                    ignoredOccupiedPosition: relocationSource,
                    includeResources: consumeResources && !isRelocation,
                    includeDetails: true,
                    ownerId: ownerId,
                    includePendingPlacements:
                        includePendingPlacements,
                    attemptSource: attemptSource,
                    allowUniquePreviewRelocation: false,
                    satisfiedReplacementBuildingId:
                        attemptSource
                        == ConstructionPlacementAttemptSource
                            .NetworkRequest
                            ? null
                            : intent
                                .SatisfiedReplacementBuildingId,
                    rotation: intent.Rotation));
            if (!placement.CanCommit)
            {
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    DescribePlacementQueryRejection(placement));
                return false;
            }

            bool replacementRemoved = false;
            bool targetRegistered = false;
            bool modelCommitted = false;
            bool relocationRemoved = false;
            Vector2Int replacedOrigin = default;
            string replacedBuildingId = null;
            try
            {
                if (placement.IsGateReplacement)
                {
                    if (!_replacementPolicy.TryResolveGateReplacement(
                            position,
                            buildingId,
                            out replacedOrigin,
                            out replacedBuildingId))
                    {
                        LogPlacementCommitRejected(
                            buildingId,
                            position,
                            "Gate replacement marker was valid during query but could not be resolved during commit.");
                        return false;
                    }

                    _footprints.Unregister(replacedOrigin, replacedBuildingId);
                    replacementRemoved = true;
                }

                if (isRelocation
                    && (!replacementRemoved
                        || replacedOrigin != relocationSource.Value))
                {
                    _footprints.Unregister(
                        relocationSource.Value,
                        buildingId);
                    relocationRemoved = true;
                }

                if (!_footprints.TryRegister(
                        position,
                        buildingId,
                        intent.Rotation))
                {
                    LogPlacementCommitRejected(
                        buildingId,
                        position,
                        "Footprint registration failed.");
                    return false;
                }
                targetRegistered = true;

                if (consumeResources
                    && !isRelocation
                    && !TryConsumeConstructionResources(
                        position,
                        buildingId,
                        ownerId,
                        out var resourceReason))
                {
                    _lastActionMessage = resourceReason;
                    LogPlacementCommitRejected(
                        buildingId,
                        position,
                        resourceReason);
                    return false;
                }

                if (replacementRemoved)
                    RemovePlacedRecordAt(replacedOrigin);
                if (isRelocation)
                    RemovePlacedRecordAt(relocationSource.Value);

                _factionPlacedBuildings[position] = (buildingId, ownerId);
                _placedRotationByOrigin[position] = intent.Rotation;
                modelCommitted = true;
            }
            finally
            {
                if (!modelCommitted)
                {
                    if (targetRegistered)
                        _footprints.Unregister(position, buildingId);
                    if (relocationRemoved)
                    {
                        RestoreBuildingFootprintOrLog(
                            relocationSource.Value,
                            buildingId,
                            "authoritative-relocation");
                    }
                    if (replacementRemoved)
                    {
                        if (!relocationRemoved
                            || replacedOrigin != relocationSource.Value)
                        {
                            RestoreBuildingFootprintOrLog(
                                replacedOrigin,
                                replacedBuildingId,
                                "authoritative-replacement");
                        }
                    }
                }
            }

            InvalidatePlacementAvailabilityCache();
            _signalBus.Fire(new BuildingPlacedSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = ownerId,
                SourceFactionId = ownerId,
                HasRelocationSource = isRelocation,
                RelocationSourcePosition =
                    relocationSource.GetValueOrDefault(),
                RotationQuarterTurns = (int)intent.Rotation,
            });
            if (isRelocation)
            {
                _buildingFogEffects.Remove(
                    relocationSource.Value,
                    ownerId);
            }
            _buildingFogEffects.ApplyOnPlaced(
                buildingId,
                position,
                ownerId);
            RecordConstructionAction(
                ownerId,
                isRelocation ? "building-relocate" : "building-place");
            return true;
        }
    }
}
