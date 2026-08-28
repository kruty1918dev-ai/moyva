using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
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
            {
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    "Confirmed placement building id is empty.");
                return false;
            }

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
                LogPlacementCommitRejected(
                    buildingId,
                    position,
                    "Confirmed relocation source is the same as target position.");
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
                        LogPlacementCommitRejected(
                            buildingId,
                            position,
                            $"Confirmed relocation source {relocationSource.Value} contains '{sourceOccupantId}', not '{buildingId}'.");
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
                if (_replacementPolicy.TryResolveGateReplacement(
                        position,
                        buildingId,
                        out replacedOrigin,
                        out replacedBuildingId))
                {
                    _footprints.Unregister(
                        replacedOrigin,
                        replacedBuildingId);
                    replacementRemoved = true;
                }

                if (relocationSourcePresent
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
                        "Confirmed placement footprint registration failed.");
                    return false;
                }

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
                        _footprints.Unregister(position, buildingId);
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
                _buildingFogEffects.Remove(
                    relocationSource.Value);
            }
            _buildingFogEffects.ApplyOnPlaced(buildingId, position);
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

    }
}
