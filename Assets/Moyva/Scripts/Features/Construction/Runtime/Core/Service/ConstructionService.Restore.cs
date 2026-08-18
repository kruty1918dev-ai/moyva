using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public IReadOnlyList<ConstructionSavedPlacement>
            GetSavedPlacements()
        {
            var result =
                new List<ConstructionSavedPlacement>(
                    _playerPlacedBuildings.Count
                    + _factionPlacedBuildings.Count);

            // Owner-indexed placements are canonical. Legacy player placements
            // are appended only when no canonical record exists at the origin.
            foreach (var pair in _factionPlacedBuildings)
            {
                result.Add(
                    new ConstructionSavedPlacement(
                        pair.Key,
                        pair.Value.BuildingId,
                        NormalizeOwnerId(pair.Value.FactionId),
                        ResolvePlacedRotation(pair.Key)));
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (_factionPlacedBuildings.ContainsKey(pair.Key))
                    continue;

                result.Add(
                    new ConstructionSavedPlacement(
                        pair.Key,
                        pair.Value,
                        NormalizeOwnerId(_activeOwnerId),
                        ResolvePlacedRotation(pair.Key)));
            }

            result.Sort(
                (left, right) =>
                {
                    int byX =
                        left.Position.x.CompareTo(
                            right.Position.x);
                    if (byX != 0)
                        return byX;

                    int byY =
                        left.Position.y.CompareTo(
                            right.Position.y);
                    if (byY != 0)
                        return byY;

                    return string.CompareOrdinal(
                        left.BuildingId,
                        right.BuildingId);
                });

            return result;
        }

        public void RestoreFromSave(
            Vector2Int position,
            string buildingId)
            => RestoreFromSave(
                position,
                buildingId,
                _activeOwnerId);

        public void RestoreFromSave(
            Vector2Int position,
            string buildingId,
            string ownerId,
            ConstructionRotation rotation =
                ConstructionRotation.Degrees0)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return;

            if (!TryRegisterBuildingFootprint(
                    position,
                    buildingId,
                    rotation))
            {
                Debug.LogWarning(
                    $"{ModuleLogTag} restore-placement skipped " +
                    $"building={buildingId} position={position} " +
                    "reason=footprint-occupied");
                return;
            }
            _placedRotationByOrigin[position] = rotation;

            string normalizedOwner =
                NormalizeOwnerId(ownerId);
            _playerPlacedBuildings.Remove(position);
            _factionPlacedBuildings[position] =
                (buildingId, normalizedOwner);

            _signalBus.Fire(
                new BuildingPlacedSignal
                {
                    BuildingId = buildingId,
                    Position = position,
                    OwnerId = normalizedOwner,
                    SourceFactionId = normalizedOwner,
                    RotationQuarterTurns = (int)rotation,
                });
            ApplyBuildingFogReveal(
                buildingId,
                position);

            if (VerboseLogs)
            {
                Debug.Log(
                    $"{ModuleLogTag} restore-placement " +
                    $"building={buildingId} position={position} " +
                    $"owner={normalizedOwner}");
            }
        }

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
                Debug.LogWarning($"[Construction] TryDirectPlace rejected: {turnReason}");
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
                Debug.LogWarning(
                    $"[Construction] TryPlaceAuthoritatively rejected: {turnReason}");
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
                Debug.LogWarning(
                    $"[Construction] Authoritative placement at {position}: buildingId is empty.");
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
                Debug.LogWarning(
                    $"[Construction] Rejected relocation of '{buildingId}': source and target are both {position}.");
                return false;
            }

            bool isRelocation = relocationSource.HasValue;
            if (isRelocation
                && IsCastleBuilding(buildingId))
            {
                Debug.Log(
                    $"{ModuleLogTag} castle authoritative relocation blocked " +
                    $"owner={ownerId} source={relocationSource.Value} " +
                    $"target={position}");
                return false;
            }

            if (isRelocation
                && !TryValidateOwnedRelocationSource(
                    buildingId,
                    relocationSource.Value,
                    ownerId))
            {
                Debug.LogWarning(
                    $"[Construction] Rejected relocation of '{buildingId}' from {relocationSource.Value}: " +
                    $"the source is not an owned relocatable placement for '{ownerId}'.");
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
                    if (!TryResolveGateReplacement(
                            position,
                            buildingId,
                            out replacedOrigin,
                            out replacedBuildingId))
                    {
                        return false;
                    }

                    UnregisterBuildingFootprint(replacedOrigin, replacedBuildingId);
                    replacementRemoved = true;
                }

                if (isRelocation
                    && (!replacementRemoved
                        || replacedOrigin != relocationSource.Value))
                {
                    UnregisterBuildingFootprint(
                        relocationSource.Value,
                        buildingId);
                    relocationRemoved = true;
                }

                if (!TryRegisterBuildingFootprint(
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
                    if (VerboseLogs)
                        Debug.Log($"[MoyvaBuildGridDiag] direct-placement-blocked building='{buildingId}' origin={position} reason='{resourceReason}'");
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
                        UnregisterBuildingFootprint(position, buildingId);
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
                _fogOfWarService?.UnregisterUnit(
                    GetBuildingFogVisionAreaId(
                        relocationSource.Value));
            }
            ApplyBuildingFogReveal(buildingId, position);
            LogPlacementAttempt(
                placement,
                emitRejectedAction: false);
            RecordConstructionAction(
                ownerId,
                isRelocation ? "building-relocate" : "building-place");
            if (VerboseLogs)
            {
                Debug.Log(
                    $"[Construction] Authoritative placement: '{buildingId}' at {position} by '{ownerId}', " +
                    $"relocationSource={relocationSource?.ToString() ?? "none"}.");
            }
            return true;
        }

        public bool TryApplyConfirmedPlacement(
            string buildingId,
            Vector2Int position,
            string ownerId)
            => TryApplyConfirmedPlacement(
                buildingId,
                position,
                ownerId,
                ConstructionPlacementCommitIntent.None);

        public bool TryApplyConfirmedPlacement(
            string buildingId,
            Vector2Int position,
            string ownerId,
            ConstructionPlacementCommitIntent intent)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            if (IsConfirmedPlacementAlreadyApplied(
                    buildingId,
                    position,
                    normalizedOwnerId))
            {
                return true;
            }

            Vector2Int? relocationSource =
                intent.RelocationSourcePosition;
            if (relocationSource.HasValue
                && relocationSource.Value == position)
            {
                return false;
            }

            bool relocationSourcePresent = false;
            if (relocationSource.HasValue)
            {
                relocationSourcePresent =
                    TryValidateOwnedPlacedSource(
                        buildingId,
                        relocationSource.Value,
                        normalizedOwnerId);
                if (!relocationSourcePresent
                    && _objectsMapService.TryGetOccupant(
                        relocationSource.Value,
                        out string sourceOccupantId))
                {
                    if (!string.Equals(
                            sourceOccupantId,
                            buildingId,
                            StringComparison.Ordinal))
                    {
                        return false;
                    }

                    // A replica may have restored the footprint before its
                    // owner-index snapshot. The host confirmation is trusted;
                    // remove the matching source footprint to converge.
                    relocationSourcePresent = true;
                }
            }

            bool replacementRemoved = false;
            bool targetRegistered = false;
            bool modelCommitted = false;
            bool relocationRemoved = false;
            Vector2Int replacedOrigin = default;
            string replacedBuildingId = null;
            try
            {
                if (TryResolveGateReplacement(
                        position,
                        buildingId,
                        out replacedOrigin,
                        out replacedBuildingId))
                {
                    UnregisterBuildingFootprint(
                        replacedOrigin,
                        replacedBuildingId);
                    replacementRemoved = true;
                }

                if (relocationSourcePresent
                    && (!replacementRemoved
                        || replacedOrigin != relocationSource.Value))
                {
                    UnregisterBuildingFootprint(
                        relocationSource.Value,
                        buildingId);
                    relocationRemoved = true;
                }

                if (!TryRegisterBuildingFootprint(
                        position,
                        buildingId,
                        intent.Rotation))
                    return false;

                targetRegistered = true;
                if (replacementRemoved)
                    RemovePlacedRecordAt(replacedOrigin);
                if (relocationSource.HasValue)
                    RemovePlacedRecordAt(relocationSource.Value);

                _factionPlacedBuildings[position] =
                    (buildingId, normalizedOwnerId);
                _placedRotationByOrigin[position] = intent.Rotation;
                modelCommitted = true;
            }
            finally
            {
                if (!modelCommitted)
                {
                    if (targetRegistered)
                        UnregisterBuildingFootprint(position, buildingId);
                    if (relocationRemoved)
                    {
                        RestoreBuildingFootprintOrLog(
                            relocationSource.Value,
                            buildingId,
                            "confirmed-relocation");
                    }
                    if (replacementRemoved)
                    {
                        if (!relocationRemoved
                            || replacedOrigin != relocationSource.Value)
                        {
                            RestoreBuildingFootprintOrLog(
                                replacedOrigin,
                                replacedBuildingId,
                                "confirmed-placement");
                        }
                    }
                }
            }

            InvalidatePlacementAvailabilityCache();
            _signalBus.Fire(new BuildingPlacedSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = normalizedOwnerId,
                SourceFactionId = normalizedOwnerId,
                HasRelocationSource =
                    relocationSource.HasValue,
                RelocationSourcePosition =
                    relocationSource.GetValueOrDefault(),
                RotationQuarterTurns = (int)intent.Rotation,
            });
            if (relocationSource.HasValue)
            {
                _fogOfWarService?.UnregisterUnit(
                    GetBuildingFogVisionAreaId(
                        relocationSource.Value));
            }
            ApplyBuildingFogReveal(buildingId, position);
            return true;
        }

        private bool TryValidateOwnedRelocationSource(
            string buildingId,
            Vector2Int sourcePosition,
            string ownerId)
        {
            BuildingDefinition definition =
                _placementBuildingRegistry?.GetById(buildingId);
            return BuildingDefinitionCapabilities
                       .SupportsUniqueRelocation(definition)
                   && TryValidateOwnedPlacedSource(
                       buildingId,
                       sourcePosition,
                       ownerId);
        }

        private bool TryValidateOwnedPlacedSource(
            string buildingId,
            Vector2Int sourcePosition,
            string ownerId)
        {
            if (_factionPlacedBuildings.TryGetValue(
                    sourcePosition,
                    out var factionPlacement))
            {
                return string.Equals(
                           factionPlacement.BuildingId,
                           buildingId,
                           StringComparison.Ordinal)
                       && string.Equals(
                           NormalizeOwnerId(
                               factionPlacement.FactionId),
                           NormalizeOwnerId(ownerId),
                           StringComparison.Ordinal);
            }

            return _playerPlacedBuildings.TryGetValue(
                       sourcePosition,
                       out string localBuildingId)
                   && string.Equals(
                       localBuildingId,
                       buildingId,
                       StringComparison.Ordinal)
                   && string.Equals(
                       NormalizeOwnerId(_activeOwnerId),
                       NormalizeOwnerId(ownerId),
                       StringComparison.Ordinal);
        }

        private bool IsConfirmedPlacementAlreadyApplied(
            string buildingId,
            Vector2Int position,
            string ownerId)
        {
            if (_factionPlacedBuildings.TryGetValue(
                    position,
                    out var factionPlacement))
            {
                return string.Equals(
                           factionPlacement.BuildingId,
                           buildingId,
                           StringComparison.Ordinal)
                       && string.Equals(
                           factionPlacement.FactionId,
                           ownerId,
                           StringComparison.Ordinal);
            }

            return _playerPlacedBuildings.TryGetValue(
                       position,
                       out string localBuildingId)
                   && string.Equals(
                       localBuildingId,
                       buildingId,
                       StringComparison.Ordinal)
                   && string.Equals(
                       NormalizeOwnerId(_activeOwnerId),
                       ownerId,
                       StringComparison.Ordinal);
        }

        public bool TryDestroyPlacedBuilding(
            Vector2Int position,
            string cause = null)
        {
            Vector2Int origin = ResolvePlacedOrigin(position);

            string buildingId = null;
            string ownerId = null;

            if (_factionPlacedBuildings.TryGetValue(
                    origin,
                    out var factionEntry))
            {
                buildingId = factionEntry.BuildingId;
                ownerId = NormalizeOwnerId(
                    factionEntry.FactionId);
            }
            else if (_playerPlacedBuildings.TryGetValue(
                         origin,
                         out string localBuildingId))
            {
                buildingId = localBuildingId;
                ownerId = NormalizeOwnerId(_activeOwnerId);
            }

            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            UnregisterBuildingFootprint(
                origin,
                buildingId);
            RemovePlacedRecordAt(origin);

            _fogOfWarService?.UnregisterUnit(
                GetBuildingFogVisionAreaId(origin));

            _signalBus.Fire(
                new BuildingDemolishedSignal
                {
                    BuildingId = buildingId,
                    Position = origin,
                    SourceFactionId = ownerId,
                });

            Debug.Log(
                $"{ModuleLogTag} building-destroyed " +
                $"building={buildingId} position={origin} " +
                $"owner={ownerId} cause={cause ?? "unknown"}");

            return true;
        }

        public bool TryDemolishByFaction(Vector2Int position, string factionId)
        {
            if (!TryAuthorizeConstructionMutation(
                    factionId,
                    "construction demolition",
                    requireLocalOwner: false,
                    out string authorizedOwnerId,
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                Debug.LogWarning(
                    $"[Construction] TryDemolishByFaction rejected: {turnReason}");
                return false;
            }

            bool demolished = TryApplyCommittedDemolition(
                position,
                authorizedOwnerId,
                recordTurnAction: true,
                idempotentWhenMissing: false,
                out string demolitionReason);
            if (!demolished)
                _lastActionMessage = demolitionReason;
            return demolished;
        }

        public bool HasPlacedBuilding(string buildingId, string ownerId = null)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            string normalizedOwner = string.IsNullOrWhiteSpace(ownerId) ? null : ownerId.Trim();

            foreach (var pair in _factionPlacedBuildings)
            {
                if (!string.Equals(pair.Value.BuildingId, buildingId, StringComparison.Ordinal))
                    continue;

                if (normalizedOwner == null || string.Equals(pair.Value.FactionId, normalizedOwner, StringComparison.Ordinal))
                    return true;
            }

            if (normalizedOwner == null || string.Equals(normalizedOwner, _activeOwnerId, StringComparison.Ordinal))
            {
                foreach (var pair in _playerPlacedBuildings)
                {
                    if (string.Equals(pair.Value, buildingId, StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }
    }
}
