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
                return false;

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
                return false;
            }

            return TryCommitAuthoritativePlacement(
                buildingId,
                position,
                authorizedOwnerId,
                intent,
                ConstructionPlacementAttemptSource.NetworkRequest,
                includePendingPlacements: false);
        }

        private bool TryCommitAuthoritativePlacement(
            string buildingId,
            Vector2Int position,
            string placedByFactionId,
            ConstructionPlacementCommitIntent intent,
            ConstructionPlacementAttemptSource attemptSource,
            bool includePendingPlacements)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                return false;
            }

            string ownerId = NormalizeOwnerId(placedByFactionId);
            Vector2Int? relocationSource =
                intent.RelocationSourcePosition;
            if (relocationSource.HasValue
                && IsCastleBuilding(buildingId))
            {
                return false;
            }

            if (relocationSource.HasValue
                && relocationSource.Value == position)
            {
                return false;
            }

            bool isRelocation = relocationSource.HasValue;
            if (isRelocation
                && IsCastleBuilding(buildingId))
            {
                return false;
            }

            if (isRelocation
                && !TryValidateOwnedRelocationSource(
                    buildingId,
                    relocationSource.Value,
                    ownerId))
            {
                return false;
            }

            ConstructionPlacementQueryResult placement = EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    position,
                    ignoredOccupiedPosition: relocationSource,
                    includeResources: !isRelocation,
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
                LogPlacementAttempt(
                    placement,
                    emitRejectedAction: true);
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
                    return false;
                targetRegistered = true;

                if (!isRelocation
                    && !TryConsumeConstructionResources(
                        position,
                        buildingId,
                        ownerId,
                        out var resourceReason))
                {
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
                    relocationSource.Value);
            }
            _buildingFogEffects.Apply(buildingId, position);
            LogPlacementAttempt(
                placement,
                emitRejectedAction: false);
            RecordConstructionAction(
                ownerId,
                isRelocation ? "building-relocate" : "building-place");
            if (VerboseLogs)
            {
            }
            return true;
        }
    }
}
