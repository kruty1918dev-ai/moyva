using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public bool TryDestroyPlacedBuilding(
            Vector2Int position,
            string cause = null)
        {
            Vector2Int origin = _footprints.ResolveOrigin(position);

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

            _footprints.Unregister(
                origin,
                buildingId);
            RemovePlacedRecordAt(origin);

            _buildingFogEffects.Remove(origin);

            _signalBus.Fire(
                new BuildingDemolishedSignal
                {
                    BuildingId = buildingId,
                    Position = origin,
                    SourceFactionId = ownerId,
                });

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
