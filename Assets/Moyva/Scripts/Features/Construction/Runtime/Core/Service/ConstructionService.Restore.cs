using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public void RestoreFromSave(Vector2Int position, string buildingId)
        {
            if (!TryRegisterBuildingFootprint(position, buildingId))
            {
                Debug.LogWarning($"[Construction] RestoreFromSave: footprint at {position} is occupied, skipping '{buildingId}'.");
                return;
            }

            _playerPlacedBuildings[position] = buildingId;
            _signalBus.Fire(new BuildingPlacedSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = _activeOwnerId,
            });
            ApplyBuildingFogReveal(buildingId, position);

            if (VerboseLogs)
                Debug.Log($"[Construction] RestoreFromSave: відновлено '{buildingId}' на {position}");
        }

        public bool TryDirectPlace(string buildingId, Vector2Int position, string placedByFactionId)
            => TryCommitAuthoritativePlacement(
                buildingId,
                position,
                placedByFactionId,
                ConstructionPlacementCommitIntent.None,
                ConstructionPlacementAttemptSource.DirectPlace,
                includePendingPlacements: true);

        public bool TryPlaceAuthoritatively(
            string buildingId,
            Vector2Int position,
            string ownerId,
            ConstructionPlacementCommitIntent intent)
        {
            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            if (IsConfirmedPlacementAlreadyApplied(
                    buildingId,
                    position,
                    normalizedOwnerId))
            {
                return true;
            }

            return TryCommitAuthoritativePlacement(
                buildingId,
                position,
                normalizedOwnerId,
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
                && relocationSource.Value == position)
            {
                Debug.LogWarning(
                    $"[Construction] Rejected relocation of '{buildingId}': source and target are both {position}.");
                return false;
            }

            bool isRelocation = relocationSource.HasValue;
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
                                .SatisfiedReplacementBuildingId));
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

                if (!TryRegisterBuildingFootprint(position, buildingId))
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

            _signalBus.Fire(new BuildingPlacedSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = ownerId,
                SourceFactionId = ownerId,
                HasRelocationSource = isRelocation,
                RelocationSourcePosition =
                    relocationSource.GetValueOrDefault(),
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

                if (!TryRegisterBuildingFootprint(position, buildingId))
                    return false;

                targetRegistered = true;
                if (replacementRemoved)
                    RemovePlacedRecordAt(replacedOrigin);
                if (relocationSource.HasValue)
                    RemovePlacedRecordAt(relocationSource.Value);

                _factionPlacedBuildings[position] =
                    (buildingId, normalizedOwnerId);
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

        public bool TryDemolishByFaction(Vector2Int position, string factionId)
        {
            Vector2Int origin = ResolvePlacedOrigin(position);
            if (!_factionPlacedBuildings.TryGetValue(origin, out var entry) || entry.FactionId != factionId)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDemolishByFaction({position}): будівля не знайдена або не належить фракції '{factionId}'.");
                return false;
            }

            UnregisterBuildingFootprint(origin, entry.BuildingId);
            _factionPlacedBuildings.Remove(origin);
            _signalBus.Fire(new BuildingDemolishedSignal
            {
                BuildingId = entry.BuildingId,
                Position = origin,
                SourceFactionId = factionId
            });
            if (VerboseLogs)
                Debug.Log($"[Construction] TryDemolishByFaction: знесено '{entry.BuildingId}' на {origin} від '{factionId}'.");
            return true;
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
